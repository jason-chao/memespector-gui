using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Memespector_GUI.APIViewModels
{
    public class GoogleVisionSettingsViewModel : ReactiveObject
    {
        public GoogleVisionSettingsViewModel()
        {
            this.WhenAnyValue(x => x.Detection_Safety, x => x.Detection_Face, x => x.Detection_Label, x => x.Detection_Web, x => x.Detection_Text, x => x.Detection_Landmark, x => x.Detection_Logo).Subscribe(_ => updateFeatureCount());
            this.WhenAnyValue(x => x.SelectedFeatureCount).Subscribe(x => { SelectedFeatureCountText = string.Format("{0} selected", x); });
            updateFeatureCount();
        }

        private string gcsCredentialFileLocation = Utilities.MemespectorConfig.GoogleCloudCredentialFileLocation;
        public string CredentialFileLocation { get => gcsCredentialFileLocation; set => this.RaiseAndSetIfChanged(ref gcsCredentialFileLocation, value); }

        private bool detection_Safety = true;
        public bool Detection_Safety { get => detection_Safety; set => this.RaiseAndSetIfChanged(ref detection_Safety, value); }
        private bool detection_Face = true;
        public bool Detection_Face { get => detection_Face; set => this.RaiseAndSetIfChanged(ref detection_Face, value); }
        private bool detection_Label = true;
        public bool Detection_Label { get => detection_Label; set => this.RaiseAndSetIfChanged(ref detection_Label, value); }
        private bool detection_Web = true;
        public bool Detection_Web { get => detection_Web; set => this.RaiseAndSetIfChanged(ref detection_Web, value); }
        private bool detection_Text = true;
        public bool Detection_Text { get => detection_Text; set => this.RaiseAndSetIfChanged(ref detection_Text, value); }
        private bool detection_Landmark = true;
        public bool Detection_Landmark { get => detection_Landmark; set => this.RaiseAndSetIfChanged(ref detection_Landmark, value); }
        private bool detection_Logo = true;
        public bool Detection_Logo { get => detection_Logo; set => this.RaiseAndSetIfChanged(ref detection_Logo, value); }

        private int selectedFeatureCount = 0;
        public int SelectedFeatureCount { get => selectedFeatureCount; set => this.RaiseAndSetIfChanged(ref selectedFeatureCount, value);  }

        private string selectedFeatureCountText = "Please select ...";
        public string SelectedFeatureCountText { get => selectedFeatureCountText; set => this.RaiseAndSetIfChanged(ref selectedFeatureCountText, value); }

        private void updateFeatureCount()
        {
            SelectedFeatureCount = (new bool[] { detection_Safety, detection_Face, detection_Label, detection_Web, detection_Text, detection_Landmark, detection_Logo }).Where(f => f).Count();
        }
    }
}
