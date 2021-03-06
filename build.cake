#addin nuget:?package=SharpZipLib&version=1.0.0
#addin nuget:?package=Cake.Compression&version=0.2.1
#addin nuget:?package=Cake.Git&version=0.18.0

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

Task("Prepare-Build-Directory")
  .Does(() => {
    EnsureDirectoryExists("./build");

    EnsureDirectoryExists("./src/ShaderPlayground.Core/Binaries");
    CleanDirectory("./src/ShaderPlayground.Core/Binaries");
  });

string DownloadCompiler(string url, string binariesFolderName, string version, bool cache)
{
  var tempFileName = $"./build/{binariesFolderName}-{version}.zip";

  if (!cache || !FileExists(tempFileName)) {
    DownloadFile(url, tempFileName);
  }

  return tempFileName;
}

string DownloadAndUnzipCompiler(string url, string binariesFolderName, string version, bool cache)
{
  var tempFileName = DownloadCompiler(url, binariesFolderName, version, cache);
  var unzippedFolder = $"./build/{binariesFolderName}/{version}";

  EnsureDirectoryExists(unzippedFolder);
  CleanDirectory(unzippedFolder);

  ZipUncompress(tempFileName, unzippedFolder);

  return unzippedFolder;
}

string DownloadAndUnzipCompiler(string url, string binariesFolderName, string version, bool cache, string filesToCopy)
{
  var unzippedFolder = DownloadAndUnzipCompiler(url, binariesFolderName, version, cache);

  var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/{binariesFolderName}/{version}";
  EnsureDirectoryExists(binariesFolder);
  CleanDirectory(binariesFolder);

  CopyFiles(
    $"{unzippedFolder}/{filesToCopy}",
    binariesFolder,
    true);

  return binariesFolder;
}

Task("Download-Dxc")
  .Does(() => {
    DownloadAndUnzipCompiler(
      "https://ci.appveyor.com/api/projects/antiagainst/directxshadercompiler/artifacts/build%2FRelease%2Fdxc-artifacts.zip?branch=master&pr=false",
      "dxc",
      "trunk",
      false,
      "bin/*.*");
  });

Task("Download-Glslang")
  .Does(() => {
    DownloadAndUnzipCompiler(
      "https://github.com/KhronosGroup/glslang/releases/download/master-tot/glslang-master-windows-x64-Release.zip",
      "glslang",
      "trunk",
      false,
      "bin/*.*");
  });

Task("Download-Mali-Offline-Compiler")
  .Does(() => {
    DownloadAndUnzipCompiler(
      "https://armkeil.blob.core.windows.net/developer/Files/downloads/opengl-es-open-cl-offline-compiler/6.2/Mali_Offline_Compiler_v6.2.0.7d271f_Windows_x64.zip",
      "mali",
      "6.2.0",
      true,
      "Mali_Offline_Compiler_v6.2.0/**/*.*");
  });

Task("Download-SPIRV-Cross")
  .Does(() => {
    var unzippedFolder = DownloadAndUnzipCompiler(
      "https://github.com/KhronosGroup/SPIRV-Cross/archive/master.zip",
      "spirv-cross",
      "trunk",
      false);

    MSBuild(unzippedFolder + "/SPIRV-Cross-master/msvc/SPIRV-Cross.vcxproj", new MSBuildSettings()
      .SetConfiguration(configuration)
      .WithProperty("WindowsTargetPlatformVersion", "10.0.17134.0")
      .WithProperty("PlatformToolset", "v141"));

    var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/spirv-cross/trunk";
    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"{unzippedFolder}/SPIRV-Cross-master/msvc/{configuration}/SPIRV-Cross.exe",
      binariesFolder,
      true);
  });

Task("Download-SPIRV-Cross-ISPC")
  .Does(() => {
    var unzippedFolder = DownloadAndUnzipCompiler(
      "https://github.com/GameTechDev/SPIRV-Cross/archive/master-ispc.zip",
      "spirv-cross-ispc",
      "trunk",
      false);

    MSBuild(unzippedFolder + "/SPIRV-Cross-master-ispc/msvc/SPIRV-Cross.vcxproj", new MSBuildSettings()
      .SetConfiguration(configuration)
      .WithProperty("WindowsTargetPlatformVersion", "10.0.17134.0"));

    var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/spirv-cross-ispc/trunk";
    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"{unzippedFolder}/SPIRV-Cross-master-ispc/msvc/{configuration}/SPIRV-Cross.exe",
      binariesFolder,
      true);
  });

Task("Download-SPIRV-Tools")
  .Does(() => {
    DownloadAndUnzipCompiler(
      "https://github.com/KhronosGroup/SPIRV-Tools/releases/download/master-tot/SPIRV-Tools-master-windows-x64-Release.zip",
      "spirv-tools",
      "trunk",
      false,
      "bin/*.*");
  });

Task("Download-XShaderCompiler")
  .Does(() => {
    DownloadAndUnzipCompiler(
      "https://github.com/LukasBanana/XShaderCompiler/releases/download/v0.10-alpha/Xsc-v0.10-alpha.zip",
      "xshadercompiler",
      "v0.10-alpha",
      true,
      "Xsc-v0.10-alpha/bin/Win32/xsc.exe");
  });

Task("Download-Slang")
  .Does(() => {
    void DownloadSlang(string version)
    {
      DownloadAndUnzipCompiler(
        $"https://github.com/shader-slang/slang/releases/download/v{version}/slang-{version}-win64.zip",
        "slang",
        $"v{version}",
        true,
        "bin/windows-x64/release/*.*");
    }
    
    DownloadSlang("0.10.24");
    DownloadSlang("0.10.25");
    DownloadSlang("0.10.26");
  });

Task("Download-HLSLParser")
  .Does(() => {
    var unzippedFolder = DownloadAndUnzipCompiler(
      "https://github.com/Thekla/hlslparser/archive/master.zip",
      "hlslparser",
      "trunk",
      false);

    MSBuild(unzippedFolder + "/hlslparser-master/hlslparser.vcxproj", new MSBuildSettings()
      .SetConfiguration(configuration)
      .WithProperty("WindowsTargetPlatformVersion", "10.0.17134.0")
      .WithProperty("PlatformToolset", "v141"));

    var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/hlslparser/trunk";
    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"{unzippedFolder}/hlslparser-master/{configuration}/hlslparser.exe",
      binariesFolder,
      true);
  });

Task("Download-zstd")
  .Does(() => {
    void DownloadZstd(string version)
    {
      DownloadAndUnzipCompiler(
        $"https://github.com/facebook/zstd/releases/download/v{version}/zstd-v{version}-win64.zip",
        "zstd",
        $"v{version}",
        true,
        "zstd.exe");
    }
    
    DownloadZstd("1.3.4");
  });

Task("Download-LZMA")
  .Does(() => {
    void DownloadLzma(string version, string displayVersion)
    {
      var tempFileName = DownloadCompiler(
        $"https://www.7-zip.org/a/lzma{version}.7z",
        "lzma",
        displayVersion,
        true);

      var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/lzma/{displayVersion}";

      StartProcess(
        @"C:\Program Files\7-Zip\7z.exe",
        $@"e -o""{binariesFolder}"" ""{tempFileName}"" bin\lzma.exe");
    }
    
    DownloadLzma("1805", "18.05");
  });

Task("Download-RGA")
  .Does(() => {
    var amdDriverExePath = DownloadCompiler(
      "https://www2.ati.com/drivers/win10-64bit-radeon-software-adrenalin-edition-18.5.1-may23.exe", 
      "amd-driver", 
      "18.5.1", 
      true);

    var amdDriverFolder = "./build/amd-driver/18.5.1";
    EnsureDirectoryExists(amdDriverFolder);
    CleanDirectory(amdDriverFolder);

    StartProcess(
      @"C:\Program Files\7-Zip\7z.exe",
      $@"e -o""{amdDriverFolder}"" ""{amdDriverExePath}"" Packages\Drivers\Display\WT6A_INF\B328940\atidxx64.dll");

    var driverDllPath = amdDriverFolder + "/atidxx64.dll";

    void DownloadRga(string version)
    {
      var binariesFolder = DownloadAndUnzipCompiler(
        $"https://github.com/GPUOpen-Tools/RGA/releases/download/{version}/rga-windows-x64-{version}.zip",
        "rga",
        version,
        true,
        "bin/**/*.*");

      CopyFiles(driverDllPath, binariesFolder);
    }
    
    DownloadRga("2.0.1");
  });

  Task("Copy-PowerVR")
  .Does(() => {
    void CopyVersion(string version)
    {
      var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/powervr/{version}";
      EnsureDirectoryExists(binariesFolder);
      CleanDirectory(binariesFolder);

      CopyFiles($"./lib/PowerVR/{version}/*.*", binariesFolder);
    }
  });

Task("Build-ANGLE")
  .Does(() => {
    StartProcess(MakeAbsolute(File("./external/angle/build.bat")), new ProcessSettings {
      WorkingDirectory = MakeAbsolute(Directory("./external/angle"))
    });
    
    var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/angle/trunk";
    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      "./external/angle/source/out/Release/shader_translator.exe",
      binariesFolder,
      true);
  });

Task("Build-Fxc-Shim")
  .Does(() => {
    DotNetCorePublish("./shims/ShaderPlayground.Shims.Fxc/ShaderPlayground.Shims.Fxc.csproj", new DotNetCorePublishSettings
    {
      Configuration = configuration
    });

    var fxcVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo("./lib/x64/d3dcompiler_47.dll").ProductVersion;
    var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/fxc/{fxcVersion}";

    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"./shims/ShaderPlayground.Shims.Fxc/bin/{configuration}/netcoreapp2.0/publish/**/*.*",
      binariesFolder,
      true);
  });

Task("Build-HLSLcc-Shim")
  .Does(() => {
    MSBuild("./shims/ShaderPlayground.Shims.HLSLcc/ShaderPlayground.Shims.HLSLcc.vcxproj", new MSBuildSettings
    {
      Configuration = configuration
    });

    var binariesFolder = "./src/ShaderPlayground.Core/Binaries/hlslcc/trunk";

    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"./shims/ShaderPlayground.Shims.HLSLcc/{configuration}/ShaderPlayground.Shims.HLSLcc.exe",
      binariesFolder,
      true);
  });

Task("Build-GLSL-Optimizer-Shim")
  .Does(() => {
    MSBuild("./shims/ShaderPlayground.Shims.GlslOptimizer/Source/projects/vs2010/glsl_optimizer_lib.vcxproj", new MSBuildSettings()
      .WithProperty("PlatformToolset", "v141")
      .WithProperty("WindowsTargetPlatformVersion", "10.0.17134.0")
      .SetConfiguration(configuration)
      .SetPlatformTarget(PlatformTarget.Win32));

    MSBuild("./shims/ShaderPlayground.Shims.GlslOptimizer/ShaderPlayground.Shims.GlslOptimizer.vcxproj", new MSBuildSettings()
      .SetConfiguration(configuration));

    var binariesFolder = "./src/ShaderPlayground.Core/Binaries/glsl-optimizer/trunk";

    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"./shims/ShaderPlayground.Shims.GlslOptimizer/{configuration}/ShaderPlayground.Shims.GlslOptimizer.exe",
      binariesFolder,
      true);
  });

Task("Build-HLSL2GLSL-Shim")
  .Does(() => {
    MSBuild("./shims/ShaderPlayground.Shims.Hlsl2Glsl/Source/hlslang.vcxproj", new MSBuildSettings()
      .WithProperty("PlatformToolset", "v141")
      .WithProperty("WindowsTargetPlatformVersion", "10.0.17134.0")
      .WithProperty("ForcedIncludeFiles", "<algorithm>")
      .SetConfiguration(configuration)
      .SetPlatformTarget(PlatformTarget.Win32));

    MSBuild("./shims/ShaderPlayground.Shims.Hlsl2Glsl/ShaderPlayground.Shims.Hlsl2Glsl.vcxproj", new MSBuildSettings()
      .SetConfiguration(configuration));

    var binariesFolder = "./src/ShaderPlayground.Core/Binaries/hlsl2glsl/trunk";

    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"./shims/ShaderPlayground.Shims.Hlsl2Glsl/{configuration}/ShaderPlayground.Shims.Hlsl2Glsl.exe",
      binariesFolder,
      true);
  });

Task("Build-SMOL-V-Shim")
  .Does(() => {
    MSBuild("./shims/ShaderPlayground.Shims.Smolv/ShaderPlayground.Shims.Smolv.vcxproj", new MSBuildSettings
    {
      Configuration = configuration
    });

    var binariesFolder = "./src/ShaderPlayground.Core/Binaries/smol-v/trunk";

    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"./shims/ShaderPlayground.Shims.Smolv/{configuration}/ShaderPlayground.Shims.Smolv.exe",
      binariesFolder,
      true);
  });

Task("Build-Miniz-Shim")
  .Does(() => {
    MSBuild("./shims/ShaderPlayground.Shims.Miniz/ShaderPlayground.Shims.Miniz.vcxproj", new MSBuildSettings
    {
      Configuration = configuration
    });

    var binariesFolder = "./src/ShaderPlayground.Core/Binaries/miniz/2.0.7";

    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"./shims/ShaderPlayground.Shims.Miniz/{configuration}/ShaderPlayground.Shims.Miniz.exe",
      binariesFolder,
      true);
  });

Task("Build-Shims")
  .IsDependentOn("Build-Fxc-Shim")
  .IsDependentOn("Build-HLSLcc-Shim")
  .IsDependentOn("Build-GLSL-Optimizer-Shim")
  .IsDependentOn("Build-HLSL2GLSL-Shim")
  .IsDependentOn("Build-SMOL-V-Shim")
  .IsDependentOn("Build-Miniz-Shim");

Task("Build")
  .IsDependentOn("Build-Shims")
  .Does(() => {
    var outputFolder = "./build/site";
    EnsureDirectoryExists(outputFolder);
    CleanDirectory(outputFolder);

    DotNetCorePublish("./src/ShaderPlayground.Web/ShaderPlayground.Web.csproj", new DotNetCorePublishSettings
    {
      Configuration = configuration,
      OutputDirectory = outputFolder
    });

    ZipCompress(outputFolder, "./build/site.zip");
  });

Task("Test")
  .IsDependentOn("Build")
  .Does(() => {
    DotNetCoreTest("./src/ShaderPlayground.Core.Tests/ShaderPlayground.Core.Tests.csproj", new DotNetCoreTestSettings
    {
      Configuration = configuration
    });
  });

Task("Default")
  .IsDependentOn("Prepare-Build-Directory")
  .IsDependentOn("Download-Dxc")
  .IsDependentOn("Download-Glslang")
  .IsDependentOn("Download-Mali-Offline-Compiler")
  .IsDependentOn("Download-SPIRV-Cross")
  .IsDependentOn("Download-SPIRV-Tools")
  .IsDependentOn("Download-XShaderCompiler")
  .IsDependentOn("Download-SPIRV-Cross-ISPC")
  .IsDependentOn("Download-Slang")
  .IsDependentOn("Download-HLSLParser")
  .IsDependentOn("Download-zstd")
  .IsDependentOn("Download-LZMA")
  .IsDependentOn("Download-RGA")
  .IsDependentOn("Copy-PowerVR")
  .IsDependentOn("Build-ANGLE")
  .IsDependentOn("Build")
  .IsDependentOn("Test");

RunTarget(target);