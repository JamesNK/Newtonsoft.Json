cls
powershell -Command "& { [Console]::WindowWidth = 150; [Console]::WindowHeight = 50; Start-Transcript %~dp0runbuild.txt; Import-Module %~dp0..\Tools\PSake\psake.psm1; Invoke-psake %~dp0..\Build\build.ps1 %*; Stop-Transcript; exit !($psake.build_success); }"

ECHO %ERRORLEVEL%
EXIT /B %ERRORLEVEL%