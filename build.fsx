#I @"packages/FAKE/tools"
#r "FakeLib.dll"
#r "System.Xml.Linq"

open System
open System.IO
open System.Text
open Fake
open Fake.FileUtils
open Fake.TaskRunnerHelper
open Fake.ProcessHelper

cd __SOURCE_DIRECTORY__

//--------------------------------------------------------------------------------
// Information about the project for Nuget and Assembly info files
//--------------------------------------------------------------------------------


let product = "Wire"
let authors = [ "Roger Johansson" ]
let copyright = "Copyright © 2013-2016 AsynkronIT"
let company = "AsynkronIT"
let description = "Binary serializer for POCO objects"
let tags = [ "serializer" ]
let configuration = "Release"
let toolDir = "tools"
let CloudCopyDir = toolDir @@ "CloudCopy"
let AzCopyDir = toolDir @@ "AzCopy"

// Read release notes and version

let root = @".\"
let solutionName = "Wire.sln"
let solutionPath = root @@ solutionName
let parsedRelease =
    File.ReadLines "RELEASE_NOTES.md"
    |> ReleaseNotesHelper.parseReleaseNotes

let envBuildNumber = System.Environment.GetEnvironmentVariable("BUILD_NUMBER")
let buildNumber = if String.IsNullOrWhiteSpace(envBuildNumber) then "0" else envBuildNumber

let version = parsedRelease.AssemblyVersion + "." + buildNumber
let preReleaseVersion = version + "-beta"

let isUnstableDocs = hasBuildParam "unstable"
let isPreRelease = hasBuildParam "nugetprerelease"
let release = if isPreRelease then ReleaseNotesHelper.ReleaseNotes.New(version, version + "-beta", parsedRelease.Notes) else parsedRelease

printfn "Assembly version: %s\nNuget version; %s\n" release.AssemblyVersion release.NugetVersion
//--------------------------------------------------------------------------------
// Directories

let binDir = "bin"
let testOutput = FullName "TestResults"
let perfOutput = FullName "PerfResults"

let nugetDir = binDir @@ "nuget"
let workingDir = binDir @@ "build"
let libDir = workingDir @@ @"lib\net45\"
let nugetExe = FullName (root @@ @".nuget\NuGet.exe")
let docDir = "bin" @@ "doc"
let sourceBrowserDocsDir = binDir @@ "sourcebrowser"
let msdeployPath = "C:\Program Files (x86)\IIS\Microsoft Web Deploy V3\msdeploy.exe"

open Fake.RestorePackageHelper
Target "RestorePackages" (fun _ -> 
     solutionPath
     |> RestoreMSSolutionPackages (fun p ->
         { p with
             OutputPath = root + "packages"
             Retries = 4 })
 )

//--------------------------------------------------------------------------------
// Clean build results

Target "Clean" <| fun _ ->
    DeleteDir binDir

//--------------------------------------------------------------------------------
// Generate AssemblyInfo files with the version for release notes 

open AssemblyInfoFile

Target "AssemblyInfo" <| fun _ ->
    CreateCSharpAssemblyInfoWithConfig (root + "SharedAssemblyInfo.cs") [
        Attribute.Company company
        Attribute.Copyright copyright
        Attribute.Trademark ""
        Attribute.Version version
        Attribute.FileVersion version ] <| AssemblyInfoFileConfig(false)

    for file in !! (root @@ "**/AssemblyInfo.fs") do
        let title =
            file
            |> Path.GetDirectoryName
            |> Path.GetDirectoryName
            |> Path.GetFileName

        CreateFSharpAssemblyInfo file [ 
            Attribute.Title title
            Attribute.Product product
            Attribute.Description description
            Attribute.Copyright copyright
            Attribute.Company company
            Attribute.ComVisible false
            Attribute.CLSCompliant true
            Attribute.Version version
            Attribute.FileVersion version ]

//--------------------------------------------------------------------------------
// Build the solution

Target "Build" <| fun _ ->
    !! solutionPath
    |> MSBuildRelease "" "Rebuild"
    |> ignore
    
//--------------------------------------------------------------------------------
// Build the docs
Target "Docs" <| fun _ ->
    !! "documentation/wiredoc.shfbproj"
    |> MSBuildRelease "" "Rebuild"
    |> ignore
    
//--------------------------------------------------------------------------------
// Push DOCs content to Windows Azure blob storage
Target "AzureDocsDeploy" (fun _ ->
    let rec pushToAzure docDir azureUrl container azureKey trialsLeft =
        let tracing = enableProcessTracing
        enableProcessTracing <- false
        let arguments = sprintf "/Source:%s /Dest:%s /DestKey:%s /S /Y /SetContentType" (Path.GetFullPath docDir) (azureUrl @@ container) azureKey
        tracefn "Pushing docs to %s. Attempts left: %d" (azureUrl) trialsLeft
        try 
            
            let result = ExecProcess(fun info ->
                info.FileName <- AzCopyDir @@ "AzCopy.exe"
                info.Arguments <- arguments) (TimeSpan.FromMinutes 120.0) //takes a very long time to upload
            if result <> 0 then failwithf "Error during AzCopy.exe upload to azure."
        with exn -> 
            if (trialsLeft > 0) then (pushToAzure docDir azureUrl container azureKey (trialsLeft-1))
            else raise exn
    let canPush = hasBuildParam "azureKey" && hasBuildParam "azureUrl"
    if (canPush) then
         printfn "Uploading API docs to Azure..."
         let azureUrl = getBuildParam "azureUrl"
         let azureKey = (getBuildParam "azureKey") + "==" //hack, because it looks like FAKE arg parsing chops off the "==" that gets tacked onto the end of each Azure storage key
         if(isUnstableDocs) then
            pushToAzure docDir azureUrl "unstable" azureKey 3
         if(not isUnstableDocs) then
            pushToAzure docDir azureUrl "stable" azureKey 3
            pushToAzure docDir azureUrl release.NugetVersion azureKey 3
    if(not canPush) then
        printfn "Missing required parameters to push docs to Azure. Run build HelpDocs to find out!"
            
)

Target "PublishDocs" DoNothing

//--------------------------------------------------------------------------------
// Build the SourceBrowser docs
//--------------------------------------------------------------------------------
Target "GenerateSourceBrowser" <| (fun _ ->
    DeleteDir sourceBrowserDocsDir

    let htmlGeneratorPath = root @@ "packages/Microsoft.SourceBrowser/tools/HtmlGenerator.exe"
    let arguments = sprintf "/out:%s %s" sourceBrowserDocsDir solutionPath
    printfn "Using SourceBrowser: %s %s" htmlGeneratorPath arguments
    
    let result = ExecProcess(fun info -> 
        info.FileName <- htmlGeneratorPath
        info.Arguments <- arguments) (System.TimeSpan.FromMinutes 20.0)
    
    if result <> 0 then failwithf "SourceBrowser failed. %s %s" htmlGeneratorPath arguments
)

//--------------------------------------------------------------------------------
// Publish the SourceBrowser docs
//--------------------------------------------------------------------------------
Target "PublishSourceBrowser" <| (fun _ ->
    let canPublish = hasBuildParam "publishsettings"
    if (canPublish) then
        let sourceBrowserPublishSettingsPath = getBuildParam "publishsettings"
        let arguments = sprintf "-verb:sync -source:contentPath=\"%s\" -dest:contentPath=sourcebrowser,publishSettings=\"%s\"" (Path.GetFullPath sourceBrowserDocsDir) sourceBrowserPublishSettingsPath
        printfn "Using MSDeploy: %s %s" msdeployPath arguments
    
        let result = ExecProcess(fun info -> 
            info.FileName <- msdeployPath
            info.Arguments <- arguments) (System.TimeSpan.FromMinutes 30.0) //takes a long time to upload
    
        if result <> 0 then failwithf "MSDeploy failed. %s %s" msdeployPath arguments
    else
        printfn "Missing required parameter to publish SourceBrowser docs. Run build HelpSourceBrowserDocs to find out!"
)

//--------------------------------------------------------------------------------
// Copy the build output to bin directory
//--------------------------------------------------------------------------------

Target "CopyOutput" <| fun _ ->
    
    let copyOutput project =
        let src = root @@ project @@ @"bin/Release/"
        let dst = binDir @@ project
        CopyDir dst src allFiles
    [ "Wire" ]
    |> List.iter copyOutput

Target "BuildRelease" DoNothing


//--------------------------------------------------------------------------------
// Clean test output

Target "CleanTests" <| fun _ ->
    DeleteDir testOutput

//--------------------------------------------------------------------------------
// Run tests

open Fake.Testing
Target "RunTests" <| fun _ ->  
    let testAssemblies = !! (root @@ "**/bin/Release/Wire.Tests.dll")

    mkdir testOutput
    let xunitToolPath = findToolInSubPath "xunit.console.exe" (root @@ "packages/xunit.runner.console*/tools")

    printfn "Using XUnit runner: %s" xunitToolPath
    xUnit2
        (fun p -> { p with XmlOutputPath = Some (testOutput + @"\XUnitTestResults.xml"); HtmlOutputPath = Some (testOutput + @"\XUnitTestResults.HTML"); ToolPath = xunitToolPath; TimeOut = System.TimeSpan.FromMinutes 30.0; Parallel = ParallelMode.NoParallelization })
        testAssemblies
 
//--------------------------------------------------------------------------------
// NBench targets 
//--------------------------------------------------------------------------------
Target "NBench" <| fun _ ->
    let testSearchPath = !! (root @@ "**/bin/Release/Wire.Tests.Performance.dll")

    mkdir perfOutput
    let nbenchTestPath = findToolInSubPath "NBench.Runner.exe" (root @@ "packges/NBench.Runner*")
    printfn "Using NBench.Runner: %s" nbenchTestPath

    let runNBench assembly =
        let spec = getBuildParam "spec"

        let args = new StringBuilder()
                |> append assembly
                |> append (sprintf "output-directory=\"%s\"" perfOutput)
                |> append (sprintf "concurrent=\"%b\"" true)
                |> append (sprintf "trace=\"%b\"" true)
                |> toText

        let result = ExecProcess(fun info -> 
            info.FileName <- nbenchTestPath
            info.WorkingDirectory <- (Path.GetDirectoryName (FullName nbenchTestPath))
            info.Arguments <- args) (System.TimeSpan.FromMinutes 15.0) (* Reasonably long-running task. *)
        if result <> 0 then failwithf "NBench.Runner failed. %s %s" nbenchTestPath args
    
    testSearchPath |> Seq.iter runNBench

//--------------------------------------------------------------------------------
// Clean NBench output
Target "CleanPerf" <| fun _ ->
    DeleteDir perfOutput

//--------------------------------------------------------------------------------
// Nuget targets 
//--------------------------------------------------------------------------------

//--------------------------------------------------------------------------------
// Clean nuget directory

Target "CleanNuget" <| fun _ ->
    CleanDir nugetDir

//--------------------------------------------------------------------------------
// Pack nuget for all projects
// Publish to nuget.org if nugetkey is specified

let createNugetPackages _ =
    let removeDir dir = 
        let del _ = 
            DeleteDir dir
            not (directoryExists dir)
        runWithRetries del 3 |> ignore

    let mutable dirId = 1

    ensureDirectory nugetDir
    for nuspec in !! (root @@ "**/Wire.nuspec") do
        printfn "Creating nuget packages for %s" nuspec
        
        let tempBuildDir = workingDir + dirId.ToString()
        ensureDirectory tempBuildDir

        CleanDir tempBuildDir

        let project = Path.GetFileNameWithoutExtension nuspec 
        let projectDir = Path.GetDirectoryName nuspec
        let projectFile = (!! (projectDir @@ project + ".*sproj")) |> Seq.head
        let releaseDir = projectDir @@ @"bin\Release"
        let packages = projectDir @@ "packages.config"
        let packageDependencies = if (fileExists packages) then (getDependencies packages) else []
        let pack outputDir symbolPackage =
            NuGetHelper.NuGet
                (fun p ->
                    { p with
                        Description = description
                        Authors = authors
                        Copyright = copyright
                        Project =  project
                        Properties = ["Configuration", "Release"]
                        ReleaseNotes = release.Notes |> String.concat "\n"
                        Version = release.NugetVersion
                        Tags = tags |> String.concat " "
                        OutputPath = outputDir
                        WorkingDir = tempBuildDir
                        SymbolPackage = symbolPackage
                        Dependencies = packageDependencies })
                nuspec

        // Copy dll, pdb and xml to libdir = tempBuildDir/lib/net45/
        let tempLibDir = tempBuildDir @@ @"lib\net45\"
        ensureDirectory tempLibDir
        !! (releaseDir @@ project + ".dll")
        ++ (releaseDir @@ project + ".pdb")
        ++ (releaseDir @@ project + ".xml")
        ++ (releaseDir @@ project + ".ExternalAnnotations.xml")
        |> CopyFiles tempLibDir

        // Copy all src-files (.cs and .fs files) to tempBuildDir/src
        let nugetSrcDir = tempBuildDir @@ root
        // CreateDir nugetSrcDir

        let isCs = hasExt ".cs"
        let isFs = hasExt ".fs"
        let isAssemblyInfo f = (filename f).Contains("AssemblyInfo")
        let isSrc f = (isCs f || isFs f) && not (isAssemblyInfo f) 
        CopyDir nugetSrcDir projectDir isSrc
        
        //Remove tempBuildDir/src/obj and tempBuildDir/src/bin
        removeDir (nugetSrcDir @@ "obj")
        removeDir (nugetSrcDir @@ "bin")

        // Create both normal nuget package and symbols nuget package. 
        // Uses the files we copied to workingDir and outputs to nugetdir
        pack nugetDir NugetSymbolPackage.Nuspec
        
        dirId <- dirId + 1

let publishNugetPackages _ = 
    let rec publishPackage url accessKey trialsLeft packageFile =
        let tracing = enableProcessTracing
        enableProcessTracing <- false
        let args p =
            match p with
            | (pack, key, "") -> sprintf "push \"%s\" %s" pack key
            | (pack, key, url) -> sprintf "push \"%s\" %s -source %s" pack key url

        tracefn "Pushing %s Attempts left: %d" (FullName packageFile) trialsLeft
        try 
            let result = ExecProcess (fun info -> 
                    info.FileName <- nugetExe
                    info.WorkingDirectory <- (Path.GetDirectoryName (FullName packageFile))
                    info.Arguments <- args (packageFile, accessKey,url)) (System.TimeSpan.FromMinutes 1.0)
            enableProcessTracing <- tracing
            if result <> 0 then failwithf "Error during NuGet symbol push. %s %s" nugetExe (args (packageFile, "key omitted",url))
        with exn -> 
            if (trialsLeft > 0) then (publishPackage url accessKey (trialsLeft-1) packageFile)
            else raise exn
    let shouldPushNugetPackages = hasBuildParam "nugetkey"
    let shouldPushSymbolsPackages = (hasBuildParam "symbolspublishurl") && (hasBuildParam "symbolskey")
    
    if (shouldPushNugetPackages || shouldPushSymbolsPackages) then
        printfn "Pushing nuget packages"
        if shouldPushNugetPackages then
            let normalPackages= 
                !! (nugetDir @@ "*.nupkg") 
                -- (nugetDir @@ "*.symbols.nupkg") |> Seq.sortBy(fun x -> x.ToLower())
            for package in normalPackages do
                try
                    publishPackage (getBuildParamOrDefault "nugetpublishurl" "") (getBuildParam "nugetkey") 3 package
                with exn ->
                    printfn "%s" exn.Message

        if shouldPushSymbolsPackages then
            let symbolPackages= !! (nugetDir @@ "*.symbols.nupkg") |> Seq.sortBy(fun x -> x.ToLower())
            for package in symbolPackages do
                try
                    publishPackage (getBuildParam "symbolspublishurl") (getBuildParam "symbolskey") 3 package
                with exn ->
                    printfn "%s" exn.Message


Target "Nuget" <| fun _ -> 
    createNugetPackages()
    publishNugetPackages()

Target "CreateNuget" <| fun _ -> 
    createNugetPackages()

Target "PublishNuget" <| fun _ -> 
    publishNugetPackages()
    
//--------------------------------------------------------------------------------
// Help 
//--------------------------------------------------------------------------------

Target "Help" <| fun _ ->
    List.iter printfn [
      "usage:"
      "build [target]"
      ""
      " Targets for building:"
      " * Build      Builds"
      " * Nuget      Create and optionally publish nugets packages"
      " * RunTests   Runs tests"
      " * All        Builds, run tests, creates and optionally publish nuget packages"
      ""
      " Other Targets"
      " * Help       Display this help" 
      " * HelpNuget  Display help about creating and pushing nuget packages" 
      ""]

Target "HelpNuget" <| fun _ ->
    List.iter printfn [
      "usage: "
      "build Nuget [nugetkey=<key> [nugetpublishurl=<url>]] "
      "            [symbolskey=<key> symbolspublishurl=<url>] "
      "            [nugetprerelease=<prefix>]"
      ""
      "Arguments for Nuget target:"
      "   nugetprerelease=<prefix>   Creates a pre-release package."
      "                              The version will be version-prefix<date>"
      "                              Example: nugetprerelease=dev =>"
      "                                       0.6.3-dev1408191917"
      ""
      "In order to publish a nuget package, keys must be specified."
      "If a key is not specified the nuget packages will only be created on disk"
      "After a build you can find them in bin/nuget"
      ""
      "For pushing nuget packages to nuget.org and symbols to symbolsource.org"
      "you need to specify nugetkey=<key>"
      "   build Nuget nugetKey=<key for nuget.org>"
      ""
      "For pushing the ordinary nuget packages to another place than nuget.org specify the url"
      "  nugetkey=<key>  nugetpublishurl=<url>  "
      ""
      "For pushing symbols packages specify:"
      "  symbolskey=<key>  symbolspublishurl=<url> "
      ""
      "Examples:"
      "  build Nuget                      Build nuget packages to the bin/nuget folder"
      ""
      "  build Nuget nugetprerelease=dev  Build pre-release nuget packages"
      ""
      "  build Nuget nugetkey=123         Build and publish to nuget.org and symbolsource.org"
      ""
      "  build Nuget nugetprerelease=dev nugetkey=123 nugetpublishurl=http://abc"
      "              symbolskey=456 symbolspublishurl=http://xyz"
      "                                   Build and publish pre-release nuget packages to http://abc"
      "                                   and symbols packages to http://xyz"
      ""]

//--------------------------------------------------------------------------------
//  Target dependencies
//--------------------------------------------------------------------------------

// build dependencies
"Clean" ==> "AssemblyInfo" ==> "RestorePackages" ==> "Build" ==> "CopyOutput" ==> "BuildRelease"

// tests dependencies
"CleanTests" ==> "RunTests"

// benchmark dependencies
"CleanPerf" ==> "NBench"

// nuget dependencies
"CleanNuget" ==> "CreateNuget"
"CleanNuget" ==> "BuildRelease" ==> "Nuget"

Target "All" DoNothing
"BuildRelease" ==> "All"
"RunTests" ==> "All"
"NBench" ==> "All"
"Nuget" ==> "All"

RunTargetOrDefault "Help"