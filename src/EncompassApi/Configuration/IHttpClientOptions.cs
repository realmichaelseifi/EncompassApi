﻿namespace EncompassApi.Configuration
{
    public interface IHttpClientOptions
    {
        ClientParameters ClientParameters { get; set; }
        HttpClientCompressionHandlerOptions CompressionOptions { get; set; }
        EncompassHttpResponseHeaderLoggerOptions EncompassHttpResponseHeaderLoggerOptions { get; set; }
        EncompassTokenClientOptions TokenClientOptions { get; set; }
    }
}