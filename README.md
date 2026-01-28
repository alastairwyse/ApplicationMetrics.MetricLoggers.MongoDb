ApplicationMetrics.MetricLoggers.MongoDb
---
An implementation of an [ApplicationMetrics](https://github.com/alastairwyse/ApplicationMetrics) [metric logger](https://github.com/alastairwyse/ApplicationMetrics/blob/master/ApplicationMetrics/IMetricLogger.cs) which writes metrics and instrumentation to a MongoDB database.

#### Setup
The code below demonstrates the setup and use case (with fake metrics logged) of the MongoDbMetricLogger class...

```C#
using (var bufferProcessor = new SizeLimitedBufferProcessor(5))
using (var metricLogger = new MongoDbMetricLogger
(
    "DefaultCategory",
    "mongodb://127.0.0.1:27017",
    "ApplicationMetrics",
    bufferProcessor,
    IntervalMetricBaseTimeUnit.Nanosecond,
    true
))
{
    metricLogger.Start();

    Guid beginId = metricLogger.Begin(new MessageSendTime());
    Thread.Sleep(20);
    metricLogger.Increment(new MessageSent());
    metricLogger.Add(new MessageSize(), 2661);
    metricLogger.End(beginId, new MessageSendTime());

    metricLogger.Stop();
}
```

The MongoDbMetricLogger class accepts the following constructor parameters...

| Parameter Name | Description |
| -------------- | ----------- |
| category | The category to log all metrics under. |
| connectionString | The string to use to connect to MongoDB. |
| databaseName | The name of the database. |
| bufferProcessingStrategy | An object implementing [IBufferProcessingStrategy](https://github.com/alastairwyse/ApplicationMetrics/blob/master/ApplicationMetrics.MetricLoggers/IBufferProcessingStrategy.cs) which decides when the buffers holding logged metric events should be flushed (and be written to MongoDB).  |
| intervalMetricBaseTimeUnit | The base time unit to use to log interval metrics. |
| intervalMetricChecking | Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).  Note that this parameter only has an effect when running in 'non-interleaved' mode. |

#### Storage
4 collections are created in the specified MongoDB database, which store the metrics...

| Collection Name |
| --------------- |
| CountMetricInstances |
| AmountMetricInstances |
| StatusMetricInstances |
| IntervalMetricInstances |

#### Non-interleaved Method Overloads
Methods which support ['non-interleaved' interval metric logging](https://github.com/alastairwyse/ApplicationMetrics#interleaved-interval-metrics) (i.e. overloads of End() and CancelBegin() methods which _don't_ accept a Guid) will be deprecated in a future version of ApplicationMetrics.  Hence it's recommended to only use the End() and CancelBegin() method overloads which accept a 'beginId' Guid parameter.

#### Links
The documentation below was written for version 1.* of ApplicationMetrics.  Minor implementation details may have changed in versions 2.0.0 and above, however the basic principles and use cases documented are still valid.  Note also that this documentation demonstrates the older ['non-interleaved'](https://github.com/alastairwyse/ApplicationMetrics#interleaved-interval-metrics) method of logging interval metrics.

Full documentation for the project...<br />
[http://www.alastairwyse.net/methodinvocationremoting/application-metrics.html](http://www.alastairwyse.net/methodinvocationremoting/application-metrics.html)

A detailed sample implementation...<br />
[http://www.alastairwyse.net/methodinvocationremoting/sample-application-5.html](http://www.alastairwyse.net/methodinvocationremoting/sample-application-5.html)

#### Release History

| Version | Changes |
| ------- | ------- |
| 1.0.0 | Initial release. | 
