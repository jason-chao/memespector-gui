//#define DEBUG_DONOTINVOKEREALAPI

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Google.Cloud.Vision.V1;
using Newtonsoft.Json;

namespace ResearchGVClient
{
    public class GVTask
    {
        public string ImageLocation { get; set; }
        public LocationType ImageLocationType { get; set; }
        public enum LocationType { Local, Uri }
        public DateTime? Processed { get; set; } = null;
        public AnnotateImageResponse AnnotationResponse { get; set; } = null;
        public ICollection<Feature.Types.Type> DetectionFeatureTypes { get; set; } = new List<Feature.Types.Type>();
        public Dictionary<Feature.Types.Type, int> FeatureMaxResults { get; set; } = new Dictionary<Feature.Types.Type, int>();

        [JsonIgnore]
        public Exception ExceptionRasied { get; set; } = null;

        [JsonIgnore]
        public Action GVAction { get { if (action == null) action = GetAction(); return action; } }

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
                    AnnotationResponse = iaClient.Annotate(GetAnnotateImageRequest());
#endif

#if DEBUG_DONOTINVOKEREALAPI
                    Random rand = new Random();
                    System.Threading.Thread.Sleep(rand.Next(5000, 15000));
                    throw new Exception("A deliberate exception to end the task for debugging purposes");
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
        public FlatAnnotationResult FlatAnnotationResult
        {
            get
            {

                var flatResult = new FlatAnnotationResult();

                flatResult.Image_Location = ImageLocation;
                flatResult.Image_From = ImageLocationType.ToString();

                if (ImageLocationType == LocationType.Local)
                {
                    var fileInfo = new FileInfo(ImageLocation);
                    flatResult.Image_BaseName = fileInfo.Name;
                }
                else if (ImageLocationType == LocationType.Uri)
                {
                    Uri fileUri = new Uri(ImageLocation);
                    flatResult.Image_BaseName = fileUri.Segments.Last();
                }

                if (ExceptionRasied != null)
                {
                    flatResult.Error = ExceptionRasied.Message;
                }

                if (AnnotationResponse == null)
                {
                    flatResult.Success = false;
                    return flatResult;
                }
                else
                {
                    flatResult.Success = true;
                }

                foreach (var featureType in DetectionFeatureTypes)
                {
                    switch (featureType)
                    {
                        case Feature.Types.Type.FaceDetection:
                            if (AnnotationResponse.FaceAnnotations.Any())
                            {
                                var faceAnnotation = AnnotationResponse.FaceAnnotations.First();
                                flatResult.Face_Joy = faceAnnotation.JoyLikelihood.ToString();
                                flatResult.Face_Sorrow = faceAnnotation.SorrowLikelihood.ToString();
                                flatResult.Face_Anger = faceAnnotation.AngerLikelihood.ToString();
                                flatResult.Face_Surprise = faceAnnotation.SurpriseLikelihood.ToString();
                                flatResult.Face_UnderExposed = faceAnnotation.UnderExposedLikelihood.ToString();
                                flatResult.Face_Blurred = faceAnnotation.BlurredLikelihood.ToString();
                                flatResult.Face_Headwear = faceAnnotation.HeadwearLikelihood.ToString();
                            }
                            break;
                        case Feature.Types.Type.LandmarkDetection:
                            if (AnnotationResponse.LandmarkAnnotations.Any())
                            {
                                flatResult.Landmark_IDs = string.Join("; ", AnnotationResponse.LandmarkAnnotations.Select(l => l.Mid));
                                flatResult.Landmark_Descriptions = string.Join("; ", AnnotationResponse.LandmarkAnnotations.Select(l => l.Description));
                            }
                            break;
                        case Feature.Types.Type.LogoDetection:
                            if (AnnotationResponse.LogoAnnotations.Any())
                            {
                                flatResult.Logo_IDs = string.Join("; ", AnnotationResponse.LogoAnnotations.Select(l => l.Mid));
                                flatResult.Logo_Descriptions = string.Join("; ", AnnotationResponse.LogoAnnotations.Select(l => l.Description));
                            }
                            break;
                        case Feature.Types.Type.LabelDetection:
                            if (AnnotationResponse.LabelAnnotations.Any())
                            {
                                flatResult.Label_IDs = string.Join("; ", AnnotationResponse.LabelAnnotations.Select(l => l.Mid));
                                flatResult.Label_Descriptions = string.Join("; ", AnnotationResponse.LabelAnnotations.Select(l => l.Description));
                            }
                            break;
                        case Feature.Types.Type.TextDetection:
                            if (AnnotationResponse.FullTextAnnotation != null)
                            {
                                flatResult.Text = AnnotationResponse.FullTextAnnotation.Text;
                            }
                            break;
                        case Feature.Types.Type.SafeSearchDetection:
                            if (AnnotationResponse.SafeSearchAnnotation != null)
                            {
                                flatResult.Safe_Adult = AnnotationResponse.SafeSearchAnnotation.Adult.ToString();
                                flatResult.Safe_Spoof = AnnotationResponse.SafeSearchAnnotation.Spoof.ToString();
                                flatResult.Safe_Medical = AnnotationResponse.SafeSearchAnnotation.Medical.ToString();
                                flatResult.Safe_Violence = AnnotationResponse.SafeSearchAnnotation.Violence.ToString();
                                flatResult.Safe_Racy = AnnotationResponse.SafeSearchAnnotation.Racy.ToString();
                            }
                            break;
                        case Feature.Types.Type.WebDetection:
                            if (AnnotationResponse.WebDetection != null)
                            {
                                flatResult.Web_Entity_IDs = string.Join("; ", AnnotationResponse.WebDetection.WebEntities.Select(l => l.EntityId));
                                flatResult.Web_Entity_Descriptions = string.Join("; ", AnnotationResponse.WebDetection.WebEntities.Select(l => l.Description));

                                flatResult.Web_FullMatchingImages = string.Join(" ; ", AnnotationResponse.WebDetection.FullMatchingImages.Select(i => i.Url));
                                flatResult.Web_PartialMatchingImages = string.Join(" ; ", AnnotationResponse.WebDetection.PartialMatchingImages.Select(i => i.Url));
                                flatResult.Web_PagesWithFullMatchingImages = string.Join(" ; ", AnnotationResponse.WebDetection.PagesWithMatchingImages.Select(i => i.Url));
                                flatResult.Web_VisuallySimilarImages = string.Join(" ; ", AnnotationResponse.WebDetection.VisuallySimilarImages.Select(i => i.Url));
                                flatResult.Web_BestGuessLabels = string.Join(";", AnnotationResponse.WebDetection.BestGuessLabels.Select(l => l.Label));
                            }
                            break;
                        default:
                            break;
                    }
                }

                return flatResult;
            }
        }

        public AnnotateImageRequest GetAnnotateImageRequest()
        {
            var request = new AnnotateImageRequest();

            if (ImageLocationType == GVTask.LocationType.Local)
            {
                request.Image = Image.FromFile(ImageLocation);
            }
            else if (ImageLocationType == GVTask.LocationType.Uri)
            {
                request.Image = Image.FromUri(ImageLocation);
            }

            foreach (var feature in DetectionFeatureTypes)
            {
                var newFeature = new Feature() { Type = feature };

                if (FeatureMaxResults.Keys.Contains(feature))
                {
                    if (FeatureMaxResults[feature] >= 0)
                        newFeature.MaxResults = FeatureMaxResults[feature];
                }

                request.Features.Add(newFeature);
            }

            return request;
        }

    }
}
