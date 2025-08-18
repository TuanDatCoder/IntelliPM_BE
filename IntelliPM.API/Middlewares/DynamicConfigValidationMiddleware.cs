using IntelliPM.Common.Attributes;
using IntelliPM.Data.DTOs;
using IntelliPM.Services.SystemConfigurationServices;
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
    public class DynamicConfigValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DynamicConfigValidationMiddleware> _logger;

        public DynamicConfigValidationMiddleware(
            RequestDelegate next,
            ILogger<DynamicConfigValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip requests that don't require validation
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
                // Read the request body
                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                if (string.IsNullOrWhiteSpace(body))
                {
                    await _next(context);
                    return;
                }

                // Parse JSON body
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(
                    body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var errors = new List<string>();

                // Get the DTO type of the current action
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
                    // Get service from DI container
                    var systemConfigService = context.RequestServices.GetRequiredService<ISystemConfigurationService>();
                    ExtractAndValidateAttributes(jsonElement, dtoType, systemConfigService, errors);
                }

                if (errors.Any())
                {
                    _logger.LogError("DynamicConfigValidationMiddleware: Validation errors: {Errors}", string.Join(" ", errors));
                    await ReturnBadRequest(context, string.Join(" ", errors));
                    return;
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DynamicConfigValidationMiddleware");
                await ReturnBadRequest(context, "Invalid request content.");
            }
            finally
            {
                context.Request.Body.Position = 0;
            }
        }

        private void ExtractAndValidateAttributes(
            JsonElement element,
            Type dtoType,
            ISystemConfigurationService systemConfigService,
            List<string> errors)
        {
            if (element.ValueKind != JsonValueKind.Object) return;

            // Get properties with validation attributes
            var propsWithAttrs = dtoType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new
                {
                    Prop = p,
                    RangeAttr = p.GetCustomAttribute<DynamicRangeAttribute>(),
                    MaxLengthAttr = p.GetCustomAttribute<DynamicMaxLengthAttribute>(),
                    MinLengthAttr = p.GetCustomAttribute<DynamicMinLengthAttribute>(),
                    DurationAttr = p.GetCustomAttribute<DynamicDurationAttribute>()
                })
                .Where(x => x.RangeAttr != null || x.MaxLengthAttr != null || x.MinLengthAttr != null || x.DurationAttr != null)
                .ToList();

            // For Duration, extract StartDate and EndDate from JSON
            DateTime? startDate = null, endDate = null;
            if (propsWithAttrs.Any(x => x.DurationAttr != null))
            {
                foreach (var prop in element.EnumerateObject())
                {
                    if (string.Equals(prop.Name, "StartDate", StringComparison.OrdinalIgnoreCase) && prop.Value.TryGetDateTime(out var start))
                        startDate = start;
                    if (string.Equals(prop.Name, "EndDate", StringComparison.OrdinalIgnoreCase) && prop.Value.TryGetDateTime(out var end))
                        endDate = end;
                }
            }

            foreach (var property in element.EnumerateObject())
            {
                var match = propsWithAttrs
                    .FirstOrDefault(p => string.Equals(p.Prop.Name, property.Name, StringComparison.OrdinalIgnoreCase));

                if (match == null) continue;

                // Handle DynamicRangeAttribute
                if (match.RangeAttr != null)
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        errors.Add($"Property '{property.Name}' must not be null.");
                        continue;
                    }

                    if (property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetDecimal(out var decimalValue))
                    {
                        var config = systemConfigService.GetSystemConfigurationByConfigKey(match.RangeAttr.GetConfigKey()).GetAwaiter().GetResult();
                        if (config == null)
                        {
                            errors.Add($"Configuration for '{match.RangeAttr.GetConfigKey()}' not found.");
                            continue;
                        }

                        if (!decimal.TryParse(config.MinValue, out var minValue) || !decimal.TryParse(config.MaxValue, out var maxValue))
                        {
                            errors.Add($"Invalid min/max values for '{match.RangeAttr.GetConfigKey()}'.");
                            continue;
                        }

                        if (decimalValue < minValue)
                            errors.Add($"Property '{property.Name}' must not be less than {minValue}.");
                        if (decimalValue > maxValue)
                            errors.Add($"Property '{property.Name}' must not exceed {maxValue}.");
                    }
                    else
                    {
                        errors.Add($"Property '{property.Name}' must be a number.");
                    }
                }

                // Handle DynamicMaxLengthAttribute
                if (match.MaxLengthAttr != null)
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        errors.Add($"Property '{property.Name}' must not be null.");
                        continue;
                    }

                    if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        var value = property.Value.GetString();
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            errors.Add($"Property '{property.Name}' must not be empty.");
                            continue;
                        }

                        var config = systemConfigService.GetSystemConfigurationByConfigKey(match.MaxLengthAttr.GetConfigKey()).GetAwaiter().GetResult();
                        if (config == null)
                        {
                            errors.Add($"Configuration for '{match.MaxLengthAttr.GetConfigKey()}' not found.");
                            continue;
                        }

                        if (!int.TryParse(config.MaxValue, out var maxLength))
                        {
                            errors.Add($"Invalid max length value for '{match.MaxLengthAttr.GetConfigKey()}'.");
                            continue;
                        }

                        if (value.Length > maxLength)
                            errors.Add($"Property '{property.Name}' must not exceed {maxLength} characters.");
                    }
                    else
                    {
                        errors.Add($"Property '{property.Name}' must be a string.");
                    }
                }

                // Handle DynamicMinLengthAttribute
                if (match.MinLengthAttr != null)
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        errors.Add($"Property '{property.Name}' must not be null.");
                        continue;
                    }

                    if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        var value = property.Value.GetString();
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            errors.Add($"Property '{property.Name}' must not be empty.");
                            continue;
                        }

                        var config = systemConfigService.GetSystemConfigurationByConfigKey(match.MinLengthAttr.GetConfigKey()).GetAwaiter().GetResult();
                        if (config == null)
                        {
                            errors.Add($"Configuration for '{match.MinLengthAttr.GetConfigKey()}' not found.");
                            continue;
                        }

                        if (!int.TryParse(config.MinValue, out var minLength))
                        {
                            errors.Add($"Invalid min length value for '{match.MinLengthAttr.GetConfigKey()}'.");
                            continue;
                        }

                        if (value.Length < minLength)
                            errors.Add($"Property '{property.Name}' must be at least {minLength} characters.");
                    }
                    else
                    {
                        errors.Add($"Property '{property.Name}' must be a string.");
                    }
                }
            }

            // Handle DynamicDurationAttribute
            if (propsWithAttrs.Any(x => x.DurationAttr != null))
            {
                if (startDate == null || endDate == null)
                {
                    errors.Add("Properties 'StartDate' or 'EndDate' must not be null or invalid.");
                }
                else if (startDate > endDate)
                {
                    errors.Add("Start date must be before or equal to end date.");
                }
                else
                {
                    var durationAttr = propsWithAttrs.FirstOrDefault(x => x.DurationAttr != null)?.DurationAttr;
                    if (durationAttr != null)
                    {
                        var config = systemConfigService.GetSystemConfigurationByConfigKey(durationAttr.GetConfigKey()).GetAwaiter().GetResult();
                        if (config == null)
                        {
                            errors.Add($"Configuration for '{durationAttr.GetConfigKey()}' not found.");
                        }
                        else if (!int.TryParse(config.MinValue, out var minDays) || !int.TryParse(config.MaxValue, out var maxDays))
                        {
                            errors.Add($"Invalid min/max duration values for '{durationAttr.GetConfigKey()}'.");
                        }
                        else
                        {
                            var duration = (endDate.Value - startDate.Value).Days;
                            if (duration < minDays)
                                errors.Add($"Project duration must not be less than {minDays} days.");
                            if (duration > maxDays)
                                errors.Add($"Project duration must not exceed {maxDays} days.");
                        }
                    }
                }
            }
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