namespace System
open System
open System.Reflection
open System.Runtime.InteropServices

[<assembly: AssemblyTitleAttribute("Wire")>]
[<assembly: AssemblyProductAttribute("Wire")>]
[<assembly: AssemblyDescriptionAttribute("Binary serializer for POCO objects")>]
[<assembly: AssemblyCopyrightAttribute("Copyright � 2013-2016 AsynkronIT")>]
[<assembly: AssemblyCompanyAttribute("AsynkronIT")>]
[<assembly: ComVisibleAttribute(false)>]
[<assembly: CLSCompliantAttribute(true)>]
[<assembly: AssemblyVersionAttribute("0.9.0.0")>]
[<assembly: AssemblyFileVersionAttribute("0.9.0.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.9.0.0"
