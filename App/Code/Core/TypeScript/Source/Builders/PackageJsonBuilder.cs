using System.Text;
using System.Text.Json;

namespace LLE.TypeScript.Builders
{
    /// <summary>
    /// Specifies which dependency section(s) of a <c>package.json</c> file an entry belongs to.
    /// </summary>
    [Flags]
    public enum Dependencies
    {
        /// <summary>The package is a runtime dependency, listed under <c>dependencies</c>.</summary>
        App = 1 << 0,

        /// <summary>The package is a development-only dependency, listed under <c>devDependencies</c>.</summary>
        Dev = 1 << 1
    }

    /// <summary>
    /// Builds the configuration for a Node.js package, equivalent to the
    /// contents of a <c>package.json</c> file.
    /// </summary>
    public sealed class PackageJsonBuilder
    {
        private const string AnyVersion = "*";

        /// <summary>
        /// Gets or sets the package name.
        /// </summary>
        private string? Name { get; set; }

        /// <summary>
        /// Gets or sets the package version.
        /// </summary>
        private string? Version { get; set; }

        /// <summary>
        /// Gets or sets the package description.
        /// </summary>
        private string? Description { get; set; }

        /// <summary>
        /// Gets or sets the entry point of the package.
        /// </summary>
        private string? Main { get; set; }

        /// <summary>
        /// Gets the npm scripts defined for the package, keyed by script name.
        /// </summary>
        private Dictionary<string, string> Scripts { get; } = [];

        /// <summary>
        /// Gets the runtime dependencies of the package, keyed by package name.
        /// </summary>
        private Dictionary<string, string> AppDependencies { get; } = [];

        /// <summary>
        /// Gets the development dependencies of the package, keyed by package name.
        /// </summary>
        private Dictionary<string, string> DevDependencies { get; } = [];

        /// <summary>
        /// Sets the package name.
        /// </summary>
        public PackageJsonBuilder WithName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the package version.
        /// </summary>
        public PackageJsonBuilder WithVersion(string version)
        {
            Version = version;
            return this;
        }

        /// <summary>
        /// Sets the package description.
        /// </summary>
        public PackageJsonBuilder WithDescription(string description)
        {
            Description = description;
            return this;
        }

        /// <summary>
        /// Sets the entry point of the package.
        /// </summary>
        public PackageJsonBuilder WithMain(string main)
        {
            Main = main;
            return this;
        }

        /// <summary>
        /// Adds or overwrites an npm script.
        /// </summary>
        /// <param name="key">The script name (e.g. <c>"build"</c>).</param>
        /// <param name="command">The shell command the script runs.</param>
        public PackageJsonBuilder AddScript(string key, string command)
        {
            Scripts[key] = command;
            return this;
        }

        /// <summary>
        /// Adds or overwrites a dependency with an explicit semantic version range.
        /// </summary>
        /// <param name="packageName">The name of the npm package.</param>
        /// <param name="semanticVersion">
        /// The semantic version range to pin the dependency to (e.g. <c>"^5.4.0"</c>).
        /// If <see langword="null"/>, the dependency is added with a version of <c>"*"</c>.
        /// </param>
        /// <param name="dependencies">
        /// Which section(s) to add the dependency to. Combine <see cref="Dependencies.App"/> and
        /// <see cref="Dependencies.Dev"/> to add the same package to both.
        /// </param>
        public PackageJsonBuilder AddDependency(string packageName, string? semanticVersion, Dependencies dependencies)
        {
            var version = semanticVersion ?? AnyVersion;

            if (dependencies.HasFlag(Dependencies.App))
            {
                AppDependencies[packageName] = version;
            }

            if (dependencies.HasFlag(Dependencies.Dev))
            {
                DevDependencies[packageName] = version;
            }

            return this;
        }

        /// <summary>
        /// Adds or overwrites a dependency with no explicit version constraint (<c>"*"</c>).
        /// </summary>
        /// <param name="packageName">The name of the npm package.</param>
        /// <param name="dependencies">
        /// Which section(s) to add the dependency to. Combine <see cref="Dependencies.App"/> and
        /// <see cref="Dependencies.Dev"/> to add the same package to both.
        /// </param>
        public PackageJsonBuilder AddDependency(string packageName, Dependencies dependencies)
            => AddDependency(packageName, semanticVersion: null, dependencies);

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append('{');

            var first = true;

            AppendStringProperty(sb, "name", Name, ref first);
            AppendStringProperty(sb, "version", Version, ref first);
            AppendStringProperty(sb, "description", Description, ref first);
            AppendStringProperty(sb, "main", Main, ref first);
            AppendStringProperty(sb, "type", "module", ref first);

            AppendObjectProperty(sb, "scripts", Scripts, ref first);
            AppendObjectProperty(sb, "dependencies", AppDependencies, ref first);
            AppendObjectProperty(sb, "devDependencies", DevDependencies, ref first);

            sb.Append('}');

            return PrettyPrint(sb.ToString());
        }

        private static string PrettyPrint(string compactJson)
        {
            using var doc = JsonDocument.Parse(compactJson);
            return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
        }

        private static void AppendStringProperty(StringBuilder sb, string name, string? value, ref bool first)
        {
            if (value is null) return;

            if (!first) sb.Append(',');
            first = false;

            sb.Append('"').Append(name).Append("\":\"").Append(EscapeJson(value)).Append('"');
        }

        private static void AppendObjectProperty(StringBuilder sb, string name, IReadOnlyDictionary<string, string> values, ref bool first)
        {
            if (values.Count == 0) return;

            if (!first) sb.Append(',');
            first = false;

            sb.Append('"').Append(name).Append("\":{");

            var firstEntry = true;
            foreach (var kvp in values)
            {
                if (!firstEntry) sb.Append(',');
                firstEntry = false;

                sb.Append('"').Append(EscapeJson(kvp.Key)).Append("\":\"").Append(EscapeJson(kvp.Value)).Append('"');
            }

            sb.Append('}');
        }

        private static string EscapeJson(string? s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            return s
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }
    }
}