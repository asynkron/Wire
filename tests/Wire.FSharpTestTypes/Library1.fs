namespace Wire.FSharpTestTypes

open Akka.Actor



type DU1 = 
| A of int
| B of string * int
and DU2 =
| C of DU1
| D of string
| E of option<DU1>

type HubType =
    | Task of unit 
    | Chat of unit 

type Connection =
  { username : string
    id : string
    hubType : HubType
    signalrAref : IActorRef }

[<CustomEquality;CustomComparison>]
type User = 
  { name : string
    aref : string option
    connections : string}

    override x.Equals(yobj) = 
        match yobj with
        | :? User as y -> (x.name = y.name)
        | _ -> false

    override x.GetHashCode() = hash x.name
    interface System.IComparable with
        member x.CompareTo yobj =
            match yobj with
            | :? User as y -> compare x.name y.name
            | _ -> invalidArg "yobj" "cannot compare values of different types"

module TestQuotations = 
    let Quotation = <@ fun (x:int) -> 
        let rec fib n = 
            match n with
            | 0 | 1 -> 1
            | _ -> fib(n-1) + fib(n-2)
        async { return fib x } @>

    

module TestMap =
    type RecordWithString = {Name:string}
    type RecordWithMap = {SomeMap: Map<int,string>}
    let createRecordWithMap = {SomeMap = Map.ofSeq [ (1, "one"); (2, "two") ] }
    

