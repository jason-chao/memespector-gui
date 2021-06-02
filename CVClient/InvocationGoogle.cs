// #define DEBUG_DONOTINVOKEREALAPI

using System;
using System.Collections.Generic;
using System.Linq;
using Google.Cloud.Vision.V1;
using Newtonsoft.Json;

namespace CVClient
{
    public class InvocationGoogle : InvocationBase
    {
        public InvocationGoogle()
        {
            API = CVClientCommon.VisionAPI.GoogleCloud;
        }

        public AnnotateImageResponse APIResponse { get; set; } = null;

        public ICollection<Feature.Types.Type> DetectionFeatureTypes { get; set; } = new List<Feature.Types.Type>();
        public Dictionary<Feature.Types.Type, int> DetectionMaxResults { get; set; } = new Dictionary<Feature.Types.Type, int> {
            { Feature.Types.Type.SafeSearchDetection, 0 },
            { Feature.Types.Type.FaceDetection, 0 },
            { Feature.Types.Type.LabelDetection, 0 },
            { Feature.Types.Type.WebDetection, 0 },
            { Feature.Types.Type.TextDetection, 0 },
            { Feature.Types.Type.LandmarkDetection, 0 },
            { Feature.Types.Type.LogoDetection, 0 }
        };
        public Dictionary<Feature.Types.Type, float> FlatteningMinScores { get; set; } = new Dictionary<Feature.Types.Type, float> {
            { Feature.Types.Type.SafeSearchDetection, 0 },
            { Feature.Types.Type.FaceDetection, 0 },
            { Feature.Types.Type.LabelDetection, 0 },
            { Feature.Types.Type.WebDetection, 0 },
            { Feature.Types.Type.TextDetection, 0 },
            { Feature.Types.Type.LandmarkDetection, 0 },
            { Feature.Types.Type.LogoDetection, 0 }
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
                    var iaClient = ImageAnnotatorClient.Create();
                    APIResponse = iaClient.Annotate(GetAnnotateImageRequest());
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

        public AnnotateImageRequest GetAnnotateImageRequest()
        {
            var request = new AnnotateImageRequest();

            if (ImageLocationType == CVClientCommon.LocationType.Local)
            {
                request.Image = Image.FromFile(ImageLocation);
            }
            else if (ImageLocationType == CVClientCommon.LocationType.Uri)
            {
                request.Image = Image.FromUri(ImageLocation);
            }

            foreach (var feature in DetectionFeatureTypes)
            {
                var newFeature = new Feature() { Type = feature };

                if (DetectionMaxResults.Keys.Contains(feature))
                {
                    if (DetectionMaxResults[feature] > 0)
                        newFeature.MaxResults = DetectionMaxResults[feature];
                }

                request.Features.Add(newFeature);
            }

            return request;
        }


        [JsonIgnore]
        public FlatResultGoogle FlatResult
        {
            get
            {
                var flatResult = new FlatResultGoogle();

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
                        case Feature.Types.Type.FaceDetection:
                            var faceAnnotations = APIResponse.FaceAnnotations.Where(a => a.DetectionConfidence >= FlatteningMinScores[Feature.Types.Type.FaceDetection]);
                            if (faceAnnotations.Any())
                            {
                                flatResult.Face_Joy = string.Join("; ", faceAnnotations.Select(l => l.JoyLikelihood.ToString()));
                                flatResult.Face_Sorrow = string.Join("; ", faceAnnotations.Select(l => l.SorrowLikelihood.ToString()));
                                flatResult.Face_Anger = string.Join("; ", faceAnnotations.Select(l => l.AngerLikelihood.ToString()));
                                flatResult.Face_Surprise = string.Join("; ", faceAnnotations.Select(l => l.SurpriseLikelihood.ToString()));
                                flatResult.Face_UnderExposed = string.Join("; ", faceAnnotations.Select(l => l.UnderExposedLikelihood.ToString()));
                                flatResult.Face_Blurred = string.Join("; ", faceAnnotations.Select(l => l.BlurredLikelihood.ToString()));
                                flatResult.Face_Headwear = string.Join("; ", faceAnnotations.Select(l => l.HeadwearLikelihood.ToString()));

                                flatResult.Face_Score = string.Join("; ", faceAnnotations.Select(l => l.DetectionConfidence.ToString()));
                            }
                            break;
                        case Feature.Types.Type.LandmarkDetection:
                            var landmarkAnnotations = APIResponse.LandmarkAnnotations.Where(a => a.Score >= FlatteningMinScores[Feature.Types.Type.LandmarkDetection]);
                            if (landmarkAnnotations.Any())
                            {
                                flatResult.Landmark_Ids = string.Join("; ", landmarkAnnotations.Select(l => l.Mid));
                                flatResult.Landmark_Descriptions = string.Join("; ", landmarkAnnotations.Select(l => l.Description));
                                flatResult.Landmark_Scores = string.Join("; ", landmarkAnnotations.Select(l => l.Score));
                            }
                            break;
                        case Feature.Types.Type.LogoDetection:
                            var logoAnnotations = APIResponse.LogoAnnotations.Where(a => a.Score >= FlatteningMinScores[Feature.Types.Type.LogoDetection]);
                            if (logoAnnotations.Any())
                            {
                                flatResult.Logo_Ids = string.Join("; ", logoAnnotations.Select(l => l.Mid));
                                flatResult.Logo_Descriptions = string.Join("; ", logoAnnotations.Select(l => l.Description));
                                flatResult.Logo_Scores = string.Join("; ", logoAnnotations.Select(l => l.Score));
                            }
                            break;
                        case Feature.Types.Type.LabelDetection:
                            var labelAnnotations = APIResponse.LabelAnnotations.Where(a => a.Score >= FlatteningMinScores[Feature.Types.Type.LabelDetection]);
                            if (labelAnnotations.Any())
                            {
                                flatResult.Label_Ids = string.Join("; ", labelAnnotations.Select(l => l.Mid));
                                flatResult.Label_Descriptions = string.Join("; ", labelAnnotations.Select(l => l.Description));
                                flatResult.Label_Scores = string.Join("; ", labelAnnotations.Select(l => l.Score));
                            }
                            break;
                        case Feature.Types.Type.TextDetection:
                            if (APIResponse.FullTextAnnotation != null)
                            {
                                flatResult.Text = APIResponse.FullTextAnnotation.Text;
                            }
                            break;
                        case Feature.Types.Type.SafeSearchDetection:
                            if (APIResponse.SafeSearchAnnotation != null)
                            {
                                flatResult.Safe_Adult = APIResponse.SafeSearchAnnotation.Adult.ToString();
                                flatResult.Safe_Spoof = APIResponse.SafeSearchAnnotation.Spoof.ToString();
                                flatResult.Safe_Medical = APIResponse.SafeSearchAnnotation.Medical.ToString();
                                flatResult.Safe_Violence = APIResponse.SafeSearchAnnotation.Violence.ToString();
                                flatResult.Safe_Racy = APIResponse.SafeSearchAnnotation.Racy.ToString();
                            }
                            break;
                        case Feature.Types.Type.WebDetection:
                            if (APIResponse.WebDetection != null)
                            {
                                float webMinScore = FlatteningMinScores[Feature.Types.Type.WebDetection];
                                var webEntities = APIResponse.WebDetection.WebEntities.Where(a => a.Score >= webMinScore);
                                flatResult.Web_Entity_Ids = string.Join("; ", webEntities.Select(l => l.EntityId));
                                flatResult.Web_Entity_Descriptions = string.Join("; ", webEntities.Select(l => l.Description));
                                flatResult.Web_Entity_Scores = string.Join("; ", webEntities.Select(l => l.Score));

                                flatResult.Web_FullMatchingImages = string.Join(" ; ", APIResponse.WebDetection.FullMatchingImages.Select(i => i.Url));
                                flatResult.Web_PartialMatchingImages = string.Join(" ; ", APIResponse.WebDetection.PartialMatchingImages.Select(i => i.Url));
                                flatResult.Web_PagesWithFullMatchingImages = string.Join(" ; ", APIResponse.WebDetection.PagesWithMatchingImages.Select(i => i.Url));
                                flatResult.Web_VisuallySimilarImages = string.Join(" ; ", APIResponse.WebDetection.VisuallySimilarImages.Select(i => i.Url));
                                flatResult.Web_BestGuessLabels = string.Join("; ", APIResponse.WebDetection.BestGuessLabels.Select(l => l.Label));
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
