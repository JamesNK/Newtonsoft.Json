Start-Transcript ($PSScriptRoot + '\Temp\runbuild.txt')

& $PSScriptRoot\runbuild.ps1 -properties @{"treatWarningsAsErrors"=$true}