cls
powershell -Command "& { Start-Transcript '%~dp0\Temp\runbuild.txt'; Import-Module '%~dp0\psake.psm1'; Invoke-psake '%~dp0..\Build\build.ps1' %*; Stop-Transcript; exit !($psake.build_success); }"

ECHO %ERRORLEVEL%
EXIT /B %ERRORLEVEL%