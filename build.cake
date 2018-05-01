#addin nuget:?package=Cake.Npm&version=0.13.0
using System.Xml.Linq;

var target = Argument("target", "default");
var runtime = Argument("runtime", "win7-x64");
var configuration = Argument("configuration", "Release");
var output = Argument("output", "./artifacts");
var versionSuffix = Argument<string>("versionSuffix", null);
var productVersion = XDocument.Load("./src/EnvironmentDashboard.Api/EnvironmentDashboard.Api.csproj").Descendants("VersionPrefix").FirstOrDefault().Value;

Information("Running on TeamCity: " + TeamCity.IsRunningOnTeamCity);
Information("Target: " + target);
Information("Configuration: " + configuration + ", tests always run under Debug");
Information("Output path: " + output);
Information("Version suffix: " + versionSuffix);
Information("Saga version: " + productVersion);

Task("clean")
    .Does(() => {
        CleanDirectories("./src/**/bin/" + configuration);
        CleanDirectories("./src/**/obj/" + configuration);
        CleanDirectory("./artifacts");

        CleanDirectories("./src/EnvironmentDashboard.Api/wwwroot/**");
    });    

Task("restore")
    .IsDependentOn("clean")
    .Does(() => {        
        DotNetCoreRestore("src/EnvironmentDashboard.Api/EnvironmentDashboard.Api.csproj");
    });

Task("npm-install")
    .Does(() => {
        var settings = new NpmInstallSettings();

        settings.Global = false;
        settings.Production = false;
        settings.WorkingDirectory = "./src/webapp";

        NpmInstall(settings);
    });

Task("ember-build")    
    .IsDependentOn("npm-install")
    .Does(() => {
        var args = "build --environment=production";
        var returnCode = 0;
        if(IsRunningOnWindows()){
            returnCode = StartProcess("cmd", new ProcessSettings{
                Arguments = "/c \"gulp " + args,
                WorkingDirectory = "./src/webapp"
            });
        } else if(IsRunningOnUnix()){
            returnCode = StartProcess("ember", new ProcessSettings{
                Arguments = args,
                WorkingDirectory = "./src/webapp"
            });
        }

        if(returnCode != 0) {
            throw new Exception("Ember build process exited with error-code != 0");
        }
    });

Task("build")    
    .IsDependentOn("restore")
    .Does(() => {
        var buildSettings = new DotNetCoreBuildSettings {
            VersionSuffix = versionSuffix,
            Configuration = configuration,
            Runtime = runtime
        };        
        DotNetCoreBuild("src/EnvironmentDashboard.Api/EnvironmentDashboard.Api.csproj", buildSettings);
    });

Task("publish")
    .IsDependentOn("build")
    .IsDependentOn("ember-build")
    .Does(() => {
        var settings = new DotNetCorePublishSettings(){
            Configuration = configuration,
            VersionSuffix = versionSuffix,
            OutputDirectory = output + "/web"            
        };

        DotNetCorePublish("./src/EnvironmentDashboard.Api/EnvironmentDashboard.Api.csproj", settings);

        if(IsRunningOnWindows()) {
            Zip(settings.OutputDirectory, String.Format("./artifacts/EnvironmentDashboard-{0}.zip", productVersion));
        } else {
            StartProcess("zip", new ProcessSettings{
                Arguments = string.Format("-r -X ../EnvironmentDashboard-{0}.zip .", productVersion),
                WorkingDirectory = "./artifacts/web"
            });
        }
    });

Task("default")    
    .IsDependentOn("publish")
    .Does(() => {                                        
    });

RunTarget(target);
