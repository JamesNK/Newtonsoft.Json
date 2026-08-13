@where pwsh >nul 2>&1 || (
	echo PowerShell 7 or later is required.
	exit /b 1
)

@cls
@pwsh -NoLogo -NoProfile -Command "& { Start-Transcript '%~dp0\Temp\runbuild.txt'; Import-Module '%~dp0\psake.psm1'; Invoke-psake '%~dp0..\Build\build.ps1' %*; Stop-Transcript; exit !($psake.build_success); }"

@ECHO %ERRORLEVEL%
@EXIT /B %ERRORLEVEL%