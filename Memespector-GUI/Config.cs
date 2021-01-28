using System;
using System.Collections.Generic;
using System.Text;

namespace Memespector_GUI
{

    public class Config
    {
        public string GCSCredentialFileLocation { get; set; } = string.Empty;
        public int MaxConcurrentAPICalls { get; set; } = 5;
        public FeatureMaxResults Feature_MaxResults { get; set; } = new FeatureMaxResults();
        public FeatureMinScores Feature_FlatteningMinScores { get; set; } = new FeatureMinScores();

        public class FeatureMaxResults
        {
            public int Face { get; set; } = 1;
            public int Label { get; set; } = 10;
            public int Web { get; set; } = 10;
            public int Landmark { get; set; } = 10;
            public int Logo { get; set; } = 10;
        }

        public class FeatureMinScores
        {
            public float Label { get; set; } = 0;
            public float Web { get; set; } = 0;
            public float Landmark { get; set; } = 0;
            public float Logo { get; set; } = 0;
        }
    }
}
