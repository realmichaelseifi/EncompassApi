using FluentAssertions.Primitives;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EncompassApi.xUnitTests.Extensions
{
    public static class JObjectAssertionsExtensions
    {
        public static ObjectAssertions Should<TObject>(this JObject jobject)
        {
            var jsn = jobject.ToString();
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<TObject>(jsn);
            return new ObjectAssertions(obj);
        }
    }
}
