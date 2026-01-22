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
using MongoDB.Driver;
using StandardAbstraction;

namespace ApplicationMetrics.MetricLoggers.MongoDb.UnitTests
{
    /// <summary>
    /// Version of the MongoDbMetricLogger class where private and protected methods are exposed as public so that they can be unit tested.
    /// </summary>
    public class MongoDbMetricLoggerWithProtectedMembers : MongoDbMetricLogger
    {
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MetricLoggers.MongoDb.UnitTests.MongoDbMetricLoggerWithProtectedMembers class.
        /// </summary>
        /// <param name="category">The category to log all metrics under.</param>
        /// <param name="connectionString">The string to use to connect to MongoDB.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="bufferProcessingStrategy">Object which implements a processing strategy for the buffers (queues).</param>
        /// <param name="intervalMetricBaseTimeUnit">The base time unit to use to log interval metrics.</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).  Note that this parameter only has an effect when running in 'non-interleaved' mode.</param>
        public MongoDbMetricLoggerWithProtectedMembers
        (
            String category,
            String connectionString,
            String databaseName,
            IBufferProcessingStrategy bufferProcessingStrategy,
            IntervalMetricBaseTimeUnit intervalMetricBaseTimeUnit,
            Boolean intervalMetricChecking
        ) : base(category, connectionString, databaseName, bufferProcessingStrategy, intervalMetricBaseTimeUnit, intervalMetricChecking)
        {
        }

        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MetricLoggers.MongoDb.UnitTests.MongoDbMetricLoggerWithProtectedMembers class.
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
        public MongoDbMetricLoggerWithProtectedMembers
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
        ) : base(category, connectionString, databaseName, bufferProcessingStrategy, intervalMetricBaseTimeUnit, intervalMetricChecking, dateTime, stopWatch, guidProvider)
        {
        }

        public new void DequeueAndProcessMetricEvents()
        {
            base.DequeueAndProcessMetricEvents();
        }
    }
}
