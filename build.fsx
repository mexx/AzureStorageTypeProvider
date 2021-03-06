// --------------------------------------------------------------------------------------
// FAKE build script 
// --------------------------------------------------------------------------------------
#r "packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile
open Fake.Git
open Fake.ReleaseNotesHelper
open System

// --------------------------------------------------------------------------------------
// START TODO: Provide project-specific details below
// --------------------------------------------------------------------------------------
// Information about the project are used
//  - for version and project name in generated AssemblyInfo file
//  - by the generated NuGet package 
//  - to run tests and to publish documentation on GitHub gh-pages
//  - for documentation, you also need to edit info in "docs/tools/generate.fsx"
// The name of the project 
// (used by attributes in AssemblyInfo, name of a NuGet package and directory in 'src')
let project = "FSharp.Azure.StorageTypeProvider"
// Short summary of the project
// (used as description in AssemblyInfo and as a short summary for NuGet package)
let summary = "Allows easy access to Azure Storage assets through F# scripts."
// Longer description of the project
// (used as a description for NuGet package; line breaks are automatically cleaned up)
let description = """The F# Azure Storage Type Provider allows simple access to Blob, Table and Queue assets, using Azure Storage metadata to
intelligently infer schema where possible, whilst providing a simple API for common tasks."""
// List of author names (for NuGet package)
let authors = [ "Isaac Abraham" ]
// Tags for your project (for NuGet package)
let tags = "azure, f#, fsharp, type provider, blob, table, queue, script"
// File system information 
// (<solutionFile>.sln is built during the building process)
let solutionFile = "UnitTests"
// Pattern specifying assemblies to be tested using XUnit
let testAssemblies = "tests/**/bin/Release/*Tests*.dll"
// Git configuration (used for publishing documentation in gh-pages branch)
// The profile where the project is posted 
let gitHome = "https://github.com/isaacabraham/AzureStorageTypeProvider"
// The name of the project on GitHub
let gitName = "AzureStorageTypeProvider"

// --------------------------------------------------------------------------------------
// END TODO: The rest of the file includes standard build steps 
// --------------------------------------------------------------------------------------
// Read additional information from the release notes document
Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

let release = parseReleaseNotes (IO.File.ReadAllLines "RELEASE_NOTES.md")

// Generate assembly info files with the right version & up-to-date information
Target "AssemblyInfo" (fun _ -> 
    let fileName = "src/" + project + "/AssemblyInfo.fs"
    CreateFSharpAssemblyInfo fileName [ Attribute.Title project
                                        Attribute.Product project
                                        Attribute.Description summary
                                        Attribute.Version release.AssemblyVersion
                                        Attribute.FileVersion release.AssemblyVersion ])
// --------------------------------------------------------------------------------------
// Clean build results & restore NuGet packages
Target "RestorePackages" RestorePackages
Target "Clean" (fun _ -> CleanDirs [ "bin"; "temp" ])
Target "CleanDocs" (fun _ -> CleanDirs [ "docs/output" ])
// --------------------------------------------------------------------------------------
// Build library & test project
Target "Build" (fun _ -> 
    !!(solutionFile + "*.sln")
    |> MSBuildRelease "" "Rebuild"
    |> ignore)
// Run integration tests
Target "Test" (fun _ ->
    !!(testAssemblies)
    |> xUnit (fun p -> { p with Verbose = true }))
// --------------------------------------------------------------------------------------
// Build a NuGet package
Target "NuGet" 
    (fun _ -> 
    NuGet 
        (fun p -> 
        { p with Authors = authors
                 Project = project                 
                 Title = "F# Azure Storage Type Provider"
                 Summary = summary
                 Description = description
                 Version = release.NugetVersion
                 ReleaseNotes = String.Join(Environment.NewLine, release.Notes)
                 Tags = tags
                 OutputPath = "bin"
                 Files = 
                     ([ "FSharp.Azure.StorageTypeProvider.xml"; "FSharp.Azure.StorageTypeProvider.dll"; 
                        "Microsoft.Data.Edm.dll"; "Microsoft.Data.OData.dll"; "Microsoft.Data.Services.Client.dll"; 
                        "Microsoft.WindowsAzure.Configuration.dll"; "Microsoft.WindowsAzure.Storage.dll"; 
                        "Newtonsoft.Json.dll"; "System.Spatial.dll" ] 
                     |> List.map (fun file -> @"..\bin\" + file, Some "lib/net40", None))
                     @ [ "StorageTypeProvider.fsx", None, None ]
                     }) 
        ("nuget/" + project + ".nuspec"))
// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override
Target "All" DoNothing
"Clean" ==> "RestorePackages" ==> "AssemblyInfo" ==> "Build" ==> "All"
"All" ==> "Test" ==> "NuGet" 
RunTargetOrDefault "NuGet"
