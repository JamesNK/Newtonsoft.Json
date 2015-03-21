cls

$path = Split-Path -Path $MyInvocation.MyCommand.Path

Import-Module ($path + '\..\Tools\PSake\psake.psm1')
Invoke-psake ($path + '\build.ps1') Test -framework 3.5
Remove-Module psake