using System.Collections.Generic;
using System.Threading.Tasks;
using RuleService.Infrastructure.Adapters;
using Xunit;

namespace RuleService.UnitTests
{
    public class DynamicExpressoEngineTests
    {
        private class DummyMatrixService : RuleService.Domain.Services.IMatrixEvaluationService
        {
            public Task<double?> LookupValueAsync(string matrixName, string[] path, double? numericalValue = null) => Task.FromResult<double?>(null);
            public Task<System.Collections.Generic.List<RuleService.Domain.Services.MatrixChoiceResult>> GetChoicesAsync(string matrixName, string[] parentPath, double inputVariable, double requiredLoad) => Task.FromResult(new System.Collections.Generic.List<RuleService.Domain.Services.MatrixChoiceResult>());
        }
        [Fact]
        public async Task Evaluates_Simple_Expression_With_Variables()
        {
            var engine = new DynamicExpressoExpressionEngine(new DummyMatrixService());
            var variables = new Dictionary<string, object?>
            {
                { "width", 200 },
                { "height", 150 }
            };

            var result = await engine.EvaluateAsync("width > 100 && height > 100", variables);
            Assert.IsType<bool>(result);
            Assert.True((bool)result!);
        }

        [Fact]
        public async Task Evaluates_String_Contains()
        {
            var engine = new DynamicExpressoExpressionEngine(new DummyMatrixService());
            var variables = new Dictionary<string, object?>
            {
                { "name", "sample-product" }
            };

            var result = await engine.EvaluateAsync("name != null && name.ToString().Contains(\"sample\")", variables);
            Assert.IsType<bool>(result);
            Assert.True((bool)result!);
        }
    }
}
