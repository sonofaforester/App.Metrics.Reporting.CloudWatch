using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using App.Metrics.Filters;
using App.Metrics.Formatters;
using App.Metrics.Logging;

namespace App.Metrics.Reporting.CloudWatch
{
    public class CloudWatchMetricsReporter : IReportMetrics
    {
        private static readonly ILog Logger = LogProvider.For<CloudWatchMetricsReporter>();

        public CloudWatchMetricsReporter(MetricsReportingCloudWatchOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.FlushInterval < TimeSpan.Zero)
            {
                throw new InvalidOperationException($"{nameof(MetricsReportingCloudWatchOptions.FlushInterval)} must not be less than zero");
            }

            FlushInterval = options.FlushInterval > TimeSpan.Zero
                ? options.FlushInterval
                : AppMetricsConstants.Reporting.DefaultFlushInterval;

            Filter = options.Filter;

            _namespace = options.AwsNamespace;

            if (!string.IsNullOrEmpty(options.AccessKeyId) && !string.IsNullOrEmpty(options.SecretAccessKey))
                _client = new AmazonCloudWatchClient(options.AccessKeyId, options.SecretAccessKey, options.Endpoint);
            else
                _client = new AmazonCloudWatchClient(options.Endpoint);

            _dimensions = options.Dimensions;

            Logger.Info($"Using Metrics Reporter {this}. FlushInterval: {FlushInterval}");
        }

        public IFilterMetrics Filter { get; set; }

        private string _namespace;
        private AmazonCloudWatchClient _client;
        private List<Dimension> _dimensions;

        public TimeSpan FlushInterval { get; set; }
        public IMetricsOutputFormatter Formatter { get; set; }

        public async Task<bool> FlushAsync(MetricsDataValueSource metricsData, CancellationToken cancellationToken = default(CancellationToken))
        {
            Logger.Trace("Flushing metrics snapshot");

            var metrics = new List<MetricDatum>();
        
            foreach (var context in metricsData.Contexts)
            {
                foreach (var item in context.ApdexScores)
                {
                    if(!await AddDatum($"[{context.Context}] {item.MultidimensionalName}", item.Unit, metricsData.Timestamp, item.Value.Score, metrics))
                        return false;
                }

                foreach (var item in context.Counters)
                {
                    if (!await AddDatum($"[{context.Context}] {item.MultidimensionalName}", item.Unit, metricsData.Timestamp, item.Value.Count, metrics))
                        return false;
                }

                foreach (var item in context.Gauges)
                {
                    if (!await AddDatum($"[{context.Context}] {item.MultidimensionalName}", item.Unit, metricsData.Timestamp, item.Value, metrics))
                        return false;
                }

                foreach (var item in context.Meters)
                {
                    if (!await AddDatum($"[{context.Context}] {item.MultidimensionalName}", item.Unit, metricsData.Timestamp, item.Value.Count, metrics))
                        return false;
                }
                
                foreach (var item in context.Timers)
                {
                    if (!await AddDatum($"[{context.Context}] {item.MultidimensionalName}", item.Unit, metricsData.Timestamp, item.Value.Histogram.Mean, metrics))
                        return false;
                }
            }

            return await SendMetrics(metrics);
        }

        private async Task<bool> AddDatum(string name, Unit unit, DateTime timestamp, double value, List<MetricDatum> metrics)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return true;

            metrics.Add(new MetricDatum
                        {
                            Dimensions = _dimensions,
                            MetricName = $"{name}-{GetCloudWatchUnitName(unit)}",
                            Timestamp = timestamp,
                            Unit = StandardUnit.None,
                            Value = value
                        });

            if (metrics.Count > 19)
                return await SendMetrics(metrics);

            return true;
        }

        private string GetCloudWatchUnitName(Unit unit)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(unit.ToString());
        }

        private async Task<bool> SendMetrics(List<MetricDatum> metrics)
        {
            var request = new PutMetricDataRequest
            {
                MetricData = metrics,
                Namespace = _namespace
            };

            try
            {
                var response = await _client.PutMetricDataAsync(request);

                metrics.Clear();

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    return true;

                Logger.Warn($"CloudMetrics returned error code {response.HttpStatusCode}");

                return false;
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "CloudMetrics error");

                return false;
            }
        }
    }
}
