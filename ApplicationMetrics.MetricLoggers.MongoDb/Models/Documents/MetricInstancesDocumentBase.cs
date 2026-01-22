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
    /// Base for MongoDB documents which store a instances of a metrics.
    /// </summary>
    public abstract class MetricInstancesDocumentBase
    {
        #pragma warning disable 1591

        public ObjectId _id;
        
        public String Category { get; protected set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime EventTime { get; protected set; }
        
        [BsonDateTimeOptions(Representation = BsonType.Int64, Kind = DateTimeKind.Utc)]
        public DateTime EventTimeTicks { get; protected set; }

        public MetricInstancesDocumentBase(String category, DateTime eventTime)
        {
            Category = category;
            EventTime = eventTime;
            EventTimeTicks = eventTime;
        }

        #pragma warning restore 1591
    }
}
