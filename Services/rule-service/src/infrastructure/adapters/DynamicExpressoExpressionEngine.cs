using DynamicExpresso;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RuleService.Domain.Services;
using GssCommon.Common.Models.Configurator;

namespace RuleService.Infrastructure.Adapters
{
    public class DynamicExpressoExpressionEngine : IExpressionEngineAdapter
    {
        private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(5000);
        private readonly IMatrixEvaluationService _matrixService;
        private readonly ICatalogServiceClient _catalogClient;

        public DynamicExpressoExpressionEngine(IMatrixEvaluationService matrixService, ICatalogServiceClient catalogClient)
        {
            _matrixService = matrixService ?? throw new ArgumentNullException(nameof(matrixService));
            _catalogClient = catalogClient ?? throw new ArgumentNullException(nameof(catalogClient));
        }

        public async Task<object?> EvaluateAsync(string expression, IDictionary<string, object?> variables, RuleEvaluationContext? ctx = null)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return null;

            var interpreter = new Interpreter(InterpreterOptions.Default)
                .SetFunction("Now", (Func<DateTime>)(() => DateTime.UtcNow))
                .Reference(typeof(Math))
                .SetFunction("MIN", new Func<double, double, double>(Math.Min))
                .SetFunction("MAX", new Func<double, double, double>(Math.Max))
                .SetFunction("IF", new Func<bool, object?, object?, object?>((cond, t, f) => cond ? t : f))
                // Register MATRIX_LOOKUP
                .SetFunction("MATRIX_LOOKUP", (Func<string, string, double, string, double?>)((name, upright, span, profile) => 
                {
                    return _matrixService.LookupValueAsync(name, new[] { "uprights", upright, profile }, span).GetAwaiter().GetResult();
                }))
                // Register MATRIX_UTIL
                .SetFunction("MATRIX_UTIL", (Func<string, string, double, string, double, double>)((name, upright, span, profile, load) => 
                {
                    var capacity = _matrixService.LookupValueAsync(name, new[] { "uprights", upright, profile }, span).GetAwaiter().GetResult();
                    if (!capacity.HasValue || capacity.Value == 0) return 999.9;
                    return Math.Round((load / capacity.Value) * 100, 2);
                }));

            // Register POC functions if context is provided
            if (ctx != null)
            {
                interpreter.SetFunction("ADD_BOM", (Func<string, object, string, bool>)((sku, qty, type) => 
                {
                    double q = 0;
                    try { q = Convert.ToDouble(qty); } catch { q = 0; }
                    var category = Enum.Parse<BomType>(type.ToUpperInvariant());
                    ctx.BomItems.Add(new BomItem { SKU = sku, Qty = q, Category = category });
                    return true;
                }));

                interpreter.SetFunction("VALIDATE", (Func<string, bool>)((message) => 
                {
                    ctx.Violations.Add(new RuleViolation { Message = message, Severity = "ERROR" });
                    return false; // Return false to indicate validation failed
                }));

                interpreter.SetFunction("LOOKUP", (Func<string, string, double, string>)((compType, attr, val) => 
                {
                    var part = _catalogClient.LookupPartAsync(compType, attr, val).GetAwaiter().GetResult();
                    return part?.PartCode ?? $"MISSING-{compType}-{attr}-{val}";
                }));
                
                // Support "this.GetNum" and "this.GetBool" style from POC by registering them as functions
                // or setting a variable 'this' if possible. In DynamicExpresso, we can register 'this' as a variable.
                // However, the rule service usually evaluates against a flat dictionary.
                // To support the POC rules exactly as they are:
                interpreter.SetFunction("GetNum", (Func<string, double>)((key) => 
                {
                    if (ctx.TryGetValue(key, out var val)) return Convert.ToDouble(val);
                    return 0;
                }));
                interpreter.SetFunction("GetBool", (Func<string, bool>)((key) => 
                {
                    if (ctx.TryGetValue(key, out var val)) return Convert.ToBoolean(val);
                    return false;
                }));
            }

            // Register variables
            if (variables != null)
            {
                foreach (var kv in variables)
                {
                    interpreter.SetVariable(kv.Key, kv.Value);
                }
            }

            // Also expose context itself if needed
            if (ctx != null) interpreter.SetVariable("this", ctx);

            // Execute with a timeout guard
            var task = System.Threading.Tasks.Task.Run(() =>
            {
                return interpreter.Eval(expression);
            });

            if (task.Wait(_timeout))
            {
                return task.Result;
            }

            throw new TimeoutException($"Expression evaluation exceeded {_timeout.TotalMilliseconds} ms");
        }
    }
}
