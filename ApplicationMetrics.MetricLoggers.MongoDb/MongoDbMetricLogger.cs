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
using System.Collections;
using System.Collections.Generic;
using MongoDB.Driver;
using ApplicationMetrics.MetricLoggers;
using ApplicationMetrics.MetricLoggers.MongoDb.Models.Documents;
using StandardAbstraction;

namespace ApplicationMetrics.MetricLoggers.MongoDb
{
    /// <summary>
    /// Writes metric events to a MongoDB database.
    /// </summary>
    public class MongoDbMetricLogger : MetricLoggerBuffer
    {
        #pragma warning disable 1591

        protected const String countMetricInstancesCollectionName = "CountMetricInstances";
        protected const String amountMetricInstancesCollectionName = "AmountMetricInstances";
        protected const String statusMetricInstancesCollectionName = "StatusMetricInstances";
        protected const String intervalMetricInstancesCollectionName = "IntervalMetricInstances";

        #pragma warning restore 1591

        /// <summary>The category to log all metrics under.</summary>
        protected String category;
        /// <summary>The string to use to connect to MongoDB.</summary>
        protected String connectionString;
        /// <summary>The name of the database.</summary>
        protected String databaseName;
        /// <summary>The client to connect to MongoDB.</summary>
        protected IMongoClient mongoClient;
        /// <summary>The MongoDB database.</summary>
        protected IMongoDatabase database;
        /// <summary>Whether any of the Process*MetricEvents() methods have already been called.</summary>
        protected Boolean processMetricEventsMethodCalled;

        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MetricLoggers.MongoDb.MongoDbMetricLogger class.
        /// </summary>
        /// <param name="category">The category to log all metrics under.</param>
        /// <param name="connectionString">The string to use to connect to MongoDB.</param>
        /// <param name="bufferProcessingStrategy">Object which implements a processing strategy for the buffers (queues).</param>
        /// <param name="intervalMetricBaseTimeUnit">The base time unit to use to log interval metrics.</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).  Note that this parameter only has an effect when running in 'non-interleaved' mode.</param>
        public MongoDbMetricLogger
        (
            String category,
            String connectionString,
            IBufferProcessingStrategy bufferProcessingStrategy,
            IntervalMetricBaseTimeUnit intervalMetricBaseTimeUnit,
            Boolean intervalMetricChecking
        ) : this(category, connectionString, "ApplicationMetrics", bufferProcessingStrategy, intervalMetricBaseTimeUnit, intervalMetricChecking)
        {
        }

        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MetricLoggers.MongoDb.MongoDbMetricLogger class.
        /// </summary>
        /// <param name="category">The category to log all metrics under.</param>
        /// <param name="connectionString">The string to use to connect to MongoDB.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="bufferProcessingStrategy">Object which implements a processing strategy for the buffers (queues).</param>
        /// <param name="intervalMetricBaseTimeUnit">The base time unit to use to log interval metrics.</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).  Note that this parameter only has an effect when running in 'non-interleaved' mode.</param>
        public MongoDbMetricLogger
        (
            String category,
            String connectionString,
            String databaseName,
            IBufferProcessingStrategy bufferProcessingStrategy,
            IntervalMetricBaseTimeUnit intervalMetricBaseTimeUnit, 
            Boolean intervalMetricChecking
        ) : base(bufferProcessingStrategy, intervalMetricBaseTimeUnit, intervalMetricChecking)
        {
            ThrowExceptionIfStringParameterNullOrWhitespace(nameof(category), category);
            ThrowExceptionIfStringParameterNullOrWhitespace(nameof(connectionString), connectionString);
            ThrowExceptionIfStringParameterNullOrWhitespace(nameof(databaseName), databaseName);

            this.category = category;
            this.connectionString = connectionString;
            this.databaseName = databaseName;
            mongoClient = new MongoClient(this.connectionString);
            database = mongoClient.GetDatabase(databaseName);
            processMetricEventsMethodCalled = false;
        }

        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MetricLoggers.MongoDb.MongoDbMetricLogger class.
        /// </summary>
        /// <param name="category">The category to log all metrics under.</param>
        /// <param name="connectionString">The string to use to connect to MongoDB.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="bufferProcessingStrategy">Object which implements a processing strategy for the buffers (queues).</param>
        /// <param name="intervalMetricBaseTimeUnit">The base time unit to use to log interval metrics.</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).  Note that this parameter only has an effect when running in 'non-interleaved' mode.</param>
        /// <param name="dateTime">A test (mock) <see cref="IDateTime"/> object.</param>
        /// <param name="stopWatch">A test (mock) <see cref="IStopwatch"/> object.</param>
        /// <param name="guidProvider">A test (mock) <see cref="IGuidProvider"/> object.</param>
        /// <remarks>This constructor is included to facilitate unit testing.</remarks>
        public MongoDbMetricLogger
        (
            String category,
            String connectionString,
            String databaseName,
            IBufferProcessingStrategy bufferProcessingStrategy,
            IntervalMetricBaseTimeUnit intervalMetricBaseTimeUnit,
            Boolean intervalMetricChecking, 
            IDateTime dateTime, 
            IStopwatch stopWatch, 
            IGuidProvider guidProvider
        ) : base(bufferProcessingStrategy, intervalMetricBaseTimeUnit, intervalMetricChecking, dateTime, stopWatch, guidProvider)
        {
            ThrowExceptionIfStringParameterNullOrWhitespace(nameof(category), category);
            ThrowExceptionIfStringParameterNullOrWhitespace(nameof(connectionString), connectionString);
            ThrowExceptionIfStringParameterNullOrWhitespace(nameof(databaseName), databaseName);

            this.category = category;
            this.connectionString = connectionString;
            this.databaseName = databaseName;
            mongoClient = new MongoClient(this.connectionString);
            database = mongoClient.GetDatabase(databaseName);
            processMetricEventsMethodCalled = false;
        }

        #region Private/Protected Methods

        #region Base Class Abstract Method Implementations

        /// <summary>
        /// Writes logged count metric events to MongoDB.
        /// </summary>
        /// <param name="countMetricEvents">The count metric events to process.</param>
        protected override void ProcessCountMetricEvents(Queue<CountMetricEventInstance> countMetricEvents)
        {
            CreateCollectionsAndIndexes();

            IMongoCollection<CountMetricInstancesDocument> countMetricInstancesCollection = database.GetCollection<CountMetricInstancesDocument>(countMetricInstancesCollectionName);
            var countMetricInstancesDocuments = new List<CountMetricInstancesDocument>();
            foreach (CountMetricEventInstance currentEventInstance in countMetricEvents)
            {
                var newDocument = new CountMetricInstancesDocument
                (
                    category, 
                    currentEventInstance.EventTime, 
                    currentEventInstance.Metric.Name
                );
                countMetricInstancesDocuments.Add(newDocument);
            }
            if (countMetricInstancesDocuments.Count > 0)
            {
                try
                {
                    countMetricInstancesCollection.InsertMany(countMetricInstancesDocuments);
                }
                catch (Exception e)
                {
                    throw new Exception($"Failed to insert documents into collection '{countMetricInstancesCollectionName}'.", e);
                }
            }
        }

        /// <summary>
        /// Writes logged amount metric events to MongoDB.
        /// </summary>
        /// <param name="amountMetricEvents">The amount metric events to process.</param>
        protected override void ProcessAmountMetricEvents(Queue<AmountMetricEventInstance> amountMetricEvents)
        {
            CreateCollectionsAndIndexes();

            IMongoCollection<AmountMetricInstancesDocument> amountMetricInstancesCollection = database.GetCollection<AmountMetricInstancesDocument>(amountMetricInstancesCollectionName);
            var amountMetricInstancesDocuments = new List<AmountMetricInstancesDocument>();
            foreach (AmountMetricEventInstance currentEventInstance in amountMetricEvents)
            {
                var newDocument = new AmountMetricInstancesDocument
                (
                    category,
                    currentEventInstance.EventTime,
                    currentEventInstance.Metric.Name,
                    currentEventInstance.Amount
                );
                amountMetricInstancesDocuments.Add(newDocument);
            }
            if (amountMetricInstancesDocuments.Count > 0)
            {
                try
                {
                    amountMetricInstancesCollection.InsertMany(amountMetricInstancesDocuments);
                }
                catch (Exception e)
                {
                    throw new Exception($"Failed to insert documents into collection '{amountMetricInstancesCollectionName}'.", e);
                }
            }
        }

        /// <summary>
        /// Writes logged status metric events to MongoDB.
        /// </summary>
        /// <param name="statusMetricEvents">The status metric events to process.</param>
        protected override void ProcessStatusMetricEvents(Queue<StatusMetricEventInstance> statusMetricEvents)
        {
            CreateCollectionsAndIndexes();

            IMongoCollection<StatusMetricInstancesDocument> statusMetricInstancesCollection = database.GetCollection<StatusMetricInstancesDocument>(statusMetricInstancesCollectionName);
            var statusMetricInstancesDocuments = new List<StatusMetricInstancesDocument>();
            foreach (StatusMetricEventInstance currentEventInstance in statusMetricEvents)
            {
                var newDocument = new StatusMetricInstancesDocument
                (
                    category,
                    currentEventInstance.EventTime,
                    currentEventInstance.Metric.Name,
                    currentEventInstance.Value
                );
                statusMetricInstancesDocuments.Add(newDocument);
            }
            if (statusMetricInstancesDocuments.Count > 0)
            {
                try
                {
                    statusMetricInstancesCollection.InsertMany(statusMetricInstancesDocuments);
                }
                catch (Exception e)
                {
                    throw new Exception($"Failed to insert documents into collection '{statusMetricInstancesCollectionName}'.", e);
                }
            }
        }

        /// <summary>
        /// Writes logged interval metric events to MongoDB.
        /// </summary>
        /// <param name="intervalMetricEventsAndDurations">The interval metric events and corresponding durations of the events (in the specified base time unit) to process.</param>
        protected override void ProcessIntervalMetricEvents(Queue<Tuple<IntervalMetricEventInstance, Int64>> intervalMetricEventsAndDurations)
        {
            CreateCollectionsAndIndexes();

            IMongoCollection<IntervalMetricInstancesDocument> intervalMetricInstancesCollection = database.GetCollection<IntervalMetricInstancesDocument>(intervalMetricInstancesCollectionName);
            var intervalMetricInstancesDocuments = new List<IntervalMetricInstancesDocument>();
            foreach (Tuple<IntervalMetricEventInstance, Int64> currentIntervalMetricEventAndDuration in intervalMetricEventsAndDurations)
            {
                var newDocument = new IntervalMetricInstancesDocument
                (
                    category,
                    currentIntervalMetricEventAndDuration.Item1.EventTime,
                    currentIntervalMetricEventAndDuration.Item1.Metric.Name,
                    currentIntervalMetricEventAndDuration.Item2
                );
                intervalMetricInstancesDocuments.Add(newDocument);
            }
            if (intervalMetricInstancesDocuments.Count > 0)
            {
                try
                {
                    intervalMetricInstancesCollection.InsertMany(intervalMetricInstancesDocuments);
                }
                catch (Exception e)
                {
                    throw new Exception($"Failed to insert documents into collection '{intervalMetricInstancesCollectionName}'.", e);
                }
            }
        }

        #endregion

        #region Collection and Index Creation Methods

        /// <summary>
        /// Creates all collections and indexes in the database if they don't already exist.
        /// </summary>
        protected void CreateCollectionsAndIndexes()
        {
            if (processMetricEventsMethodCalled == false)
            {
                CreateCollections();
                CreateIndexes();
                processMetricEventsMethodCalled = true;
            }
        }

        /// <summary>
        /// Creates all ApplicationMetrics collections in the database if they don't already exist.
        /// </summary>
        protected void CreateCollections()
        {
            var allCollectionNames = new List<String>()
            {
                countMetricInstancesCollectionName,
                amountMetricInstancesCollectionName,
                statusMetricInstancesCollectionName,
                intervalMetricInstancesCollectionName
            };
            foreach (String currentCollectionName in allCollectionNames)
            {
                CreateCollection(currentCollectionName);
            }
        }

        /// <summary>
        /// Creates a collection in the database if it doesn't already exist.
        /// </summary>
        /// <param name="collectionName">The name of the collection to create.</param>
        protected void CreateCollection(String collectionName)
        {
            foreach (String currentCollectionName in database.ListCollectionNames().ToEnumerable())
            {
                if (currentCollectionName == collectionName)
                {
                    return;
                }
            }
            try
            {
                database.CreateCollection(collectionName);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to create collection '{collectionName}' in MongoDB.", e);
            }
        }

        /// <summary>
        /// Creates all indexes in the database if they don't already exist.
        /// </summary>
        protected void CreateIndexes()
        {
            void CreateCommonPropertyIndexes<T>(IMongoCollection<T> collection) where T : MetricInstancesDocumentBase
            {
                var categoryIndexModel = new CreateIndexModel<T>(Builders<T>.IndexKeys.Ascending(document => document.Category));
                collection.Indexes.CreateOne(categoryIndexModel);
                var eventTimeIndexModel = new CreateIndexModel<T>(Builders<T>.IndexKeys
                    .Ascending(document => document.EventTime)
                    .Ascending(document => document.EventTimeTicks)
                );
                collection.Indexes.CreateOne(eventTimeIndexModel);
            }

            // 'CountMetricInstances' indexes
            var countMetricInstancesCollection = database.GetCollection<CountMetricInstancesDocument>(countMetricInstancesCollectionName);
            CreateCommonPropertyIndexes<CountMetricInstancesDocument>(countMetricInstancesCollection);
            var countMetricInstancesCollectionCountMetricIndexModel = new CreateIndexModel<CountMetricInstancesDocument>(Builders<CountMetricInstancesDocument>.IndexKeys.Ascending(document => document.CountMetric));
            countMetricInstancesCollection.Indexes.CreateOne(countMetricInstancesCollectionCountMetricIndexModel);
            // 'AmountMetricInstances' indexes
            var amountMetricInstancesCollection = database.GetCollection<AmountMetricInstancesDocument>(amountMetricInstancesCollectionName);
            CreateCommonPropertyIndexes<AmountMetricInstancesDocument>(amountMetricInstancesCollection);
            var amountMetricInstancesCollectionCountMetricIndexModel = new CreateIndexModel<AmountMetricInstancesDocument>(Builders<AmountMetricInstancesDocument>.IndexKeys.Ascending(document => document.AmountMetric));
            amountMetricInstancesCollection.Indexes.CreateOne(amountMetricInstancesCollectionCountMetricIndexModel);
            // 'StatusMetricInstances' indexes
            var statusMetricInstancesCollection = database.GetCollection<StatusMetricInstancesDocument>(statusMetricInstancesCollectionName);
            CreateCommonPropertyIndexes<StatusMetricInstancesDocument>(statusMetricInstancesCollection);
            var statusMetricInstancesCollectionCountMetricIndexModel = new CreateIndexModel<StatusMetricInstancesDocument>(Builders<StatusMetricInstancesDocument>.IndexKeys.Ascending(document => document.StatusMetric));
            statusMetricInstancesCollection.Indexes.CreateOne(statusMetricInstancesCollectionCountMetricIndexModel);
            // 'IntervalMetricInstances' indexes
            var intervalMetricInstancesCollection = database.GetCollection<IntervalMetricInstancesDocument>(intervalMetricInstancesCollectionName);
            CreateCommonPropertyIndexes<IntervalMetricInstancesDocument>(intervalMetricInstancesCollection);
            var intervalMetricInstancesCollectionCountMetricIndexModel = new CreateIndexModel<IntervalMetricInstancesDocument>(Builders<IntervalMetricInstancesDocument>.IndexKeys.Ascending(document => document.IntervalMetric));
            intervalMetricInstancesCollection.Indexes.CreateOne(intervalMetricInstancesCollectionCountMetricIndexModel);
        }

        #endregion

        #pragma warning disable 1591

        protected void ThrowExceptionIfStringParameterNullOrWhitespace(String parameterName, String parameterValue)
        {
            if (String.IsNullOrWhiteSpace(parameterValue))
            {
                throw new ArgumentNullException(parameterName, $"Parameter '{parameterName}' must contain a value.");
            }
        }

        #pragma warning restore 1591

        #endregion

        #region Finalize / Dispose Methods

        #pragma warning disable 1591

        ~MongoDbMetricLogger()
        {
            Dispose(false);
        }

        #pragma warning restore 1591

        /// <summary>
        /// Provides a method to free unmanaged resources used by this class.
        /// </summary>
        /// <param name="disposing">Whether the method is being called as part of an explicit Dispose routine, and hence whether managed resources should also be freed.</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                    mongoClient.Dispose();
                }
                // Free your own state (unmanaged objects).

                // Set large fields to null.

                disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        #endregion
    }
}
