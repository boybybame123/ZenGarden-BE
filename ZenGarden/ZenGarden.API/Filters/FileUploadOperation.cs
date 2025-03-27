using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ZenGarden.API.Filters;

public class FileUploadOperation : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        var fileParams = context.MethodInfo.GetParameters()
            .Where(p => HasFileUpload(p.ParameterType)) // Kiểm tra nếu tham số có chứa IFormFile
            .ToList();

        if (!fileParams.Any())
            return;

        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new()
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = GetFileSchema(fileParams)
                    }
                }
            }
        };
    }

    private bool HasFileUpload(Type type)
    {
        return type == typeof(IFormFile) ||
               type == typeof(IFormFileCollection) ||
               type.GetProperties().Any(p =>
                   p.PropertyType == typeof(IFormFile) || p.PropertyType == typeof(IFormFileCollection));
    }

    private Dictionary<string, OpenApiSchema> GetFileSchema(List<ParameterInfo> parameters)
    {
        var schema = new Dictionary<string, OpenApiSchema>();

        foreach (var param in parameters)
            if (param.ParameterType == typeof(IFormFile) || param.ParameterType == typeof(IFormFileCollection))
                schema[param.Name ?? "file"] = new OpenApiSchema { Type = "string", Format = "binary" };
            else
                foreach (var prop in param.ParameterType.GetProperties().Where(p =>
                             p.PropertyType == typeof(IFormFile) || p.PropertyType == typeof(IFormFileCollection)))
                    schema[prop.Name] = new OpenApiSchema { Type = "string", Format = "binary" };

        return schema;
    }
}