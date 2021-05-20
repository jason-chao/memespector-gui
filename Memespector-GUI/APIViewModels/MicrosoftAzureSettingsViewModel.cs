using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Memespector_GUI.APIViewModels
{
    public class MicrosoftAzureSettingsViewModel : ReactiveObject
    {
        public MicrosoftAzureSettingsViewModel()
        {
            this.WhenAnyValue(x => x.Detection_Adult, x => x.Detection_Brands, x => x.Detection_Categories, x => x.Detection_Description, x => x.Detection_Faces, x => x.Detection_Objects, x => x.Detection_Tags).Subscribe(_ => updateFeatureCount());
            this.WhenAnyValue(x => x.SelectedFeatureCount).Subscribe(x => { SelectedFeatureCountText = string.Format("{0} selected", x); });
            updateFeatureCount();
        }
        private string endpoint = Utilities.MemespectorConfig.MicrosoftAzureEndpoint;
        public string Endpoint { get => endpoint; set => this.RaiseAndSetIfChanged(ref endpoint, value); }

        private string subscriptionKey = Utilities.MemespectorConfig.MicrosoftAzureSubscriptionKey;
        public string SubscriptionKey { get => subscriptionKey; set => this.RaiseAndSetIfChanged(ref subscriptionKey, value); }

        private bool detection_Adult = true;
        public bool Detection_Adult { get => detection_Adult; set => this.RaiseAndSetIfChanged(ref detection_Adult, value); } 

        private bool detection_Brands = true;
        public bool Detection_Brands { get => detection_Brands; set => this.RaiseAndSetIfChanged(ref detection_Brands, value); } 

        private bool detection_Categories = true;
        public bool Detection_Categories { get => detection_Categories; set => this.RaiseAndSetIfChanged(ref detection_Categories, value); } 

        private bool detection_Description = true;
        public bool Detection_Description { get => detection_Description; set => this.RaiseAndSetIfChanged(ref detection_Description, value); } 

        private bool detection_Faces = true;
        public bool Detection_Faces { get => detection_Faces; set => this.RaiseAndSetIfChanged(ref detection_Faces, value); } 

        private bool detection_Objects = true;
        public bool Detection_Objects { get => detection_Objects; set => this.RaiseAndSetIfChanged(ref detection_Objects, value); } 

        private bool detection_Tags = true;
        public bool Detection_Tags { get => detection_Tags; set => this.RaiseAndSetIfChanged(ref detection_Tags, value); } 

        private int selectedFeatureCount = 0;
        public int SelectedFeatureCount { get => selectedFeatureCount; set => this.RaiseAndSetIfChanged(ref selectedFeatureCount, value); } 

        private string selectedFeatureCountText = "Please select ...";
        public string SelectedFeatureCountText { get => selectedFeatureCountText; set => this.RaiseAndSetIfChanged(ref selectedFeatureCountText, value); }

        private void updateFeatureCount()
        {
            SelectedFeatureCount = (new bool[] { detection_Adult, detection_Brands, detection_Categories, detection_Description, detection_Faces, detection_Objects, detection_Tags }).Where(f => f).Count();
        }

    }
}
