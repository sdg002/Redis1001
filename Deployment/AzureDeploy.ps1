Set-StrictMode -Version "2.0"
$ErrorActionPreference="Stop"
cls

$envname="dev"
$resourcegroupname="rg-$envname-redis-demo"
$location="uksouth"
$rediscache="saudemo001-$envname"
$storageaccountname="stgsaudemo001"
$appinsights="saudemo001-appinsights-$envname"
$appserviceplan="sauplan001-$envname"

$scriptfolder=$PSScriptRoot

New-AzResourceGroup -Name $resourcegroupname -Location $location -Force -Verbose


New-AzResourceGroupDeployment -ResourceGroupName $resourcegroupname -TemplateFile $scriptfolder\arm-storageaccount\template.json -TemplateParameterFile $scriptfolder\arm-storageaccount\parameters.json -location $location -storageAccountName $storageaccountname -Verbose
$oStorageAccount=Get-AzResource -ResourceGroupName $resourcegroupname -Name $storageaccountname


"Creating Redis Cache"
New-AzResourceGroupDeployment -ResourceGroupName $resourcegroupname -TemplateFile $scriptfolder\arm-redis\template.json -TemplateParameterFile $scriptfolder\arm-redis\parameters.json -redisCacheName $rediscache -redisCacheSKU "Basic" -existingDiagnosticsStorageAccountId $oStorageAccount.ResourceId  -Verbose
#
#Get connection parameters for the newly created Redis instance
#
$config=Get-AzRedisCache -name $rediscache
$keys=Get-AzRedisCacheKey -Name $rediscache
"Host name {0}" -f $config.hostname
"Port number {0}" -f $config.port
"Key {0}" -f $keys.PrimaryKey

#
#Create application insights
#
"Creating Application Insights"
New-AzResourceGroupDeployment -TemplateFile $scriptfolder\arm2-appinsights\template.json -TemplateParameterFile $scriptfolder\arm2-appinsights\parameters.json  -ResourceGroupName $resourcegroupname  -nameFromTemplate $appinsights -regionId $location -Verbose
#
#Create plan
#
"Creating plan"
New-AzResourceGroupDeployment -TemplateFile $scriptfolder\arm-plan-only\template.json -TemplateParameterFile $scriptfolder\arm-plan-only\parameters.json  -ResourceGroupName $resourcegroupname  -nameFromTemplate $appserviceplan -location $location
