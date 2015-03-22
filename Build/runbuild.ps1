cls

$path = Split-Path -Path $MyInvocation.MyCommand.Path

Import-Module ($path + '\..\Tools\PSake\psake.psm1')

Try
{
  Invoke-psake ($path + '\build.ps1') Test -framework 3.5

  if ($error -ne '')
  {
    $exitCode = $error.Count
    write-host "build.ps1 exit code:  $exitCode" -fore RED
    exit  $exitCode
  }
}
Finally
{
  Remove-Module psake
}