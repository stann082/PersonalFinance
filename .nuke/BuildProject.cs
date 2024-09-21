using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Serilog;

// ReSharper disable UnusedMember.Local
// ReSharper disable AllUnderscoreLocalParameterName
class BuildProject : NukeBuild
{

    public static int Main() => Execute<BuildProject>();

    #region Parameters

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = Configuration.Debug;

    [Parameter("Verbosity level of the build output - Default is 'Minimal'")]
    readonly DotNetVerbosity DotNetVerbosityLevel = DotNetVerbosity.quiet;
    
    [Parameter("Skips the build step if specified.")]
    readonly bool NoBuild;

    [Parameter("Paths to the test project(s) to run. You can provide multiple paths as separate arguments.")]
    readonly AbsolutePath[] TestProjectPaths;

    #endregion

    #region Targets

    Target Clean => _ => _
        .Before(Build)
        .Executes(() =>
        {
            DotNetTasks.DotNetClean(s => s
                .SetConfiguration(Configuration)
                .SetVerbosity(DotNetVerbosityLevel)
                .SetProject(RootDirectory / "PersonalFinance.sln"));
        });

    Target Build => _ => _
        .Executes(() =>
        {
            DotNetTasks.DotNetBuild(s => s
                .SetConfiguration(Configuration)
                .SetProjectFile(RootDirectory / "PersonalFinance.sln")
                .SetVerbosity(DotNetVerbosityLevel)
                .EnableNoRestore());
        });

    Target DeepClean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            var binAndObjDirectories = RootDirectory.GlobDirectories("**/bin", "**/obj")
                .Where(directory => !directory.ToString().Contains(".nuke"));
            foreach (var directory in binAndObjDirectories)
            {
                Log.Information("Cleaning directory {Directory}", directory);
                directory.DeleteDirectory();
            }
            
            var buildDirectory = RootDirectory / "build";
            Log.Information("Cleaning directory {BuildDirectory}", buildDirectory);
            buildDirectory.DeleteDirectory();

            var publishDirectory = RootDirectory / "pub";
            Log.Information("Cleaning directory {PublishDirectory}", publishDirectory);
            publishDirectory.DeleteDirectory();
        });

    Target Deploy => _ => _
        .DependsOn(Publish)
        .Executes(() =>
        {
            // var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            // var targetDirectory = Path.Combine(appDataPath, "utils");
            // if (!Directory.Exists(targetDirectory))
            // {
            //     Directory.CreateDirectory(targetDirectory);
            // }
            //
            // var targetFile = Path.Combine(targetDirectory, "gamespace.exe");
            // if (File.Exists(targetFile))
            // {
            //     File.Delete(targetFile);
            // }
            //
            // var sourceFile = RootDirectory / "pub" / "gamespace.exe";
            // File.Copy(sourceFile, targetFile);
            // Log.Information("Deployed gamespace.exe to {TargetDirectory}", targetDirectory);
        });

    Target Publish => _ => _
        .Before(Deploy)
        .Executes(() =>
        {
            DotNetTasks.DotNetPublish(s => s
                .SetProject(RootDirectory / "PersonalFinance.sln")
                .SetConfiguration(Configuration)
                .SetVerbosity(DotNetVerbosityLevel)
                .SetOutput(RootDirectory / "pub"));
        });

    Target Rebuild => _ => _
        .DependsOn(DeepClean, Restore, Clean, Build);
            
    Target Restore => _ => _
        .Before(Clean)
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(s => s
                .SetVerbosity(DotNetVerbosityLevel)
                .SetProjectFile(RootDirectory / "PersonalFinance.sln"));
        });

    Target Test => _ => _
        .Executes(() =>
        {
            GetTestProjects().AsParallel().ForAll(project =>
            {
                DotNetTasks.DotNetTest(s => s
                    .SetProjectFile(project)
                    .SetConfiguration(Configuration)
                    .SetVerbosity(DotNetVerbosityLevel)
                    .EnableNoBuild());
            });
        });

    #endregion

    #region Helper Methods

    AbsolutePath[] GetTestProjects() =>
        (TestProjectPaths ?? []).Length > 0
            ? TestProjectPaths
            :
            [
                RootDirectory / "tests" / "PersonalFinance.Application.Tests" / "PersonalFinance.Application.Tests.csproj",
                RootDirectory / "tests" / "PersonalFinance.Infrastructure.Tests" / "PersonalFinance.Infrastructure.Tests.csproj"
            ];

    #endregion

}
