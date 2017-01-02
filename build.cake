#addin Cake.Coveralls

#tool "nuget:?package=xunit.runner.console"
#tool "nuget:https://www.nuget.org/api/v2?package=OpenCover&version=4.6.519"
#tool "nuget:https://www.nuget.org/api/v2?package=ReportGenerator&version=2.4.5"
#tool coveralls.io

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var skipOpenCover = Argument("skipOpenCover", false);

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var artifactsDir = (DirectoryPath) Directory("./artifacts");
var testResultsDir = (DirectoryPath) artifactsDir.Combine("test-results");
var testCoverageOutputFilePath = testResultsDir.CombineWithFilePath("OpenCover.xml");
var outputNugets = artifactsDir.Combine("nugets");

var isAppVeyorBuild = AppVeyor.IsRunningOnAppVeyor;
var coverallsToken = Context.EnvironmentVariable("COVERALLS_REPO_TOKEN");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetCoreRestore("./", new DotNetCoreRestoreSettings
    {
        Verbose = false,
        Verbosity = DotNetCoreRestoreVerbosity.Warning,
        Sources = new [] {
            "https://www.myget.org/F/xunit/api/v3/index.json",
            "https://dotnet.myget.org/F/dotnet-core/api/v3/index.json",
            "https://dotnet.myget.org/F/cli-deps/api/v3/index.json",
            "https://api.nuget.org/v3/index.json",
        }
    });
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    var projects = GetFiles("./**/*.xproj");
    
    foreach(var project in projects)
    {
        Context.Information("Project: " + project.GetDirectory().FullPath);

        DotNetCoreBuild(project.GetDirectory().FullPath, new DotNetCoreBuildSettings {
            Configuration = configuration
        });
    }
    
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    var projects = GetFiles("./test/**/*Tests.xproj");

    CreateDirectory(testResultsDir);

    Context.Information("Found " + projects.Count() + " projects");

    foreach (var project in projects)
    {
        if (IsRunningOnWindows())
        {
            var apiUrl = EnvironmentVariable("APPVEYOR_API_URL");

            try
            {
                if (!string.IsNullOrEmpty(apiUrl))
                {
                    // Disable XUnit AppVeyorReporter see https://github.com/cake-build/cake/issues/1200
                    System.Environment.SetEnvironmentVariable("APPVEYOR_API_URL", null);
                }

                Action<ICakeContext> testAction = tool => {

                    tool.DotNetCoreTest(project.GetDirectory().FullPath, new DotNetCoreTestSettings {
                        Configuration = configuration,
                        NoBuild = true,
                        Verbose = false,
                        ArgumentCustomization = args =>
                            args.Append("-xml").Append(testResultsDir.CombineWithFilePath(project.GetFilenameWithoutExtension()).FullPath + ".xml")
                    });
                };

                if (!skipOpenCover) {
                    OpenCover(testAction,
                        testCoverageOutputFilePath,
                        new OpenCoverSettings {
                            ReturnTargetCodeOffset = 0,
                            ArgumentCustomization = args => args.Append("-mergeoutput")
                        }
                        .WithFilter("+[EnjoyCQRS*]* -[xunit.*]* -[FluentAssertions*]* -[*Tests]* -[*Tests.Shared]* ")
                        .ExcludeByAttribute("*.ExcludeFromCodeCoverage*")
                        .ExcludeByFile("*/*Designer.cs;*/*.g.cs;*/*.g.i.cs"));
                } 
                else 
                {
                    testAction(Context);
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(apiUrl))
                {
                    System.Environment.SetEnvironmentVariable("APPVEYOR_API_URL", apiUrl);
                }
            }
        }
        else
        {
            var settings = new DotNetCoreTestSettings
            {
                Configuration = configuration, 
                Framework = "netcoreapp1.1"
            };

            DotNetCoreTest(project.GetDirectory().FullPath, settings);
        }
    }

    // Generate the HTML version of the Code Coverage report if the XML file exists
    if (FileExists(testCoverageOutputFilePath))
    {
        ReportGenerator(testCoverageOutputFilePath, testResultsDir);
    }
});

Task("Create-NuGet-Packages")
    .WithCriteria(() => !BuildSystem.AppVeyor.Environment.PullRequest.IsPullRequest)
    .IsDependentOn("Build")
    .Does(() => 
{
    var branch = Context.EnvironmentVariable("APPVEYOR_REPO_BRANCH");
    var versionSuffix = branch + "-" + Context.EnvironmentVariable("APPVEYOR_BUILD_NUMBER");

    var nuspecs = GetFiles("./src/**/*.nuspec");

    foreach (var nuspec in nuspecs)
    {
        var dotNetCorePackSettings = new DotNetCorePackSettings {
            Configuration = configuration,
            OutputDirectory = outputNugets,
            NoBuild = true,
            Verbose = false
        };

        if (isAppVeyorBuild && branch != "master") 
        {
            dotNetCorePackSettings.VersionSuffix = versionSuffix.ToString();
        }

        DotNetCorePack(nuspec.GetDirectory().FullPath, dotNetCorePackSettings);
    }
});

Task("Code-Coverage")
    .WithCriteria(() => !BuildSystem.AppVeyor.Environment.PullRequest.IsPullRequest)
    .WithCriteria(() => FileExists(testCoverageOutputFilePath))
    .WithCriteria(() => !BuildSystem.IsLocalBuild)
    .WithCriteria(() => !string.IsNullOrEmpty(coverallsToken))
    .IsDependentOn("Run-Unit-Tests")
    .Does(() => 
{
    CoverallsIo(testCoverageOutputFilePath, new CoverallsIoSettings()
    {
        RepoToken = coverallsToken
    });
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Run-Unit-Tests");

Task("AppVeyor")
    .IsDependentOn("Code-Coverage")
    .IsDependentOn("Create-NuGet-Packages")
    .IsDependentOn("Run-Unit-Tests");
//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);