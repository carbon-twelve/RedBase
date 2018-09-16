namespace DatabaseTest

open Persimmon

module test =

    let ``test`` = test "test" {
        do! assertEquals 1 (4 % 2)  
    }