using Amazon;
using Amazon.CloudWatch.Model;
using App.Metrics.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Metrics.Reporting.CloudWatch
{
    public class MetricsReportingCloudWatchOptions
    {
        public MetricsReportingCloudWatchOptions(string awsNamespace)
        {
            if (string.IsNullOrWhiteSpace(awsNamespace))
            {
                throw new ArgumentNullException(nameof(awsNamespace));
            }

            AwsNamespace = awsNamespace;
            Dimensions = new List<Dimension>();
            Endpoint = RegionEndpoint.GetBySystemName("us-east-1");
        }

        public string AwsNamespace { get; }
        public List<Dimension> Dimensions { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="IFilterMetrics" /> to use for just this reporter.
        /// </summary>
        /// <value>
        ///     The <see cref="IFilterMetrics" /> to use for this reporter.
        /// </value>
        public IFilterMetrics Filter { get; set; }

        /// <summary>
        ///     Gets or sets the interval between flushing metrics.
        /// </summary>
        public TimeSpan FlushInterval { get; set; }
        public RegionEndpoint Endpoint { get; set; }
        public string AccessKeyId { get; set; }
        public string SecretAccessKey { get; set; }
    }
}
