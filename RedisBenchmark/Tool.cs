using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleToAttribute("UnitTestProject1")]

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
            Stopwatch swOuter = new Stopwatch();
            swOuter.Start();
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
            swOuter.Stop();
            result.TotalTime = swOuter.ElapsedMilliseconds;
            result.Iterations = iterations;
            result.Allocations = this.ObjectsAllocated;
            result.TotalReads = _cacheOperations.Count(d => d.OperationType == CacheOperationType.Read);
            result.TotalWrites = _cacheOperations.Count(d => d.OperationType == CacheOperationType.Write);

            var readOperations = _cacheOperations
                .Where(d => d.OperationType == CacheOperationType.Read)
                .ToArray();

            result.ReadLatency = CreateLatencyResult(readOperations);

            var writeOperations = _cacheOperations
                .Where(d => d.OperationType == CacheOperationType.Write)
                .ToArray();

            result.WriteLatency = CreateLatencyResult(writeOperations);

            return result;
        }

        private Latency CreateLatencyResult(IEnumerable<TestOperation> writeOperations)
        {
            var latencyResult = new Latency();
            double[] timings = writeOperations
                .Select(op => Convert.ToDouble(op.TimeTaken))
                .ToArray();
            latencyResult.AveragePayloadSize = (int)writeOperations
                .Select(op => op.TestData.Payload.Length)
                .Average();
            latencyResult.Count = timings.Length;
            latencyResult.MaxLatency = timings.Max();
            latencyResult.MinLatency = timings.Min();
            latencyResult.MeanLatency = timings.Average();
            latencyResult.NinetyFiveReadPercentLatency = Compute95Percentile(timings);
            return latencyResult;
        }

        internal T Compute95Percentile<T>(IEnumerable<T> enumerable)
        {
            var sorted = enumerable.OrderBy(o => o).ToArray();
            int index95 = (int)Math.Round(sorted.Length * 0.95);
            var result = sorted[index95 - 1];
            return result;
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
            var buffer = new byte[sizeOfPayload];
            _random.NextBytes(buffer);
            return buffer;
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