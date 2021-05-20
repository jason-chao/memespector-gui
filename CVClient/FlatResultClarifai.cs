using System;
using System.Collections.Generic;
using System.Text;

namespace CVClient
{
    public class FlatResultClarifai
    {
        public bool Success { get; set; } = false;

        public string Concepts { get; set; } = string.Empty;
        public string Concept_Ids { get; set; } = string.Empty;
        public string Concept_Scores { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }
}
