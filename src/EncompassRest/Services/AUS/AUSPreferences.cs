﻿using Newtonsoft.Json;

namespace EncompassRest.Services.AUS
{
    public sealed class AUSPreferences : SerializableObject
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? ImportConditions { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public StringEnumValue<AUSConditionType> ConditionType { get; set; }
    }
}