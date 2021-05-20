using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;

using CVClient;

namespace Memespector_GUI.APIViewModels
{
    public class ClarifaiSettingsViewModel : ReactiveObject
    {
        public ClarifaiSettingsViewModel() { Model = AvailableModels.First();  }

        private string apiKey = Utilities.MemespectorConfig.ClarifaiAPIKey;
        public string APIKey { get => apiKey; set => this.RaiseAndSetIfChanged(ref apiKey, value); }

        private string model = string.Empty;
        public string Model { get => model; set => this.RaiseAndSetIfChanged(ref model, value); }

        public List<string> AvailableModels
        {
            get
            {
                return new List<string>(CVClientCommon.GetAPIAvailableModels(CVClientCommon.VisionAPI.Clarifai).Keys);
            }
        }
    }
}
