
# About
to be done , mention about scope of this arcticle
mention about advanced features of Redis which are not in this scope

# What is a distributed cache?
to be done.
Talk about the needs for a  cache
Scalability
Database bottle necks
Repeated read and round trips

<<a picture would be good, show some web servers, a DB, a memory cache>>
Look at Wikipedia for definitions

# What is not a distributed cache?
to be done

Talk about earliest versions of caching (ASP.NET)


# Example application scenarios where a distributed cache helps
# Scenarios where caching is not always helpful
to be done , repetitive access of the same data items.

# A bief history of caching in the Microsoft world
to be done
- ASP.NET Cache
- Session state server
- Microsoft Appfabric
- Third party products like NCache, Memcached and Redis

## ASP.NET Framework Cache
to be done (talk about serialization, page caching, session state caching)

## Memcached
to be done

## Microsoft AppFabric
to be done

## Redis
to be done

# Using Redis for local development
to be done

## Installing Redis locally on Windows
- Where to install Windows binaries?
Redis server sets up a TCP/IP server.
The Redis server can be run as follows
- Open a CMD prompt
- Navigate to the folder where the redis-server.exe was extracted
- You should see a file redis.conf in this folder
- The port number is specified in this (Usually 6379)
- Launch redis server with the following command

```
c:\redis-server
```
The log messages on the console will help affirming the actual port number

```
[36388] 31 May 22:07:17 * The server is now ready to accept connections on port 6379
```

## Redis command line client
### How to launch?

```
redis-cli.exe -h localhost -p 6379
```

### List all keys
```
 redis localhost:6379> keys * 

39) "foo:rand:000000000016"
40) "foo:rand:000000000017"
41) "foo:rand:000000000018"
42) "foo:rand:000000000019"
43) "mykey-f9d21bca-48f6-4cf8-b68d-1652b9533633"
44) "mykey-43d5b08a-5b60-4c72-8820-dcb6f6372b4b"
```

### Delete a specific key

```
redis localhost:6379> del mykey-0001 
```
### Delete all keys

```
redis localhost:6379> flushall
```

## C# client application  - Programming with StackExchange general purpose client

```
Install-Package StackExchange.Redis
```

## C# client application  - Programming with IDistributedCache

## Dependency injection


## Is it compulsory to intall Redis server locally?

# Getting started with Redis on Azure
## Creating a Redis cache using the Portal
## Creating a Redis cache using PowerShell
## Connection string

# Benchmarking Redis performance
TODO Talk about redis-benchmark.exe tool


# Further reading

- Pluralsight video (to be done)
- <a href="https://www.youtube.com/watch?v=UH7wkvcf0ys">How Facebook uses Memcached(By Mark Zuckerberg) ?</a>
- <a href="https://www.youtube.com/watch?v=Rzdxgx3RC0Q&t=200s">How Netflix uses caching?</a>
## Others

# Advanced features of Redis not discussed here
to be done. A short bulleted list


# Misc
Managing Redis using PowerShell
https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-how-to-manage-redis-cache-powershell
Caveat about Premium edition

How to reboot Redis? Reset-AzRedisCache
https://stackoverflow.com/questions/45978130/clearing-azure-redis-cache-using-powershell-during-deployment

# How to flush the cache using CI/CD Azure Devops?
You are releasing a new build and you want to clear any items that are held in the cache.
See Redis Cache Utils Task on Azure Devops
https://marketplace.visualstudio.com/items?itemName=gbnz.redis-cache-clear&targetId=545b42c4-2c3c-4eef-bbeb-ca8970eab77e


# Redis port on Microsoft Windows
https://github.com/MicrosoftArchive/redis/
Look for the Releases section

