using IntelliPM.Common.Attributes;
using IntelliPM.Data.Contexts;
using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.Requirement.Request;
using IntelliPM.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

        public async Task InvokeAsync(HttpContext context, Su25Sep490IntelliPmContext dbContext)
        {
            // Only process JSON requests with a body
            if (context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) != true)
            {
                await _next(context);
                return;
            }

            context.Request.EnableBuffering();

            try
            {
                // Read and deserialize the body into a JsonElement
                context.Request.Body.Position = 0;
                var jsonElement = await JsonSerializer.DeserializeAsync<JsonElement>(
                    context.Request.Body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Reset stream for downstream middleware/controller
                context.Request.Body.Position = 0;

                // Extract dynamic category validations
                var requiredByGroup = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
                var replacements = new List<(string propName, string group, string rawValue)>();
                var errors = new List<string>();

                // Process the JSON element
                ExtractAttributesFromJson(jsonElement, requiredByGroup, replacements, errors);

                if (errors.Any())
                {
                    await ReturnBadRequest(context, string.Join(" ", errors));
                    return;
                }

                if (!requiredByGroup.Any())
                {
                    await _next(context);
                    return;
                }

                // Query DB once for all category groups
                var groups = requiredByGroup.Keys.ToList();
                var categories = await dbContext.DynamicCategory
                    .Where(dc => dc.IsActive && groups.Contains(dc.CategoryGroup))
                    .ToListAsync();

                // Check for missing categories
                var missingMessages = new List<string>();
                foreach (var kv in requiredByGroup)
                {
                    var group = kv.Key;
                    var wanted = kv.Value;
                    var foundSet = categories
                        .Where(c => string.Equals(c.CategoryGroup, group, StringComparison.OrdinalIgnoreCase))
                        .SelectMany(c => new[] { c.Name, c.Label })
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    var missingValues = wanted.Where(w => !foundSet.Contains(w)).ToList();
                    if (missingValues.Any())
                    {
                        missingMessages.Add($"Invalid {group}: {string.Join(", ", missingValues)} is not a valid value.");
                    }
                }

                if (missingMessages.Any())
                {
                    _logger.LogWarning("DynamicCategoryValidationMiddleware: invalid categories {Messages}", missingMessages);
                    await ReturnBadRequest(context, string.Join(" ", missingMessages));
                    return;
                }

                // Map Label to Name in the request body
                if (replacements.Any())
                {
                    var modifiedBody = await ModifyRequestBody(context, jsonElement, categories, replacements);
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

        private void ExtractAttributesFromJson(JsonElement element, Dictionary<string, HashSet<string>> requiredByGroup, List<(string propName, string group, string rawValue)> replacements, List<string> errors)
        {
            if (element.ValueKind != JsonValueKind.Object) return;

            // Assume the DTO type for this endpoint
            var dtoType = typeof(RequirementRequestDTO); // Adjust based on your endpoint

            foreach (var property in element.EnumerateObject())
            {
                var propName = property.Name;
                var propInfo = dtoType.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (propInfo == null) continue;

                var attr = propInfo.GetCustomAttribute<DynamicCategoryValidationAttribute>();
                if (attr == null) continue;

                if (property.Value.ValueKind == JsonValueKind.Null || property.Value.ValueKind == JsonValueKind.Undefined)
                {
                    if (attr.Required)
                    {
                        errors.Add($"Property '{propName}' (CategoryGroup='{attr.CategoryGroup}') is required but was null.");
                    }
                    continue;
                }

                if (property.Value.ValueKind == JsonValueKind.String)
                {
                    var value = property.Value.GetString();
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        if (attr.Required)
                            errors.Add($"Property '{propName}' (CategoryGroup='{attr.CategoryGroup}') cannot be empty.");
                        continue;
                    }
                    AddRequired(requiredByGroup, attr.CategoryGroup, value);
                    replacements.Add((propName, attr.CategoryGroup, value));
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
                                AddRequired(requiredByGroup, attr.CategoryGroup, value);
                                replacements.Add((propName, attr.CategoryGroup, value));
                            }
                        }
                    }
                }
            }
        }

        private static async Task<string> ModifyRequestBody(HttpContext context, JsonElement originalElement, List<DynamicCategory> categories, List<(string propName, string group, string rawValue)> replacements)
        {
            var jsonObj = originalElement.Deserialize<Dictionary<string, object>>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new Dictionary<string, object>();

            foreach (var rep in replacements)
            {
                var match = categories
                    .FirstOrDefault(c =>
                        string.Equals(c.CategoryGroup, rep.group, StringComparison.OrdinalIgnoreCase) &&
                        (string.Equals(c.Label, rep.rawValue, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(c.Name, rep.rawValue, StringComparison.OrdinalIgnoreCase)));

                if (match != null && !string.Equals(rep.rawValue, match.Name, StringComparison.OrdinalIgnoreCase))
                {
                    jsonObj[rep.propName] = match.Name;
                }
            }

            return JsonSerializer.Serialize(jsonObj, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private static void AddRequired(Dictionary<string, HashSet<string>> dict, string group, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            if (!dict.TryGetValue(group, out var set))
            {
                set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                dict[group] = set;
            }
            set.Add(value.Trim());
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