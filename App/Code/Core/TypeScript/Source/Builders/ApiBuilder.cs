using System.Text;
using System.Text.RegularExpressions;
using LLE.Kernel.Registry;

namespace LLE.TypeScript.Builders;

public class ApiBuilder : IDisposable
{
    private readonly Dictionary<string, StringBuilder> _sourceBuilder = [];
    private readonly HashSet<(string Group, string Name)> _emittedFeatures = [];
    private readonly TypeScriptTypeMapper _typeMapper = new();

    private static readonly Regex ParamPattern = new(@"\{(\w+)\}", RegexOptions.Compiled);

    private static string[] ExtractPathParams(string route) =>
        ParamPattern.Matches(route).Select(m => m.Groups[1].Value).ToArray();

    private static string BuildRouteString(string route, string[] pathParams)
    {
        if (pathParams.Length == 0)
            return $"'{route}'";

        var interpolated = route;
        foreach (var p in pathParams)
            interpolated = interpolated.Replace($"{{{p}}}", $"${{{p}}}");
        return $"`{interpolated}`";
    }

    public void AddFeature(FeatureDefinition feature)
    {
        if (!_emittedFeatures.Add((feature.FeatureGroup, feature.FeatureName)))
            return;

        var builder = GetSourceBuilder(feature);
        var isGet = feature.Method == HttpMethod.Get;
        var pathParams = ExtractPathParams(feature.Route);

        // Emit the type declarations this feature depends on (DTOs, entities,
        // enums, and anything they reference) before the function that uses them.
        if (!isGet)
        {
            _typeMapper.EmitTypeAndDependencies(builder, feature.FeatureGroup, feature.InputType);
        }
        _typeMapper.EmitTypeAndDependencies(builder, feature.FeatureGroup, feature.OutputType);

        var inputTypeName = _typeMapper.GetTypeReference(feature.InputType);
        var outputTypeName = _typeMapper.GetTypeReference(feature.OutputType);

        var paramSignature = pathParams.Length > 0
            ? string.Join(", ", pathParams.Select(p => $"{p}: string"))
            : isGet ? string.Empty : $"payload: {inputTypeName}";

        var routeString = BuildRouteString(feature.Route, pathParams);

        builder.AppendLine($"export const {feature.FeatureName} = ({paramSignature}): Promise<{outputTypeName}> => {{");
        builder.AppendLine($"    return fetch({routeString}, {{");
        builder.AppendLine($"        method: \"{feature.Method}\",");

        if (!isGet)
        {
            builder.AppendLine("        headers: {");
            builder.AppendLine("            \"Content-Type\": \"application/json\"");
            builder.AppendLine("        },");
            builder.AppendLine("        body: JSON.stringify(payload)");
        }

        builder.AppendLine("    })");
        builder.AppendLine("    .then((response: Response) => {");
        builder.AppendLine("        if (!response.ok) {");
        builder.AppendLine("            throw new Error(`Request failed with status ${response.status}`);");
        builder.AppendLine("        }");
        builder.AppendLine("        return response.json();");
        builder.AppendLine("    });");
        builder.AppendLine("};");
        builder.AppendLine();
    }

    private StringBuilder GetSourceBuilder(FeatureDefinition feature)
    {
        if (_sourceBuilder.TryGetValue(feature.FeatureGroup, out var builder))
        {
            return builder;
        }

        builder = new StringBuilder();
        _sourceBuilder[feature.FeatureGroup] = builder;
        return builder;
    }

    /// <summary>
    /// Returns the generated TypeScript source per feature group,
    /// keyed by group name (e.g. "users" -> "users.ts" contents).
    /// </summary>
    public IReadOnlyDictionary<string, string> Build()
    {
        return _sourceBuilder.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToString());
    }

    /// <summary>
    /// Writes each feature group's generated source to <paramref name="outputDirectory"/>
    /// as "{group}.ts".
    /// </summary>
    public void WriteToDisk(string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        foreach (var (group, source) in Build())
        {
            var path = Path.Combine(outputDirectory, $"{group}.ts");
            File.WriteAllText(path, source);
        }
    }

    public void Dispose()
    {
        _sourceBuilder.Clear();
        _emittedFeatures.Clear();
    }
}