using System.Collections.Generic;

namespace GssCommon.Common.Models.Configurator
{
    // Represents a node in the hierarchical blueprint tree
    public class HierarchicalBlueprint 
    {
        public string ProductGroup { get; set; }
        public List<string> Rules { get; set; } = new List<string>();
        public Dictionary<string, HierarchicalBlueprint> Components { get; set; } = new Dictionary<string, HierarchicalBlueprint>();
    }

    // Flattened blueprint for efficient lookup
    public class FlattenedBlueprint 
    {
        public string ComponentPath { get; set; }  // e.g., "SPR_Unit.Structural_System.Frame_System.Upright"
        public string ProductGroup { get; set; }
        public List<string> Rules { get; set; } = new List<string>();
        public Dictionary<string, double> Defaults { get; set; } = new Dictionary<string, double>();
    }
}
