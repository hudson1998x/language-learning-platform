using System.Text;
using LLE.Kernel.Registry;
using LLE.ReactFrontend.Configurations;

namespace LLE.ReactFrontend.Generators
{
    /// <summary>
    /// Scans Design/React/Components for self-named component directories
    /// (e.g. UserCard/UserCard.tsx) and generates a registry .tsx file that
    /// imports and registers every one of them under its @component/* path.
    /// </summary>
    public class ComponentRegistryGenerator
    {
        private const string BasePath = "App/Design/React/Components";
        private static readonly string[] SourceDirs = ["Local", "Community", "Core"];

        /// <summary>
        /// Entry point — scans all source dirs and writes the generated registry file.
        /// </summary>
        public void GenerateComponentRegistry()
        {
            var components = new List<string>();

            foreach (var dir in SourceDirs)
            {
                var fullPath = string.Join('/', BasePath, dir);

                if (!Directory.Exists(fullPath))
                {
                    continue;
                }

                CollectComponents(fullPath, fullPath, components);
            }

            WriteRegistry(components, "App/Code/Community/ReactFrontend/Source/web/generated.registry.tsx");
        }

        /// <summary>
        /// Recursively walks <paramref name="currentPath"/>, collecting every directory
        /// whose own name matches a .tsx file directly inside it (e.g. UserCard/UserCard.tsx).
        /// The collected value is the path relative to <paramref name="rootPath"/> (the
        /// Local/Community/Core dir), using '/' separators — this doubles as the @component/* path.
        /// </summary>
        /// <param name="rootPath">The Local/Community/Core root, used to compute relative paths.</param>
        /// <param name="currentPath">The directory currently being inspected.</param>
        /// <param name="results">Flat accumulator of matched component relative paths.</param>
        private void CollectComponents(string rootPath, string currentPath, List<string> results)
        {
            var dirName = Path.GetFileName(currentPath);
            var selfNamedFile = Path.Combine(currentPath, $"index.tsx");

            if (File.Exists(selfNamedFile))
            {
                var relativePath = Path.GetRelativePath(rootPath, currentPath)
                    .Replace(Path.DirectorySeparatorChar, '/');

                results.Add(relativePath);
            }

            foreach (var subDir in Directory.GetDirectories(currentPath))
            {
                CollectComponents(rootPath, subDir, results);
            }
        }

        /// <summary>
        /// Writes a .tsx file that imports each collected component (default export,
        /// named after its final path segment) and registers it under '@component/{relativePath}'.
        /// </summary>
        /// <param name="components">Flat list of component relative paths, e.g. "Users/UserCard".</param>
        /// <param name="outputPath">Filesystem path the generated .tsx file should be written to.</param>
        private void WriteRegistry(List<string> components, string outputPath)
        {
            var sb = new StringBuilder();

            sb.AppendLine("// AUTO-GENERATED FILE. Do not edit directly.");
            sb.AppendLine("// Run the component registry generator to regenerate.");
            sb.AppendLine();
            sb.AppendLine("import { register } from './registry'");
            
            // import the theme.
            var theme = ConfigurationCatalog.GetConfiguration<ThemeConfiguration>();

            sb.AppendLine($"import FrontendTheme from '@theme:frontend/{theme.FrontendTheme}'");
            sb.AppendLine($"import AdminTheme from '@theme:admin/{theme.BackendTheme}'");
            sb.AppendLine("export { FrontendTheme, AdminTheme } ");
            
            sb.AppendLine();

            foreach (var component in components)
            {
                var componentName = component.Split('/')[^1];
                sb.AppendLine($"import {{ {componentName} }} from '@component/{component}'");
            }

            sb.AppendLine();

            foreach (var component in components)
            {
                var componentName = component.Split('/')[^1];
                sb.AppendLine($"register('@component/{component}', {componentName})");
            }

            sb.AppendLine();

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(outputPath, sb.ToString());
        }
    }
}