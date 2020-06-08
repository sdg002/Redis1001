
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

	Important steps during deployment
	---------------------------------
		You configured the following
		INSTRUMENTATION_KEY
		REDISDEMO_CNSTRING_ADMIN (remember to add allowAdmin=true preceded by a comma)
		REDISDEMO_CNSTRING_TRANSACTIONS

	Find out
	--------
		How to flush all keys with a specific prefix? e.g. customer_
		