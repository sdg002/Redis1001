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
"Going to use subscription:{0}" -f $subscription
#
#Create resource group
#
New-AzResourceGroup -Name $resourcegroupname -Location $location -Force -Verbose
"Created resource group:{0}" -f $resourcegroupname
#
#Create storage account
#
"Creating storage account"
New-AzResourceGroupDeployment -ResourceGroupName $resourcegroupname -TemplateFile $scriptfolder\arm-storageaccount\template.json -TemplateParameterFile $scriptfolder\arm-storageaccount\parameters.json -location $location -storageAccountName $storageaccountname -Verbose
$oStorageAccount=Get-AzResource -ResourceGroupName $resourcegroupname -Name $storageaccountname
"Storage account created:{0}" -f $oStorageAccount.Name
#
#Create Plan+FunctionApp+AppInsights
#
"Deploying ARM template 101-function-app-create-dynamic" 
$uriTemplate="https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/101-function-app-create-dynamic/azuredeploy.json"
New-AzResourceGroupDeployment -TemplateUri $uriTemplate -ResourceGroupName $resourcegroupname -appname  $webappname -runtime dotnet
"Function app+plan created"
#
#Create Redis cache
#
"Creating Redis Cache"
New-AzResourceGroupDeployment -ResourceGroupName $resourcegroupname -TemplateFile $scriptfolder\arm-redis\template.json -TemplateParameterFile $scriptfolder\arm-redis\parameters.json -redisCacheName $rediscache -redisCacheSKU "Basic" -existingDiagnosticsStorageAccountId $oStorageAccount.ResourceId  -Verbose
"Redis cache complete"
#
#Get connection parameters for the newly created Redis instance
#
$redisConfig=Get-AzRedisCache -name $rediscache
$redisKeys=Get-AzRedisCacheKey -Name $rediscache
"Host name {0}" -f $redisConfig.hostname
"Port number {0}" -f $redisConfig.port
"SSL Port number {0}" -f $redisConfig.SslPort
"Key {0}" -f $redisKeys.PrimaryKey
$redisCnStringTxn="{0}:{1},password={2},ssl=True,abortConnect=False" -f $redisConfig.hostname,$redisConfig.SslPort,$redisKeys.PrimaryKey
#
#Update redis cn string
#
"Updating AppSetting in function app with Redis connection string"
az functionapp config appsettings set --name $webappname --resource-group $resourcegroupname --settings REDISDEMO_CNSTRING=$redisCnStringTxn --subscription $subscription
"Appsetting of function app updated"

