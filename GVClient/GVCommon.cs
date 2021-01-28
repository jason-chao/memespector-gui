using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ServiceStack.Text;
using System.Linq;

namespace ResearchGVClient
{
    static public class GVCommon
    {
        // For the list of file formats accepted by Google Vision API, see https://cloud.google.com/vision/docs/supported-files
        static public List<string> GVSupportedFormats = new List<string> { "jpeg", "jpg", "png", "gif", "bmp", "webp", "raw", "ico", "pdf", "tiff", "tif" };

        static public bool MayBeAValidGCSCredentialFile(string filename)
        {
            try
            {
                var gcsCredentialFileProperties = new string[] { "project_id", "private_key_id", "private_key", "client_id" };
                var objectKeys = JsonObject.Parse(File.ReadAllText(filename)).Select(co => co.Key);
                return gcsCredentialFileProperties.All(p => objectKeys.Any(k => k.Contains(p)));
            }
            catch { }
            return false;
        }
    }
}
