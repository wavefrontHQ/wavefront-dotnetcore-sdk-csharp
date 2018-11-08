﻿using Microsoft.Extensions.DependencyInjection;
using OpenTracing;
using OpenTracing.Util;
using Wavefront.AspNetCore.SDK.CSharp.Mvc;
using Wavefront.AspNetCore.SDK.CSharp.Tracing;

namespace Wavefront.AspNetCore.SDK.CSharp.Common
{
    /// <summary>
    ///     Extension methods for <see cref="IServiceCollection"/> to enable out-of-the-box
    ///     Wavefront metrics and reporting for ASP.NET Core applications.
    /// </summary>
    public static class WavefrontServiceCollectionExtensions
    {
        /// <summary>
        ///     Enables out-of-the-box Wavefront metrics and reporting for an ASP.NET Core MVC
        ///     application.
        /// </summary>
        /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
        /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
        /// <param name="wfAspNetCoreReporter">The Wavefront ASP.NET Core reporter.</param>
        public static IServiceCollection AddWavefrontForMvc(
            this IServiceCollection services,
            WavefrontAspNetCoreReporter wfAspNetCoreReporter,
            ITracer tracer)
        {
            // register App Metrics registry and services
            services.AddMetrics(wfAspNetCoreReporter.Metrics);

            // register App Metrics reporting scheduler
            services.AddMetricsReportScheduler();

            // register Wavefront ASP.NET Core reporter
            services.AddSingleton(wfAspNetCoreReporter);

            // register tracer
            if (tracer != null)
            {
                GlobalTracer.Register(tracer);
            }
            services.AddSingleton(GlobalTracer.Instance);

            // register HttpClient that automatically handles span context propagation
            services.AddTransient<SpanContextPropagationHandler>();
            services.AddHttpClient(NamedHttpClients.SpanContextPropagationClient)
                    .AddHttpMessageHandler<SpanContextPropagationHandler>();

            // register Wavefront Heartbeater hosted service
            services.AddHostedService<HeartbeaterHostedService>();

            // register MVC resource filter for capturing Wavefront metrics, histograms, and traces
            services.AddSingleton<WavefrontMetricsResourceFilter>();
            services.AddMvc(options =>
            {
                options.Filters.AddService<WavefrontMetricsResourceFilter>();
            });

            return services;
        }
    }
}
