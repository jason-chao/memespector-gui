using System;
using System.Collections.Generic;
using System.Text;

namespace CVClient
{
    public class FlatResultAzure
    {
        public bool Success { get; set; } = false;

        public string Categories { get; set; } = string.Empty;
        public string Categories_Scores { get; set; } = string.Empty;
        public bool? Adult_Adult { get; set; } = null;
        public bool? Adult_Racy { get; set; } = null;
        public bool? Adult_Gory { get; set; } = null;
        public string Adult_Adult_Score { get; set; } = null;
        public string Adult_Racy_Score { get; set; } = null;
        public string Adult_Gory_Score { get; set; } = null;
        public string Tags { get; set; } = string.Empty;
        public string Tag_Scores { get; set; } = string.Empty;
        public string Description_Tags { get; set; } = string.Empty;
        public string Description_Captions { get; set; } = string.Empty;
        public string Description_Caption_Scores { get; set; } = string.Empty;
        public string Face_Ages { get; set; } = string.Empty;
        public string Face_Genders { get; set; } = string.Empty;
        public string Objects { get; set; } = string.Empty;
        public string Object_Scores { get; set; } = string.Empty;
        public string Brands { get; set; } = string.Empty;
        public string Brand_Scores { get; set; } = string.Empty;

        public string Error { get; set; } = string.Empty;
    }
}
