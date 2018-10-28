Start-Transcript ($PSScriptRoot + '\Temp\runbuild.txt')

$version = Get-Content "$PSScriptRoot\version.json" | Out-String | ConvertFrom-Json

& $PSScriptRoot\runbuild.ps1 -properties @{"majorVersion"="$($version.Major).0"; "majorWithReleaseVersion"="$($version.Major).0.$($version.Release)"; "nugetPrerelease"=$version.Prerelease; "zipFileName"="Json$($version.Major)0r$($version.Release).zip"; "treatWarningsAsErrors"=$true}