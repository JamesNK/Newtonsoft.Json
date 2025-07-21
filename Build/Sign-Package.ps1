
$currentDirectory = split-path $MyInvocation.MyCommand.Definition

# See if we have the SignKeyVaultCertificate available
if ([string]::IsNullOrEmpty($Env:SignKeyVaultCertificate)){
    Write-Host "Key vault detail not found, not signing packages"
    return;
}

dotnet tool install --tool-path . --prerelease sign

$files = gci $Env:ArtifactDirectory\*.nupkg,*.zip -recurse | Select -ExpandProperty FullName
$signKeyVaultCertificate = $Env:SignKeyVaultCertificate
$signKeyVaultUrl = $Env:SignKeyVaultUrl

foreach ($file in $files){
    Write-Host "Submitting $file for signing"

    .\sign code azure-key-vault $file `
        --publisher-name "Newtonsoft" `
        --description "Json.NET" `
        --description-url "https://www.newtonsoft.com/json" `
        --azure-key-vault-certificate $signKeyVaultCertificate `
        --azure-key-vault-url $signKeyVaultUrl

    Write-Host "Finished signing $file"
}

Write-Host "Sign-package complete"