//#define DEBUG_DONOTINVOKEREALAPI

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Converters;

namespace CVClient
{
    public class InvocationAzure : InvocationBase
    {
        public InvocationAzure ()
        {
            API = CVClientCommon.VisionAPI.MicrosoftAzure;

        }

        public ImageAnalysis APIResponse { get; set; } = null;

        public ICollection<VisualFeatureTypes?> DetectionFeatureTypes { get; set; } = new List<VisualFeatureTypes?>();

        public Dictionary<VisualFeatureTypes, float> FlatteningMinScores { get; set; } = new Dictionary<VisualFeatureTypes, float> {
            { VisualFeatureTypes.Categories, 0 },
            { VisualFeatureTypes.Description, 0 },
            { VisualFeatureTypes.Tags, 0 },
            { VisualFeatureTypes.Brands, 0 },
            { VisualFeatureTypes.Objects, 0 }
         };

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
                    
                    var client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(Environment.GetEnvironmentVariable("MICROSOFT_AZURE_CV_KEY"))) { Endpoint = Environment.GetEnvironmentVariable("MICROSOFT_AZURE_CV_ENDPOINT") };
                    Task<ImageAnalysis> imageAnalysisTask = null;

                    if (ImageLocationType == CVClientCommon.LocationType.Local)
                    {
                        Stream imageStream = new MemoryStream(File.ReadAllBytes(ImageLocation));
                        imageAnalysisTask = client.AnalyzeImageInStreamAsync(imageStream, visualFeatures: DetectionFeatureTypes.ToList());
                    }
                    else if (ImageLocationType == CVClientCommon.LocationType.Uri)
                    {
                        imageAnalysisTask = client.AnalyzeImageAsync(ImageLocation, visualFeatures: DetectionFeatureTypes.ToList());
                    }

                    imageAnalysisTask.Wait();
                    APIResponse = imageAnalysisTask.Result;
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

        [JsonIgnore]
        public FlatResultAzure FlatResult
        {
            get
            {
                var flatResult = new FlatResultAzure();

                if (ExceptionRasied != null)
                {
                    flatResult.Error = ExceptionRasied.Message;
                }

                if (APIResponse != null)
                {
                    flatResult.Success = true;
                }

                if (!flatResult.Success)
                    return flatResult;

                foreach (var featureType in DetectionFeatureTypes)
                {
                    switch (featureType)
                    {
                        case VisualFeatureTypes.Categories:
                            var categories = APIResponse.Categories.Where(a => a.Score >= FlatteningMinScores[VisualFeatureTypes.Categories]);
                            if (categories.Any())
                            {
                                flatResult.Categories = string.Join("; ", categories.Select(l => l.Name));
                                flatResult.Categories_Scores = string.Join("; ", categories.Select(l => l.Score));
                            }
                            break;
                        case VisualFeatureTypes.Adult:
                            if (APIResponse.Adult != null)
                            {
                                flatResult.Adult_Adult = APIResponse.Adult.IsAdultContent;
                                flatResult.Adult_Racy = APIResponse.Adult.IsRacyContent;
                                flatResult.Adult_Gory = APIResponse.Adult.IsGoryContent;

                                flatResult.Adult_Adult_Score = APIResponse.Adult.AdultScore.ToString();
                                flatResult.Adult_Racy_Score = APIResponse.Adult.RacyScore.ToString();
                                flatResult.Adult_Gory_Score = APIResponse.Adult.GoreScore.ToString();
                            }
                            break;
                        case VisualFeatureTypes.Tags:
                            var tags = APIResponse.Tags.Where(a => a.Confidence >= FlatteningMinScores[VisualFeatureTypes.Tags]);
                            if (tags.Any())
                            {
                                flatResult.Tags = string.Join("; ", tags.Select(l => l.Name));
                                flatResult.Tag_Scores = string.Join("; ", tags.Select(l => l.Confidence));
                            }
                            break;
                        case VisualFeatureTypes.Description:
                            if (APIResponse.Description.Tags.Any())
                            {
                                flatResult.Description_Tags = string.Join("; ", APIResponse.Description.Tags);
                            }
                            var captions = APIResponse.Description.Captions.Where(a => a.Confidence >= FlatteningMinScores[VisualFeatureTypes.Description]);
                            if (captions.Any())
                            {
                                flatResult.Description_Captions = string.Join("; ", captions.Select(l => l.Text));
                                flatResult.Description_Caption_Scores = string.Join("; ", captions.Select(l => l.Confidence));
                            }
                            break;
                        case VisualFeatureTypes.Faces:
                            if (APIResponse.Faces.Any())
                            {
                                flatResult.Face_Ages = string.Join("; ", APIResponse.Faces.Select(l => l.Age));
                                flatResult.Face_Genders = string.Join("; ", APIResponse.Faces.Select(l => l.Gender));
                            }
                            break;
                        case VisualFeatureTypes.Objects:
                            var objects = APIResponse.Objects.Where(a => a.Confidence >= FlatteningMinScores[VisualFeatureTypes.Objects]);
                            if (objects.Any())
                            {
                                flatResult.Objects = string.Join("; ", objects.Select(l => l.ObjectProperty));
                                flatResult.Object_Scores = string.Join("; ", objects.Select(l => l.Confidence));
                            }
                            break;
                        case VisualFeatureTypes.Brands:
                            var brands = APIResponse.Brands.Where(a => a.Confidence >= FlatteningMinScores[VisualFeatureTypes.Brands]);
                            if (brands.Any())
                            {
                                flatResult.Brands = string.Join("; ", brands.Select(l => l.Name));
                                flatResult.Brand_Scores = string.Join("; ", brands.Select(l => l.Confidence));
                            }
                            break;
                        default:
                            break;
                    }
                }

                return flatResult;
            }
        }
    }
}
