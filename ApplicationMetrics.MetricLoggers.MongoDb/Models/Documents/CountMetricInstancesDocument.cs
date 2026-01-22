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
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApplicationMetrics.MetricLoggers.MongoDb.Models.Documents
{
    /// <summary>
    /// Holds a MongoDB document which stores an instance of a <see cref="ApplicationMetrics.CountMetric"/>.
    /// </summary>
    public class CountMetricInstancesDocument : MetricInstancesDocumentBase
    {
        public String CountMetric { get; protected set; }

        public CountMetricInstancesDocument(String category, DateTime eventTime, String countMetric)
            :base(category, eventTime)
        {
            this.CountMetric = countMetric;
        }
    }
}
