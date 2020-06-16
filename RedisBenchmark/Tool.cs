using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RedisBenchmark
{
    public class Tool
    {
        private readonly IDistributedCache _cache;
        private readonly Random _random;
        private List<ObjectAllocation> _objectAllocations;
        private List<TestOperation> _cacheOperations;

        public Tool(
            IDistributedCache cache,
            int readsWeight, int writesWeight,
            ILogger<Tool> logger,
            int iterations,
            int countOfObjectAllocations,
            int sizeOfPayloadBytes
            )
        {
            this.SizeOfPayload = sizeOfPayloadBytes;
            this.Iterations = iterations;
            this.WeightedReads = readsWeight;
            this.WeightedWrites = writesWeight;
            this.ObjectsAllocated = countOfObjectAllocations;
            _random = new Random(DateTime.Now.Second);
            _cache = cache;
        }

        /// <summary>
        /// Gets the size of every object which will be used for the Read/Write benchmarking tests
        /// </summary>
        public int SizeOfPayload { get; }

        /// <summary>
        /// Total count of iterations used for the benchmarking tests
        /// </summary>
        public int Iterations { get; }

        public int WeightedReads { get; }
        public int WeightedWrites { get; }
        public int ObjectsAllocated { get; }

        public Result Run()
        {
            AllocateObjectsAndSeedCache();
            CreateWeightedSetOfCacheOperations();
            Result result = new Result();
            int iterations = 0;
            Action<TestOperation> fnCacheOperation = delegate (TestOperation op)
            {
                Interlocked.Increment(ref iterations);
                Stopwatch sw = new Stopwatch();
                sw.Start();
                if (op.OperationType == CacheOperationType.Read)
                {
                    _cache.Get(op.TestData.CacheKey);
                }
                else
                {
                    _cache.Set(op.TestData.CacheKey, op.TestData.Payload);
                }
                sw.Stop();
                op.TimeTaken = sw.ElapsedMilliseconds;
            };
            Parallel.ForEach(_cacheOperations, fnCacheOperation);
            result.Iterations = iterations;
            result.TotalReads = _cacheOperations.Count(d => d.OperationType == CacheOperationType.Read);
            result.TotalWrites = _cacheOperations.Count(d => d.OperationType == CacheOperationType.Write);
            result.MeanLatency = _cacheOperations.Average(d => d.TimeTaken);
            result.NinetyFivePercentLatency = Compute95Percentile(_cacheOperations.Select(c => c.TimeTaken));
            return result;
        }

        private double Compute95Percentile(IEnumerable<long> enumerable)
        {
            //TODO do this
            return 0.0;//TODO to be done
        }

        private void CreateWeightedSetOfCacheOperations()
        {
            _cacheOperations = new List<TestOperation>();
            for (int i = 0; i < this.Iterations; i++)
            {
                var cacheOpType = RandomlyGenerateOperationType();
                var op = new TestOperation
                {
                    OperationType = cacheOpType,
                    TestData = PickRandomPayload()
                };
                _cacheOperations.Add(op);
            }
        }

        private ObjectAllocation PickRandomPayload()
        {
            int randomIndex = _random.Next(_objectAllocations.Count);
            return _objectAllocations[randomIndex];
        }

        private void AllocateObjectsAndSeedCache()
        {
            _objectAllocations = new List<ObjectAllocation>();
            for (int i = 0; i < this.ObjectsAllocated; i++)
            {
                var data = new ObjectAllocation();
                data.Payload = GeneratePayloadWithSize(this.SizeOfPayload);
                data.CacheKey = Guid.NewGuid().ToString();
                _cache.Set(data.CacheKey, data.Payload);
                _objectAllocations.Add(data);
            }
        }

        private byte[] GeneratePayloadWithSize(int sizeOfPayload)
        {
            //TODO do this
            return new byte[] { 1, 2, 3, 4, 5 };
        }

        private CacheOperationType RandomlyGenerateOperationType()
        {
            int max = this.WeightedReads + this.WeightedWrites;
            int weightedThrowOfDice = _random.Next(max);
            if (weightedThrowOfDice > this.WeightedReads)
            {
                return CacheOperationType.Write;
            }
            else
            {
                return CacheOperationType.Read;
            }
        }

        /// <summary>
        /// Represents a Cache operatin to be carried out on one of the allocated objects
        /// </summary>
        private class TestOperation
        {
            public CacheOperationType OperationType { get; set; }
            public ObjectAllocation TestData { get; set; }

            /// <summary>
            /// Records the time taken to complete the operation(ms)
            /// </summary>
            public long TimeTaken { get; internal set; }
        }

        /// <summary>
        /// Represents an unique object which will be added to the cache with an unique key
        /// </summary>
        private class ObjectAllocation
        {
            public ObjectAllocation()
            {
                IsComplete = false;
            }

            public byte[] Payload { get; internal set; }
            public string CacheKey { get; internal set; }

            /// <summary>
            /// Set to True when this cache operation has been carried out
            /// </summary>
            public bool IsComplete { get; set; }
        }

        private enum CacheOperationType
        {
            Read,
            Write
        }
    }
}