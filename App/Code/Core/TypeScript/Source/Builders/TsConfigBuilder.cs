using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
// ReSharper disable InconsistentNaming

namespace LLE.TypeScript.Builders
{
    /// <summary>
    /// Builds the configuration for a TypeScript project, equivalent to the
    /// contents of a <c>tsconfig.json</c> file.
    /// </summary>
    public sealed class TsConfigBuilder
    {
        /// <summary>
        /// Gets the compiler options applied to the TypeScript project.
        /// </summary>
        public TsCompilerOptions CompilerOptions { get; } = new();

        /// <summary>
        /// Gets the set of file or glob patterns to include in the compilation.
        /// </summary>
        public HashSet<string> Include { get; } = [];

        /// <summary>
        /// Gets the set of file or glob patterns to exclude from the compilation.
        /// </summary>
        public HashSet<string> Exclude { get; } = [];

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append('{');

            sb.Append("\"compilerOptions\":").Append(CompilerOptions);

            AppendArrayProperty(sb, "include", Include);
            AppendArrayProperty(sb, "exclude", Exclude);

            sb.Append('}');

            return PrettyPrint(sb.ToString());
        }

        private static string PrettyPrint(string compactJson)
        {
            using var doc = JsonDocument.Parse(compactJson);
            return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
        }

        private static void AppendArrayProperty(StringBuilder sb, string name, IEnumerable<string> values)
        {
            sb.Append(",\"").Append(name).Append("\":[");

            var first = true;
            foreach (var value in values)
            {
                if (!first)
                {
                    sb.Append(',');
                }
                first = false;

                sb.Append('"').Append(EscapeJson(value)).Append('"');
            }

            sb.Append(']');
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

    /// <summary>
    /// Represents the set of options passed to the TypeScript compiler,
    /// corresponding to the <c>compilerOptions</c> section of a <c>tsconfig.json</c> file.
    /// </summary>
    public sealed class TsCompilerOptions
    {
        /// <summary>
        /// Gets or sets the ECMAScript target version that the compiler emits.
        /// </summary>
        public TsTarget? Target { get; set; }

        /// <summary>
        /// Gets or sets the module system used for the emitted JavaScript code.
        /// </summary>
        public TsModule? Module { get; set; }

        /// <summary>
        /// Gets or sets the strategy the compiler uses to resolve module imports.
        /// </summary>
        public TsModuleResolution? ModuleResolution { get; set; }

        /// <summary>
        /// Gets or sets the JSX code generation mode.
        /// </summary>
        public JsxMode? Jsx { get; set; }

        /// <summary>
        /// Gets or sets the module specifier used to import the JSX factory functions
        /// when <see cref="Jsx"/> is set to <see cref="JsxMode.ReactJsx"/> or <see cref="JsxMode.ReactJsxDev"/>.
        /// </summary>
        public string? JsxImportSource { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all strict type-checking options are enabled.
        /// </summary>
        public bool? Strict { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether type checking is skipped for all declaration (<c>.d.ts</c>) files.
        /// </summary>
        public bool? SkipLibCheck { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether importing <c>.json</c> files as modules is allowed.
        /// </summary>
        public bool? ResolveJsonModule { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether each file is required to be safely transpilable
        /// without relying on other imports, as required by some non-TypeScript-aware transpilers.
        /// </summary>
        public bool? IsolatedModules { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether JavaScript files are allowed to be compiled.
        /// </summary>
        public bool? AllowJs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether JavaScript files are type-checked.
        /// </summary>
        public bool? CheckJs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether corresponding <c>.d.ts</c> declaration files are generated.
        /// </summary>
        public bool? Declaration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether source map files are generated for emitted JavaScript.
        /// </summary>
        public bool? SourceMap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether interop helpers are emitted to simplify importing
        /// CommonJS modules under ES module semantics.
        /// </summary>
        public bool? EsModuleInterop { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether default imports are allowed from modules
        /// with no default export, by treating them as having one.
        /// </summary>
        public bool? AllowSyntheticDefaultImports { get; set; }

        /// <summary>
        /// Gets the set of path mapping entries used to resolve module specifiers to specific locations,
        /// relative to <c>baseUrl</c>.
        /// </summary>
        public Dictionary<string, string[]> Paths { get; } = [];

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append('{');

            var first = true;

            AppendEnumProperty(sb, "target", Target, ref first);
            AppendEnumProperty(sb, "module", Module, ref first);
            AppendEnumProperty(sb, "moduleResolution", ModuleResolution, ref first);
            AppendEnumProperty(sb, "jsx", Jsx, ref first);
            AppendStringProperty(sb, "jsxImportSource", JsxImportSource, ref first);
            AppendBoolProperty(sb, "strict", Strict, ref first);
            AppendBoolProperty(sb, "skipLibCheck", SkipLibCheck, ref first);
            AppendBoolProperty(sb, "resolveJsonModule", ResolveJsonModule, ref first);
            AppendBoolProperty(sb, "isolatedModules", IsolatedModules, ref first);
            AppendBoolProperty(sb, "allowJs", AllowJs, ref first);
            AppendBoolProperty(sb, "checkJs", CheckJs, ref first);
            AppendBoolProperty(sb, "declaration", Declaration, ref first);
            AppendBoolProperty(sb, "sourceMap", SourceMap, ref first);
            AppendBoolProperty(sb, "esModuleInterop", EsModuleInterop, ref first);
            AppendBoolProperty(sb, "allowSyntheticDefaultImports", AllowSyntheticDefaultImports, ref first);

            if (Paths.Count > 0)
            {
                if (!first) sb.Append(',');
                first = false;

                sb.Append("\"paths\":{");
                var firstPath = true;
                foreach (var kvp in Paths)
                {
                    if (!firstPath) sb.Append(',');
                    firstPath = false;

                    sb.Append('"').Append(EscapeJson(LowerCamelCase(kvp.Key))).Append("\":[");
                    sb.Append(string.Join(",", kvp.Value.Select(v => $"\"{EscapeJson(v)}\"")));
                    sb.Append(']');
                }
                sb.Append('}');
            }

            sb.Append('}');
            return sb.ToString();
        }

        private static void AppendEnumProperty<T>(StringBuilder sb, string name, T? value, ref bool first)
            where T : struct, Enum
        {
            if (value is null) return;

            if (!first) sb.Append(',');
            first = false;

            sb.Append('"').Append(name).Append("\":\"").Append(GetEnumJsonValue(value.Value)).Append('"');
        }

        private static string GetEnumJsonValue<T>(T value)
            where T : struct, Enum
        {
            var member = typeof(T).GetField(value.ToString());
            var attribute = member?.GetCustomAttributes(typeof(EnumMemberAttribute), inherit: false)
                .OfType<EnumMemberAttribute>()
                .FirstOrDefault();

            return attribute?.Value ?? LowerCamelCase(value.ToString());
        }

        private static void AppendBoolProperty(StringBuilder sb, string name, bool? value, ref bool first)
        {
            if (value is null) return;

            if (!first) sb.Append(',');
            first = false;

            sb.Append('"').Append(name).Append("\":").Append(value.Value ? "true" : "false");
        }

        private static void AppendStringProperty(StringBuilder sb, string name, string? value, ref bool first)
        {
            if (value is null) return;

            if (!first) sb.Append(',');
            first = false;

            sb.Append('"').Append(name).Append("\":\"").Append(EscapeJson(value)).Append('"');
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

        private static string LowerCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToLowerInvariant(s[0]) + s[1..];
        }
    }

    /// <summary>
    /// Specifies the JSX code generation mode used by the compiler.
    /// </summary>
    public enum JsxMode
    {
        /// <summary>JSX is left unchanged in the output, to be processed by another tool.</summary>
        [EnumMember(Value = "preserve")]
        Preserve,

        /// <summary>JSX is compiled to <c>React.createElement</c> calls.</summary>
        [EnumMember(Value = "react")]
        React,

        /// <summary>JSX is compiled using the React 17+ automatic runtime.</summary>
        [EnumMember(Value = "react-jsx")]
        ReactJsx,

        /// <summary>JSX is compiled using the React 17+ automatic runtime with additional debug information.</summary>
        [EnumMember(Value = "react-jsxdev")]
        ReactJsxDev,

        /// <summary>JSX is compiled in a form suitable for React Native, leaving JSX unchanged but with a <c>.js</c> file extension.</summary>
        [EnumMember(Value = "react-native")]
        ReactNative
    }

    /// <summary>
    /// Specifies the module system used for emitted JavaScript code.
    /// </summary>
    public enum TsModule
    {
        /// <summary>Emits modules using the CommonJS (<c>require</c>/<c>module.exports</c>) format.</summary>
        CommonJs,

        /// <summary>Emits modules using the ES2015 module syntax.</summary>
        ES2015,

        /// <summary>Emits modules using the ES2020 module syntax.</summary>
        ES2020,

        /// <summary>Emits modules using the ES2022 module syntax.</summary>
        ES2022,

        /// <summary>Emits modules using the latest supported ECMAScript module syntax.</summary>
        ESNext,

        /// <summary>Emits modules compatible with Node.js 16's module system.</summary>
        Node16,

        /// <summary>Emits modules compatible with the latest Node.js module system.</summary>
        NodeNext,

        /// <summary>Leaves module syntax unchanged in the output.</summary>
        Preserve
    }

    /// <summary>
    /// Specifies the strategy the compiler uses to resolve module import paths.
    /// </summary>
    public enum TsModuleResolution
    {
        /// <summary>Uses TypeScript's original, legacy resolution strategy.</summary>
        Classic,

        /// <summary>Mimics the Node.js CommonJS module resolution strategy.</summary>
        Node10,
        
        /// <summary>
        /// Node.
        /// </summary>
        Node,

        /// <summary>Mimics the Node.js 16 module resolution strategy, supporting both ESM and CommonJS.</summary>
        Node16,

        /// <summary>Mimics the latest Node.js module resolution strategy, supporting both ESM and CommonJS.</summary>
        NodeNext,

        /// <summary>Mimics the resolution strategy used by modern bundlers.</summary>
        Bundler
    }

    /// <summary>
    /// Specifies the ECMAScript target version that the compiler emits code for.
    /// </summary>
    public enum TsTarget
    {
        /// <summary>Targets ECMAScript 5.</summary>
        ES5,

        /// <summary>Targets ECMAScript 2015 (ES6).</summary>
        ES2015,

        /// <summary>Targets ECMAScript 2016.</summary>
        ES2016,

        /// <summary>Targets ECMAScript 2017.</summary>
        ES2017,

        /// <summary>Targets ECMAScript 2018.</summary>
        ES2018,

        /// <summary>Targets ECMAScript 2019.</summary>
        ES2019,

        /// <summary>Targets ECMAScript 2020.</summary>
        ES2020,

        /// <summary>Targets ECMAScript 2021.</summary>
        ES2021,

        /// <summary>Targets ECMAScript 2022.</summary>
        ES2022,

        /// <summary>Targets ECMAScript 2023.</summary>
        ES2023,

        /// <summary>Targets ECMAScript 2024.</summary>
        ES2024,

        /// <summary>Targets the latest supported version of ECMAScript.</summary>
        ESNext
    }
}