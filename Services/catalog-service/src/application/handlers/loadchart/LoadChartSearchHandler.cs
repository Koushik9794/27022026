using System.Text.Json;
using System.Text.Json.Serialization;
using CatalogService.application.commands.loadchart;
using CatalogService.application.dtos;
using CatalogService.Infrastructure.Persistence;
using NPOI.SS.Formula.Functions;

namespace CatalogService.application.handlers.loadchart;

public class LoadChartSearchHandler
{
    private readonly ILoadChartRepository _repository;

    public LoadChartSearchHandler(ILoadChartRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<LoadChartCandidateDto>> Handle(
            LoadChartSearchCommand request,
            CancellationToken ct)
    {


        // Serialize the command as payload JSON
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null, // respect [JsonPropertyName]
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() } // enum -> "UPRIGHT"/"BEAM"
        };

        var payloadJson = JsonSerializer.Serialize(request, jsonOptions);


        return _repository.GetLoadchartbysearch(payloadJson);
    }
}


