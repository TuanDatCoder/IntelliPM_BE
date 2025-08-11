using IntelliPM.Common.Attributes;
using IntelliPM.Data.DTOs;
using IntelliPM.Repositories.DynamicCategoryRepos;
using IntelliPM.Services.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace IntelliPM.API.Middlewares
{
    public class DynamicCategoryValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DynamicCategoryValidationMiddleware> _logger;

        public DynamicCategoryValidationMiddleware(
            RequestDelegate next,
            ILogger<DynamicCategoryValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Bỏ qua các request không cần validate
            if (context.Request.Method == HttpMethod.Options.Method || context.Request.Method == HttpMethod.Get.Method)
            {
                await _next(context);
                return;
            }

            if (context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) != true)
            {
                await _next(context);
                return;
            }

            context.Request.EnableBuffering();

            try
            {
                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                if (string.IsNullOrWhiteSpace(body))
                {
                    await _next(context);
                    return;
                }

                var jsonElement = JsonSerializer.Deserialize<JsonElement>(
                    body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var errors = new List<string>();
                var replacements = new List<(string propName, string group, string rawValue, bool isRequired)>();

                // 🔹 Lấy DTO type của action hiện tại
                var endpoint = context.GetEndpoint();
                var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
                var dtoType = actionDescriptor?.Parameters
                    .FirstOrDefault(p =>
                        p.ParameterType.Namespace != null &&
                        p.ParameterType.Namespace.StartsWith("IntelliPM.Data.DTOs") &&
                        p.ParameterType.IsClass)
                    ?.ParameterType;

                if (dtoType != null)
                {
                    ExtractAttributesFromJson(jsonElement, dtoType, replacements, errors);
                }

                if (errors.Any())
                {
                    _logger.LogError("DynamicCategoryValidationMiddleware: Validation errors: {Errors}", string.Join(" ", errors));
                    await ReturnBadRequest(context, string.Join(" ", errors));
                    return;
                }

                if (!replacements.Any())
                {
                    await _next(context);
                    return;
                }

                // Lấy repository từ DI container
                var categoryRepo = context.RequestServices.GetRequiredService<IDynamicCategoryRepository>();

                var modifiedBody = await ValidateAndMapValues(categoryRepo, jsonElement, replacements, errors);
                if (errors.Any())
                {
                    _logger.LogError("DynamicCategoryValidationMiddleware: Validation errors: {Errors}", string.Join(" ", errors));
                    await ReturnBadRequest(context, string.Join(" ", errors));
                    return;
                }

                if (modifiedBody != null)
                {
                    context.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(modifiedBody));
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DynamicCategoryValidationMiddleware");
                await ReturnBadRequest(context, "Invalid request body.");
            }
            finally
            {
                context.Request.Body.Position = 0;
            }
        }

        private void ExtractAttributesFromJson(
            JsonElement element,
            Type dtoType,
            List<(string propName, string group, string rawValue, bool isRequired)> replacements,
            List<string> errors)
        {
            if (element.ValueKind != JsonValueKind.Object) return;

            var propsWithAttr = dtoType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new { Prop = p, Attr = p.GetCustomAttribute<DynamicCategoryValidationAttribute>() })
                .Where(x => x.Attr != null)
                .ToList();

            foreach (var property in element.EnumerateObject())
            {
                var match = propsWithAttr
                    .FirstOrDefault(p => string.Equals(p.Prop.Name, property.Name, StringComparison.OrdinalIgnoreCase));

                if (match == null) continue;

                var attr = match.Attr!;

                if (property.Value.ValueKind == JsonValueKind.Null || property.Value.ValueKind == JsonValueKind.Undefined)
                {
                    if (attr.Required)
                    {
                        errors.Add($"Property '{property.Name}' (CategoryGroup='{attr.CategoryGroup}') is required but was null.");
                    }
                    continue;
                }

                if (property.Value.ValueKind == JsonValueKind.String)
                {
                    var value = property.Value.GetString();
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        if (attr.Required)
                            errors.Add($"Property '{property.Name}' (CategoryGroup='{attr.CategoryGroup}') cannot be empty.");
                        continue;
                    }
                    replacements.Add((property.Name, attr.CategoryGroup, value!, attr.Required));
                }
                else if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in property.Value.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.String)
                        {
                            var value = item.GetString();
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                replacements.Add((property.Name, attr.CategoryGroup, value!, attr.Required));
                            }
                        }
                    }
                }
            }
        }

        private async Task<string?> ValidateAndMapValues(
            IDynamicCategoryRepository categoryRepo,
            JsonElement originalElement,
            List<(string propName, string group, string rawValue, bool isRequired)> replacements,
            List<string> errors)
        {
            if (!replacements.Any()) return null;

            var jsonObj = originalElement.Deserialize<Dictionary<string, object>>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new Dictionary<string, object>();

            foreach (var rep in replacements)
            {
                try
                {
                    var mappedValue = await DynamicCategoryUtility.ValidateAndMapAsync(
                        categoryRepo,
                        rep.group,
                        rep.rawValue,
                        rep.isRequired);

                    if (!string.Equals(rep.rawValue, mappedValue, StringComparison.OrdinalIgnoreCase))
                    {
                        jsonObj[rep.propName] = mappedValue;
                    }
                }
                catch (ArgumentException ex)
                {
                    errors.Add(ex.Message);
                }
            }

            return errors.Any() ? null : JsonSerializer.Serialize(jsonObj, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private async Task ReturnBadRequest(HttpContext context, string message)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            var payload = JsonSerializer.Serialize(new ApiResponseDTO
            {
                IsSuccess = false,
                Code = 400,
                Message = message
            });
            await context.Response.WriteAsync(payload);
        }
    }
}
