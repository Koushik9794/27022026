using GssCommon.Common.Models.Configurator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace ConfigurationService.Application.Services
{
    public class ConfiguratorService
    {
        private readonly IRuleServiceClient _ruleClient;
        private readonly IBomServiceClient _bomClient;

        public ConfiguratorService(IRuleServiceClient ruleClient, IBomServiceClient bomClient)
        {
            _ruleClient = ruleClient;
            _bomClient = bomClient;
        }

        public async Task<ConfiguratorResult> ProcessLayoutAsync(Guid configId, string projectName, string layoutJson, Guid ruleSetId)
        {
            // 1. Expand Layout to Hierarchy
            var root = ExpandLayout(layoutJson);
            
            var allComponents = FlattenHierarchy(root);
            var totalBom = new List<BomItem>();
            var allViolations = new List<string>();

            // 2. For each component, evaluate rules
            foreach (var comp in allComponents)
            {
                var request = new RuleEvaluationRequest
                {
                    RuleSetId = ruleSetId,
                    Variables = comp.Attributes.ToDictionary(k => k.Key, v => v.Value)
                };

                var response = await _ruleClient.EvaluateRulesAsync(request);
                
                foreach (var outcome in response.Outcomes)
                {
                    if (!outcome.Passed)
                    {
                        allViolations.Add(outcome.Message);
                    }

                    if (outcome.Data.TryGetValue("BomItems", out var itemsObj) && itemsObj is JsonElement itemsList)
                    {
                         var items = itemsList.Deserialize<List<BomItem>>();
                         if (items != null) totalBom.AddRange(items);
                    }
                }
            }

            // 3. Push to BOM Service
            if (totalBom.Any())
            {
                await _bomClient.PushBomBatchAsync(configId, projectName, totalBom);
            }

            return new ConfiguratorResult
            {
                Success = !allViolations.Any(),
                Violations = allViolations,
                BomCount = totalBom.Count
            };
        }

        private GenericComponent ExpandLayout(string layoutJson)
        {
            // Mock expansion logic - in a real system this would parse the complex layout JSON
            // and build the tree of Uprights, Beams, etc.
            var root = new GenericComponent { Type = "Layout" };
            
            // Example: Add a bay
            var bay = new GenericComponent { Type = "Bay" };
            bay.Attributes["RackHeight"] = 5000.0;
            bay.Attributes["RackWidth"] = 2700.0;
            bay.Attributes["RackDepth"] = 1100.0;
            bay.Attributes["TotalFrameLoad"] = 8000.0;
            bay.Attributes["MaxFrameCapacity"] = 10000.0;
            
            // Add uprights
            var upright1 = new GenericComponent { Type = "Upright" };
            upright1.Attributes["RackHeight"] = 5000.0;
            bay.Children.Add(upright1);
            
            root.Children.Add(bay);
            return root;
        }

        private List<GenericComponent> FlattenHierarchy(GenericComponent root)
        {
            var result = new List<GenericComponent> { root };
            foreach (var child in root.Children)
            {
                result.AddRange(FlattenHierarchy(child));
            }
            return result;
        }
    }

    public class ConfiguratorResult
    {
        public bool Success { get; set; }
        public List<string> Violations { get; set; } = new();
        public int BomCount { get; set; }
    }
}
