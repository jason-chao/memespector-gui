//#define DEBUG_DONOTINVOKEREALAPI

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using Clarifai.Api;
using Clarifai.Channels;
using Grpc.Core;

namespace CVClient
{
    public class InvocationClarifai : InvocationBase
    {
        public InvocationClarifai()
        {
            API = CVClientCommon.VisionAPI.Clarifai;
        }

        public MultiOutputResponse APIResponse { get; set; } = null;

        private string model = null;

        public string Model { get { return model; } set { if (ModelsAvailable.Keys.Contains(value)) model = value; else throw new Exception("This model does not exist"); } }
        public double FlatteningMinScore { get; set; } = 0;

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

                    var client = new V2.V2Client(ClarifaiChannel.Grpc());

                    var metadata = new Metadata { { "Authorization", $"Key {Environment.GetEnvironmentVariable("CLARIFAI_API_KEY")}" } };

                    PostModelOutputsRequest postModelOutputRequest = null;

                    if (ImageLocationType == CVClientCommon.LocationType.Uri)
                    {
                        postModelOutputRequest = new PostModelOutputsRequest()
                        {
                            ModelId = "aaa03c23b3724a16a56b629203edc62c",
                            Inputs = { new List<Input>() { new Input() { Data = new Data() { Image = new Image() { Url = ImageLocation } } } } }
                        };
                    }
                    else if (ImageLocationType == CVClientCommon.LocationType.Local)
                    {
                        using (Stream imageStream = File.OpenRead(ImageLocation))
                        {
                            postModelOutputRequest = new PostModelOutputsRequest()
                            {
                                ModelId = "aaa03c23b3724a16a56b629203edc62c",
                                Inputs = { new List<Input>() { new Input() { Data = new Data() { Image = new Image() { Base64 = Google.Protobuf.ByteString.FromStream(imageStream) } } } } }
                            };
                        }
                    }

                    APIResponse = client.PostModelOutputs(postModelOutputRequest, metadata);

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
        public FlatResultClarifai FlatResult
        {
            get
            {
                var flatResult = new FlatResultClarifai();

                if (ExceptionRasied != null)
                {
                    flatResult.Error = ExceptionRasied.Message;
                }

                if (APIResponse != null)
                {
                    if (APIResponse.Status.Code == Clarifai.Api.Status.StatusCode.Success)
                    {
                        flatResult.Success = true;
                    }
                }

                if (!flatResult.Success)
                    return flatResult;

                var concepts = APIResponse.Outputs[0].Data.Concepts.Where(a => a.Value >= FlatteningMinScore);

                if (concepts.Any())
                {
                    flatResult.Concepts = string.Join("; ", concepts.Select(l => l.Name));
                    flatResult.Concept_Ids = string.Join("; ", concepts.Select(l => l.Id));
                    flatResult.Concept_Scores = string.Join("; ", concepts.Select(l => l.Value));
                }

                flatResult.Model = model;

                return flatResult;
            }
        }
    }
}
