using CatalogService.Infrastructure.Persistence;
using GssCommon.Common.Models.Configurator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CatalogService.Application.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly ISkuRepository _skuRepository;

        public CatalogService(ISkuRepository skuRepository)
        {
            _skuRepository = skuRepository;
        }

        public async Task<PartMetadata?> LookupPartAsync(string componentType, string attributeName, double targetValue)
        {
            var skus = await _skuRepository.GetAllAsync();
            var partMaster = skus.Select(MapToPartMetadata).ToList();

            var matches = partMaster.Where(p => IsMatchByType(p, componentType)).ToList();

            var bestMatch = matches
                .Where(p => 
                {
                    var attrValue = GetAttributeValue(p, attributeName, targetValue);
                    
                    // Specialized logic for HeavyDuty check (same as POC)
                    if (attributeName.Equals("HeavyDuty", StringComparison.OrdinalIgnoreCase))
                    {
                        double thickness = GetAttributeValue(p, "Thickness", 0);
                        return (targetValue > 0) ? thickness >= 8.0 : thickness < 8.0;
                    }

                    return attrValue >= (targetValue - 0.1);
                })
                .OrderBy(p => GetAttributeValue(p, attributeName, targetValue))
                .FirstOrDefault();

            return bestMatch;
        }

        private bool IsMatchByType(PartMetadata p, string componentType)
        {
            string typeSearch = (p.PartCode + " " + p.Description + " " + (p.ComponentTypeCode ?? "")).ToUpperInvariant();
            if (!typeSearch.Contains(componentType.ToUpperInvariant()))
            {
                // Specialized maps from POC
                if (componentType.Equals("UPRIGHT", StringComparison.OrdinalIgnoreCase) && typeSearch.Contains("UPR")) return true;
                if (componentType.Equals("BEAM", StringComparison.OrdinalIgnoreCase) && (typeSearch.Contains("BM") || typeSearch.Contains("GBX") || typeSearch.Contains("GSB"))) return true;
                if (componentType.Equals("HORIZONTAL-BRACING", StringComparison.OrdinalIgnoreCase) && typeSearch.Contains("HORZ")) return true;
                if (componentType.Equals("DIAGONAL-BRACING", StringComparison.OrdinalIgnoreCase) && typeSearch.Contains("DIAG")) return true;
                if (componentType.Equals("STABILITY", StringComparison.OrdinalIgnoreCase) && (typeSearch.Contains("STIFFNER") || typeSearch.Contains("TIEBEAM"))) return true;
                return false;
            }
            return true;
        }

        private double GetAttributeValue(PartMetadata p, string key, double targetVal)
        {
            if (p.Attributes != null && p.Attributes.TryGetValue(key, out var attrVal))
            {
                if (attrVal is JsonElement je)
                {
                    if (je.ValueKind == JsonValueKind.Number) return je.GetDouble();
                    if (je.ValueKind == JsonValueKind.String && double.TryParse(je.GetString(), out double d)) return d;
                }
                else if (attrVal is string s && double.TryParse(s, out double dbl))
                {
                    return dbl;
                }
                else
                {
                    try { return Convert.ToDouble(attrVal); } catch { }
                }
            }

            // Fallback: Parse from description (POC heuristic)
            string combined = (p.Description + " " + p.ShortDescription + " " + p.PartCode).ToUpperInvariant();
            if (key.Equals("Span", StringComparison.OrdinalIgnoreCase) || key.Equals("Length(mm)", StringComparison.OrdinalIgnoreCase) || key.Equals("Height", StringComparison.OrdinalIgnoreCase))
            {
                var matches = Regex.Matches(combined, @"(?<=\D|^)(\d{3,5})(?=\D|$)");
                foreach (Match m in matches)
                {
                    if (double.TryParse(m.Value, out double d) && Math.Abs(d - targetVal) < 10) return d;
                }
            }

            return 0;
        }

        private PartMetadata MapToPartMetadata(Domain.Aggregates.Sku sku)
        {
            var attributes = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(sku.AttributeSchema))
            {
                try
                {
                    attributes = JsonSerializer.Deserialize<Dictionary<string, object>>(sku.AttributeSchema) ?? new Dictionary<string, object>();
                }
                catch { }
            }

            return new PartMetadata
            {
                PartCode = sku.Code,
                Description = sku.Name, // Using Name as Description if Description is null, but Sku has Description too
                ShortDescription = sku.Description ?? "",
                ComponentTypeCode = "", // Needs to be populated if available in Sku schema
                Attributes = attributes
                // Other fields can be mapped as needed or extended in Sku model
            };
        }
    }
}
