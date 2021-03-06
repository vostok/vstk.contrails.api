﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vostok.Contrails.Client;
using Vostok.Logging;
using Vostok.Tracing;

namespace Vostok.Contrails.Api.Controllers
{
    public class TracesByIdResponce
    {
        public Guid TraceId { get; set; }
        public IEnumerable<Span> Spans { get; set; }
    }

    public class ApiController : Controller
    {
        private readonly Func<IContrailsClient> contrailsClientFunc;
        private readonly MetricContainer metricsContainer;
        private readonly ILog log;

        public ApiController(Func<IContrailsClient> contrailsClientFunc, MetricContainer metricsContainer, ILog log)
        {
            this.contrailsClientFunc = contrailsClientFunc;
            this.metricsContainer = metricsContainer;
            this.log = log;
        }

        [HttpGet]
        [Route("/")]
        public string Root()
        {
            return "ContrailsApi is runnig";
        }

        [HttpGet]
        [Route("api/search")]
        public async Task<TracesByIdResponce> TracesById(Guid traceId, [Bind(Prefix = "fromTs")] DateTimeOffset? fromTimestamp, Guid? fromSpan, [Bind(Prefix = "toTs")] DateTimeOffset? toTimestamp, Guid? toSpan, int limit = 1000, bool ascending = true)
        {
            try
            {
                if (traceId == Guid.Empty)
                    return new TracesByIdResponce {TraceId = traceId, Spans = new Span[] {}};
                var spans = await contrailsClientFunc().GetTracesById(traceId, fromTimestamp, fromSpan, toTimestamp, toSpan, ascending, limit);
                metricsContainer.SuccessCounter.Add();
                return new TracesByIdResponce {TraceId = traceId, Spans = spans};
            }
            catch (Exception e)
            {
                metricsContainer.ErrorCounter.Add();
                throw;
            }
        }
    }
}
