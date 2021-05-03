using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EncompassApi.xUnitTests.Seeds
{
    public static class LoanDocuments
    {
        public static JObject[] GetLoanDocumentsSeed()
        {
            var fullPath = $"Payloads/LoanDocuments.json";

            // Get the absolute path to the JSON file
            var path = Path.IsPathRooted(fullPath)
                ? fullPath
                : Directory.GetCurrentDirectory() + "/" + fullPath;

            if (!File.Exists(path))
            {
                throw new ArgumentException($"Could not find file at path: {path}");
            }
            // Load the file
            var fileData = File.ReadAllText(fullPath);
            return JsonConvert.DeserializeObject<JObject[]>(fileData);
        }

        public static JObject GetLoanDocumentSeed()
        {
            var fullPath = $"Payloads/LoanDocuments.json";

            // Get the absolute path to the JSON file
            var path = Path.IsPathRooted(fullPath)
                ? fullPath
                : Directory.GetCurrentDirectory() + "/" + fullPath;

            if (!File.Exists(path))
            {
                throw new ArgumentException($"Could not find file at path: {path}");
            }
            // Load the file
            var fileData = File.ReadAllText(fullPath);
            var objs = JsonConvert.DeserializeObject<JObject[]>(fileData);
            return objs[0];
        }
    }
}
