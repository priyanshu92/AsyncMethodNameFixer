param (
    [Parameter(Mandatory=$true)][string]$filePath
)

(new-object Net.WebClient).DownloadString("https://raw.github.com/madskristensen/ExtensionScripts/master/AppVeyor/vsix.ps1") | iex

Vsix-PublishToGallery $filePath
