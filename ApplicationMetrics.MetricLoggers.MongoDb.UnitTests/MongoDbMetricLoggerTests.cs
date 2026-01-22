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

using NSubstitute;
using NUnit.Framework;
using System;

namespace ApplicationMetrics.MetricLoggers.MongoDb.UnitTests
{
    /// <summary>
    /// Unit tests for the ApplicationMetrics.MetricLoggers.MongoDb.MongoDbMetricLogger class.
    /// </summary>
    public class MongoDbMetricLoggerTests
    {
        protected const String testMetricCategory = "TestCategory";

        private IBufferProcessingStrategy mockBufferProcessingStrategy;
        private MongoDbMetricLoggerWithProtectedMembers testMongoDbMetricLogger;

        [SetUp]
        protected void SetUp()
        {
            mockBufferProcessingStrategy = Substitute.For<IBufferProcessingStrategy>();
            testMongoDbMetricLogger = new MongoDbMetricLoggerWithProtectedMembers
            (
                testMetricCategory,
                "mongodb://testServer:27017",
                "ApplicationMetrics",
                mockBufferProcessingStrategy,
                IntervalMetricBaseTimeUnit.Nanosecond,
                true
            );
        }

        [TearDown]
        public void TearDown()
        {
            testMongoDbMetricLogger.Dispose();
        }

        [Test]
        public void Constructor_CategoryNull()
        {
            var e = Assert.Throws<ArgumentNullException>(delegate
            {
                testMongoDbMetricLogger = new MongoDbMetricLoggerWithProtectedMembers
                (
                    null,
                    "mongodb://testServer:27017",
                    "ApplicationMetrics",
                    mockBufferProcessingStrategy,
                    IntervalMetricBaseTimeUnit.Nanosecond,
                    true
                );
            });

            Assert.That(e.Message, Does.StartWith($"Parameter 'category' must contain a value."));
            Assert.AreEqual("category", e.ParamName);
        }

        [Test]
        public void Constructor_CategoryWhitespace()
        {
            var e = Assert.Throws<ArgumentNullException>(delegate
            {
                testMongoDbMetricLogger = new MongoDbMetricLoggerWithProtectedMembers
                (
                    " ",
                    "mongodb://testServer:27017",
                    "ApplicationMetrics",
                    mockBufferProcessingStrategy,
                    IntervalMetricBaseTimeUnit.Nanosecond,
                    true
                );
            });

            Assert.That(e.Message, Does.StartWith($"Parameter 'category' must contain a value."));
            Assert.AreEqual("category", e.ParamName);
        }

        [Test]
        public void Constructor_ConnectionStringNull()
        {
            var e = Assert.Throws<ArgumentNullException>(delegate
            {
                testMongoDbMetricLogger = new MongoDbMetricLoggerWithProtectedMembers
                (
                    testMetricCategory,
                    null,
                    "ApplicationMetrics",
                    mockBufferProcessingStrategy,
                    IntervalMetricBaseTimeUnit.Nanosecond,
                    true
                );
            });

            Assert.That(e.Message, Does.StartWith($"Parameter 'connectionString' must contain a value."));
            Assert.AreEqual("connectionString", e.ParamName);
        }

        [Test]
        public void Constructor_ConnectionStringWhitespace()
        {
            var e = Assert.Throws<ArgumentNullException>(delegate
            {
                testMongoDbMetricLogger = new MongoDbMetricLoggerWithProtectedMembers
                (
                    testMetricCategory,
                    "",
                    "ApplicationMetrics",
                    mockBufferProcessingStrategy,
                    IntervalMetricBaseTimeUnit.Nanosecond,
                    true
                );
            });

            Assert.That(e.Message, Does.StartWith($"Parameter 'connectionString' must contain a value."));
            Assert.AreEqual("connectionString", e.ParamName);
        }

        [Test]
        public void Constructor_DatabaseNameNull()
        {
            var e = Assert.Throws<ArgumentNullException>(delegate
            {
                testMongoDbMetricLogger = new MongoDbMetricLoggerWithProtectedMembers
                (
                    testMetricCategory,
                    "mongodb://testServer:27017",
                    null,
                    mockBufferProcessingStrategy,
                    IntervalMetricBaseTimeUnit.Nanosecond,
                    true
                );
            });

            Assert.That(e.Message, Does.StartWith($"Parameter 'databaseName' must contain a value."));
            Assert.AreEqual("databaseName", e.ParamName);
        }

        [Test]
        public void Constructor_DatabaseNameWhitespace()
        {
            var e = Assert.Throws<ArgumentNullException>(delegate
            {
                testMongoDbMetricLogger = new MongoDbMetricLoggerWithProtectedMembers
                (
                    testMetricCategory,
                    "mongodb://testServer:27017",
                    " ",
                    mockBufferProcessingStrategy,
                    IntervalMetricBaseTimeUnit.Nanosecond,
                    true
                );
            });

            Assert.That(e.Message, Does.StartWith($"Parameter 'databaseName' must contain a value."));
            Assert.AreEqual("databaseName", e.ParamName);
        }
    }
}
