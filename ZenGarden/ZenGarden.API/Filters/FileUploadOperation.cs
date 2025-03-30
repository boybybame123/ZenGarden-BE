using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ZenGarden.API.Filters;

public class FileUploadOperation : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        var formParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType.IsClass && p.ParameterType != typeof(string))
            .SelectMany(p => p.ParameterType.GetProperties())
            .Where(p => p.PropertyType != typeof(IFormFile) && p.PropertyType != typeof(IFormFileCollection))
            .ToList();

        var fileParams = context.MethodInfo.GetParameters()
            .Where(p => HasFileUpload(p.ParameterType))
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
                        Properties = GetCombinedSchema(formParams, fileParams)
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

    private Dictionary<string, OpenApiSchema> GetCombinedSchema(
        List<PropertyInfo> formParams, List<ParameterInfo> fileParams)
    {
        var schema = new Dictionary<string, OpenApiSchema>();

        foreach (var prop in formParams)
        {
            schema[prop.Name] = new OpenApiSchema
            {
                Type = GetOpenApiType(prop.PropertyType),
                Format = GetOpenApiFormat(prop.PropertyType)
            };
        }

        foreach (var param in fileParams)
        {
            foreach (var prop in param.ParameterType.GetProperties()
                         .Where(p => p.PropertyType == typeof(IFormFile) || p.PropertyType == typeof(IFormFileCollection)))
            {
                schema[prop.Name] = new OpenApiSchema { Type = "string", Format = "binary" };
            }
        }

        return schema;
    }

    private string GetOpenApiType(Type type)
    {
        return type switch
        {
            _ when type == typeof(int) || type == typeof(int?) => "integer",
            _ when type == typeof(decimal) || type == typeof(float) || type == typeof(double) => "number",
            _ when type == typeof(bool) || type == typeof(bool?) => "boolean",
            _ => "string"
        };
    }

    private string GetOpenApiFormat(Type type)
    {
        return type switch
        {
            _ when type == typeof(decimal) || type == typeof(float) || type == typeof(double) => "double",
            _ => null
        };
    }
}
