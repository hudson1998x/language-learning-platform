using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using LLE.Kernel.Attributes;

namespace LLE.Kernel.Registry;

public static class ConfigurationCatalog
{
    private static readonly ConcurrentDictionary<Type, object> ConfigMap = [];

    private static readonly string ConfigDirectory =
        Path.Combine(AppContext.BaseDirectory, "var", "configs");

    public static object GetConfiguration(Type type)
    {
        if (ConfigMap.TryGetValue(type, out var cfg))
            return cfg;

        var instance = Activator.CreateInstance(type)
            ?? throw new InvalidOperationException("Unable to instantiate a null configuration.");

        MergeFromFile(instance);
        ApplyEnvironmentOverrides(instance);

        ConfigMap.TryAdd(type, instance);
        return instance;
    }

    private static void MergeFromFile(object instance)
    {
        Directory.CreateDirectory(ConfigDirectory);

        var filePath = Path.Combine(ConfigDirectory, $"{instance.GetType().Name}.json");
        if (!File.Exists(filePath))
            return;

        var json = File.ReadAllText(filePath);

        using var doc = JsonDocument.Parse(json);
        var jsonProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var prop in doc.RootElement.EnumerateObject())
            jsonProperties.Add(prop.Name);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var jsonInstance = JsonSerializer.Deserialize(json, instance.GetType(), options);
        if (jsonInstance is null) return;

        foreach (var property in instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanWrite || !property.CanRead) continue;
            if (!jsonProperties.Contains(property.Name)) continue;

            property.SetValue(instance, property.GetValue(jsonInstance));
        }
    }

    private static void ApplyEnvironmentOverrides(object instance)
    {
        foreach (var property in instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanWrite) continue;

            var attr = property.GetCustomAttributes(true)
                .FirstOrDefault(a => a.GetType().IsGenericType &&
                                     a.GetType().GetGenericTypeDefinition() == typeof(FromEnvironmentAttribute<>));

            if (attr is null) continue;

            var keyField = attr.GetType().GetField("Key");
            var key = keyField?.GetValue(attr) as string;
            if (key is null) continue;

            var envValue = Environment.GetEnvironmentVariable(key);
            if (envValue is null) continue;

            var convertedValue = Convert.ChangeType(envValue, property.PropertyType);
            property.SetValue(instance, convertedValue);
        }
    }

    public static T GetConfiguration<T>() where T : class => (T)GetConfiguration(typeof(T));
}