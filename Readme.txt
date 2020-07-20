
You were following this
https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-dotnet-how-to-use-azure-redis-cache



What does a Redis connection string look like?
----------------------------------------------
	Find out??

What was I doing?
-----------------
	Add to "How to run in-memory cache for local development?"
	Test ARM template with modified storage account identifier line
	You wrote code against IDistributedCache and verified with redis-cli.exe

	Create Azure function
		Get cache key
		Populate Cache (10000)
		Flush Cache
		Get item with specific key

	Deploy to Azure manually
		configure appinsights
		configure environment variables
	Automate deployment to Azure
		Create service plan ARM		
	Log some latencies to App insights
	Fix ARM to create service plan
	Use local.settings.json for config management
	How to delete a resource safely? e.g function app



	Lessons learn/Important steps during deployment
	---------------------------------

		How to access function app settings?
		-------------------------------------
		$junk=get-AzWebApp -ResourceGroupName $resourcegroupname -Name $webappname
		$junk.SiteConfig.AppSettings  #will give you a list of NameValue 

		How to set function app settings?
		----------------------------------
		Set-AzWebApp -ResourceGroupName $resourcegroupname -Name $webappname -AppSettings @{"name002"="value00222"; "name003"="value003"}
		Attention! You are going to erase all existing settings
		See example in https://stackoverflow.com/questions/55487426/azure-function-and-powershell-unable-to-set-connection-strings-with-set-azwebap

		When dropping App Service, also drop the App Service Plan (must). You cannot drop the plan if there is a cild App Service - so drop App Service

		How to create a App Service Plan programmatically?
		--------------------------------------------------
		New-AzAppServicePlan -ResourceGroupName "rg-dev-redis-demo" -Location "uksouth" -Tier "basic" -NumberofWorkers 2 -Name MyPowerShellAppServicePlan

		How to create a web app using Powershell?
		------------------------------------------
		New-AzWebApp -ResourceGroupName rg-dev-redis-demo -Location uksouth -AppServicePlan MyPowerShellAppServicePlan  -Name MyPowerShellWebApp 
		Note - this will not work for Function apps

		How to use a Github template URL to create plan+functionapp+appinsights?
		------------------------------------------------------------------------
		New-AzResourceGroupDeployment -TemplateUri https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/101-function-app-create-dynamic/azuredeploy.json -ResourceGroupName rg-githubtry1 -appname  GithubFuncApp -runtime dotnet
		Note - I had to change the runtime to "dotnet" from the default "node"

		Attention!While constructing the Reds connection string using Powershell
		-------------------------------------------------------------------------
		Use the SslPort and not regular Port otherwise you are going to get connection refused

		Attention! Add Singleton and not Transient for performance
		-----------------------------------------------------------
		You do not want to re-create the connection multiplexer for every HTTP request. Creating Redis each time is expensive

		How to set config parameters?
		-----------------------------
		Use the Az command line option "functionapp" and "config" and "appsettings"
		az functionapp config appsettings set --name $webappname --resource-group $resourcegroupname --settings REDISDEMO_CNSTRING=$redisCnStringTxn --subscription $subscription
		Attention!The --subscription parameter is important

		How to Create the zip for Azure?
		--------------------------------
		7za.exe [path to new zip file] [source folder]\*
		The ZIP must NOT contain an outer folder. It must begin at host.json

		How to invoke the REST end point via Powershell?
		-------------------------------------------------
		$url="https://redis-demo-webapp-001.azurewebsites.net/api/benchmark/?iterations=5000&allocations=10000&payload=500&readweight=95&writeweight=5"
		$url2="https://redis-demo-webapp-001.azurewebsites.net/api/benchmark/?iterations=20000&allocations=10000&payload=2500&readweight=95&writeweight=5"
		$r3=Invoke-WebRequest -Uri $url2 -Method Post 
		$r3.Content 
		$r3.Content | Out-File -FilePath .\results.r3.txt

		How to flush all keys with a specific prefix? e.g. customer_
		------------------------------------------------------------
		You cannot. You can delete all keys using FLUSHALL


	Find out/Next steps
	-------------------
	*Finish the article
	*Write PowerShell script to gather statistics
	*Do a sense check using Redis benchmark tool if it matches C#
	

	Good links
	----------
	https://stackexchange.github.io/StackExchange.Redis/Basics.html (Redis connections, servers)
	https://docs.microsoft.com/en-us/azure/azure-functions/functions-infrastructure-as-code (ARM template properties for function apps)

	

	