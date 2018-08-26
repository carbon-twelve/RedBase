#r "paket:
nuget Fake.IO.FileSystem
nuget Fake.DotNet.MSBuild
nuget Fake.Core.Target //"
#load "./.fake/build.fsx/intellisense.fsx"

open System.IO
open System.Text
open Fake.Core
open Fake.IO.Globbing.Operators
open Fake.DotNet
open Fake.IO.Globbing
open Fake.IO.FileSystemOperators

Target.create "BuildSrc" (fun _ ->
    !! "src/**/*.fsproj"
    |> MSBuild.runDebug id null "Build"
    |> Trace.logItems "Build-Output: "
)
Target.create "BuildTest" (fun _ ->
    !! "test/**/*.fsproj"
    |> MSBuild.runDebug id null "Build"
    |> Trace.logItems "TestBuild-Output: "
)

Target.create "Test" (fun _ ->
    let assemblies =
        !! "test/*/bin/*.dll"
    let details = String.separated ", " assemblies
    use __ = Trace.traceTask "Persimmon" details
    let toolPath =
        Tools.findToolInSubPath
            "Persimmon.Console.exe"
            (Directory.GetCurrentDirectory() @@ "tools" @@ "Persimmon.Console")
    let args =
        StringBuilder()
        |> StringBuilder.appendFileNamesIfNotNull assemblies
        |> StringBuilder.toText
    let processResult =
        Process.execSimple
            (fun info ->
                { info with
                    FileName = toolPath
                    Arguments = args })
            (System.TimeSpan.FromMinutes 5.)
    if 0 <> processResult then
        let message = sprintf "Persimmon test failed on %s." details
        Trace.traceImportant message
    __.MarkSuccess()
)

open Fake.Core.TargetOperators

"BuildTest"
    ==> "Test"