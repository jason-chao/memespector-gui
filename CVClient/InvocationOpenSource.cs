//#define DEBUG_DONOTINVOKEREALAPI

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.Net.Http;

namespace CVClient
{
    public class InvocationOpenSource : InvocationBase
    {
        public InvocationOpenSource()
        {
            API = CVClientCommon.VisionAPI.OpenSource;
        }

        public OpenSourceModelResult APIResponse { get; set; } = null;

        public int DetectionMaxResults { get; set; } = 5;
        public double FlatteningMinScore { get; set; } = 0;

        public TimeSpan HTTPConnectionTimeout { get; set; } = new TimeSpan(0, 0, 60);
        public TimeSpan HTTPReadTimeout { get; set; } = new TimeSpan(0, 0, 60);

        private string model = null;

        public string Model { get { return model; } set { if (ModelsAvailable.Keys.Contains(value)) model = value; else throw new Exception("This model does not exist"); } }

        private Dictionary<string, string> modelsAvailable = null;

        [JsonIgnore]
        public Dictionary<string, string> ModelsAvailable
        {
            get
            {
                if (modelsAvailable == null)
                {
                    modelsAvailable = CVClientCommon.GetAPIAvailableModels(API.Value);
                }

                return modelsAvailable;
            }
        }

        [JsonIgnore]
        public Action InvocationAction { get { if (action == null) action = GetAction(); return action; } }

        [JsonIgnore]
        private Action action = null;

        private Action GetAction()
        {
            return () =>
            {
                try
                {

#if (!DEBUG_DONOTINVOKEREALAPI)

                    string endpoint = Environment.GetEnvironmentVariable("OPEN_SOURCE_CV_ENDPOINT");

                    var camelCaseJsonSerializerSetting = new JsonSerializerSettings() { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() };

                    var client = new HttpClient() { Timeout = HTTPConnectionTimeout };
                    var stringContent = new StringContent(JsonConvert.SerializeObject(GetOpenSourceRequest(), camelCaseJsonSerializerSetting), Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync(endpoint, stringContent);
                    System.Diagnostics.Debug.WriteLine($"Posting to API for {this.ImageLocation}");
                    postTask.Wait(HTTPReadTimeout);
                    var httpResponse = postTask.Result;
                    var readStringTask = httpResponse.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Reading API response for {this.ImageLocation}");
                    readStringTask.Wait(HTTPReadTimeout);
                    try
                    {
                        APIResponse = JsonConvert.DeserializeObject<OpenSourceModelResult>(readStringTask.Result, camelCaseJsonSerializerSetting);
                    }
                    catch
                    {
                        throw new Exception($"Invalid response from API: {readStringTask.Result}");
                    }

                    foreach (var modelResult in APIResponse.Results)
                    {
                        modelResult.Labels = modelResult.Labels.Where(l => l.Score >= FlatteningMinScore).OrderByDescending(l => l.Score).ToArray();
                    }
#endif

#if DEBUG_DONOTINVOKEREALAPI
                    CVClientCommon.SimulateAPIInvocation();
#endif
                }
                catch (Exception ex)
                {
                    ExceptionRasied = ex;
                }
                finally
                {

                    Processed = DateTime.Now;
                }
            };
        }

        private dynamic GetOpenSourceRequest()
        {
            if (ImageLocationType == CVClientCommon.LocationType.Uri)
            {
                return new { Models = new string[] { modelsAvailable[model] }, Image = new { Url = ImageLocation }, TopResults = DetectionMaxResults };
            }
            else if (ImageLocationType == CVClientCommon.LocationType.Local)
            {
                return new { Models = new string[] { modelsAvailable[model] }, Image = new { Base64 = Convert.ToBase64String(File.ReadAllBytes(ImageLocation)) }, TopResults = DetectionMaxResults };
            }

            return null;
        }

        public class OpenSourceModelResult
        {
            public string Error { get; set; } = null;
            public ICollection<OpenSourceModelResultLabel> Results { get; set; }
        }

        public class OpenSourceModelResultLabel
        {
            public string ModelName { get; set; }
            public ICollection<OpenSourceModelLabel> Labels { get; set; }
        }

        public class OpenSourceModelLabel
        {
            public string Id { get; set; }
            public string Label { get; set; }
            public double Score { get; set; }
        }

        [JsonIgnore]
        public FlatResultOpenSource FlatResult
        {
            get
            {
                var flatResult = new FlatResultOpenSource();

                if (ExceptionRasied != null)
                {
                    flatResult.Error = ExceptionRasied.Message;
                }

                if (APIResponse != null)
                {
                    if (APIResponse.Results.Any())
                    {
                        if (string.IsNullOrEmpty(APIResponse.Error))
                            flatResult.Success = true;
                        else
                            flatResult.Error += APIResponse.Error;
                    }
                }

                if (!flatResult.Success)
                    return flatResult;

                var labels = APIResponse.Results.First().Labels.Where(a => a.Score >= FlatteningMinScore);

                if (labels.Any())
                {
                    flatResult.Labels = string.Join("; ", labels.Select(l => l.Label));
                    flatResult.Label_Ids = string.Join("; ", labels.Select(l => l.Id));
                    flatResult.Label_Scores = string.Join("; ", labels.Select(l => l.Score));
                }

                flatResult.Model = model;

                return flatResult;
            }
        }
    }
}
