using System;
using System.Collections.Generic;
using System.Text;

namespace CVClient
{
    public class FlatResultOpenSource
    {
        public bool Success { get; set; } = false;

        public string Labels { get; set; } = string.Empty;
        public string Label_Ids { get; set; } = string.Empty;
        public string Label_Scores { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }
}
