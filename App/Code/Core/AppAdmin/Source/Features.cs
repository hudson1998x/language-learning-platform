using System.Reflection;
using System.Text.Json;
using LLE.AppAdmin.Dto;
using LLE.AppAdmin.Events;
using LLE.Eventing;
using LLE.Kernel.Attributes;
using LLE.Kernel.Dto;
using LLE.Kernel.Registry;

namespace LLE.AppAdmin;

public static class Features
{
    public static void LoadFeatures()
    {
        FeatureRegistry.Add(new Feature<object, ApiResponse<Dictionary<string, Dictionary<string, ConfigFieldInfo>>>>
        {
            FeatureName = "listConfigs",
            FeatureGroup = "admin",
            Route = "/api/configuration/list",
            Method = HttpMethod.Get,
            Handler = async (_, _) =>
            {
                var result = new Dictionary<string, Dictionary<string, ConfigFieldInfo>>();

                foreach (var (typeName, config) in ConfigurationCatalog.GetAllConfigurations())
                {
                    var fields = new Dictionary<string, ConfigFieldInfo>();

                    foreach (var property in config.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (!property.CanRead) continue;

                        var componentAttr = property.GetCustomAttribute<ConfigComponentAttribute>();
                        var value = property.GetValue(config);

                        fields[property.Name] = new ConfigFieldInfo
                        {
                            Value = value,
                            Type = MapPropertyType(property.PropertyType),
                            Component = componentAttr?.Component
                        };
                    }

                    result[typeName] = fields;
                }

                return new ApiResponse<Dictionary<string, Dictionary<string, ConfigFieldInfo>>>
                {
                    Success = true,
                    Data = result
                };
            }
        });

        FeatureRegistry.Add(new Feature<ConfigurationChangeRequest, ApiResponse<ConfigurationChangeResponse>>
        {
            FeatureName = "changeSettings",
            FeatureGroup = "admin",
            Route = "/api/configuration/update",
            Method = HttpMethod.Post,
            Handler = async (request, _) =>
            {
                var configs = ConfigurationCatalog.GetAllConfigurations();

                if (!configs.TryGetValue(request.ConfigurationType, out var config))
                {
                    var type = FindConfigurationType(request.ConfigurationType);
                    if (type is null)
                    {
                        return new ApiResponse<ConfigurationChangeResponse>
                        {
                            Success = false,
                            Message = $"Configuration '{request.ConfigurationType}' not found."
                        };
                    }

                    config = ConfigurationCatalog.GetConfiguration(type);
                }

                var json = request.Configuration.GetRawText();
                object? updatedConfig;

                try
                {
                    updatedConfig = JsonSerializer.Deserialize(json, config.GetType(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (JsonException ex)
                {
                    return new ApiResponse<ConfigurationChangeResponse>
                    {
                        Success = false,
                        Message = $"Invalid configuration values: {ex.Message}"
                    };
                }

                if (updatedConfig is null)
                {
                    return new ApiResponse<ConfigurationChangeResponse>
                    {
                        Success = false,
                        Message = "Failed to deserialize configuration values."
                    };
                }

                foreach (var property in config.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!property.CanWrite || !property.CanRead) continue;
                    property.SetValue(config, property.GetValue(updatedConfig));
                }

                ConfigurationCatalog.SaveConfiguration(config.GetType());

                await Eventing.Eventing.Of<ConfigurationEvents>().Changed.DispatchAsync(config);

                return new ApiResponse<ConfigurationChangeResponse>
                {
                    Success = true,
                    Message = $"Configuration '{request.ConfigurationType}' updated successfully."
                };
            }
        });
    }

    private static string MapPropertyType(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(bool)) return "boolean";
        if (type == typeof(int) || type == typeof(long) || type == typeof(short) ||
            type == typeof(float) || type == typeof(double) || type == typeof(decimal))
            return "number";
        return "string";
    }

    private static Type? FindConfigurationType(string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.Name == typeName && type.GetCustomAttribute<ConfigurationAttribute>() is not null)
                    return type;
            }
        }

        return null;
    }
}
