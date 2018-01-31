// <copyright file="MetricsCloudWatchReporterBuilder.cs" company="KCF Technologies, Inc.">
// Copyright (c) Mark Edwards. All rights reserved.
// </copyright>

using System;
using App.Metrics.Builder;
using App.Metrics.Reporting.CloudWatch;

// ReSharper disable CheckNamespace
namespace App.Metrics
    // ReSharper restore CheckNamespace
{
    /// <summary>
    ///     Builder for configuring metrics CloudWatch reporting using an
    ///     <see cref="IMetricsReportingBuilder" />.
    /// </summary>
    public static class MetricsCloudWatchReporterBuilder
    {
        /// <summary>
        ///     Add the <see cref="HttpMetricsReporter" /> allowing metrics to be reported over HTTP.
        /// </summary>
        /// <param name="metricReporterProviderBuilder">
        ///     The <see cref="IMetricsReportingBuilder" /> used to configure metrics reporters.
        /// </param>
        /// <param name="setupAction">The HTTP reporting options to use.</param>
        /// <returns>
        ///     An <see cref="IMetricsBuilder" /> that can be used to further configure App Metrics.
        /// </returns>
        public static IMetricsBuilder ToCloudWatch(
            this IMetricsReportingBuilder metricReporterProviderBuilder, string awsNamespace,
            Action<MetricsReportingCloudWatchOptions> setupAction)
        {
            if (metricReporterProviderBuilder == null)
            {
                throw new ArgumentNullException(nameof(metricReporterProviderBuilder));
            }

            var options = new MetricsReportingCloudWatchOptions(awsNamespace);

            setupAction?.Invoke(options);

            var provider = new CloudWatchMetricsReporter(options);

            return metricReporterProviderBuilder.Using(provider);
        }

        /// <summary>
        ///     Add the <see cref="HttpMetricsReporter" /> allowing metrics to be reported over HTTP.
        /// </summary>
        /// <param name="metricReporterProviderBuilder">
        ///     The <see cref="IMetricsReportingBuilder" /> used to configure metrics reporters.
        /// </param>
        /// <param name="endpoint">The HTTP endpoint where metrics are POSTed.</param>
        /// <returns>
        ///     An <see cref="IMetricsBuilder" /> that can be used to further configure App Metrics.
        /// </returns>
        public static IMetricsBuilder ToCloudWatch(
            this IMetricsReportingBuilder metricReporterProviderBuilder, string awsNamespace)
        {
            if (metricReporterProviderBuilder == null)
            {
                throw new ArgumentNullException(nameof(metricReporterProviderBuilder));
            }

            var options = new MetricsReportingCloudWatchOptions(awsNamespace);
            var provider = new CloudWatchMetricsReporter(options);

            return metricReporterProviderBuilder.Using(provider);
        }

        /// <summary>
        ///     Add the <see cref="HttpMetricsReporter" /> allowing metrics to be reported over HTTP.
        /// </summary>
        /// <param name="metricReporterProviderBuilder">
        ///     The <see cref="IMetricsReportingBuilder" /> used to configure metrics reporters.
        /// </param>
        /// <param name="endpoint">The HTTP endpoint where metrics are POSTed.</param>
        /// <param name="flushInterval">
        ///     The <see cref="T:System.TimeSpan" /> interval used if intended to schedule metrics
        ///     reporting.
        /// </param>
        /// <returns>
        ///     An <see cref="IMetricsBuilder" /> that can be used to further configure App Metrics.
        /// </returns>
        public static IMetricsBuilder ToCloudWatch(
            this IMetricsReportingBuilder metricReporterProviderBuilder, string awsNamespace,
            TimeSpan flushInterval)
        {
            if (metricReporterProviderBuilder == null)
            {
                throw new ArgumentNullException(nameof(metricReporterProviderBuilder));
            }

            var options = new MetricsReportingCloudWatchOptions(awsNamespace)
            {
                FlushInterval = flushInterval
            };

            var provider = new CloudWatchMetricsReporter(options);

            return metricReporterProviderBuilder.Using(provider);
        }
    }
}