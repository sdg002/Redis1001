Set-StrictMode -Version "2.0"
$ErrorActionPreference="Stop"
$subscription="Pay-As-You-Go-demo"
cls
$subscription="Pay-As-You-Go-demo"
$scriptfolder=$PSScriptRoot
$folderFuncCsproj=Join-Path -Path $scriptfolder -ChildPath "..\FunctionApp1\FunctionApp1.csproj"
$subdirName=(Get-Date).ToString("dd-MMM-yyyy-HH-mm")
$folderPublish=Join-Path -Path $scriptfolder -ChildPath "Zips\$subdirName"
"Invoking dotnet publish"
dotnet publish $folderFuncCsproj -c release -o $folderPublish
"Creating of binaries complete at $folderPublish"
"Now creating zip"
#
#Compress the output using 7z
#
$folderZips=Join-Path -Path $scriptfolder -ChildPath "Zips"
$fileZip=Join-Path -Path $folderZips -ChildPath "$subdirName.zip"
$path7z=Join-Path -Path $scriptfolder -ChildPath "\7z\7za.exe"
& $path7z a $fileZip "$folderPublish\*"
"Zip file created at $fileZip"

"Going to use az functionapp CLI for uploading the zip $fileZip"
az functionapp deployment source config-zip -g "rg-dev-redis-demo" --src $fileZip -n "redis-demo-webapp-001" --subscription $subscription
"CLI complete"



#Publish-AzWebApp -ResourceGroupName "rg-dev-redis-demo" -Name "redis-demo-webapp-001" -ArchivePath $fileZip