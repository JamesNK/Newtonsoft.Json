﻿properties { 
  $zipFileName = "Json70r2.zip"
  $majorVersion = "7.0"
  $majorWithReleaseVersion = "7.0.2"
  $nugetPrelease = "beta1"
  $version = GetVersion $majorWithReleaseVersion
  $packageId = "Newtonsoft.Json"
  $signAssemblies = $false
  $signKeyPath = "C:\Development\Releases\newtonsoft.snk"
  $buildDocumentation = $false
  $buildNuGet = $true
  $treatWarningsAsErrors = $false
  $workingName = if ($workingName) {$workingName} else {"Working"}
  $dnvmVersion = "1.0.0-beta8-15618"
  
  $baseDir  = resolve-path ..
  $buildDir = "$baseDir\Build"
  $sourceDir = "$baseDir\Src"
  $toolsDir = "$baseDir\Tools"
  $docDir = "$baseDir\Doc"
  $releaseDir = "$baseDir\Release"
  $workingDir = "$baseDir\$workingName"
  $workingSourceDir = "$workingDir\Src"
  $builds = @(
    @{Name = "Newtonsoft.Json.Dotnet"; TestsName = "Newtonsoft.Json.Tests.Dotnet"; BuildFunction = "DnxBuild"; TestsFunction = "DnxTests"; Constants="dotnet"; FinalDir="Dotnet"; NuGetDir = "dotnet"; Framework=$null},
    @{Name = "Newtonsoft.Json"; TestsName = "Newtonsoft.Json.Tests"; BuildFunction = "MSBuildBuild"; TestsFunction = "NUnitTests"; Constants=""; FinalDir="Net45"; NuGetDir = "net45"; Framework="net-4.0"},
    @{Name = "Newtonsoft.Json.Portable"; TestsName = "Newtonsoft.Json.Tests.Portable"; BuildFunction = "MSBuildBuild"; TestsFunction = "NUnitTests"; Constants="PORTABLE"; FinalDir="Portable"; NuGetDir = "portable-net45+wp80+win8+wpa81"; Framework="net-4.0"},
    @{Name = "Newtonsoft.Json.Portable40"; TestsName = "Newtonsoft.Json.Tests.Portable40"; BuildFunction = "MSBuildBuild"; TestsFunction = "NUnitTests"; Constants="PORTABLE40"; FinalDir="Portable40"; NuGetDir = "portable-net40+sl5+wp80+win8+wpa81"; Framework="net-4.0"},
    @{Name = "Newtonsoft.Json.Net40"; TestsName = "Newtonsoft.Json.Tests.Net40"; BuildFunction = "MSBuildBuild"; TestsFunction = "NUnitTests"; Constants="NET40"; FinalDir="Net40"; NuGetDir = "net40"; Framework="net-4.0"},
    @{Name = "Newtonsoft.Json.Net35"; TestsName = "Newtonsoft.Json.Tests.Net35"; BuildFunction = "MSBuildBuild"; TestsFunction = "NUnitTests"; Constants="NET35"; FinalDir="Net35"; NuGetDir = "net35"; Framework="net-2.0"},
    @{Name = "Newtonsoft.Json.Net20"; TestsName = "Newtonsoft.Json.Tests.Net20"; BuildFunction = "MSBuildBuild"; TestsFunction = "NUnitTests"; Constants="NET20"; FinalDir="Net20"; NuGetDir = "net20"; Framework="net-2.0"}
  )
}

framework '4.6x86'

task default -depends Test

# Ensure a clean working directory
task Clean {
  Write-Host "Setting location to $baseDir"
  Set-Location $baseDir
  
  if (Test-Path -path $workingDir)
  {
    Write-Host "Deleting existing working directory $workingDir"
    
    Execute-Command -command { del $workingDir -Recurse -Force }
  }
  
  Write-Host "Creating working directory $workingDir"
  New-Item -Path $workingDir -ItemType Directory
}

# Build each solution, optionally signed
task Build -depends Clean { 

  Write-Host "Copying source to working source directory $workingSourceDir"
  robocopy $sourceDir $workingSourceDir /MIR /NP /XD bin obj TestResults AppPackages $packageDirs /XF *.suo *.user *.project.lock.json | Out-Default

  Write-Host -ForegroundColor Green "Updating assembly version"
  Write-Host
  Update-AssemblyInfoFiles $workingSourceDir ($majorVersion + '.0.0') $version

  Update-Project $workingSourceDir\Newtonsoft.Json\project.json $signAssemblies

  foreach ($build in $builds)
  {
    $name = $build.Name
    if ($name -ne $null)
    {
      Write-Host -ForegroundColor Green "Building " $name
      Write-Host -ForegroundColor Green "Signed " $signAssemblies
      Write-Host -ForegroundColor Green "Key " $signKeyPath

      & $build.BuildFunction $build
    }
  }
}

# Optional build documentation, add files to final zip
task Package -depends Build {
  foreach ($build in $builds)
  {
    $name = $build.TestsName
    $finalDir = $build.FinalDir
    
    robocopy "$workingSourceDir\Newtonsoft.Json\bin\Release\$finalDir" $workingDir\Package\Bin\$finalDir *.dll *.pdb *.xml /NFL /NDL /NJS /NC /NS /NP /XO /XF *.CodeAnalysisLog.xml | Out-Default
  }
  
  if ($buildNuGet)
  {
    $nugetVersion = $majorWithReleaseVersion
    if ($nugetPrelease -ne $null)
    {
      $nugetVersion = $nugetVersion + "-" + $nugetPrelease
    }

    New-Item -Path $workingDir\NuGet -ItemType Directory

    $nuspecPath = "$workingDir\NuGet\Newtonsoft.Json.nuspec"
    Copy-Item -Path "$buildDir\Newtonsoft.Json.nuspec" -Destination $nuspecPath -recurse

    Write-Host "Updating nuspec file at $nuspecPath" -ForegroundColor Green
    Write-Host

    $xml = [xml](Get-Content $nuspecPath)
    Edit-XmlNodes -doc $xml -xpath "//*[local-name() = 'id']" -value $packageId
    Edit-XmlNodes -doc $xml -xpath "//*[local-name() = 'version']" -value $nugetVersion

    Write-Host $xml.OuterXml

    $xml.save($nuspecPath)

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
          robocopy "$workingSourceDir\Newtonsoft.Json\bin\Release\$finalDir" $workingDir\NuGet\lib\$frameworkDir *.dll *.pdb *.xml /NFL /NDL /NJS /NC /NS /NP /XO /XF *.CodeAnalysisLog.xml | Out-Default
        }
      }
    }
  
    robocopy $workingSourceDir $workingDir\NuGet\src *.cs /S /NFL /NDL /NJS /NC /NS /NP /XD Newtonsoft.Json.Tests Newtonsoft.Json.TestConsole obj .vs artifacts | Out-Default

    Write-Host "Building NuGet package with ID $packageId and version $nugetVersion" -ForegroundColor Green
    Write-Host

    exec { .\Tools\NuGet\NuGet.exe pack $nuspecPath -Symbols }
    move -Path .\*.nupkg -Destination $workingDir\NuGet
  }

  Write-Host "Build documentation: $buildDocumentation"
  
  if ($buildDocumentation)
  {
    $mainBuild = $builds | where { $_.Name -eq "Newtonsoft.Json" } | select -first 1
    $mainBuildFinalDir = $mainBuild.FinalDir
    $documentationSourcePath = "$workingDir\Package\Bin\$mainBuildFinalDir"
    $docOutputPath = "$workingDir\Documentation\"
    Write-Host -ForegroundColor Green "Building documentation from $documentationSourcePath"
    Write-Host "Documentation output to $docOutputPath"

    # Sandcastle has issues when compiling with .NET 4 MSBuild - http://shfb.codeplex.com/Thread/View.aspx?ThreadId=50652
    exec { msbuild "/t:Clean;Rebuild" /p:Configuration=Release "/p:DocumentationSourcePath=$documentationSourcePath" "/p:OutputPath=$docOutputPath" $docDir\doc.shfbproj | Out-Default } "Error building documentation. Check that you have Sandcastle, Sandcastle Help File Builder and HTML Help Workshop installed."
    
    move -Path $workingDir\Documentation\LastBuild.log -Destination $workingDir\Documentation.log
  }
  
  Copy-Item -Path $docDir\readme.txt -Destination $workingDir\Package\
  Copy-Item -Path $docDir\license.txt -Destination $workingDir\Package\

  # exclude package directories but keep packages\repositories.config
  $packageDirs = gci $workingSourceDir\packages | where {$_.PsIsContainer} | Select -ExpandProperty Name

  robocopy $workingSourceDir $workingDir\Package\Source\Src /MIR /NFL /NDL /NJS /NC /NS /NP /XD bin obj TestResults AppPackages $packageDirs /XF *.suo *.user | Out-Default
  robocopy $buildDir $workingDir\Package\Source\Build /MIR /NFL /NDL /NJS /NC /NS /NP /XF runbuild.txt | Out-Default
  robocopy $docDir $workingDir\Package\Source\Doc /MIR /NFL /NDL /NJS /NC /NS /NP | Out-Default
  robocopy $toolsDir $workingDir\Package\Source\Tools /MIR /NFL /NDL /NJS /NC /NS /NP | Out-Default
  
  exec { .\Tools\7-zip\7za.exe a -tzip $workingDir\$zipFileName $workingDir\Package\* | Out-Default } "Error zipping"
}

# Unzip package to a location
task Deploy -depends Package {
  exec { .\Tools\7-zip\7za.exe x -y "-o$workingDir\Deployed" $workingDir\$zipFileName | Out-Default } "Error unzipping"
}

# Run tests on deployed files
task Test -depends Deploy {

  Update-Project $workingSourceDir\Newtonsoft.Json\project.json $false

  foreach ($build in $builds)
  {
    if ($build.TestsFunction -ne $null)
    {
      & $build.TestsFunction $build
    }
  }
}

function MSBuildBuild($build)
{
  $name = $build.Name
  $finalDir = $build.FinalDir

  Write-Host
  Write-Host "Restoring $workingSourceDir\$name.sln" -ForegroundColor Green
  [Environment]::SetEnvironmentVariable("EnableNuGetPackageRestore", "true", "Process")
  exec { .\Tools\NuGet\NuGet.exe update -self }
  exec { .\Tools\NuGet\NuGet.exe restore "$workingSourceDir\$name.sln" -verbosity detailed -configfile $workingSourceDir\nuget.config | Out-Default } "Error restoring $name"

  $constants = GetConstants $build.Constants $signAssemblies

  Write-Host
  Write-Host "Building $workingSourceDir\$name.sln" -ForegroundColor Green
  exec { msbuild "/t:Clean;Rebuild" /p:Configuration=Release "/p:CopyNuGetImplementations=true" "/p:Platform=Any CPU" "/p:PlatformTarget=AnyCPU" /p:OutputPath=bin\Release\$finalDir\ /p:AssemblyOriginatorKeyFile=$signKeyPath "/p:SignAssembly=$signAssemblies" "/p:TreatWarningsAsErrors=$treatWarningsAsErrors" "/p:VisualStudioVersion=14.0" /p:DefineConstants=`"$constants`" "$workingSourceDir\$name.sln" | Out-Default } "Error building $name"
}

function DnxBuild($build)
{
  $name = $build.Name
  $projectPath = "$workingSourceDir\Newtonsoft.Json\project.json"

  exec { dnvm install $dnvmVersion -r clr -u | Out-Default }
  exec { dnvm use $dnvmVersion -r clr | Out-Default }

  Write-Host -ForegroundColor Green "Restoring packages for $name"
  Write-Host
  exec {
    try {
      dnu restore $projectPath | Out-Default
      Write-Host "Restore last exit code: $lastexitcode"
    }
    catch [System.Management.Automation.RemoteException]
    {
      Write-Host "Restore last exit code: $lastexitcode"
      Write-Host ("Restore exception: " + $_.ToString())
      $global:lastexitcode = 0
      $lastexitcode = 0
    }
  }

  Write-Host -ForegroundColor Green "Building $projectPath"
  exec { dnu build $projectPath --configuration Release | Out-Default }
}

function DnxTests($build)
{
  $name = $build.TestsName

  #Write-Host -ForegroundColor Green "Ensuring latest CoreCLR is installed for $name"
  #Write-Host
  #exec { & $toolsDir\Kvm\kvm.ps1 upgrade -r CoreCLR -NoNative | Out-Default }

  exec { dnvm install $dnvmVersion -r coreclr -u | Out-Default }
  exec { dnvm use $dnvmVersion -r coreclr | Out-Default }

  Write-Host -ForegroundColor Green "Restoring packages for $name"
  Write-Host
  exec {
    try {
      dnu restore "$workingSourceDir\Newtonsoft.Json.Tests\project.json" | Out-Default
      Write-Host "Restore last exit code: $lastexitcode"
    }
    catch [System.Management.Automation.RemoteException]
    {
      Write-Host "Restore last exit code: $lastexitcode"
      Write-Host ("Restore exception: " + $_.ToString())
      $global:lastexitcode = 0
      $lastexitcode = 0
    }
  }

  Write-Host -ForegroundColor Green "Ensuring test project builds for $name"
  Write-Host

  try
  {
    Set-Location "$workingSourceDir\Newtonsoft.Json.Tests"
    exec { dnx --configuration Release test | Out-Default }
  }
  finally
  {
    Set-Location $baseDir
  }
}

function NUnitTests($build)
{
  $name = $build.TestsName
  $finalDir = $build.FinalDir
  $framework = $build.Framework

  Write-Host -ForegroundColor Green "Copying test assembly $name to deployed directory"
  Write-Host
  robocopy "$workingSourceDir\Newtonsoft.Json.Tests\bin\Release\$finalDir" $workingDir\Deployed\Bin\$finalDir /MIR /NFL /NDL /NJS /NC /NS /NP /XO | Out-Default

  Copy-Item -Path "$workingSourceDir\Newtonsoft.Json.Tests\bin\Release\$finalDir\Newtonsoft.Json.Tests.dll" -Destination $workingDir\Deployed\Bin\$finalDir\

  Write-Host -ForegroundColor Green "Running NUnit tests " $name
  Write-Host
  exec { .\Tools\NUnit\nunit-console.exe "$workingDir\Deployed\Bin\$finalDir\Newtonsoft.Json.Tests.dll" /framework=$framework /xml:$workingDir\$name.xml | Out-Default } "Error running $name tests"
}

function GetConstants($constants, $includeSigned)
{
  $signed = switch($includeSigned) { $true { ";SIGNED" } default { "" } }

  return "CODE_ANALYSIS;TRACE;$constants$signed"
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

function Update-AssemblyInfoFiles ([string] $workingSourceDir, [string] $assemblyVersionNumber, [string] $fileVersionNumber)
{
    $assemblyVersionPattern = 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $fileVersionPattern = 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $assemblyVersion = 'AssemblyVersion("' + $assemblyVersionNumber + '")';
    $fileVersion = 'AssemblyFileVersion("' + $fileVersionNumber + '")';
    
    Get-ChildItem -Path $workingSourceDir -r -filter AssemblyInfo.cs | ForEach-Object {
        
        $filename = $_.Directory.ToString() + '\' + $_.Name
        Write-Host $filename
        $filename + ' -> ' + $version
    
        (Get-Content $filename) | ForEach-Object {
            % {$_ -replace $assemblyVersionPattern, $assemblyVersion } |
            % {$_ -replace $fileVersionPattern, $fileVersion }
        } | Set-Content $filename
    }
}

function Edit-XmlNodes {
    param (
        [xml] $doc,
        [string] $xpath = $(throw "xpath is a required parameter"),
        [string] $value = $(throw "value is a required parameter")
    )
    
    $nodes = $doc.SelectNodes($xpath)
    $count = $nodes.Count

    Write-Host "Found $count nodes with path '$xpath'"
    
    foreach ($node in $nodes) {
        if ($node -ne $null) {
            if ($node.NodeType -eq "Element")
            {
                $node.InnerXml = $value
            }
            else
            {
                $node.Value = $value
            }
        }
    }
}

function Update-Project {
  param (
    [string] $projectPath,
    [string] $sign
  )

  $file = switch($sign) { $true { $signKeyPath } default { $null } }

  $json = (Get-Content $projectPath) -join "`n" | ConvertFrom-Json
  $options = @{"warningsAsErrors" = $true; "keyFile" = $file; "define" = ((GetConstants "dotnet" $sign) -split ";") }
  Add-Member -InputObject $json -MemberType NoteProperty -Name "compilationOptions" -Value $options -Force

  ConvertTo-Json $json -Depth 10 | Set-Content $projectPath
}

function Execute-Command($command) {
    $currentRetry = 0
    $success = $false
    do {
        try
        {
            & $command
            $success = $true
        }
        catch [System.Exception]
        {
            if ($currentRetry -gt 5) {
                throw $_.Exception.ToString()
            } else {
                write-host "Retry $currentRetry"
                Start-Sleep -s 1
            }
            $currentRetry = $currentRetry + 1
        }
    } while (!$success)
}