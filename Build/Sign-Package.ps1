
$currentDirectory = split-path $MyInvocation.MyCommand.Definition

# See if we have the ClientSecret available
if([string]::IsNullOrEmpty($Env:SignClientSecret)){
	Write-Host "Client Secret not found, not signing packages"
	return;
}

dotnet tool install --tool-path . SignClient

# Setup Variables we need to pass into the sign client tool

$appSettings = "$currentDirectory\appsettings.json"

$files = gci $Env:ArtifactDirectory\*.nupkg,*.zip -recurse | Select -ExpandProperty FullName

foreach ($file in $files){
	Write-Host "Submitting $file for signing"

	.\SignClient 'sign' -c $appSettings -i $file -r $Env:SignClientUser -s $Env:SignClientSecret -n 'Json.NET' -d 'Json.NET' -u 'https://www.newtonsoft.com/json' 

	Write-Host "Finished signing $file"
}

Write-Host "Sign-package complete"