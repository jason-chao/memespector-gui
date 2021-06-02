using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;

namespace CVClient
{
    public class CVImageTask
    {
        public string ImageLocation { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public CVClientCommon.LocationType ImageLocationType { get; set; }

        public InvocationGoogle GoogleInvocation { get; set; } = null;
        public InvocationAzure AzureInvocation { get; set; } = null;
        public InvocationClarifai ClarifaiInvocation { get; set; } = null;
        public InvocationOpenSource OpenSourceInvocation { get; set; } = null;

        public Exception ExceptionRasied { get; private set; } = null;
        public DateTime? Completed { get; private set; }

        [JsonIgnore]
        public bool HideScoresInFlatResults { get; set; } = false;

        [JsonIgnore]
        public Action AllAPIsAction { get { if (action == null) action = GetAction(); return action; } }

        [JsonIgnore]
        private Action action = null;

        private Action GetAction()
        {
            return () =>
            {
                try
                {
                    var apiTasks = new List<Task>();

                    if (GoogleInvocation != null)
                    {
                        GoogleInvocation.ImageLocation = this.ImageLocation;
                        GoogleInvocation.ImageLocationType = this.ImageLocationType;
                        apiTasks.Add(Task.Run(GoogleInvocation.InvocationAction));
                    }

                    if (AzureInvocation != null)
                    {
                        AzureInvocation.ImageLocation = this.ImageLocation;
                        AzureInvocation.ImageLocationType = this.ImageLocationType;
                        apiTasks.Add(Task.Run(AzureInvocation.InvocationAction));
                    }

                    if (ClarifaiInvocation != null)
                    {
                        ClarifaiInvocation.ImageLocation = this.ImageLocation;
                        ClarifaiInvocation.ImageLocationType = this.ImageLocationType;
                        apiTasks.Add(Task.Run(ClarifaiInvocation.InvocationAction));
                    }

                    if (OpenSourceInvocation != null)
                    {
                        OpenSourceInvocation.ImageLocation = this.ImageLocation;
                        OpenSourceInvocation.ImageLocationType = this.ImageLocationType;
                        apiTasks.Add(Task.Run(OpenSourceInvocation.InvocationAction));
                    }

                    Task.WaitAll(apiTasks.ToArray());
                }
                catch (Exception ex)
                {
                    ExceptionRasied = ex;
                }
                finally
                {
                    Completed = DateTime.Now;
                }
            };
        }

        [JsonIgnore]
        public dynamic FlatResults
        {
            get
            {

                IDictionary<string, object> flatResults = new ExpandoObject();

                var imageInfoFlatResult = getImageInfoFlat();
                mergeObjects(ref flatResults, imageInfoFlatResult, string.Empty);

                if (Completed.HasValue)
                {
                    if (GoogleInvocation != null)
                    {
                        mergeObjects(ref flatResults, GoogleInvocation.FlatResult, "GV_");
                    }

                    if (AzureInvocation != null)
                    {
                        mergeObjects(ref flatResults, AzureInvocation.FlatResult, "MA_");
                    }

                    if (ClarifaiInvocation != null)
                    {
                        mergeObjects(ref flatResults, ClarifaiInvocation.FlatResult, "CL_");
                    }

                    if (OpenSourceInvocation != null)
                    {
                        mergeObjects(ref flatResults, OpenSourceInvocation.FlatResult, "OS_");
                    }
                }

                return flatResults;
            }
        }

        private void mergeObjects(ref IDictionary<string, object> baseObject, object newObject, string prefixForNewObject)
        {
            foreach (var property in newObject.GetType().GetProperties())
            {
                if (property.CanRead)
                {
                    if (HideScoresInFlatResults)
                    {
                        if (property.Name.EndsWith("_Score") || property.Name.EndsWith("_Scores"))
                            continue;
                    }
                    baseObject[$"{prefixForNewObject}{property.Name}"] = property.GetValue(newObject);
                }
            }
        }

        private FlatResultImageInfo getImageInfoFlat()
        {
            var flatResult = new FlatResultImageInfo();

            flatResult.Image_Location = ImageLocation;
            flatResult.Image_From = ImageLocationType.ToString();

            if (ImageLocationType == CVClientCommon.LocationType.Local)
            {
                var fileInfo = new FileInfo(ImageLocation);
                flatResult.Image_BaseName = fileInfo.Name;
            }
            else if (ImageLocationType == CVClientCommon.LocationType.Uri)
            {
                Uri fileUri = new Uri(ImageLocation);
                flatResult.Image_BaseName = fileUri.Segments.Last();
            }

            return flatResult;
        }


    }
}
