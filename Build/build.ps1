properties { 
  $zipFileName = "Json60r4.zip"
  $majorVersion = "6.0"
  $majorWithReleaseVersion = "6.0.4"
  $version = GetVersion $majorWithReleaseVersion
  $signAssemblies = $false
  $signKeyPath = "C:\Development\Releases\newtonsoft.snk"
  $buildDocumentation = $false
  $buildNuGet = $true
  $treatWarningsAsErrors = $false
  
  $baseDir  = resolve-path ..
  $buildDir = "$baseDir\Build"
  $sourceDir = "$baseDir\Src"
  $toolsDir = "$baseDir\Tools"
  $docDir = "$baseDir\Doc"
  $releaseDir = "$baseDir\Release"
  $workingDir = "$baseDir\Working"
  $builds = @(
    @{Name = "Newtonsoft.Json"; TestsName = "Newtonsoft.Json.Tests"; Constants=""; FinalDir="Net45"; NuGetDir = "net45"; Framework="net-4.0"; Sign=$true},
    @{Name = "Newtonsoft.Json.Portable"; TestsName = "Newtonsoft.Json.Tests.Portable"; Constants="PORTABLE"; FinalDir="Portable"; NuGetDir = "portable-net45+wp80+win8+wpa81"; Framework="net-4.0"; Sign=$true},
    @{Name = "Newtonsoft.Json.Portable40"; TestsName = "Newtonsoft.Json.Tests.Portable40"; Constants="PORTABLE40"; FinalDir="Portable40"; NuGetDir = "portable-net40+sl5+wp80+win8+wpa81"; Framework="net-4.0"; Sign=$true},
    @{Name = "Newtonsoft.Json.WinRT"; TestsName = $null; Constants="NETFX_CORE"; FinalDir="WinRT"; NuGetDir = "netcore45"; Framework="net-4.5"; Sign=$true},
    @{Name = "Newtonsoft.Json.Net40"; TestsName = "Newtonsoft.Json.Tests.Net40"; Constants="NET40"; FinalDir="Net40"; NuGetDir = "net40"; Framework="net-4.0"; Sign=$true},
    @{Name = "Newtonsoft.Json.Net35"; TestsName = "Newtonsoft.Json.Tests.Net35"; Constants="NET35"; FinalDir="Net35"; NuGetDir = "net35"; Framework="net-2.0"; Sign=$true},
    @{Name = "Newtonsoft.Json.Net20"; TestsName = "Newtonsoft.Json.Tests.Net20"; Constants="NET20"; FinalDir="Net20"; NuGetDir = "net20"; Framework="net-2.0"; Sign=$true}
  )
}

$framework = '4.0x86'

task default -depends Test

# Ensure a clean working directory
task Clean {
  Set-Location $baseDir
  
  if (Test-Path -path $workingDir)
  {
    Write-Output "Deleting Working Directory"
    
    del $workingDir -Recurse -Force
  }
  
  New-Item -Path $workingDir -ItemType Directory
}

# Build each solution, optionally signed
task Build -depends Clean { 
  Write-Host -ForegroundColor Green "Updating assembly version"
  Write-Host
  Update-AssemblyInfoFiles $sourceDir ($majorVersion + '.0.0') $version

  foreach ($build in $builds)
  {
    $name = $build.Name
    $finalDir = $build.FinalDir
    $sign = ($build.Sign -and $signAssemblies)

    Write-Host -ForegroundColor Green "Building " $name
    Write-Host -ForegroundColor Green "Signed " $sign

    Write-Host
    Write-Host "Restoring"
    exec { .\Tools\NuGet\NuGet.exe restore ".\Src\$name.sln" | Out-Default } "Error restoring $name"

    Write-Host
    Write-Host "Building"
    exec { msbuild "/t:Clean;Rebuild" /p:Configuration=Release "/p:Platform=Any CPU" /p:OutputPath=bin\Release\$finalDir\ /p:AssemblyOriginatorKeyFile=$signKeyPath "/p:SignAssembly=$sign" "/p:TreatWarningsAsErrors=$treatWarningsAsErrors" "/p:VisualStudioVersion=12.0" (GetConstants $build.Constants $sign) ".\Src\$name.sln" | Out-Default } "Error building $name"
  }
}

# Optional build documentation, add files to final zip
task Package -depends Build {
  foreach ($build in $builds)
  {
    $name = $build.TestsName
    $finalDir = $build.FinalDir
    
    robocopy "$sourceDir\Newtonsoft.Json\bin\Release\$finalDir" $workingDir\Package\Bin\$finalDir *.dll *.pdb *.xml /NP /XO /XF *.CodeAnalysisLog.xml | Out-Default
  }
  
  if ($buildNuGet)
  {
    New-Item -Path $workingDir\NuGet -ItemType Directory    
    Copy-Item -Path "$buildDir\Newtonsoft.Json.nuspec" -Destination $workingDir\NuGet\Newtonsoft.Json.nuspec -recurse

    New-Item -Path $workingDir\NuGet\tools -ItemType Directory
    Copy-Item -Path "$buildDir\install.ps1" -Destination $workingDir\NuGet\tools\install.ps1 -recurse
    
    foreach ($build in $builds)
    {
      if ($build.NuGetDir)
      {
        $name = $build.TestsName
        $finalDir = $build.FinalDir
        $frameworkDirs = $build.NuGetDir.Split(",")
        
        foreach ($frameworkDir in $frameworkDirs)
        {
          robocopy "$sourceDir\Newtonsoft.Json\bin\Release\$finalDir" $workingDir\NuGet\lib\$frameworkDir *.dll *.pdb *.xml /NP /XO /XF *.CodeAnalysisLog.xml | Out-Default
        }
      }
    }
  
    robocopy $sourceDir $workingDir\NuGet\src *.cs /S /NP /XD Newtonsoft.Json.Tests obj | Out-Default

    exec { .\Tools\NuGet\NuGet.exe pack $workingDir\NuGet\Newtonsoft.Json.nuspec -Symbols }
    move -Path .\*.nupkg -Destination $workingDir\NuGet
  }
  
  if ($buildDocumentation)
  {
    $mainBuild = $builds | where { $_.Name -eq "Newtonsoft.Json" } | select -first 1
    $mainBuildFinalDir = $mainBuild.FinalDir
    $documentationSourcePath = "$workingDir\Package\Bin\$mainBuildFinalDir"
    Write-Host -ForegroundColor Green "Building documentation from $documentationSourcePath"

    # Sandcastle has issues when compiling with .NET 4 MSBuild - http://shfb.codeplex.com/Thread/View.aspx?ThreadId=50652
    exec { msbuild "/t:Clean;Rebuild" /p:Configuration=Release "/p:DocumentationSourcePath=$documentationSourcePath" $docDir\doc.shfbproj | Out-Default } "Error building documentation. Check that you have Sandcastle, Sandcastle Help File Builder and HTML Help Workshop installed."
    
    move -Path $workingDir\Documentation\LastBuild.log -Destination $workingDir\Documentation.log
  }
  
  Copy-Item -Path $docDir\readme.txt -Destination $workingDir\Package\
  Copy-Item -Path $docDir\license.txt -Destination $workingDir\Package\

  # exclude package directories but keep packages\repositories.config
  $packageDirs = gci $sourceDir\packages | where {$_.PsIsContainer} | Select -ExpandProperty Name

  robocopy $sourceDir $workingDir\Package\Source\Src /MIR /NP /XD bin obj TestResults AppPackages $packageDirs /XF *.suo *.user | Out-Default
  robocopy $buildDir $workingDir\Package\Source\Build /MIR /NP /XF runbuild.txt | Out-Default
  robocopy $docDir $workingDir\Package\Source\Doc /MIR /NP | Out-Default
  robocopy $toolsDir $workingDir\Package\Source\Tools /MIR /NP | Out-Default
  
  exec { .\Tools\7-zip\7za.exe a -tzip $workingDir\$zipFileName $workingDir\Package\* | Out-Default } "Error zipping"
}

# Unzip package to a location
task Deploy -depends Package {
  exec { .\Tools\7-zip\7za.exe x -y "-o$workingDir\Deployed" $workingDir\$zipFileName | Out-Default } "Error unzipping"
}

# Run tests on deployed files
task Test -depends Deploy {
  foreach ($build in $builds)
  {
    $name = $build.TestsName
    if ($name -ne $null)
    {
        $finalDir = $build.FinalDir
        $framework = $build.Framework
        
        Write-Host -ForegroundColor Green "Copying test assembly $name to deployed directory"
        Write-Host
        robocopy ".\Src\Newtonsoft.Json.Tests\bin\Release\$finalDir" $workingDir\Deployed\Bin\$finalDir /MIR /NP /XO /XF LinqBridge.dll | Out-Default
        
        Copy-Item -Path ".\Src\Newtonsoft.Json.Tests\bin\Release\$finalDir\Newtonsoft.Json.Tests.dll" -Destination $workingDir\Deployed\Bin\$finalDir\

        Write-Host -ForegroundColor Green "Running tests " $name
        Write-Host
        exec { .\Tools\NUnit\nunit-console.exe "$workingDir\Deployed\Bin\$finalDir\Newtonsoft.Json.Tests.dll" /framework=$framework /xml:$workingDir\$name.xml | Out-Default } "Error running $name tests"
    }
  }
}

function GetConstants($constants, $includeSigned)
{
  $signed = switch($includeSigned) { $true { ";SIGNED" } default { "" } }

  return "/p:DefineConstants=`"CODE_ANALYSIS;TRACE;$constants$signed`""
}

function GetVersion($majorVersion)
{
    $now = [DateTime]::Now
    
    $year = $now.Year - 2000
    $month = $now.Month
    $totalMonthsSince2000 = ($year * 12) + $month
    $day = $now.Day
    $minor = "{0}{1:00}" -f $totalMonthsSince2000, $day
    
    $hour = $now.Hour
    $minute = $now.Minute
    $revision = "{0:00}{1:00}" -f $hour, $minute
    
    return $majorVersion + "." + $minor
}

function Update-AssemblyInfoFiles ([string] $sourceDir, [string] $assemblyVersionNumber, [string] $fileVersionNumber)
{
    $assemblyVersionPattern = 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $fileVersionPattern = 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $assemblyVersion = 'AssemblyVersion("' + $assemblyVersionNumber + '")';
    $fileVersion = 'AssemblyFileVersion("' + $fileVersionNumber + '")';
    
    Get-ChildItem -Path $sourceDir -r -filter AssemblyInfo.cs | ForEach-Object {
        
        $filename = $_.Directory.ToString() + '\' + $_.Name
        Write-Host $filename
        $filename + ' -> ' + $version
    
        (Get-Content $filename) | ForEach-Object {
            % {$_ -replace $assemblyVersionPattern, $assemblyVersion } |
            % {$_ -replace $fileVersionPattern, $fileVersion }
        } | Set-Content $filename
    }
}