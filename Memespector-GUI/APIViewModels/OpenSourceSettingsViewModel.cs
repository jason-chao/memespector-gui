using CVClient;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Memespector_GUI.APIViewModels
{
    public class OpenSourceSettingsViewModel : ReactiveObject
    {
        public OpenSourceSettingsViewModel() { Model = AvailableModels.First(); }

        private string endpoint = Utilities.MemespectorConfig.OpenSourceEndpoint;
        public string Endpoint { get => endpoint; set => this.RaiseAndSetIfChanged(ref endpoint, value); }

        private string model = string.Empty;
        public string Model { get => model; set => this.RaiseAndSetIfChanged(ref model, value); }

        public List<string> AvailableModels
        {
            get
            {
                return new List<string>(CVClientCommon.GetAPIAvailableModels(CVClientCommon.VisionAPI.OpenSource).Keys);
            }
        }
    }
}
