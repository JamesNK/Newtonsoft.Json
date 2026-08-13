#Requires -Version 7.0
#Requires -PSEdition Core

Start-Transcript ($PSScriptRoot + '\Temp\runbuild.txt')

& $PSScriptRoot\runbuild.ps1 -properties @{"treatWarningsAsErrors"=$true}