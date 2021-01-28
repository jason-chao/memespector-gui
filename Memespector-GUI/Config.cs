using System;
using System.Collections.Generic;
using System.Text;

namespace Memespector_GUI
{

    public class Config
    {
        public string GCSCredentialFileLocation { get; set; } = string.Empty;
        public MaxResults FeatureMaxResults { get; set; } = new MaxResults();
    }

    public class MaxResults
    {
        public int Face { get; set; } = 1;
        public int Label { get; set; } = 10;
        public int Web { get; set; } = 10;
        public int Landmark { get; set; } = 10;
        public int Logo { get; set; } = 10;
    }
}
