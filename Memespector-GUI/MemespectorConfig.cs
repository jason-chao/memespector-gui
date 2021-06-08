using System;
using System.Collections.Generic;
using System.Text;

namespace Memespector_GUI
{

    public class MemespectorConfig
    {
        public bool SelectedGoogleVision { get; set; } = false;
        public bool SelectedMicrosoftAzure { get; set; } = false;
        public bool SelectedClarifai { get; set; } = false;
        public bool SelectedOpenSource { get; set; } = false;

        public string GoogleCloudCredentialFileLocation { get; set; } = string.Empty;
        public string MicrosoftAzureSubscriptionKey { get; set; } = string.Empty;
        public string MicrosoftAzureEndpoint { get; set; } = string.Empty;
        public string ClarifaiAPIKey { get; set; } = string.Empty;
        public string OpenSourceEndpoint { get; set; } = "https://europe-west1-digital-methods-resources.cloudfunctions.net/classify_image_v1";

        public int MaxConcurrentImageTasks { get; set; } = Environment.ProcessorCount;

        public GoogleFeatureMaxResults GoogleVision_MaxResults { get; set; } = new GoogleFeatureMaxResults();
        public GoogleFeatureMinScores GoogleVision_CSV_MinScores { get; set; } = new GoogleFeatureMinScores();
        public MicrosoftFeatureMinScores MicrosoftAzure_CSV_MinScores { get; set; } = new MicrosoftFeatureMinScores();

        public int MinUIProgressUpdateIntervalInMilliseconds { get; set; } = 250;
        public int MinWriteResultsIntervalInSeconds { get; set; } = 15;
        public int MaxUIImageSourceLinesToDisplay { get; set; } = 256;

        public float ClarifaiMinScore { get; set; } = 0;
        public float OpenSourceMinScore { get; set; } = 0;
        public int OpenSourceMaxResults { get; set; } = 10;

        public class MicrosoftFeatureMinScores
        {
            public float Categories { get; set; } = 0;
            public float Description { get; set; } = 0;
            public float Tags { get; set; } = 0;
            public float Brands { get; set; } = 0;
            public float Objects { get; set; } = 0;
        }

        public class GoogleFeatureMaxResults
        {
            public int Face { get; set; } = 0;
            public int Label { get; set; } = 0;
            public int Web { get; set; } = 0;
            public int Landmark { get; set; } = 0;
            public int Logo { get; set; } = 0;
        }

        public class GoogleFeatureMinScores
        {
            public float Label { get; set; } = 0;
            public float Web { get; set; } = 0;
            public float Landmark { get; set; } = 0;
            public float Logo { get; set; } = 0;
            public float Face { get; set; } = 0;
        }
    }
}
