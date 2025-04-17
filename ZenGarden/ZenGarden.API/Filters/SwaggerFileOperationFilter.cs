using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ZenGarden.API.Filters;

public class SwaggerFileOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParameters = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile) ||
                        (p.ParameterType.IsGenericType &&
                         p.ParameterType.GetGenericArguments()[0] == typeof(IFormFile)));

        var parameterInfos = fileParameters as ParameterInfo[] ?? fileParameters.ToArray();
        if (parameterInfos.Length == 0)
        {
            // Kiểm tra xem dto có chứa IFormFile không
            var complexParameters = context.MethodInfo.GetParameters()
                .Where(p => !p.ParameterType.IsPrimitive && p.ParameterType != typeof(string));

            foreach (var parameter in complexParameters)
            {
                var properties = parameter.ParameterType.GetProperties();
                var fileProperties = properties.Where(p => p.PropertyType == typeof(IFormFile) ||
                                                           (p.PropertyType.IsGenericType &&
                                                            p.PropertyType.GetGenericArguments()[0] ==
                                                            typeof(IFormFile)));

                if (!fileProperties.Any()) continue;
                operation.RequestBody ??= new OpenApiRequestBody();

                // Thêm content type multipart/form-data
                operation.RequestBody.Content ??= new Dictionary<string, OpenApiMediaType>();

                if (!operation.RequestBody.Content.TryGetValue("multipart/form-data", out var value))
                {
                    value = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>(),
                            Required = new HashSet<string>()
                        }
                    };
                    operation.RequestBody.Content.Add("multipart/form-data", value);
                }

                foreach (var property in properties)
                {
                    if (value.Schema.Properties.ContainsKey(property.Name)) continue;

                    if (property.PropertyType == typeof(IFormFile) ||
                        (property.PropertyType.IsGenericType &&
                         property.PropertyType.GetGenericArguments()[0] == typeof(IFormFile)))
                        value.Schema.Properties[property.Name] = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary"
                        };
                    else
                        try
                        {
                            var schema =
                                context.SchemaGenerator.GenerateSchema(property.PropertyType, context.SchemaRepository);
                            value.Schema.Properties[property.Name] = schema;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error generating schema for {property.Name}: {ex.Message}");
                        }
                }
            }
        }
        else
        {
            // Xử lý các tham số IFormFile trực tiếp
            foreach (var parameter in parameterInfos)
            {
                var operationParameter = operation.Parameters.FirstOrDefault(p => p.Name == parameter.Name);
                if (operationParameter != null) operation.Parameters.Remove(operationParameter);

                operation.RequestBody ??= new OpenApiRequestBody();
                operation.RequestBody.Content ??= new Dictionary<string, OpenApiMediaType>();

                if (!operation.RequestBody.Content.TryGetValue("multipart/form-data", out var value))
                {
                    value = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>(),
                            Required = new HashSet<string>()
                        }
                    };
                    operation.RequestBody.Content.Add("multipart/form-data", value);
                }

                if (parameter.Name != null && !value.Schema.Properties.ContainsKey(parameter.Name))
                    value.Schema.Properties.Add(parameter.Name, new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    });
            }
        }
    }
}