﻿using EncompassApi.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace EncompassApi.xUnitTests.TestServices
{
    public interface IMockedEncompassHttpClientService
    {
        Uri BaseAddress { get; }
        IHttpClientFactory HttpFactory { get; }
        HttpClient MockedClient { get; }
        EncompassApiService MockedEncompassClient { get; }
        Mock<HttpMessageHandler> MockedHandler { get; }

        IMockedEncompassHttpClientService SetupResponseMessage(Action<HttpResponseMessage> action, KeyValuePair<string, string>? testHeader = null);
        IMockedEncompassHttpClientService SetOptions(Action<Mock<IHttpClientOptions>> options);
        IMockedEncompassHttpClientService SetOptions(Mock<IHttpClientOptions> options);
        IMockedEncompassHttpClientService AddDefaultRequestHeaders();
        MockedEncompassHttpClientService SetClientApiResponse(EventHandler<ApiResponseEventArgs> callBack);
        EncompassApi.Webhook.Webhook SetWebhookApiResponseCallback(EventHandler<ApiResponseEventArgs> action);
        EncompassApi.Loans.Documents.LoanDocuments SetDocumentsApiResponseCallback(string mockedLoanId, EventHandler<ApiResponseEventArgs> action);
        EncompassApi.Loans.Documents.LoanDocuments GetDocumentsApiAsync(string mockedLoanId);
    }
}