using System;
using System.Collections.Generic;
using System.Text;

namespace ResearchGVClient
{
    public class FlatAnnotationResult
    {
        public string Image_Location { get; set; }
        public string Image_From { get; set; }
        public string Image_BaseName { get; set; }
        public bool Success { get; set; }

        public string Safe_Adult { get; set; } = string.Empty;
        public string Safe_Spoof { get; set; } = string.Empty;
        public string Safe_Medical { get; set; } = string.Empty;
        public string Safe_Violence { get; set; } = string.Empty;
        public string Safe_Racy { get; set; } = string.Empty;

        public string Face_Joy { get; set; } = string.Empty;
        public string Face_Sorrow { get; set; } = string.Empty;
        public string Face_Anger { get; set; } = string.Empty;
        public string Face_Surprise { get; set; } = string.Empty;
        public string Face_UnderExposed { get; set; } = string.Empty;
        public string Face_Blurred { get; set; } = string.Empty;
        public string Face_Headwear { get; set; } = string.Empty;

        public string Label_Descriptions { get; set; } = string.Empty;
        public string Label_IDs { get; set; } = string.Empty;

        public string Logo_Descriptions { get; set; } = string.Empty;
        public string Logo_IDs { get; set; } = string.Empty;

        public string Landmark_Descriptions { get; set; } = string.Empty;
        public string Landmark_IDs { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public string Web_Entity_Descriptions { get; set; } = string.Empty;
        public string Web_Entity_IDs { get; set; } = string.Empty;

        public string Web_FullMatchingImages { get; set; } = string.Empty;
        public string Web_PagesWithFullMatchingImages { get; set; } = string.Empty;
        public string Web_PartialMatchingImages { get; set; } = string.Empty;
        public string Web_VisuallySimilarImages { get; set; } = string.Empty;
        public string Web_BestGuessLabels { get; set; } = string.Empty;

        public string Error { get; set; }
    }
}
