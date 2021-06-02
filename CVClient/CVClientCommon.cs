// #define DEBUG_DONOTINVOKEREALAPI

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using ServiceStack.Text;
using System.Linq;
using System.Reflection;

namespace CVClient
{
    static public class CVClientCommon
    {
        public enum LocationType { Local, Uri }

        public enum VisionAPI { OpenSource, GoogleCloud, MicrosoftAzure, Clarifai }

        public static Dictionary<string, string> GetAPIAvailableModels(VisionAPI api)
        {
            switch (api)
            {
                case VisionAPI.Clarifai:
                    return readModelOptions("CVClient.ModelOptions.ClarifaiModels.json");
                case VisionAPI.OpenSource:
                    return readModelOptions("CVClient.ModelOptions.OpenSourceModels.json");
                default:
                    break;
            }

            return new Dictionary<string, string>();
        }

        private static Dictionary<string, string> readModelOptions(string resourceId)
        {
            var modelDict = new Dictionary<string, string>();
            var cvModels = JsonConvert.DeserializeObject<List<CVModel>>(CVClientCommon.GetEmbeddedResourceText(resourceId));
            foreach (var cvModel in cvModels)
            {
                modelDict.Add(cvModel.Title, cvModel.ModelId);
            }
            return modelDict;
        }

        public static Stream GetEmbeddedResourceStream(string ResourceId)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream(ResourceId);
        }

        public static string GetEmbeddedResourceText(string ResourceId)
        {
            Stream stream = GetEmbeddedResourceStream(ResourceId);
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        // Image formats supported by each API
        // Google Vision https://cloud.google.com/vision/docs/supported-files
        // Microsoft Azure https://docs.microsoft.com/en-us/azure/cognitive-services/computer-vision/overview
        // Clarifai https://docs.clarifai.com/portal-guide/data/supported-formats
        // The formats supported by all these APIs are JPEG, PNG, GIF and BMP
        static public List<string> SupportedFormats = new List<string> { "jpeg", "jpg", "png", "gif", "bmp" };

        static public bool Google_MayBeAValidCredentialFile(string filename)
        {
            try
            {
                var gcsCredentialFileProperties = new string[] { "project_id", "private_key_id", "private_key", "client_id" };
                var objectKeys = JsonObject.Parse(File.ReadAllText(filename)).Select(co => co.Key);
                return gcsCredentialFileProperties.All(p => objectKeys.Any(k => k.Contains(p)));
            }
            catch { }
            return false;
        }

#if DEBUG_DONOTINVOKEREALAPI
        static public void SimulateAPIInvocation()
        {
            Random rand = new Random();
            System.Threading.Thread.Sleep(rand.Next(3000, 15000));
            throw new Exception("A deliberate exception to end the task for debugging purposes");
        }
#endif

    }
}
