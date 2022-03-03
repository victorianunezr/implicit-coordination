namespace Functional

module Formula =

    type Proposition =
        | Prop

    type Formula =
        | Top
        | Bottom
        | Proposition of bool
        | And of Formula * Formula
        | Or of Formula * Formula
        | Not of Formula

    let rec evaluate = function
        | Top            -> true
        | Bottom         -> false
        | Proposition(p) -> p
        | And(f1, f2)    -> evaluate f1 && evaluate f2
        | Or(f1, f2)     -> evaluate f1 || evaluate f2
        | Not(f)         -> not(evaluate f)


    let hello name =
        printfn "Hello %s" name
