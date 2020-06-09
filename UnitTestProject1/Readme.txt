
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
	How to configure ENV variable post deployment?



	Lessons learn/Important steps during deployment
	---------------------------------
		You configured the following
		INSTRUMENTATION_KEY
		REDISDEMO_CNSTRING_ADMIN (remember to add allowAdmin=true preceded by a comma)
		REDISDEMO_CNSTRING_TRANSACTIONS
		"kind": "functionapp", is very important in the function app template (use template from portal)

	Find out
	--------
		How to flush all keys with a specific prefix? e.g. customer_
		Good link on deploying Function Apps through ARM template
			https://docs.microsoft.com/en-us/azure/azure-functions/functions-infrastructure-as-code
			All essential parameters are defined
		See the following properties 
			computeMode
			sku:name lets you decide the Plan
	

	Good links
	----------
	https://stackexchange.github.io/StackExchange.Redis/Basics.html (Redis connections, servers)
	https://docs.microsoft.com/en-us/azure/azure-functions/functions-infrastructure-as-code (ARM template properties for function apps)

	