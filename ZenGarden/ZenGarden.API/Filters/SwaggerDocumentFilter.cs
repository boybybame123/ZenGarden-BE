using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ZenGarden.API.Filters;

public class SwaggerDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Remove unused schemas
        var unusedSchemas = swaggerDoc.Components.Schemas
            .Where(s => !swaggerDoc.Paths.Values
                .SelectMany(p => p.Operations.Values)
                .SelectMany(o => o.Responses.Values)
                .Any(r => r.Content?.Values.Any(c => c.Schema?.Reference?.Id == s.Key) == true))
            .ToList();

        foreach (var schema in unusedSchemas)
        {
            swaggerDoc.Components.Schemas.Remove(schema.Key);
        }

        // Sort paths alphabetically
        var paths = swaggerDoc.Paths.OrderBy(p => p.Key).ToList();
        swaggerDoc.Paths.Clear();
        foreach (var path in paths)
        {
            swaggerDoc.Paths.Add(path.Key, path.Value);
        }
    }
} 