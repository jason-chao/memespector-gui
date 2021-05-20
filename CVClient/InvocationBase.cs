using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CVClient
{
    public class InvocationBase
    {
        [JsonIgnore]
        public CVClientCommon.VisionAPI? API { get; set; } = null;

        public string ImageLocation { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public CVClientCommon.LocationType ImageLocationType { get; set; }
        
        public DateTime? Processed { get; set; } = null;

        [JsonIgnore]
        public Exception ExceptionRasied { get; set; } = null;
    }
}
