using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.IO;
using DynamicExpresso;

namespace GSS.RulesEngine.DecoupledPoc
{
    // --- 1. CORE AGNOSTIC MODELS ---
    
    public enum BomType { EBOM, MBOM, IBOM }

    public class BomItem {
        public string SKU { get; set; }
        public int Qty { get; set; }
        public BomType Category { get; set; }
    }

    public class GenericComponent 
    {
        public string Type { get; set; }
        public GenericComponent Parent { get; set; }
        public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, double> Defaults { get; set; } = new Dictionary<string, double>();
        public List<string> Rules { get; set; } = new List<string>();
        public List<GenericComponent> Children { get; set; } = new List<GenericComponent>();

        // Safe accessors for DSL (handles all JsonValueKind variations)
        public double GetNum(string key) {
            // Path-based lookup for Defaults (e.g., "Defaults.MAX_RACK_HEIGHT")
            if (key.StartsWith("Defaults.", StringComparison.OrdinalIgnoreCase)) {
                string defaultKey = key.Substring(9);
                if (Defaults.TryGetValue(defaultKey, out double defVal)) return defVal;
                // Check parent for defaults if not found locally
                return Parent?.GetNum(key) ?? 0;
            }

            if (Attributes.TryGetValue(key, out var val)) {
                if (val is JsonElement elem) {
                    if (elem.ValueKind == JsonValueKind.Number) return elem.GetDouble();
                    if (elem.ValueKind == JsonValueKind.String && double.TryParse(elem.GetString(), out var d)) return d;
                    return 0; 
                }
                if (val is double dbl) return dbl;
                if (val is int i) return i;
                return 0;
            }
            
            // Recursive check in parent if not found (useful for inherited variables)
            return Parent?.GetNum(key) ?? 0;
        }

        public bool GetBool(string key) {
            if (Attributes.TryGetValue(key, out var val)) {
                if (val is JsonElement elem) {
                    if (elem.ValueKind == JsonValueKind.True) return true;
                    if (elem.ValueKind == JsonValueKind.False) return false;
                    if (elem.ValueKind == JsonValueKind.String && bool.TryParse(elem.GetString(), out var b)) return b;
                    return false;
                }
                if (val is bool bval) return bval;
                return false;
            }
            return false;
        }
        public void ExplodeParallel(ExplosionContext ctx, RuleCompiler compiler)
        {
            foreach (var ruleText in Rules)
            {
                var ruleAction = compiler.GetCompiledRule(ruleText);
                try {
                    ruleAction(this, ctx);
                } catch (Exception ex) {
                    var msg = $"[RULE FAIL] {Type} | Rule: {ruleText} | Error: {ex.Message}";
                    if (ex.InnerException != null) msg += $" | Inner: {ex.InnerException.Message}";
                    ctx.Errors.Add(msg);
                }
            }

            foreach (var child in Children) child.ExplodeParallel(ctx, compiler);
        }
    }

    public class ExplosionContext {
        public ConcurrentBag<BomItem> Items { get; } = new ConcurrentBag<BomItem>();
        public ConcurrentBag<string> SystemLinks { get; } = new ConcurrentBag<string>();
        public ConcurrentBag<string> Errors { get; } = new ConcurrentBag<string>();

        // Extreme robustness for DSL quantity
        public bool ADD_BOM(string sku, object qty, string type) {
            double q = 0;
            if (qty is JsonElement elem) {
                if (elem.ValueKind == JsonValueKind.Number) q = elem.GetDouble();
                else if (elem.ValueKind == JsonValueKind.String) double.TryParse(elem.GetString(), out q);
            } else {
                try { q = Convert.ToDouble(qty); } catch { q = 0; }
            }

            var category = Enum.Parse<BomType>(type.ToUpper());
            Items.Add(new BomItem { SKU = sku, Qty = (int)Math.Max(0, Math.Floor(q)), Category = category });
            return true;
        }

        public bool REGISTER_LINK(string linkId) {
            SystemLinks.Add(linkId);
            return true;
        }
        
        public bool VALIDATE(string message) {
            Errors.Add(message);
            return true;
        }
    }

    public class PartMetadata {
        [JsonPropertyName("part_code")]
        public string PartCode { get; set; }  // SKU identifier
        
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
        
        public double? WeightKg { get; set; }  // Calculated from attributes["Unit weight"]
        public double? InstallTimeMins { get; set; }  // Not in JSON, kept for compatibility
        public string UoM { get; set; }  // Not in JSON, kept for compatibility
        
        // Quotation fields
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
        
        public double DiscountPercent { get; set; }  // Not in JSON, default to 0
        
        // Legacy compatibility
        public string Family => ComponentTypeCode;  // Map to old field name
        public Dictionary<string, object> Attr => Attributes;  // Map to old field name
    }

    // --- 2. DECOUPLED LOADING & COMPILATION ---
    public class RuleCompiler {
        private readonly Interpreter _interpreter = new Interpreter();
        private readonly ConcurrentDictionary<string, Action<GenericComponent, ExplosionContext>> _cache = new();

        public RuleCompiler(Func<string, object, string, bool> bomAction, Func<string, bool> linkAction, Func<string, bool> validateAction, Dictionary<string, PartMetadata> partMaster) {
            _interpreter.SetFunction("ADD_BOM", bomAction);
            _interpreter.SetFunction("REGISTER_LINK", linkAction);
            _interpreter.SetFunction("VALIDATE", validateAction);
            _interpreter.Reference(typeof(Math));
            _interpreter.SetFunction("MIN", new Func<double, double, double>(Math.Min));
            _interpreter.SetFunction("MAX", new Func<double, double, double>(Math.Max));
            _interpreter.SetFunction("IF", new Func<bool, object?, object?, object?>((cond, t, f) => cond ? t : f));

            // 1. DYNAMIC LOOKUP (For finite lists in Item Master)
            _interpreter.SetFunction("LOOKUP", new Func<string, string, object, string>((componentType, attrName, val) => {
                double targetVal = 0;
                try { targetVal = Convert.ToDouble(val); } catch { targetVal = (val is bool b && b) ? 1.0 : 0.0; }

                // Helper function to extract numeric value from attribute
                double GetAttributeValue(PartMetadata p, string key) {
                    if (p.Attributes != null && p.Attributes.ContainsKey(key)) {
                        object attrVal = p.Attributes[key];
                        if (attrVal is JsonElement je) {
                            if (je.ValueKind == JsonValueKind.Number) return je.GetDouble();
                            if (je.ValueKind == JsonValueKind.String && double.TryParse(je.GetString(), out double d)) return d;
                        } else if (attrVal is string s && double.TryParse(s, out double dbl)) {
                            return dbl;
                        } else {
                            try { return Convert.ToDouble(attrVal); } catch { }
                        }
                    }

                    // Fallback: Parse from description if searching for Height or Span
                    string combined = (p.Description + " " + p.ShortDescription + " " + p.PartCode).ToUpper();
                    if (key.Equals("Span", StringComparison.OrdinalIgnoreCase) || key.Equals("Length(mm)", StringComparison.OrdinalIgnoreCase)) {
                        // Look for patterns like "L=4000", "L4000", "4000x", "x4000"
                        var matches = System.Text.RegularExpressions.Regex.Matches(combined, @"(?<=\D|^)(\d{3,5})(?=\D|$)");
                        foreach (System.Text.RegularExpressions.Match m in matches) {
                            if (double.TryParse(m.Value, out double d) && Math.Abs(d - targetVal) < 10) return d;
                        }
                    } else if (key.Equals("Height", StringComparison.OrdinalIgnoreCase)) {
                        var matches = System.Text.RegularExpressions.Regex.Matches(combined, @"(?<=\D|^)(\d{3,5})(?=\D|$)");
                        foreach (System.Text.RegularExpressions.Match m in matches) {
                            if (double.TryParse(m.Value, out double d) && Math.Abs(d - targetVal) < 10) return d;
                        }
                    }
                    
                    return 0;
                }

                var matches = partMaster.Values
                    .Where(p => {
                        // Identify component type via PartCode, Description or ComponentTypeCode
                        string typeSearch = (p.PartCode + " " + p.Description + " " + (p.ComponentTypeCode ?? "")).ToUpper();
                        if (!typeSearch.Contains(componentType.ToUpper())) {
                            // Specialized maps
                            if (componentType.Equals("UPRIGHT", StringComparison.OrdinalIgnoreCase) && typeSearch.Contains("UPR")) return true;
                            if (componentType.Equals("BEAM", StringComparison.OrdinalIgnoreCase) && (typeSearch.Contains("BM") || typeSearch.Contains("GBX") || typeSearch.Contains("GSB"))) return true;
                            if (componentType.Equals("HORIZONTAL-BRACING", StringComparison.OrdinalIgnoreCase) && typeSearch.Contains("HORZ")) return true;
                            if (componentType.Equals("DIAGONAL-BRACING", StringComparison.OrdinalIgnoreCase) && typeSearch.Contains("DIAG")) return true;
                            if (componentType.Equals("STABILITY", StringComparison.OrdinalIgnoreCase) && (typeSearch.Contains("STIFFNER") || typeSearch.Contains("TIEBEAM"))) return true;
                            return false;
                        }
                        return true;
                    })
                    .ToList();

                var bestMatch = matches
                    .Where(p => {
                        var attrValue = GetAttributeValue(p, attrName);
                        // If it's a "HeavyDuty" boolean check for Baseplate
                        if (attrName.Equals("HeavyDuty", StringComparison.OrdinalIgnoreCase)) {
                            double thickness = GetAttributeValue(p, "Thickness");
                            return (targetVal > 0) ? thickness >= 8.0 : thickness < 8.0;
                        }
                        
                        return attrValue >= (targetVal - 0.1);
                    })
                    .OrderBy(p => GetAttributeValue(p, attrName))
                    .FirstOrDefault();

                if (bestMatch != null) {
                    return bestMatch.PartCode ?? "UNKNOWN-PART";
                }
                
                // Absolute fallback: Just find the first part of that type if we are desperate
                if (matches.Any()) {
                     // return matches.First().PartCode; // Uncomment if we want approximate matches
                }

                return $"MISSING-{componentType}-{attrName}-{targetVal}";
            }));

            // 2. PATTERN-BASED GENERATION (For thousands of variants like Uprights)
            _interpreter.SetFunction("GENERATE_SKU", new Func<string, string, double, string>((prefix, gauge, height) => {
                return $"{prefix}-{gauge}-{(int)height}";
            }));
        }

        public Action<GenericComponent, ExplosionContext> GetCompiledRule(string ruleText) {
            return _cache.GetOrAdd(ruleText, t => {
                try {
                    // Console.WriteLine($"[TRACE] Compiling Rule: {t.Substring(0, Math.Min(50, t.Length))}...");
                    return _interpreter.ParseAsDelegate<Action<GenericComponent, ExplosionContext>>(t, "this", "ctx");
                } catch (Exception ex) {
                    Console.WriteLine($"\n[COMPILATION ERROR] Rule: {t}");
                    Console.WriteLine($"Error: {ex.Message}");
                    throw;
                }
            });
        }
    }

    // --- HIERARCHICAL BLUEPRINT MODELS ---
    
    // Represents a node in the hierarchical blueprint tree
    public class HierarchicalBlueprint {
        public string ProductGroup { get; set; }
        public List<string> Rules { get; set; } = new List<string>();
        public Dictionary<string, HierarchicalBlueprint> Components { get; set; } = new Dictionary<string, HierarchicalBlueprint>();
    }

    // Flattened blueprint for efficient lookup
    public class FlattenedBlueprint {
        public string ComponentPath { get; set; }  // e.g., "SPR_Unit.Structural_System.Frame_System.Upright"
        public string ProductGroup { get; set; }
        public List<string> Rules { get; set; } = new List<string>();
        public Dictionary<string, double> Defaults { get; set; } = new Dictionary<string, double>();
    }

    // Loads and flattens hierarchical blueprints
    public class BlueprintLoader {
        public static Dictionary<string, FlattenedBlueprint> LoadHierarchical(string jsonPath) {
            var result = new Dictionary<string, FlattenedBlueprint>(StringComparer.OrdinalIgnoreCase);
            
            string json = File.ReadAllText(jsonPath);
            var doc = JsonDocument.Parse(json);
            
            // Each top-level key is a product group (e.g., "SPR_System", "Cantilever_System", "Mezzanine_System")
            foreach (var productGroup in doc.RootElement.EnumerateObject()) {
                string productGroupName = productGroup.Name;
                Console.WriteLine($"  Loading Product Group: {productGroupName}");
                
                // Extract Defaults if they exist (recursively flatten for readability)
                var defaults = new Dictionary<string, double>();
                if (productGroup.Value.TryGetProperty("Defaults", out var defaultsElem)) {
                    FlattenJson(defaultsElem, defaults);
                }

                // Recursively traverse and flatten the hierarchy
                TraverseAndFlatten(productGroup.Value, productGroupName, "", defaults, result);
            }
            
            return result;
        }

        private static void FlattenJson(JsonElement element, Dictionary<string, double> result) {
            if (element.ValueKind == JsonValueKind.Object) {
                foreach (var prop in element.EnumerateObject()) {
                    if (prop.Value.ValueKind == JsonValueKind.Number) {
                        result[prop.Name] = prop.Value.GetDouble();
                    } else if (prop.Value.ValueKind == JsonValueKind.Object) {
                        FlattenJson(prop.Value, result);
                    }
                }
            }
        }

        private static void TraverseAndFlatten(
            JsonElement element, 
            string productGroup, 
            string currentPath, 
            Dictionary<string, double> defaults,
            Dictionary<string, FlattenedBlueprint> result) 
        {
            if (element.ValueKind != JsonValueKind.Object) return;

            foreach (var prop in element.EnumerateObject()) {
                string componentName = prop.Name;
                
                // Skip metadata fields
                if (componentName == "ProductGroup") continue;
                
                string fullPath = string.IsNullOrEmpty(currentPath) 
                    ? componentName 
                    : $"{currentPath}.{componentName}";

                // Check if this component has Rules
                if (prop.Value.ValueKind == JsonValueKind.Object && 
                    prop.Value.TryGetProperty("Rules", out var rulesElement)) {
                    
                    var rules = new List<string>();
                    if (rulesElement.ValueKind == JsonValueKind.Array) {
                        foreach (var rule in rulesElement.EnumerateArray()) {
                            if (rule.ValueKind == JsonValueKind.String) {
                                rules.Add(rule.GetString());
                            }
                        }
                    }

                    // Store this component with its rules
                    result[fullPath] = new FlattenedBlueprint {
                        ComponentPath = fullPath,
                        ProductGroup = productGroup,
                        Rules = rules,
                        Defaults = defaults
                    };
                    
                    Console.WriteLine($"    Registered: {fullPath} ({rules.Count} rules)");
                }

                // Recursively process nested components
                if (prop.Value.ValueKind == JsonValueKind.Object && componentName != "Defaults") {
                    TraverseAndFlatten(prop.Value, productGroup, fullPath, defaults, result);
                }
            }
        }
    }

    // --- RACK METADATA MODELS ---
    
    public class RackConfiguration {
        public string ConfigName { get; set; }
        public string DisplayName { get; set; }
        public int UnitCount { get; set; }
        public RackSpecs Specs { get; set; }
        public List<RackUnit> Units { get; set; }
    }

    public class RackSpecs {
        public string ProductGroup { get; set; }
        public int Levels { get; set; }
        public double BeamSpan { get; set; }
        public double FrameDepth { get; set; }
        public double LevelLoad { get; set; }
        public double ClearHeight { get; set; }
        public double AisleWidth { get; set; }
        public string MheType { get; set; }
        public int PalletsPerLevel { get; set; }
        public double PalletLength { get; set; }
        public double PalletWidth { get; set; }
        public double PalletHeight { get; set; }
        public double PalletWeight { get; set; }
        public bool RowGuard { get; set; }
        public bool ColumnGuard { get; set; }
        public bool MeshCladding { get; set; }
        public string BeamProfile { get; set; }
        public string SkuUprightProfile { get; set; }
        
        // Add other properties as needed
    }

    public class RackUnit {
        public string Id { get; set; }
        public Dictionary<string, double> Position { get; set; }
        public Dictionary<string, double> Dimensions { get; set; }
        public string GroupId { get; set; }
        public string GroupName { get; set; }
    }

    // Layout Project Models
    public class LayoutNode {
        public string Type { get; set; }
        public string RepeatBy { get; set; }
        public Dictionary<string, object> Attributes { get; set; }
        public List<LayoutNode> Children { get; set; }
    }

    public class LayoutProject {
        public string ProjectName { get; set; }
        public LayoutNode Root { get; set; }
    }

    public class DecoupledFactory {
        private readonly Dictionary<string, FlattenedBlueprint> _library;

        public DecoupledFactory(Dictionary<string, FlattenedBlueprint> library) {
            _library = library;
        }

        public GenericComponent Build(LayoutNode node, Dictionary<string, int> inputs, GenericComponent parent = null) {
            var comp = new GenericComponent { 
                Type = node.Type, 
                Parent = parent
            };

            // SANITIZE ATTRIBUTES (Convert JsonElement to Native Types)
            if (node.Attributes != null) {
                foreach (var kvp in node.Attributes) {
                    if (kvp.Value is JsonElement elem) {
                        if (elem.ValueKind == JsonValueKind.Number) comp.Attributes[kvp.Key] = elem.GetDouble();
                        else if (elem.ValueKind == JsonValueKind.True) comp.Attributes[kvp.Key] = true;
                        else if (elem.ValueKind == JsonValueKind.False) comp.Attributes[kvp.Key] = false;
                        else if (elem.ValueKind == JsonValueKind.String) comp.Attributes[kvp.Key] = elem.GetString();
                        else comp.Attributes[kvp.Key] = elem.ToString();
                    } else {
                        comp.Attributes[kvp.Key] = kvp.Value;
                    }
                }
            }

            // RESOLVE LOGIC FROM HIERARCHICAL LIBRARY
            // Try multiple lookup strategies:
            // 1. Exact match on node.Type
            // 2. Partial path match (e.g., "SPR_Unit.Load_System.Beam" matches "Beam")
            // 3. Component name match (last segment of path)
            
            if (_library.TryGetValue(node.Type, out var blueprint)) {
                comp.Rules = blueprint.Rules ?? new List<string>();
                comp.Defaults = blueprint.Defaults ?? new Dictionary<string, double>();
            } else {
                // Try to find by partial path or component name
                var matchingBlueprint = _library.Values.FirstOrDefault(b => 
                    b.ComponentPath.EndsWith("." + node.Type, StringComparison.OrdinalIgnoreCase) ||
                    b.ComponentPath.Equals(node.Type, StringComparison.OrdinalIgnoreCase)
                );
                
                if (matchingBlueprint != null) {
                    comp.Rules = matchingBlueprint.Rules ?? new List<string>();
                    comp.Defaults = matchingBlueprint.Defaults ?? new Dictionary<string, double>();
                }
            }

            if (node.Children != null) {
                foreach (var childDef in node.Children) {
                    int count = (childDef.RepeatBy != null && inputs.ContainsKey(childDef.RepeatBy)) ? inputs[childDef.RepeatBy] : 1;
                    for (int i = 0; i < count; i++) comp.Children.Add(Build(childDef, inputs, comp));
                }
            }
            return comp;
        }
    }

    // --- 3. PROJECT EXECUTION ---
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("    GSS DECOUPLED PRODUCTION ENGINE POC         ");
            Console.WriteLine("    Structure + Logic + Item Master Enrichment  ");
            Console.WriteLine("=================================================\n");

            var ctx = new ExplosionContext();
            
            // 1. LOAD THE HIERARCHICAL BLUEPRINT LIBRARY (Multi-Product Support)
            string libPath = "blueprints.json";
            Console.WriteLine($"Loading Hierarchical Blueprints from: {libPath}");
            var lib = BlueprintLoader.LoadHierarchical(libPath);
            Console.WriteLine($"Loaded {lib.Count} component types from hierarchical blueprints\n");

            // 2. LOAD THE PART MASTER (Item Master Metadata)
            string partMasterPath = "part_master_full.json";
            string partMasterJson = File.ReadAllText(partMasterPath);
            var partMasterArray = JsonSerializer.Deserialize<List<PartMetadata>>(partMasterJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
                             ?? new List<PartMetadata>();
            
            // Convert array to dictionary keyed by partCode
            var partMaster = partMasterArray
                .Where(p => !string.IsNullOrEmpty(p.PartCode))
                .ToDictionary(p => p.PartCode, StringComparer.OrdinalIgnoreCase);
            
            // Post-process to populate WeightKg from attributes
            foreach (var p in partMaster.Values) {
                if (p.Attributes != null && p.Attributes.ContainsKey("Unit weight")) {
                    object w = p.Attributes["Unit weight"];
                    if (w is JsonElement je && je.ValueKind == JsonValueKind.String) {
                        if (double.TryParse(je.GetString(), out double d)) p.WeightKg = d;
                    } else if (w is string s && double.TryParse(s, out double dbl)) {
                        p.WeightKg = dbl;
                    } else {
                        try { p.WeightKg = Convert.ToDouble(w); } catch { }
                    }
                }
            }
            
            Console.WriteLine($"Loaded {partMaster.Count} parts from part master\n");
            
            // Re-map to Case-Insensitive dictionary
            var ciPartMaster = new Dictionary<string, PartMetadata>(partMaster, StringComparer.OrdinalIgnoreCase);
            Console.WriteLine($"Loaded Part Master ({ciPartMaster.Count} SKUs enriched)");

            var compiler = new RuleCompiler(ctx.ADD_BOM, ctx.REGISTER_LINK, ctx.VALIDATE, ciPartMaster);

            // 3. LOAD RACK CONFIGURATIONS (replaces layout.json)
            string rackConfigPath = "rack_metadata_grouped.json";
            string rackConfigJson = File.ReadAllText(rackConfigPath);
            var rackConfigs = JsonSerializer.Deserialize<List<RackConfiguration>>(rackConfigJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Console.WriteLine($"Loaded {rackConfigs.Count} rack configuration groups\n");

            // Process first configuration as example
            var config = rackConfigs.FirstOrDefault();
            if (config == null) {
                Console.WriteLine("No rack configurations found!");
                return;
            }

            Console.WriteLine($"Processing Configuration: '{config.DisplayName}' ({config.UnitCount} units)");
            Console.WriteLine($"  Product Group: {config.Specs.ProductGroup}");
            Console.WriteLine($"  Dimensions: {config.Specs.BeamSpan}mm (W) x {config.Specs.FrameDepth}mm (D) x {config.Specs.Levels} levels");
            Console.WriteLine($"  Level Load: {config.Specs.LevelLoad}kg\n");

            // 4. BUILD COMPONENT TREE FROM RACK CONFIGURATION
            try {
                var factory = new DecoupledFactory(lib);
                var stopwatch = Stopwatch.StartNew();
                
                // Build component tree from rack specs
                var assembly = BuildFromRackConfig(config, factory);
                Console.WriteLine($"Assembly built from Rack Configuration in: {stopwatch.ElapsedMilliseconds}ms");

                stopwatch.Restart();
                assembly.ExplodeParallel(ctx, compiler);
                stopwatch.Stop();
                
                Console.WriteLine($"\n[PERFORMANCE STATS]:");
                Console.WriteLine($"  - Total Time (Parallel + Pre-compiled): {stopwatch.ElapsedMilliseconds}ms");
            } catch (Exception ex) {
                Console.WriteLine($"\n[FATAL ENGINE ERROR]: {ex}");
                if (ex.InnerException != null) Console.WriteLine($"\n[INNER ERROR]: {ex.InnerException}");
            }

            if (ctx.Errors.Any()) {
                Console.WriteLine("\n[VALIDATION ERRORS]:");
                foreach (var err in ctx.Errors.Distinct().Take(5)) Console.WriteLine($"  ! {err}");
            } else {
                Console.WriteLine("\n[VALIDATION]: All design rules passed.");
            }

            PrintCategorizedBom(ctx.Items, partMaster);
            PrintQuotationBom(ctx.Items, partMaster);
        }

        static GenericComponent BuildFromRackConfig(RackConfiguration config, DecoupledFactory factory) {
            // Create root component
            var root = new GenericComponent {
                Type = "Warehouse",
                Attributes = new Dictionary<string, object>()
            };

            // Calculate derived dimensions
            double rackHeight = config.Specs.Levels * config.Specs.ClearHeight;
            
            Console.WriteLine($"  Building BOM for 1 rack unit (from config with {config.UnitCount} total units)...\n");

            // Create components for ONE rack unit (not all units)
            // The unitCount in the config represents total units in the warehouse,
            // but we're generating BOM for a single unit configuration
            
            // Create rack structural components
            var uprightNode = new LayoutNode {
                Type = "Upright",
                Attributes = new Dictionary<string, object> {
                    { "RackHeight", rackHeight },
                    { "TotalFrameLoad", config.Specs.LevelLoad * config.Specs.Levels },
                    { "MaxFrameCapacity", config.Specs.LevelLoad * config.Specs.Levels * 1.5 },
                    { "RackDepth", config.Specs.FrameDepth }
                }
            };

            var bracingNode = new LayoutNode {
                Type = "Bracing",
                Attributes = new Dictionary<string, object> {
                    { "RackHeight", rackHeight },
                    { "RackDepth", config.Specs.FrameDepth }
                }
            };

            var basePlateNode = new LayoutNode {
                Type = "BasePlate",
                Attributes = new Dictionary<string, object>()
            };

            var stabilityNode = new LayoutNode {
                Type = "Height_Depth_Stability",
                Attributes = new Dictionary<string, object> {
                    { "RackHeight", rackHeight },
                    { "RackDepth", config.Specs.FrameDepth }
                }
            };

            var safetyNode = new LayoutNode {
                Type = "Safety_And_Accessories",
                Attributes = new Dictionary<string, object> {
                    { "RearMeshRequired", config.Specs.MeshCladding },
                    { "MHE_Traffic", config.Specs.ColumnGuard },
                    { "RackHeight", rackHeight }
                }
            };

            // Create beam levels
            for (int i = 0; i < config.Specs.Levels; i++) {
                var beamLevelNode = new LayoutNode {
                    Type = "Beam_Level",
                    Attributes = new Dictionary<string, object> {
                        { "LevelPitch", 50.0 },
                        { "LoadPerLevel", config.Specs.LevelLoad },
                        { "BeamCapacity", config.Specs.LevelLoad * 1.5 },
                        { "LevelToLevelHeight", config.Specs.ClearHeight },
                        { "FirstBeamHeight", 400.0 },
                        { "MHE_MinForkHeight", 300.0 }
                    }
                };

                var beamNode = new LayoutNode {
                    Type = "Beam",
                    Attributes = new Dictionary<string, object> {
                        { "LoadPerLevel", config.Specs.LevelLoad },
                        { "BeamCapacity", config.Specs.LevelLoad * 1.5 },
                        { "RackWidth", config.Specs.BeamSpan }
                    }
                };

                root.Children.Add(factory.Build(beamLevelNode, new Dictionary<string, int>()));
                root.Children.Add(factory.Build(beamNode, new Dictionary<string, int>()));
            }

            // Add structural components
            root.Children.Add(factory.Build(uprightNode, new Dictionary<string, int>()));
            root.Children.Add(factory.Build(bracingNode, new Dictionary<string, int>()));
            root.Children.Add(factory.Build(basePlateNode, new Dictionary<string, int>()));
            root.Children.Add(factory.Build(stabilityNode, new Dictionary<string, int>()));
            root.Children.Add(factory.Build(safetyNode, new Dictionary<string, int>()));

            return root;
        }

        static void PrintCategorizedBom(IEnumerable<BomItem> items, Dictionary<string, PartMetadata> master) {
            foreach (BomType type in Enum.GetValues(typeof(BomType))) {
                Console.WriteLine($"\n[{type}] View:");
                var summary = items.Where(i => i.Category == type)
                                   .GroupBy(i => i.SKU)
                                   .Select(g => new { SKU = g.Key, Qty = g.Sum(x => x.Qty) })
                                   .OrderBy(x => x.SKU) // Sort by name for readability
                                   .ToList();

                double totalWeight = 0;
                double totalHours = 0;
                bool showHours = (type == BomType.IBOM); // Only show hours for IBOM

                foreach (var item in summary) {
                    string metaInfo = "";
                    if (master.TryGetValue(item.SKU, out var meta)) {
                        double w = item.Qty * (meta.WeightKg ?? 0);
                        double h = (item.Qty * (meta.InstallTimeMins ?? 0)) / 60.0;
                        totalWeight += w;
                        totalHours += h;
                        
                        // EBOM/MBOM: show weight only, IBOM: show weight and hours
                        if (showHours) {
                            metaInfo = $" | {meta.Description} ({w:N1}kg, {h:N1} hrs)";
                        } else {
                            metaInfo = $" | {meta.Description} ({w:N1}kg)";
                        }
                    } else {
                        metaInfo = " | [DYNAMIC/UNMAPPED PART]";
                    }
                    Console.WriteLine($"  • {item.Qty}x {item.SKU}{metaInfo}");
                }

                if (summary.Any()) {
                    Console.WriteLine($"  --- LAYER STATS ---");
                    Console.WriteLine($"  > Total Weight: {totalWeight:N1} kg");
                    if (showHours) {
                        Console.WriteLine($"  > Total Installation: {totalHours:N1} man-hours");
                    }
                } else {
                    Console.WriteLine("  • [Empty]");
                }
            }
        }

        static void PrintQuotationBom(IEnumerable<BomItem> items, Dictionary<string, PartMetadata> master) {
            Console.WriteLine("\n" + new string('=', 120));
            Console.WriteLine("DETAILED QUOTATION - MBOM (Manufacturing BOM)");
            Console.WriteLine(new string('=', 120));
            Console.WriteLine($"{"Description",-50} {"Qty",6} {"UNSPSC Code",-18} {"Drawing No",-18} {"Colour",-15} {"Unit Price",12} {"Total Price",12}");
            Console.WriteLine(new string('-', 120));

            var mbomItems = items.Where(i => i.Category == BomType.MBOM)
                                 .GroupBy(i => i.SKU)
                                 .Select(g => new { SKU = g.Key, Qty = g.Sum(x => x.Qty) })
                                 .OrderBy(x => x.SKU)
                                 .ToList();

            double subtotal = 0;
            double totalCBM = 0;
            double totalWeight = 0;

            foreach (var item in mbomItems) {
                if (master.TryGetValue(item.SKU, out var meta)) {
                    double unitPrice = meta.UnitBasicPrice ?? 0;
                    double discountedPrice = unitPrice * (1 - meta.DiscountPercent / 100.0);
                    double totalPrice = discountedPrice * item.Qty;
                    double itemCBM = (meta.CBM ?? 0) * item.Qty;
                    double itemWeight = (meta.WeightKg ?? 0) * item.Qty;

                    subtotal += totalPrice;
                    totalCBM += itemCBM;
                    totalWeight += itemWeight;

                    Console.WriteLine($"{meta.Description,-50} {item.Qty,6} {meta.UNSPSCCode ?? "N/A",-18} {meta.DrawingNo ?? "N/A",-18} {meta.Colour ?? "N/A",-15} {discountedPrice,12:N2} {totalPrice,12:N2}");
                } else {
                    Console.WriteLine($"{item.SKU,-50} {item.Qty,6} {"[UNMAPPED]",-18} {"N/A",-18} {"N/A",-15} {0.0,12:N2} {0.0,12:N2}");
                }
            }

            Console.WriteLine(new string('-', 120));
            Console.WriteLine($"{"SUBTOTAL:",-50} {"",-6} {"",-18} {"",-18} {"",-15} {"",-12} {subtotal,12:N2}");
            
            double sgst = subtotal * 0.09;
            double cgst = subtotal * 0.09;
            double igst = 0.0; // Assuming intra-state
            double grandTotal = subtotal + sgst + cgst + igst;

            Console.WriteLine($"{"SGST @ 9%:",-50} {"",-6} {"",-18} {"",-18} {"",-15} {"",-12} {sgst,12:N2}");
            Console.WriteLine($"{"CGST @ 9%:",-50} {"",-6} {"",-18} {"",-18} {"",-15} {"",-12} {cgst,12:N2}");
            Console.WriteLine($"{"IGST @ 0%:",-50} {"",-6} {"",-18} {"",-18} {"",-15} {"",-12} {igst,12:N2}");
            Console.WriteLine(new string('=', 120));
            Console.WriteLine($"{"GRAND TOTAL (INR):",-50} {"",-6} {"",-18} {"",-18} {"",-15} {"",-12} {grandTotal,12:N2}");
            Console.WriteLine(new string('=', 120));
            Console.WriteLine($"\nTotal CBM: {totalCBM:N2} m³");
            Console.WriteLine($"Total Weight: {totalWeight:N1} kg");
        }
    }
}
