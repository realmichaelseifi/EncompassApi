﻿using System;
using System.Threading;
using System.Threading.Tasks;
using EncompassApi.Loans;
using EncompassApi.Utilities;
using Newtonsoft.Json;

namespace EncompassApi
{
    public interface IClientParameters
    {
        string ApiClientId { get; set; }
        string ApiClientSecret { get; set; }
        EventHandler<ApiResponseEventArgs>? ApiResponse { get; set; }
        string? BaseAddress { get; set; }
        CommonCache? CommonCache { get; set; }
        TimeSpan Timeout { get; set; }
        int TimeoutRetryCount { get; set; }
        UndefinedCustomFieldHandling UndefinedCustomFieldHandling { get; set; }
    }
    /// <summary>
    /// The parameters for initializing a client object.
    /// </summary>
    public sealed class ClientParameters : IClientParameters
    {
        private int _timeoutRetryCount;

        /// <summary>
        /// The Api Client Id.
        /// </summary>
        public string ApiClientId { get; set; }

        /// <summary>
        /// The Api Client Secret.
        /// </summary>
        public string ApiClientSecret { get; set; }

        /// <summary>
        /// The time span before Api requests are considered timed-out. Default is 100 seconds.
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Property for sharing common cache between multiple clients such as custom field descriptors.
        /// </summary>
        [JsonIgnore]
        public CommonCache? CommonCache { get; set; }

        /// <summary>
        /// Specifies when the custom fields cache should be initialized or refreshed.
        /// </summary>
        public CacheInitialization CustomFieldsCacheInitialization { get; set; }

        /// <summary>
        /// Specifies when the standard fields cache should be refreshed.
        /// </summary>
        public CacheInitialization StandardFieldsCacheInitialization { get; set; }

        /// <summary>
        /// The number of times to retry requests when there's a gateway timeout. Default is 0.
        /// </summary>
        public int TimeoutRetryCount
        {
            get => _timeoutRetryCount;
            set
            {
                Preconditions.GreaterThanOrEquals(value, nameof(TimeoutRetryCount), 0);
                Preconditions.LessThanOrEquals(value, nameof(TimeoutRetryCount), 3);

                _timeoutRetryCount = value;
            }
        }

        /// <summary>
        /// The event handler to attach to the <see cref="EncompassApiClient.ApiResponse"/> event of the created client.
        /// </summary>
        public EventHandler<ApiResponseEventArgs>? ApiResponse { get; set; }

        /// <summary>
        /// Specifies how the client should handle undefined custom fields.
        /// </summary>
        public UndefinedCustomFieldHandling UndefinedCustomFieldHandling { get; set; }

        /// <summary>
        /// The URL to call for API calls. Defaults to "https://api.elliemae.com/".
        /// </summary>
        public string? BaseAddress { get; set; }

        /// <summary>
        /// The client parameters constructor.
        /// </summary>
        /// <param name="apiClientId">The Api Client Id.</param>
        /// <param name="apiClientSecret">The Api Client Secret.</param>

        internal async Task TryInitializeAsync(IEncompassApiClient client, CommonCache commonCache, CancellationToken cancellationToken)
        {
            if (CustomFieldsCacheInitialization != CacheInitialization.Never && !((DateTime.UtcNow - commonCache.CustomFieldsLastRefreshedUtc)?.TotalMinutes < (int)CustomFieldsCacheInitialization))
            {
                await commonCache.RefreshCustomFieldsAsync(client, cancellationToken).ConfigureAwait(false);
            }
            if (StandardFieldsCacheInitialization != CacheInitialization.Never && !((DateTime.UtcNow - LoanFieldDescriptors.StandardFieldsLastRefreshedUtc)?.TotalMinutes < (int)StandardFieldsCacheInitialization))
            {
                await LoanFieldDescriptors.RefreshStandardFieldsAsync(client, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}