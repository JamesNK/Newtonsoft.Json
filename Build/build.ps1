properties { 
  $zipFileName = "Json35r7.zip"
  $signAssemblies = $false
  $signKeyPath = "D:\Development\Releases\newtonsoft.snk"
  $buildDocumentation = $false
  
  $baseDir  = resolve-path ..
  $buildDir = "$baseDir\Build"
  $sourceDir = "$baseDir\Src"
  $toolsDir = "$baseDir\Tools"
  $releaseDir = "$baseDir\Release"
  $workingDir = "$baseDir\Working"
  $builds = @(
    @{Name = "Newtonsoft.Json.Silverlight"; TestsName = "Newtonsoft.Json.Tests.Silverlight"; Constants="SILVERLIGHT"; FinalDir="Silverlight"},
    @{Name = "Newtonsoft.Json.Compact"; TestsName = "Newtonsoft.Json.Tests.Compact"; Constants="PocketPC"; FinalDir="Compact"},
    @{Name = "Newtonsoft.Json.Net20"; TestsName = "Newtonsoft.Json.Tests.Net20"; Constants="NET20"; FinalDir="DotNet20"},
    @{Name = "Newtonsoft.Json"; TestsName = "Newtonsoft.Json.Tests"; Constants=""; FinalDir="DotNet"}
  )
} 

task default -depends Build

task Clean {
  Set-Location $baseDir
  
  if (Test-Path -path $workingDir)
  {
    Write-Output "Deleting Working Directory"
    
    del $workingDir -Recurse -Force
  }
  
  New-Item -Path $workingDir -ItemType Directory
}

task Build -depends Clean { 
  
  foreach ($build in $builds)
  {
    $name = $build.Name
    Write-Host -ForegroundColor Green "Building " $name
    Write-Host
    exec { msbuild "/t:Clean;Rebuild" /p:Configuration=Release /p:AssemblyOriginatorKeyFile=$signKeyPath "/p:SignAssembly=$signAssemblies" (GetConstants $build.Constants $signAssemblies) ".\Src\Newtonsoft.Json\$name.csproj" } "Error building $name"
  }
}

task Package -depends Build {
  $compileOutputDir = "$sourceDir\Newtonsoft.Json\bin\Release"
  
  Copy-Item -Path $compileOutputDir -Destination $workingDir -recurse

  New-Item -Path $workingDir\Merge -ItemType Directory
  
  $ilMergeKeyFile = switch($signAssemblies) { $true { "/keyfile:$signKeyPath" } default { "" } }
  
  exec { .\Tools\ILMerge\ilmerge.exe "/internalize" $ilMergeKeyFile "/out:$workingDir\Merge\Newtonsoft.Json.Net20.dll" "$workingDir\Release\Newtonsoft.Json.Net20.dll" "$workingDir\Release\LinqBridge.dll" } "Error executing ILMerge"

  del $workingDir\Release\Newtonsoft.Json.Net20.dll
  del $workingDir\Release\Newtonsoft.Json.Net20.pdb
  del $workingDir\Release\LinqBridge.dll

  Copy-Item -Path $workingDir\Merge\Newtonsoft.Json.Net20.dll -Destination $workingDir\Release\
  Copy-Item -Path $workingDir\Merge\Newtonsoft.Json.Net20.pdb -Destination $workingDir\Release\

  foreach ($build in $builds)
  {
    $name = $build.Name
    $finalDir = $build.FinalDir
    
    New-Item -Path $workingDir\Package\Bin\$finalDir -ItemType Directory    
    dir $workingDir\Release | where { $_.Name -like "*$name*" -and $_.PSIsContainer -ne $true } | move -Destination $workingDir\Package\Bin\$finalDir\$_
  }
  
  del $workingDir\Merge -Recurse
  del $workingDir\Release -Recurse
  
  if ($buildDocumentation)
  {
    exec { msbuild "/t:Clean;Rebuild" /p:Configuration=Release .\Doc\doc.shfbproj } "Error building documentation. Check that you have Sandcastle, Sandcastle Help File Builder and HTML Help Workshop installed."
    
    New-Item -Path $workingDir\Package\Documentation -ItemType Directory
    move -Path $workingDir\Documentation\Documentation.chm -Destination $workingDir\Package\Documentation\Documentation.chm
    move -Path $workingDir\Documentation\LastBuild.log -Destination $workingDir\Documentation.log
  }

  robocopy $sourceDir $workingDir\Package\Source\Src /MIR /NP /XD .svn bin obj /XF *.suo *.user
  robocopy $buildDir $workingDir\Package\Source\Build /MIR /NP /XD .svn
  robocopy $docDir $workingDir\Package\Source\Doc /MIR /NP /XD .svn
  robocopy $toolsDir $workingDir\Package\Source\Tools /MIR /NP /XD .svn
  
  exec { .\Tools\7-zip\7za.exe a -tzip $workingDir\$zipFileName $workingDir\Package\* } "Error zipping"
}

task Deploy -depends Package {
  exec { .\Tools\7-zip\7za.exe x -y "-o$workingDir\Deployed" $workingDir\$zipFileName } "Error unzipping"
}

task Test -depends Deploy {
  foreach ($build in $builds)
  {
    $name = $build.TestsName
    Write-Host -ForegroundColor Green "Building " $name
    Write-Host
    exec { msbuild "/t:Clean;Rebuild" /p:Configuration=Release /p:AssemblyOriginatorKeyFile=$signKeyPath "/p:SignAssembly=$signAssemblies" (GetConstants $build.Constants $signAssemblies) ".\Src\Newtonsoft.Json.Tests\$name.csproj" } "Error executing MSBUILD"
    Write-Host -ForegroundColor Green "Running tests " $name
    Write-Host
    exec { .\Tools\NUnit\nunit-console.exe ".\Src\Newtonsoft.Json.Tests\bin\Release\$name.dll" /xml:$workingDir\$name.xml } "Error running $name tests"
  }
}

function GetConstants($constants, $includeSigned)
{
  $signed = switch($includeSigned) { $true { ";SIGNED" } default { "" } }

  return "/p:DefineConstants=`"TRACE;$constants$signed`""
}