using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Ontec.WebUI.Filters
{
    public class SwaggerFileOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileUploadMine = "multipart/form-data";
            if(operation.RequestBody==null || !operation.RequestBody.Content.Any(x=>x.Key.Equals(fileUploadMine,StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }
            var fileParams = context.MethodInfo.GetParameters().Where(p => p.ParameterType == typeof(IFormFile));
            operation.RequestBody.Content[fileUploadMine].Schema.Properties = fileParams.ToDictionary(k => k.Name, v => new OpenApiSchema()
            {
                Type = "string",
                Format = "binary"
            });
        }
    }
}
