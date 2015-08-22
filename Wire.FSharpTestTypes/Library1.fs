namespace Wire.FSharpTestTypes

type DU1 = 
| A of int
| B of string * int
and DU2 =
| C of DU1
| D of string
| E of option<DU1>



