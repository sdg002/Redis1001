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
$webappname="redis-demo-webapp-001"

$scriptfolder=$PSScriptRoot

New-AzResourceGroup -Name $resourcegroupname -Location $location -Force -Verbose

#
#Create resource group
#
New-AzResourceGroupDeployment -ResourceGroupName $resourcegroupname -TemplateFile $scriptfolder\arm-storageaccount\template.json -TemplateParameterFile $scriptfolder\arm-storageaccount\parameters.json -location $location -storageAccountName $storageaccountname -Verbose
$oStorageAccount=Get-AzResource -ResourceGroupName $resourcegroupname -Name $storageaccountname
#
#Create Redis cache
#
"Creating Redis Cache"
New-AzResourceGroupDeployment -ResourceGroupName $resourcegroupname -TemplateFile $scriptfolder\arm-redis\template.json -TemplateParameterFile $scriptfolder\arm-redis\parameters.json -redisCacheName $rediscache -redisCacheSKU "Basic" -existingDiagnosticsStorageAccountId $oStorageAccount.ResourceId  -Verbose
#
#Get connection parameters for the newly created Redis instance
#
$config=Get-AzRedisCache -name $rediscache
$redisKeys=Get-AzRedisCacheKey -Name $rediscache
"Host name {0}" -f $config.hostname
"Port number {0}" -f $config.port
"Key {0}" -f $keys.PrimaryKey
$redisCnStringTxn="{0}:{1},password={2},ssl=True,abortConnect=False" -f $config.hostname,$config.Port,$redisKeys.PrimaryKey,
#
#Create application insights
#
"Creating Application Insights"
New-AzResourceGroupDeployment -TemplateFile $scriptfolder\arm2-appinsights\template.json -TemplateParameterFile $scriptfolder\arm2-appinsights\parameters.json  -ResourceGroupName $resourcegroupname  -nameFromTemplate $appinsights -regionId $location -Verbose
#Get the instru key
"Getting the instru key"
$appInsights=Get-AzApplicationInsights -resourceGroupName $resourcegroupname -name $appinsights
#
#Delete existing App Service if exists
#
"Checking for presence of App Service"
$appFuncs=Get-AzResource -ResourceGroupName $resourcegroupname | where -FilterScript {$_.Name -eq $webappname}
if ($appFuncs -ne $null)
{
    "Deleting App Service $webappname" 
    Remove-AzResource -ResourceGroupName $resourcegroupname -ResourceName $webappname -ResourceType "Microsoft.Web/sites" -Force
    "Deleted app service $webappname" 
}
#
#Delete App Service Plan if exists
#
"Checking for presence of App Service Plan $appserviceplan"
$existingAppPlans=Get-AzResource -ResourceGroupName $resourcegroupname | where -FilterScript {$_.Name -eq $appserviceplan}
if ($existingAppPlans -ne $null)
{
    "Deleting App Service Plan $appserviceplan"
    Remove-AzResource -ResourceGroupName $resourcegroupname -ResourceName $appserviceplan -ResourceType "Microsoft.Web/serverfarms" -Force
    "Deleted App Service Plan $appserviceplan" 
    
}
#
#Create App Service Plan
#
"Creating plan $appserviceplan"
New-AzResourceGroupDeployment -TemplateFile $scriptfolder\arm-plan-only\template.json -TemplateParameterFile $scriptfolder\arm-plan-only\parameters.json  -ResourceGroupName $resourcegroupname  -planname $appserviceplan -location $location
#
#Create App Service
#
"Creating app service $webappname"
New-AzResourceGroupDeployment -TemplateFile $scriptfolder\arm-app-only\template.json -TemplateParameterFile $scriptfolder\arm-app-only\parameters.json -ResourceGroupName $resourcegroupname -nameFromTemplate $webappname  -location $location  -hostingPlanName  $appserviceplan -instrumentationkey $appInsights.InstrumentationKey -serverFarmResourceGroup  $resourcegroupname -rediscnstring $redisCnStringTxn  -Verbose
