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
$subscription="Pay-As-You-Go-demo"

$scriptfolder=$PSScriptRoot

Set-AzContext -Subscription $subscription
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
$redisConfig=Get-AzRedisCache -name $rediscache
$redisKeys=Get-AzRedisCacheKey -Name $rediscache
"Host name {0}" -f $redisConfig.hostname
"Port number {0}" -f $redisConfig.port
"Key {0}" -f $redisKeys.PrimaryKey
$redisCnStringTxn="{0}:{1},password={2},ssl=True,abortConnect=False" -f $config.hostname,$config.SslPort,$redisKeys.PrimaryKey
#
#Create Plan+FunctionApp+AppInsights
#
$uriTemplate="https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/101-function-app-create-dynamic/azuredeploy.json"
New-AzResourceGroupDeployment -TemplateUri $uriTemplate -ResourceGroupName $resourcegroupname -appname  $webappname -runtime dotnet

#
#Update redis cn string
#
az functionapp config appsettings set --name $webappname --resource-group $resourcegroupname --settings REDISDEMO_CNSTRING=$redisCnStringTxn --subscription $subscription

