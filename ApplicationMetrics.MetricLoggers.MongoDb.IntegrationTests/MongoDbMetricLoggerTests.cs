/*
 * Copyright 2026 Alastair Wyse (https://github.com/alastairwyse/ApplicationMetrics.MetricLoggers.MongoDb/)
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using MongoDB.Bson;
using MongoDB.Driver;
using ApplicationMetrics.MetricLoggers.MongoDb.Models.Documents;
using ApplicationMetrics.MetricLoggers.MongoDb.UnitTests;
using StandardAbstraction;
using EphemeralMongo;
using NUnit.Framework;
using NSubstitute;

namespace ApplicationMetrics.MetricLoggers.MongoDb.IntegrationTests
{
    /// <summary>
    /// Integration tests for the ApplicationMetrics.MetricLoggers.MongoDb.MongoDbMetricLogger class.
    /// </summary>
    public class MongoDbMetricLoggerTests
    {
        protected const String countMetricInstancesCollectionName = "CountMetricInstances";
        protected const String amountMetricInstancesCollectionName = "AmountMetricInstances";
        protected const String statusMetricInstancesCollectionName = "StatusMetricInstances";
        protected const String intervalMetricInstancesCollectionName = "IntervalMetricInstances";
        protected const String testMetricCategory = "TestCategory";

        private IBufferProcessingStrategy mockBufferProcessingStrategy;
        private IDateTime mockDateTimeProvider;
        private IStopwatch mockStopwatch;
        private IGuidProvider mockGuidProvider;
        private IMongoRunner mongoRunner;
        private IMongoClient mongoClient;
        private IMongoDatabase mongoDatabase;
        private IMongoCollection<CountMetricInstancesDocument> countMetricInstancesCollection;
        private IMongoCollection<AmountMetricInstancesDocument> amountMetricInstancesCollection;
        private IMongoCollection<StatusMetricInstancesDocument> statusMetricInstancesCollection;
        private IMongoCollection<IntervalMetricInstancesDocument> intervalMetricInstancesCollection;
        private MongoDbMetricLoggerWithProtectedMembers testMongoDbMetricLogger;

        [OneTimeSetUp]
        protected void OneTimeSetUp()
        {
            mongoRunner = MongoRunner.Run();
            mongoClient = new MongoClient(mongoRunner.ConnectionString);
            mongoDatabase = mongoClient.GetDatabase("ApplicationMetrics");
        }

        [SetUp]
        protected void SetUp()
        {
            mockBufferProcessingStrategy = Substitute.For<IBufferProcessingStrategy>();
            mockDateTimeProvider = Substitute.For<IDateTime>();
            mockStopwatch = Substitute.For<IStopwatch>();
            mockStopwatch.Frequency.Returns<Int64>(10000000);
            mockGuidProvider = Substitute.For<IGuidProvider>();
            mongoDatabase.DropCollection(countMetricInstancesCollectionName);
            mongoDatabase.DropCollection(amountMetricInstancesCollectionName);
            mongoDatabase.DropCollection(statusMetricInstancesCollectionName);
            mongoDatabase.DropCollection(intervalMetricInstancesCollectionName);
            countMetricInstancesCollection = mongoDatabase.GetCollection<CountMetricInstancesDocument>(countMetricInstancesCollectionName);
            amountMetricInstancesCollection = mongoDatabase.GetCollection<AmountMetricInstancesDocument>(amountMetricInstancesCollectionName);
            statusMetricInstancesCollection = mongoDatabase.GetCollection<StatusMetricInstancesDocument>(statusMetricInstancesCollectionName);
            intervalMetricInstancesCollection = mongoDatabase.GetCollection<IntervalMetricInstancesDocument>(intervalMetricInstancesCollectionName);
            testMongoDbMetricLogger = new MongoDbMetricLoggerWithProtectedMembers
            (
                testMetricCategory,
                mongoRunner.ConnectionString,
                "ApplicationMetrics",
                mockBufferProcessingStrategy,
                IntervalMetricBaseTimeUnit.Nanosecond,
                true, 
                mockDateTimeProvider, 
                mockStopwatch, 
                mockGuidProvider
            );
        }

        [TearDown]
        public void TearDown()
        {
            testMongoDbMetricLogger.Dispose();
        }

        [OneTimeTearDown]
        protected void OneTimeTearDown()
        {
            mongoClient.Dispose();
            mongoRunner.Dispose();
        }

        [Test]
        public void CreateCollectionsAndIndexes()
        {
            mongoDatabase.DropCollection(countMetricInstancesCollectionName);
            mongoDatabase.DropCollection(amountMetricInstancesCollectionName);
            mongoDatabase.DropCollection(statusMetricInstancesCollectionName);
            mongoDatabase.DropCollection(intervalMetricInstancesCollectionName);
            System.DateTime testStartTime = CreateDataTimeFromString("2026-01-16 10:57:10.0000000");
            mockDateTimeProvider.UtcNow.Returns(testStartTime);
            mockStopwatch.ElapsedTicks.Returns
            (
                5000000L
            );
            testMongoDbMetricLogger.Start();
            testMongoDbMetricLogger.Increment(new DiskReadOperation());
            
            testMongoDbMetricLogger.DequeueAndProcessMetricEvents();

            List<String> collectionNames = mongoDatabase.ListCollectionNames().ToList();
            Assert.AreEqual(true, collectionNames.Contains(countMetricInstancesCollectionName));
            Assert.AreEqual(true, collectionNames.Contains(amountMetricInstancesCollectionName));
            Assert.AreEqual(true, collectionNames.Contains(statusMetricInstancesCollectionName));
            Assert.AreEqual(true, collectionNames.Contains(intervalMetricInstancesCollectionName));
            countMetricInstancesCollection = mongoDatabase.GetCollection<CountMetricInstancesDocument>(countMetricInstancesCollectionName);
            amountMetricInstancesCollection = mongoDatabase.GetCollection<AmountMetricInstancesDocument>(amountMetricInstancesCollectionName);
            statusMetricInstancesCollection = mongoDatabase.GetCollection<StatusMetricInstancesDocument>(statusMetricInstancesCollectionName);
            intervalMetricInstancesCollection = mongoDatabase.GetCollection<IntervalMetricInstancesDocument>(intervalMetricInstancesCollectionName);
            List<BsonDocument> countMetricInstancesIndexes = countMetricInstancesCollection.Indexes.List().ToList();
            Assert.AreEqual(4, countMetricInstancesIndexes.Count);
            List<BsonDocument> amountMetricInstancesIndexes = amountMetricInstancesCollection.Indexes.List().ToList();
            Assert.AreEqual(4, amountMetricInstancesIndexes.Count);
            List<BsonDocument> statusMetricInstancesIndexes = statusMetricInstancesCollection.Indexes.List().ToList();
            Assert.AreEqual(4, statusMetricInstancesIndexes.Count);
            List<BsonDocument> intervalMetricInstancesIndexes = intervalMetricInstancesCollection.Indexes.List().ToList();
            Assert.AreEqual(4, intervalMetricInstancesIndexes.Count);
        }

        [Test]
        public void ProcessCountMetricEvents()
        {
            System.DateTime testStartTime = CreateDataTimeFromString("2026-01-16 10:57:10.0000000");
            mockDateTimeProvider.UtcNow.Returns(testStartTime);
            mockStopwatch.ElapsedTicks.Returns
            (
                5000001L,
                ConvertMilliseondsToTicks(750),
                ConvertMilliseondsToTicks(1333)
            );
            testMongoDbMetricLogger.Start();
            testMongoDbMetricLogger.Increment(new DiskReadOperation());
            testMongoDbMetricLogger.Increment(new MessageReceived());
            testMongoDbMetricLogger.Increment(new DiskReadOperation());

            testMongoDbMetricLogger.DequeueAndProcessMetricEvents();

            List<CountMetricInstancesDocument> allDocuments = countMetricInstancesCollection.Find(FilterDefinition<CountMetricInstancesDocument>.Empty)
                .SortBy(document => document.EventTimeTicks)
                .ToList();
            Assert.AreEqual(3, allDocuments.Count);
            Assert.AreEqual(testMetricCategory, allDocuments[0].Category);
            Assert.AreEqual(new DiskReadOperation().Name, allDocuments[0].CountMetric);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-16 10:57:10.5000000"), allDocuments[0].EventTime);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-16 10:57:10.5000001"), allDocuments[0].EventTimeTicks);
            Assert.AreEqual(testMetricCategory, allDocuments[1].Category);
            Assert.AreEqual(new MessageReceived().Name, allDocuments[1].CountMetric);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-16 10:57:10.7500000"), allDocuments[1].EventTime);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-16 10:57:10.7500000"), allDocuments[1].EventTimeTicks);
            Assert.AreEqual(testMetricCategory, allDocuments[2].Category);
            Assert.AreEqual(new DiskReadOperation().Name, allDocuments[2].CountMetric);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-16 10:57:11.3330000"), allDocuments[2].EventTime);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-16 10:57:11.3330000"), allDocuments[2].EventTimeTicks);
        }

        [Test]
        public void ProcessAmountMetricEvents()
        {
            System.DateTime testStartTime = CreateDataTimeFromString("2026-01-22 00:03:13.0000000");
            mockDateTimeProvider.UtcNow.Returns(testStartTime);
            mockStopwatch.ElapsedTicks.Returns
            (
                6000001L,
                ConvertMilliseondsToTicks(950),
                ConvertMilliseondsToTicks(1666)
            );
            testMongoDbMetricLogger.Start();
            testMongoDbMetricLogger.Add(new DiskBytesRead(), 2000);
            testMongoDbMetricLogger.Add(new MessageSize(), 123);
            testMongoDbMetricLogger.Add(new DiskBytesRead(), 5000);

            testMongoDbMetricLogger.DequeueAndProcessMetricEvents();

            List<AmountMetricInstancesDocument> allDocuments = amountMetricInstancesCollection.Find(FilterDefinition<AmountMetricInstancesDocument>.Empty)
                .SortBy(document => document.EventTimeTicks)
                .ToList();
            Assert.AreEqual(3, allDocuments.Count);
            Assert.AreEqual(testMetricCategory, allDocuments[0].Category);
            Assert.AreEqual(new DiskBytesRead().Name, allDocuments[0].AmountMetric);
            Assert.AreEqual(2000, allDocuments[0].Amount);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-22 00:03:13.6000000"), allDocuments[0].EventTime);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-22 00:03:13.6000001"), allDocuments[0].EventTimeTicks);
            Assert.AreEqual(testMetricCategory, allDocuments[1].Category);
            Assert.AreEqual(123, allDocuments[1].Amount);
            Assert.AreEqual(new MessageSize().Name, allDocuments[1].AmountMetric);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-22 00:03:13.9500000"), allDocuments[1].EventTime);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-22 00:03:13.9500000"), allDocuments[1].EventTimeTicks);
            Assert.AreEqual(testMetricCategory, allDocuments[2].Category);
            Assert.AreEqual(5000, allDocuments[2].Amount);
            Assert.AreEqual(new DiskBytesRead().Name, allDocuments[2].AmountMetric);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-22 00:03:14.6660000"), allDocuments[2].EventTime);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-22 00:03:14.6660000"), allDocuments[2].EventTimeTicks);
        }

        [Test]
        public void ProcessStatusMetricEvents()
        {
            System.DateTime testStartTime = CreateDataTimeFromString("2026-01-22 22:04:29.0000000");
            mockDateTimeProvider.UtcNow.Returns(testStartTime);
            mockStopwatch.ElapsedTicks.Returns
            (
                2000123L,
                ConvertMilliseondsToTicks(499),
                ConvertMilliseondsToTicks(1001)
            );
            testMongoDbMetricLogger.Start();
            testMongoDbMetricLogger.Set(new AvailableMemory(), 1024);
            testMongoDbMetricLogger.Set(new ActiveWorkerThreads(), 8);
            testMongoDbMetricLogger.Set(new AvailableMemory(), 2048);

            testMongoDbMetricLogger.DequeueAndProcessMetricEvents();

            List<StatusMetricInstancesDocument> allDocuments = statusMetricInstancesCollection.Find(FilterDefinition<StatusMetricInstancesDocument>.Empty)
                .SortBy(document => document.EventTimeTicks)
                .ToList();
            Assert.AreEqual(3, allDocuments.Count);
            Assert.AreEqual(testMetricCategory, allDocuments[0].Category);
            Assert.AreEqual(new AvailableMemory().Name, allDocuments[0].StatusMetric);
            Assert.AreEqual(1024, allDocuments[0].Value);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-22 22:04:29.2000000"), allDocuments[0].EventTime);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-22 22:04:29.2000123"), allDocuments[0].EventTimeTicks);
            Assert.AreEqual(testMetricCategory, allDocuments[1].Category);
            Assert.AreEqual(8, allDocuments[1].Value);
            Assert.AreEqual(new ActiveWorkerThreads().Name, allDocuments[1].StatusMetric);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-22 22:04:29.4990000"), allDocuments[1].EventTime);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-22 22:04:29.4990000"), allDocuments[1].EventTimeTicks);
            Assert.AreEqual(testMetricCategory, allDocuments[2].Category);
            Assert.AreEqual(2048, allDocuments[2].Value);
            Assert.AreEqual(new AvailableMemory().Name, allDocuments[2].StatusMetric);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-22 22:04:30.0010000"), allDocuments[2].EventTime);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-22 22:04:30.0010000"), allDocuments[2].EventTimeTicks);
        }

        [Test]
        public void ProcessIntervalMetricEvents()
        {
            System.DateTime testStartTime = CreateDataTimeFromString("2026-01-22 22:12:32.0000000");
            mockDateTimeProvider.UtcNow.Returns(testStartTime);
            mockGuidProvider.NewGuid().Returns
            (
                Guid.Parse("00000000-0000-0000-0000-000000000000"),
                Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Guid.Parse("00000000-0000-0000-0000-000000000002")
            );
            mockStopwatch.ElapsedTicks.Returns
            (
                0000001L,
                0000002L,
                0000003L,
                0000005L,
                0000007L,
                00000011L
            );
            testMongoDbMetricLogger.Start();
            Guid diskReadTime1BeginId = testMongoDbMetricLogger.Begin(new DiskReadTime());
            Guid diskReadTime2BeginId = testMongoDbMetricLogger.Begin(new DiskReadTime());
            Guid messageReceiveTimeBeginId = testMongoDbMetricLogger.Begin(new MessageReceiveTime());
            testMongoDbMetricLogger.End(diskReadTime1BeginId, new DiskReadTime());
            testMongoDbMetricLogger.End(diskReadTime2BeginId, new DiskReadTime());
            testMongoDbMetricLogger.End(messageReceiveTimeBeginId, new MessageReceiveTime());

            testMongoDbMetricLogger.DequeueAndProcessMetricEvents();

            List<IntervalMetricInstancesDocument> allDocuments = intervalMetricInstancesCollection.Find(FilterDefinition<IntervalMetricInstancesDocument>.Empty)
                .SortBy(document => document.EventTimeTicks)
                .ToList();
            Assert.AreEqual(3, allDocuments.Count);
            Assert.AreEqual(testMetricCategory, allDocuments[0].Category);
            Assert.AreEqual(new DiskReadTime().Name, allDocuments[0].IntervalMetric);
            Assert.AreEqual(400, allDocuments[0].Duration);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-22 22:12:32.0000000"), allDocuments[0].EventTime);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-22 22:12:32.0000001"), allDocuments[0].EventTimeTicks);
            Assert.AreEqual(testMetricCategory, allDocuments[1].Category);
            Assert.AreEqual(500, allDocuments[1].Duration);
            Assert.AreEqual(new DiskReadTime().Name, allDocuments[1].IntervalMetric);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-22 22:12:32.0000000"), allDocuments[1].EventTime);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-22 22:12:32.0000002"), allDocuments[1].EventTimeTicks);
            Assert.AreEqual(testMetricCategory, allDocuments[2].Category);
            Assert.AreEqual(800, allDocuments[2].Duration);
            Assert.AreEqual(new MessageReceiveTime().Name, allDocuments[2].IntervalMetric);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-22 22:12:32.0000000"), allDocuments[2].EventTime);
            Assert.AreEqual(CreateDataTimeFromString("2026-01-22 22:12:32.0000003"), allDocuments[2].EventTimeTicks);
        }

        [Test]
        public void DequeueAndProcessMetricEvents_NonTestConstructor()
        {
            testMongoDbMetricLogger.Dispose();
            testMongoDbMetricLogger = new MongoDbMetricLoggerWithProtectedMembers
            (
                testMetricCategory,
                mongoRunner.ConnectionString,
                "ApplicationMetrics",
                mockBufferProcessingStrategy,
                IntervalMetricBaseTimeUnit.Nanosecond,
                true
            );
            Guid diskReadTimeBeginId = testMongoDbMetricLogger.Begin(new DiskReadTime());
            testMongoDbMetricLogger.Increment(new DiskReadOperation());
            testMongoDbMetricLogger.Add(new DiskBytesRead(), 2000);
            testMongoDbMetricLogger.Set(new AvailableMemory(), 1024);
            testMongoDbMetricLogger.End(diskReadTimeBeginId, new DiskReadTime());

            testMongoDbMetricLogger.DequeueAndProcessMetricEvents();

            List<CountMetricInstancesDocument> allCountMetricDocuments = countMetricInstancesCollection.Find(FilterDefinition<CountMetricInstancesDocument>.Empty)
                .SortBy(document => document.EventTimeTicks)
                .ToList();
            Assert.AreEqual(1, allCountMetricDocuments.Count);
            Assert.AreEqual(testMetricCategory, allCountMetricDocuments[0].Category);
            Assert.AreEqual(new DiskReadOperation().Name, allCountMetricDocuments[0].CountMetric);
            List<AmountMetricInstancesDocument> allAmountMetricDocuments = amountMetricInstancesCollection.Find(FilterDefinition<AmountMetricInstancesDocument>.Empty)
                .SortBy(document => document.EventTimeTicks)
                .ToList();
            Assert.AreEqual(1, allAmountMetricDocuments.Count);
            Assert.AreEqual(testMetricCategory, allAmountMetricDocuments[0].Category);
            Assert.AreEqual(new DiskBytesRead().Name, allAmountMetricDocuments[0].AmountMetric);
            Assert.AreEqual(2000, allAmountMetricDocuments[0].Amount);
            List<StatusMetricInstancesDocument> allStatusMetricDocuments = statusMetricInstancesCollection.Find(FilterDefinition<StatusMetricInstancesDocument>.Empty)
                .SortBy(document => document.EventTimeTicks)
                .ToList();
            Assert.AreEqual(1, allStatusMetricDocuments.Count);
            Assert.AreEqual(testMetricCategory, allStatusMetricDocuments[0].Category);
            Assert.AreEqual(new AvailableMemory().Name, allStatusMetricDocuments[0].StatusMetric);
            Assert.AreEqual(1024, allStatusMetricDocuments[0].Value);
            List<IntervalMetricInstancesDocument> allIntervalMetricDocuments = intervalMetricInstancesCollection.Find(FilterDefinition<IntervalMetricInstancesDocument>.Empty)
                .SortBy(document => document.EventTimeTicks)
                .ToList();
            Assert.AreEqual(1, allIntervalMetricDocuments.Count);
            Assert.AreEqual(testMetricCategory, allIntervalMetricDocuments[0].Category);
            Assert.AreEqual(new DiskReadTime().Name, allIntervalMetricDocuments[0].IntervalMetric);
        }
        
        #region Private/Protected Methods

        /// <summary>
        /// Creates a DateTime from the specified yyyy-MM-dd HH:mm:ss.fffffff format string.
        /// </summary>
        /// <param name="stringifiedDateTime">The stringified date/time to convert.</param>
        /// <returns>A DateTime.</returns>
        protected System.DateTime CreateDataTimeFromString(String stringifiedDateTime)
        {
            System.DateTime returnDateTime = System.DateTime.ParseExact(stringifiedDateTime, "yyyy-MM-dd HH:mm:ss.fffffff", DateTimeFormatInfo.InvariantInfo);

            return System.DateTime.SpecifyKind(returnDateTime, DateTimeKind.Utc);
        }

        /// <summary>
        /// Converts the specified number of milliseonds to ticks.
        /// </summary>
        /// <param name="millisecondValue">The millisecond value to convert.</param>
        /// <returns>The millisecond value in ticks.</returns>
        private Int32 ConvertMilliseondsToTicks(Int32 millisecondValue)
        {
            return millisecondValue * 10000;
        }

        #endregion
    }
}
