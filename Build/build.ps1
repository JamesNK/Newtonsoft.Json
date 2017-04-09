properties { 
  $zipFileName = "Json100r2.zip"
  $majorVersion = "10.0"
  $majorWithReleaseVersion = "10.0.2"
  $nugetPrerelease = $null
  $version = GetVersion $majorWithReleaseVersion
  $packageId = "Newtonsoft.Json"
  $signAssemblies = $false
  $signKeyPath = "C:\Development\Releases\newtonsoft.snk"
  $buildDocumentation = $false
  $buildNuGet = $true
  $treatWarningsAsErrors = $false
  $workingName = if ($workingName) {$workingName} else {"Working"}
  $netCliVersion = "1.0.0"
  $nugetUrl = "http://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
  
  $baseDir  = resolve-path ..
  $buildDir = "$baseDir\Build"
  $sourceDir = "$baseDir\Src"
  $toolsDir = "$baseDir\Tools"
  $docDir = "$baseDir\Doc"
  $releaseDir = "$baseDir\Release"
  $workingDir = "$baseDir\$workingName"
  $workingSourceDir = "$workingDir\Src"

  $nugetPath = "$buildDir\Temp\nuget.exe"
  $vswhereVersion = "1.0.58"
  $vswherePath = "$buildDir\Temp\vswhere.$vswhereVersion"
  $nunitConsoleVersion = "3.6.1"
  $nunitConsolePath = "$buildDir\Temp\NUnit.ConsoleRunner.$nunitConsoleVersion"

  $builds = @(
    @{Framework = "netstandard1.3"; TestsFunction = "NetCliTests"; TestFramework = "netcoreapp1.1"; Enabled=$true},
    @{Framework = "netstandard1.0"; TestsFunction = "NetCliTests"; TestFramework = "netcoreapp1.0"; Enabled=$true},
    @{Framework = "net45"; TestsFunction = "NUnitTests"; NUnitFramework="net-4.0"; Enabled=$true},
    @{Framework = "net40"; TestsFunction = "NUnitTests"; NUnitFramework="net-4.0"; Enabled=$true},
    @{Framework = "net35"; TestsFunction = "NUnitTests"; NUnitFramework="net-2.0"; Enabled=$true},
    @{Framework = "net20"; TestsFunction = "NUnitTests"; NUnitFramework="net-2.0"; Enabled=$true},
    @{Framework = "portable-net45+win8+wpa81+wp8"; TestsFunction = "NUnitTests"; TestFramework = "net452"; NUnitFramework="net-4.0"; Enabled=$true},
    @{Framework = "portable-net40+win8+wpa81+wp8+sl5"; TestsFunction = "NUnitTests"; TestFramework = "net451"; NUnitFramework="net-4.0"; Enabled=$true}
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
  $script:enabledBuilds = $builds | ? {$_.Enabled}
  Write-Host -ForegroundColor Green "Found $($script:enabledBuilds.Length) enabled builds"

  mkdir "$buildDir\Temp" -Force
  EnsureNuGetExists
  EnsureNuGetPacakge "vswhere" $vswherePath $vswhereVersion
  EnsureNuGetPacakge "NUnit.ConsoleRunner" $nunitConsolePath $nunitConsoleVersion

  $script:msBuildPath = GetMsBuildPath
  Write-Host "MSBuild path $script:msBuildPath"

  Write-Host "Copying source to working source directory $workingSourceDir"
  robocopy $sourceDir $workingSourceDir /MIR /NP /XD bin obj TestResults AppPackages $packageDirs .vs artifacts /XF *.suo *.user *.lock.json | Out-Default
  Copy-Item -Path $baseDir\LICENSE.md -Destination $workingDir\
  mkdir "$workingDir\Build" -Force
  Copy-Item -Path $buildDir\install.ps1 -Destination $workingDir\Build\

  Write-Host -ForegroundColor Green "Updating assembly version"
  Write-Host
  Update-AssemblyInfoFiles $workingSourceDir ($majorVersion + '.0.0') $version

  $xml = [xml](Get-Content "$workingSourceDir\Newtonsoft.Json\Newtonsoft.Json.Roslyn.csproj")
  Edit-XmlNodes -doc $xml -xpath "/Project/PropertyGroup/PackageId" -value $packageId
  Edit-XmlNodes -doc $xml -xpath "/Project/PropertyGroup/VersionPrefix" -value $majorWithReleaseVersion
  Edit-XmlNodes -doc $xml -xpath "/Project/PropertyGroup/VersionSuffix" -value $nugetPrerelease
  $xml.save("$workingSourceDir\Newtonsoft.Json\Newtonsoft.Json.Roslyn.csproj")

  $projectPath = "$workingSourceDir\Newtonsoft.Json\Newtonsoft.Json.Roslyn.csproj"

  NetCliBuild
}

# Optional build documentation, add files to final zip
task Package -depends Build {
  foreach ($build in $script:enabledBuilds)
  {
    $finalDir = $build.Framework

    $sourcePath = "$workingSourceDir\Newtonsoft.Json\bin\Release\$finalDir"

    if (!(Test-Path -path $sourcePath))
    {
      throw "Could not find $sourcePath"
    }

    robocopy $sourcePath $workingDir\Package\Bin\$finalDir *.dll *.pdb *.xml /NFL /NDL /NJS /NC /NS /NP /XO /XF *.CodeAnalysisLog.xml | Out-Default
  }
  
  if ($buildNuGet)
  {
    Write-Host -ForegroundColor Green "Creating NuGet package"

    $targetFrameworks = ($script:enabledBuilds | Select-Object @{Name="Framework";Expression={$_.Framework}} | select -expand Framework) -join ";"

    exec { & $script:msBuildPath "/t:pack" "/p:IncludeSource=true" "/p:Configuration=Release" "/p:TargetFrameworks=`"$targetFrameworks`"" "$workingSourceDir\Newtonsoft.Json\Newtonsoft.Json.Roslyn.csproj" }

    mkdir $workingDir\NuGet
    move -Path $workingSourceDir\Newtonsoft.Json\bin\Release\*.nupkg -Destination $workingDir\NuGet
  }

  Write-Host "Build documentation: $buildDocumentation"
  
  if ($buildDocumentation)
  {
    $mainBuild = $script:enabledBuilds | where { $_.Framework -eq "net45" } | select -first 1
    $mainBuildFinalDir = $mainBuild.Framework
    $documentationSourcePath = "$workingDir\Package\Bin\$mainBuildFinalDir"
    $docOutputPath = "$workingDir\Documentation\"
    Write-Host -ForegroundColor Green "Building documentation from $documentationSourcePath"
    Write-Host "Documentation output to $docOutputPath"

    # Sandcastle has issues when compiling with .NET 4 MSBuild - http://shfb.codeplex.com/Thread/View.aspx?ThreadId=50652
    exec { & $script:msBuildPath "/t:Clean;Rebuild" "/p:Configuration=Release" "/p:DocumentationSourcePath=$documentationSourcePath" "/p:OutputPath=$docOutputPath" "$docDir\doc.shfbproj" | Out-Default } "Error building documentation. Check that you have Sandcastle, Sandcastle Help File Builder and HTML Help Workshop installed."
    
    move -Path $workingDir\Documentation\LastBuild.log -Destination $workingDir\Documentation.log
  }
  
  Copy-Item -Path $docDir\readme.txt -Destination $workingDir\Package\
  Copy-Item -Path $docDir\license.txt -Destination $workingDir\Package\

  robocopy $workingSourceDir $workingDir\Package\Source\Src /MIR /NFL /NDL /NJS /NC /NS /NP /XD bin obj TestResults AppPackages .vs artifacts /XF *.suo *.user *.lock.json | Out-Default
  robocopy $buildDir $workingDir\Package\Source\Build /MIR /NFL /NDL /NJS /NC /NS /NP /XD Temp /XF runbuild.txt | Out-Default
  robocopy $docDir $workingDir\Package\Source\Doc /MIR /NFL /NDL /NJS /NC /NS /NP | Out-Default
  robocopy $toolsDir $workingDir\Package\Source\Tools /MIR /NFL /NDL /NJS /NC /NS /NP | Out-Default
  
  Compress-Archive -Path $workingDir\Package\* -DestinationPath $workingDir\$zipFileName
}

# Unzip package to a location
task Deploy -depends Package {
  Expand-Archive -Path $workingDir\$zipFileName -DestinationPath "$workingDir\Deployed" 
}

# Run tests on deployed files
task Test -depends Deploy {
  foreach ($build in $script:enabledBuilds)
  {
    Write-Host "Calling $($build.TestsFunction)"
    & $build.TestsFunction $build
  }
}

function NetCliBuild()
{
  $projectPath = "$workingSourceDir\Newtonsoft.Json.Roslyn.sln"
  $libraryFrameworks = ($script:enabledBuilds | Select-Object @{Name="Framework";Expression={$_.Framework}} | select -expand Framework) -join ";"
  $testFrameworks = ($script:enabledBuilds | Select-Object @{Name="Resolved";Expression={if ($_.TestFramework -ne $null) { $_.TestFramework } else { $_.Framework }}} | select -expand Resolved) -join ";"

  $additionalConstants = switch($signAssemblies) { $true { "SIGNED" } default { "" } }

  Write-Host -ForegroundColor Green "Restoring packages for $libraryFrameworks in $projectPath"
  Write-Host

  exec { & $script:msBuildPath "/t:restore" "/p:Configuration=Release" "/p:LibraryFrameworks=`"$libraryFrameworks`"" "/p:TestFrameworks=`"$testFrameworks`"" $projectPath | Out-Default } "Error restoring $projectPath"

  Write-Host -ForegroundColor Green "Building $libraryFrameworks in $projectPath"
  Write-Host

  exec { & $script:msBuildPath "/t:build" $projectPath "/p:Configuration=Release" "/p:LibraryFrameworks=`"$libraryFrameworks`"" "/p:TestFrameworks=`"$testFrameworks`"" "/p:AssemblyOriginatorKeyFile=$signKeyPath" "/p:SignAssembly=$signAssemblies" "/p:TreatWarningsAsErrors=$treatWarningsAsErrors" "/p:AdditionalConstants=$additionalConstants" }
}

function EnsureNuGetExists()
{
  if (!(Test-Path $nugetPath))
  {
    Write-Host "Couldn't find nuget.exe. Downloading from $nugetUrl to $nugetPath"
    (New-Object System.Net.WebClient).DownloadFile($nugetUrl, $nugetPath)
  }
}

function EnsureNuGetPacakge($packageName, $packagePath, $packageVersion)
{
  if (!(Test-Path $packagePath))
  {
    Write-Host "Couldn't find $packagePath. Downloading with NuGet"
    exec { & $nugetPath install $packageName -OutputDirectory $buildDir\Temp -Version $packageVersion | Out-Default } "Error restoring $packagePath"
  }
}

function GetMsBuildPath()
{
  $path = & $vswherePath\tools\vswhere.exe -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
  if (!($path)) {
    throw "Could not find Visual Studio install path"
  }
  return join-path $path 'MSBuild\15.0\Bin\MSBuild.exe'
}

function NetCliTests($build)
{
  $projectPath = "$workingSourceDir\Newtonsoft.Json.Tests\Newtonsoft.Json.Tests.Roslyn.csproj"
  $location = "$workingSourceDir\Newtonsoft.Json.Tests"
  $testDir = if ($build.TestFramework -ne $null) { $build.TestFramework } else { $build.Framework }

  exec { .\Tools\Dotnet\dotnet-install.ps1 -Version $netCliVersion | Out-Default }

  try
  {
    Set-Location $location

    exec { dotnet --version | Out-Default }

    Write-Host -ForegroundColor Green "Running tests for $testDir"
    Write-Host

    exec { dotnet test $projectPath -f $testDir -c Release -l trx | Out-Default }
    copy-item -Path "$location\TestResults\*.trx" -Destination $workingDir
  }
  finally
  {
    Set-Location $baseDir
  }
}

function NUnitTests($build)
{
  $testDir = if ($build.TestFramework -ne $null) { $build.TestFramework } else { $build.Framework }
  $framework = $build.NUnitFramework
  $testRunDir = "$workingDir\Deployed\Bin\$($build.Framework)"

  Write-Host -ForegroundColor Green "Copying test assembly $testDir to deployed directory"
  Write-Host
  robocopy "$workingSourceDir\Newtonsoft.Json.Tests\bin\Release\$testDir" $testRunDir /MIR /NFL /NDL /NJS /NC /NS /NP /XO | Out-Default

  Write-Host -ForegroundColor Green "Running NUnit tests " $testDir
  Write-Host
  try
  {
    Set-Location $testRunDir
    exec { & $nunitConsolePath\tools\nunit3-console.exe "$testRunDir\Newtonsoft.Json.Tests.dll" --framework=$framework --result=$workingDir\$testDir.xml | Out-Default } "Error running $testDir tests"
  }
  finally
  {
    Set-Location $baseDir
  }
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