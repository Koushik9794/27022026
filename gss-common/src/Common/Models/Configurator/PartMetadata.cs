using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GssCommon.Common.Models.Configurator
{
    public class PartMetadata 
    {
        [JsonPropertyName("part_code")] 
        public string PartCode { get; set; }
        
        [JsonPropertyName("description")] 
        public string Description { get; set; }
        
        [JsonPropertyName("short_description")] 
        public string ShortDescription { get; set; }
        
        [JsonPropertyName("component_group_code")]
        public string ComponentGroupCode { get; set; }
        
        [JsonPropertyName("component_group_name")]
        public string ComponentGroupName { get; set; }
        
        [JsonPropertyName("component_type_code")]
        public string ComponentTypeCode { get; set; }
        
        [JsonPropertyName("component_type_name")]
        public string ComponentTypeName { get; set; }
        
        [JsonPropertyName("component_name_code")]
        public string ComponentNameCode { get; set; }
        
        [JsonPropertyName("component_name_name")]
        public string ComponentNameName { get; set; }
        
        [JsonPropertyName("attributes")]
        public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
        
        public double? WeightKg { get; set; }
        public double? InstallTimeMins { get; set; }
        public string UoM { get; set; }
        
        [JsonPropertyName("unspsc_code")]
        public string UNSPSCCode { get; set; }
        
        [JsonPropertyName("drawing_no")]
        public string DrawingNo { get; set; }
        
        [JsonPropertyName("rev_no")]
        public string RevNo { get; set; }
        
        [JsonPropertyName("colour")]
        public string Colour { get; set; }
        
        [JsonPropertyName("gfa_flag")]
        public bool GfaFlag { get; set; }
        
        [JsonPropertyName("cbm")]
        public double? CBM { get; set; }
        
        [JsonPropertyName("unit_basic_price")]
        public double? UnitBasicPrice { get; set; }
        
        public double DiscountPercent { get; set; }
        
        public string Family => ComponentTypeCode;
        public Dictionary<string, object> Attr => Attributes;
    }
}
