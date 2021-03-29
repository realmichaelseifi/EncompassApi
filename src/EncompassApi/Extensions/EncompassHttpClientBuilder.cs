﻿using EncompassApi.Clients;
using EncompassApi.Configuration;
using EncompassApi.MessageHandlers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Polly;
using Polly.Extensions.Http;
using System;
using System.Net.Http;

namespace EncompassApi.Extensions
{
    public class EncompassHttpClientBuilder
    {
        private readonly IHttpClientBuilder _builder;
        private readonly HttpClientOptions _options;

        public EncompassHttpClientBuilder(IHttpClientBuilder builder, IServiceCollection services)
        {
            var options = services.BuildServiceProvider().GetRequiredService<IOptions<HttpClientOptions>>();
            if (options == null) { throw new NullReferenceException("Options cannot be null!"); }
            _builder = builder;
            _options = options.Value;
        }

        public  EncompassHttpClientBuilder AddEncompassTokenMessageHandler()
        {
            _builder.AddHttpMessageHandler(sp => new TokenHandler(sp.GetService<ITokenClient>()));
            return this;
        }


        public EncompassHttpClientBuilder AddEncompassHttpResponseHeaderLoggingHandler(params string[] headers)
        {
            _builder.AddHttpMessageHandler(sp => new EncompassResponseHeadersLoggingHandler(sp.GetService<ILogger<EncompassResponseHeadersLoggingHandler>>(), headers));
            return this;
        }


        public EncompassHttpClientBuilder AddEncompassMessageHandler (Func<IServiceProvider, DelegatingHandler> func)
        {
            _builder.AddHttpMessageHandler(func);
            return this;
        }

        public EncompassHttpClientBuilder AddEncompassRetryPolicyHandler()
        {
            var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError().RetryAsync(_options.TokenClientOptions.RetryCount);
            _builder.AddPolicyHandler(retryPolicy);
            return this;
        }


        public EncompassHttpClientBuilder AddEncompassTimeoutPolicyHandler()
        {
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(_options.TokenClientOptions.TimeoutInSeconds);
            _builder.AddPolicyHandler(timeoutPolicy);
            return this;
        }

        public EncompassHttpClientBuilder AddEncompassPolicyHandler(IAsyncPolicy<HttpResponseMessage> policy)
        {
            _builder.AddPolicyHandler(policy);
            return this;
        }



        public void Build(IServiceCollection service)
        {
            service.AddTransient<IEncompassApiClient>(sp => new EncompassApiService(sp.GetService<IHttpClientFactory>().CreateClient("EncompassClient"), _options.ClientParameters));

        }

    }

}
