// =========================
// Notice:
// =========================
// This was generated quickly
// using claude. 
// feel free to improve this
// in any way you see fit
//

using LLE.Frontend.Enums;

namespace LLE.Frontend.Builders;

/// <summary>
/// Provides a fluent API for building up a TypeScript <c>tsconfig.json</c> configuration
/// in memory: <c>compilerOptions</c> (target, module, strictness, output, interop, JSX, paths,
/// lib, etc.), top-level <c>include</c>/<c>exclude</c> globs, and an <see cref="SetRaw"/> escape
/// hatch for any option not exposed via a named setter.
/// </summary>
/// <remarks>
/// This builder is intentionally framework-agnostic — nothing here assumes React, Vue, Svelte,
/// or any other specific frontend framework. Callers wire up framework-specific defaults
/// (e.g. <c>jsx: "react-jsx"</c>) themselves by calling the relevant setters; the builder
/// does not hardcode any of them.
/// </remarks>
public class TsConfigBuilder
{
    // --- Named (typed) compiler options -----------------------------------------------------

    /// <summary>The ECMAScript target version (<c>compilerOptions.target</c>), or <c>null</c> if unset.</summary>
    public TsTarget? Target;

    /// <summary>The module system to emit (<c>compilerOptions.module</c>), or <c>null</c> if unset.</summary>
    public TsModule? Module;

    /// <summary>The module resolution strategy (<c>compilerOptions.moduleResolution</c>), or <c>null</c> if unset.</summary>
    public TsModuleResolution? ModuleResolution;

    /// <summary>The JSX transform mode (<c>compilerOptions.jsx</c>), or <c>null</c> if unset.</summary>
    public TsJsxMode? JsxMode;

    /// <summary>The JSX factory function used by the classic <see cref="TsJsxMode.React"/> transform (<c>compilerOptions.jsxFactory</c>).</summary>
    public string? JsxFactory;

    /// <summary>The JSX fragment factory used by the classic <see cref="TsJsxMode.React"/> transform (<c>compilerOptions.jsxFragmentFactory</c>).</summary>
    public string? JsxFragmentFactory;

    /// <summary>The module specifier the automatic JSX runtime imports from (<c>compilerOptions.jsxImportSource</c>).</summary>
    public string? JsxImportSource;

    /// <summary>The line-ending style for emitted files (<c>compilerOptions.newLine</c>), or <c>null</c> if unset.</summary>
    public TsNewLine? NewLine;

    /// <summary>The output directory for compiled files (<c>compilerOptions.outDir</c>).</summary>
    public string? OutDir;

    /// <summary>The root directory of input files (<c>compilerOptions.rootDir</c>).</summary>
    public string? RootDir;

    /// <summary>Bundles all output into a single file (<c>compilerOptions.outFile</c>). Only valid for AMD/System modules.</summary>
    public string? OutFile;

    /// <summary>The base directory used to resolve non-relative module names (<c>compilerOptions.baseUrl</c>).</summary>
    /// <remarks>
    /// Deprecated upstream by TypeScript in favor of resolving relative to <c>tsconfig.json</c>
    /// itself, but still exposed here for projects/tooling that require it explicitly.
    /// </remarks>
    public string? BaseUrl;

    /// <summary>The built-in type declaration sets available to the program (<c>compilerOptions.lib</c>), e.g. <c>["ES2022", "DOM"]</c>.</summary>
    public readonly List<string> Lib = [];

    /// <summary>
    /// Path alias mappings (<c>compilerOptions.paths</c>), keyed by alias pattern
    /// (e.g. <c>"@app/*"</c>), with each value being the ordered list of fallback
    /// paths TypeScript should try when resolving that alias.
    /// </summary>
    public readonly Dictionary<string, List<string>> Paths = [];

    /// <summary>Type declaration packages to automatically include (<c>compilerOptions.types</c>), e.g. <c>["node", "jest"]</c>.</summary>
    public readonly List<string> Types = [];

    /// <summary>Additional folders to search for type declarations (<c>compilerOptions.typeRoots</c>).</summary>
    public readonly List<string> TypeRoots = [];

    /// <summary>Enables all strict type-checking options (<c>compilerOptions.strict</c>).</summary>
    public bool? Strict;

    /// <summary>Raises an error on expressions/declarations with an implied <c>any</c> type (<c>compilerOptions.noImplicitAny</c>).</summary>
    public bool? NoImplicitAny;

    /// <summary>Ensures <c>this</c> is typed correctly wherever it's used (<c>compilerOptions.noImplicitThis</c>).</summary>
    public bool? NoImplicitThis;

    /// <summary>When type checking, considers <c>null</c> and <c>undefined</c> as distinct, narrower types (<c>compilerOptions.strictNullChecks</c>).</summary>
    public bool? StrictNullChecks;

    /// <summary>Checks function parameter types contravariantly rather than bivariantly (<c>compilerOptions.strictFunctionTypes</c>).</summary>
    public bool? StrictFunctionTypes;

    /// <summary>Checks that the arguments for <c>bind</c>, <c>call</c>, and <c>apply</c> methods match the original function (<c>compilerOptions.strictBindCallApply</c>).</summary>
    public bool? StrictBindCallApply;

    /// <summary>Ensures class properties are initialized in the constructor (<c>compilerOptions.strictPropertyInitialization</c>).</summary>
    public bool? StrictPropertyInitialization;

    /// <summary>Checks for class properties declared but not set in the constructor with strict null checks on (<c>compilerOptions.useUnknownInCatchVariables</c>).</summary>
    public bool? UseUnknownInCatchVariables;

    /// <summary>Disables checks that produce errors for fallthrough cases in switch statements (<c>compilerOptions.noFallthroughCasesInSwitch</c>).</summary>
    public bool? NoFallthroughCasesInSwitch;

    /// <summary>Raises an error when a local variable isn't used (<c>compilerOptions.noUnusedLocals</c>).</summary>
    public bool? NoUnusedLocals;

    /// <summary>Raises an error when a function parameter isn't used (<c>compilerOptions.noUnusedParameters</c>).</summary>
    public bool? NoUnusedParameters;

    /// <summary>Adds <c>undefined</c> to a type when accessed using an index (<c>compilerOptions.noUncheckedIndexedAccess</c>).</summary>
    public bool? NoUncheckedIndexedAccess;

    /// <summary>Ensures all code paths in a function return a value (<c>compilerOptions.noImplicitReturns</c>).</summary>
    public bool? NoImplicitReturns;

    /// <summary>Disallows overriding base class members without an explicit <c>override</c> keyword (<c>compilerOptions.noImplicitOverride</c>).</summary>
    public bool? NoImplicitOverride;

    /// <summary>Generates <c>.d.ts</c> declaration files for the project (<c>compilerOptions.declaration</c>).</summary>
    public bool? Declaration;

    /// <summary>Generates source maps for declaration files (<c>compilerOptions.declarationMap</c>).</summary>
    public bool? DeclarationMap;

    /// <summary>Generates source map files for emitted JavaScript (<c>compilerOptions.sourceMap</c>).</summary>
    public bool? SourceMap;

    /// <summary>Disables emitting compiled output (useful for type-checking-only builds) (<c>compilerOptions.noEmit</c>).</summary>
    public bool? NoEmit;

    /// <summary>Skips emitting output if any errors were reported (<c>compilerOptions.noEmitOnError</c>).</summary>
    public bool? NoEmitOnError;

    /// <summary>Allows JavaScript files to be imported and compiled alongside TypeScript files (<c>compilerOptions.allowJs</c>).</summary>
    public bool? AllowJs;

    /// <summary>Enables type checking on JavaScript files (<c>compilerOptions.checkJs</c>).</summary>
    public bool? CheckJs;

    /// <summary>Allows default imports from modules with no default export, for interop purposes (<c>compilerOptions.allowSyntheticDefaultImports</c>).</summary>
    public bool? AllowSyntheticDefaultImports;

    /// <summary>Emits additional JS to ease support for importing CommonJS modules; implies <see cref="AllowSyntheticDefaultImports"/> (<c>compilerOptions.esModuleInterop</c>).</summary>
    public bool? EsModuleInterop;

    /// <summary>Ensures that casing is correct in imports (<c>compilerOptions.forceConsistentCasingInFileNames</c>).</summary>
    public bool? ForceConsistentCasingInFileNames;

    /// <summary>Skips type-checking of declaration (<c>.d.ts</c>) files, including those in <c>node_modules</c> (<c>compilerOptions.skipLibCheck</c>).</summary>
    public bool? SkipLibCheck;

    /// <summary>Allows importing modules with a <c>.json</c> extension, generating a type for it (<c>compilerOptions.resolveJsonModule</c>).</summary>
    public bool? ResolveJsonModule;

    /// <summary>Ensures each file can be safely transpiled without relying on whole-program type information (<c>compilerOptions.isolatedModules</c>).</summary>
    public bool? IsolatedModules;

    /// <summary>Enforces that each file is treated as an ES module, requiring explicit imports/exports (<c>compilerOptions.verbatimModuleSyntax</c>).</summary>
    public bool? VerbatimModuleSyntax;

    /// <summary>Allows accessing UMD globals from modules (<c>compilerOptions.allowUmdGlobalAccess</c>).</summary>
    public bool? AllowUmdGlobalAccess;

    /// <summary>Disallows features that require cross-file information for emit (<c>compilerOptions.isolatedDeclarations</c>).</summary>
    public bool? IsolatedDeclarations;

    /// <summary>Enables experimental support for legacy (Stage 2) decorators (<c>compilerOptions.experimentalDecorators</c>).</summary>
    public bool? ExperimentalDecorators;

    /// <summary>Emits design-type metadata for decorated declarations, for use with the <c>reflect-metadata</c> library (<c>compilerOptions.emitDecoratorMetadata</c>).</summary>
    public bool? EmitDecoratorMetadata;

    /// <summary>Emits <c>__importStar</c> and <c>__importDefault</c> helpers used for ECMAScript interop (<c>compilerOptions.importHelpers</c>).</summary>
    public bool? ImportHelpers;

    /// <summary>Disallows importing types using a regular import, requiring <c>import type</c> instead — a legacy flag from before <c>verbatimModuleSyntax</c> (<c>compilerOptions.preserveValueImports</c>).</summary>
    public bool? PreserveValueImports;

    /// <summary>Omits emit helpers (e.g. <c>__extends</c>) from generated output, the inverse companion to <see cref="ImportHelpers"/> (<c>compilerOptions.noEmitHelpers</c>).</summary>
    public bool? NoEmitHelpers;

    /// <summary>Disables emitting comments in generated output (<c>compilerOptions.removeComments</c>).</summary>
    public bool? RemoveComments;

    /// <summary>Includes source code in source maps inside the emitted JavaScript (<c>compilerOptions.inlineSources</c>).</summary>
    public bool? InlineSources;

    /// <summary>Emits source maps inline in the emitted JavaScript instead of as separate files (<c>compilerOptions.inlineSourceMap</c>).</summary>
    public bool? InlineSourceMap;

    /// <summary>Watches input files for changes and incrementally recompiles (<c>compilerOptions.incremental</c> / project-level <c>watch</c> mode).</summary>
    public bool? Incremental;

    /// <summary>Enables project compilation using composite project references (<c>compilerOptions.composite</c>).</summary>
    public bool? Composite;

    /// <summary>Disallows referencing project files not part of the root file list, without explicit <c>include</c> (<c>compilerOptions.noResolve</c>).</summary>
    public bool? NoResolve;

    /// <summary>Use <c>Object.defineProperty</c> semantics for class fields rather than simple assignment, matching the JS spec (<c>compilerOptions.useDefineForClassFields</c>).</summary>
    public bool? UseDefineForClassFields;

    /// <summary>Allows importing modules with an explicit <c>.ts</c> extension (<c>compilerOptions.allowImportingTsExtensions</c>). Requires <see cref="NoEmit"/> or a bundler-based <see cref="ModuleResolution"/>.</summary>
    public bool? AllowImportingTsExtensions;

    // --- Top-level (non-compilerOptions) fields ---------------------------------------------

    /// <summary>Glob patterns of files to include in the program (top-level <c>include</c>).</summary>
    public readonly List<string> Includes = [];

    /// <summary>Glob patterns of files to exclude from the program (top-level <c>exclude</c>).</summary>
    public readonly List<string> Excludes = [];

    /// <summary>Specific files to always include, independent of <see cref="Includes"/>/<see cref="Excludes"/> (top-level <c>files</c>).</summary>
    public readonly List<string> Files = [];

    /// <summary>The path of another tsconfig to inherit settings from (top-level <c>extends</c>).</summary>
    public string? Extends;

    /// <summary>
    /// Raw, untyped compiler options not covered by a named setter on this builder.
    /// Keyed by the literal <c>compilerOptions</c> field name (e.g. <c>"emitBOM"</c>),
    /// with values being whatever should be serialized for that field.
    /// </summary>
    public readonly Dictionary<string, object?> RawOptions = [];

    // --- Target / module / resolution --------------------------------------------------------

    /// <summary>Sets the ECMAScript target version (<c>compilerOptions.target</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetTarget(TsTarget target)
    {
        Target = target;
        return this;
    }

    /// <summary>Sets the module system to emit (<c>compilerOptions.module</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetModule(TsModule module)
    {
        Module = module;
        return this;
    }

    /// <summary>Sets the module resolution strategy (<c>compilerOptions.moduleResolution</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetModuleResolution(TsModuleResolution moduleResolution)
    {
        ModuleResolution = moduleResolution;
        return this;
    }

    // --- JSX -----------------------------------------------------------------------------------

    /// <summary>Sets the JSX transform mode (<c>compilerOptions.jsx</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetJsxMode(TsJsxMode jsxMode)
    {
        JsxMode = jsxMode;
        return this;
    }

    /// <summary>
    /// Sets the JSX factory function (<c>compilerOptions.jsxFactory</c>) used by the classic
    /// <see cref="TsJsxMode.React"/> transform, e.g. <c>"h"</c> for Preact or <c>"React.createElement"</c> for React.
    /// </summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetJsxFactory(string jsxFactory)
    {
        JsxFactory = jsxFactory;
        return this;
    }

    /// <summary>
    /// Sets the JSX fragment factory (<c>compilerOptions.jsxFragmentFactory</c>) used by the classic
    /// <see cref="TsJsxMode.React"/> transform, e.g. <c>"Fragment"</c>.
    /// </summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetJsxFragmentFactory(string jsxFragmentFactory)
    {
        JsxFragmentFactory = jsxFragmentFactory;
        return this;
    }

    /// <summary>
    /// Sets the module specifier the automatic JSX runtime imports from
    /// (<c>compilerOptions.jsxImportSource</c>), e.g. <c>"react"</c>, <c>"preact"</c>, or <c>"solid-js"</c>.
    /// Only relevant when <see cref="JsxMode"/> is <see cref="TsJsxMode.ReactJsx"/> or <see cref="TsJsxMode.ReactJsxDev"/>.
    /// </summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetJsxImportSource(string jsxImportSource)
    {
        JsxImportSource = jsxImportSource;
        return this;
    }

    // --- Output / paths -------------------------------------------------------------------------

    /// <summary>Sets the line-ending style for emitted files (<c>compilerOptions.newLine</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetNewLine(TsNewLine newLine)
    {
        NewLine = newLine;
        return this;
    }

    /// <summary>Sets the output directory for compiled files (<c>compilerOptions.outDir</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetOutDir(string outDir)
    {
        OutDir = outDir;
        return this;
    }

    /// <summary>Sets the root directory of input files (<c>compilerOptions.rootDir</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetRootDir(string rootDir)
    {
        RootDir = rootDir;
        return this;
    }

    /// <summary>
    /// Sets a single output file that all input files are bundled into (<c>compilerOptions.outFile</c>).
    /// Only valid when <see cref="Module"/> is <see cref="TsModule.Amd"/> or <see cref="TsModule.System"/>.
    /// </summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetOutFile(string outFile)
    {
        OutFile = outFile;
        return this;
    }

    /// <summary>
    /// Sets the base directory used to resolve non-relative module names (<c>compilerOptions.baseUrl</c>).
    /// </summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetBaseUrl(string baseUrl)
    {
        BaseUrl = baseUrl;
        return this;
    }

    /// <summary>
    /// Adds a built-in type declaration set to <c>compilerOptions.lib</c> (e.g. <c>"ES2022"</c>, <c>"DOM"</c>, <c>"DOM.Iterable"</c>).
    /// </summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder AddLib(string lib)
    {
        Lib.Add(lib);
        return this;
    }

    /// <summary>Adds multiple built-in type declaration sets to <c>compilerOptions.lib</c>.</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder AddLibs(IEnumerable<string> libs)
    {
        Lib.AddRange(libs);
        return this;
    }

    /// <summary>
    /// Adds a single fallback path to the given alias in <c>compilerOptions.paths</c>, appending
    /// it to any paths already registered for that alias.
    /// </summary>
    /// <param name="alias">The path alias pattern (e.g. <c>"@app/*"</c>).</param>
    /// <param name="path">The fallback path to add (e.g. <c>"./src/app/*"</c>).</param>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder AddPath(string alias, string path)
    {
        if (!Paths.TryGetValue(alias, out var paths))
        {
            paths = [];
            Paths[alias] = paths;
        }

        paths.Add(path);
        return this;
    }

    /// <summary>
    /// Adds multiple fallback paths to the given alias in <c>compilerOptions.paths</c>, appending
    /// them to any paths already registered for that alias.
    /// </summary>
    /// <param name="alias">The path alias pattern (e.g. <c>"@app/*"</c>).</param>
    /// <param name="paths">The fallback paths to add, in resolution order.</param>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder AddPaths(string alias, IEnumerable<string> paths)
    {
        if (!Paths.TryGetValue(alias, out var existing))
        {
            existing = [];
            Paths[alias] = existing;
        }

        existing.AddRange(paths);
        return this;
    }

    /// <summary>Adds a type declaration package to automatically include (<c>compilerOptions.types</c>), e.g. <c>"node"</c>.</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder AddType(string type)
    {
        Types.Add(type);
        return this;
    }

    /// <summary>Adds a folder to search for type declarations (<c>compilerOptions.typeRoots</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder AddTypeRoot(string typeRoot)
    {
        TypeRoots.Add(typeRoot);
        return this;
    }

    // --- Strictness family ----------------------------------------------------------------------

    /// <summary>Enables or disables all strict type-checking options at once (<c>compilerOptions.strict</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetStrict(bool enabled)
    {
        Strict = enabled;
        return this;
    }

    /// <summary>Sets whether an implied <c>any</c> type raises an error (<c>compilerOptions.noImplicitAny</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetNoImplicitAny(bool enabled)
    {
        NoImplicitAny = enabled;
        return this;
    }

    /// <summary>Sets whether <c>this</c> must be typed correctly wherever it's used (<c>compilerOptions.noImplicitThis</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetNoImplicitThis(bool enabled)
    {
        NoImplicitThis = enabled;
        return this;
    }

    /// <summary>Sets whether <c>null</c>/<c>undefined</c> are treated as distinct, narrower types (<c>compilerOptions.strictNullChecks</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetStrictNullChecks(bool enabled)
    {
        StrictNullChecks = enabled;
        return this;
    }

    /// <summary>Sets whether function parameters are checked contravariantly (<c>compilerOptions.strictFunctionTypes</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetStrictFunctionTypes(bool enabled)
    {
        StrictFunctionTypes = enabled;
        return this;
    }

    /// <summary>Sets whether <c>bind</c>/<c>call</c>/<c>apply</c> argument types are checked against the original function (<c>compilerOptions.strictBindCallApply</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetStrictBindCallApply(bool enabled)
    {
        StrictBindCallApply = enabled;
        return this;
    }

    /// <summary>Sets whether class properties must be initialized in the constructor (<c>compilerOptions.strictPropertyInitialization</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetStrictPropertyInitialization(bool enabled)
    {
        StrictPropertyInitialization = enabled;
        return this;
    }

    /// <summary>Sets whether caught exceptions default to <c>unknown</c> rather than <c>any</c> (<c>compilerOptions.useUnknownInCatchVariables</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetUseUnknownInCatchVariables(bool enabled)
    {
        UseUnknownInCatchVariables = enabled;
        return this;
    }

    // --- Linting-adjacent checks -----------------------------------------------------------------

    /// <summary>Sets whether fallthrough cases in switch statements raise an error (<c>compilerOptions.noFallthroughCasesInSwitch</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetNoFallthroughCasesInSwitch(bool enabled)
    {
        NoFallthroughCasesInSwitch = enabled;
        return this;
    }

    /// <summary>Sets whether unused local variables raise an error (<c>compilerOptions.noUnusedLocals</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetNoUnusedLocals(bool enabled)
    {
        NoUnusedLocals = enabled;
        return this;
    }

    /// <summary>Sets whether unused function parameters raise an error (<c>compilerOptions.noUnusedParameters</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetNoUnusedParameters(bool enabled)
    {
        NoUnusedParameters = enabled;
        return this;
    }

    /// <summary>Sets whether indexed access adds <c>undefined</c> to the resulting type (<c>compilerOptions.noUncheckedIndexedAccess</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetNoUncheckedIndexedAccess(bool enabled)
    {
        NoUncheckedIndexedAccess = enabled;
        return this;
    }

    /// <summary>Sets whether all code paths in a function must return a value (<c>compilerOptions.noImplicitReturns</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetNoImplicitReturns(bool enabled)
    {
        NoImplicitReturns = enabled;
        return this;
    }

    /// <summary>Sets whether overriding a base class member requires an explicit <c>override</c> keyword (<c>compilerOptions.noImplicitOverride</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetNoImplicitOverride(bool enabled)
    {
        NoImplicitOverride = enabled;
        return this;
    }

    // --- Emit / declarations ---------------------------------------------------------------------

    /// <summary>Sets whether <c>.d.ts</c> declaration files are generated (<c>compilerOptions.declaration</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetDeclaration(bool enabled)
    {
        Declaration = enabled;
        return this;
    }

    /// <summary>Sets whether source maps are generated for declaration files (<c>compilerOptions.declarationMap</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetDeclarationMap(bool enabled)
    {
        DeclarationMap = enabled;
        return this;
    }

    /// <summary>Sets whether source map files are generated for emitted JavaScript (<c>compilerOptions.sourceMap</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetSourceMap(bool enabled)
    {
        SourceMap = enabled;
        return this;
    }

    /// <summary>Sets whether compiled output is emitted at all (<c>compilerOptions.noEmit</c>); useful for type-checking-only builds.</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetNoEmit(bool enabled)
    {
        NoEmit = enabled;
        return this;
    }

    /// <summary>Sets whether output emission is skipped when any errors are reported (<c>compilerOptions.noEmitOnError</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetNoEmitOnError(bool enabled)
    {
        NoEmitOnError = enabled;
        return this;
    }

    /// <summary>Sets whether comments are stripped from generated output (<c>compilerOptions.removeComments</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetRemoveComments(bool enabled)
    {
        RemoveComments = enabled;
        return this;
    }

    /// <summary>Sets whether source code is embedded directly in source maps (<c>compilerOptions.inlineSources</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetInlineSources(bool enabled)
    {
        InlineSources = enabled;
        return this;
    }

    /// <summary>Sets whether source maps are emitted inline in the JavaScript output rather than as separate files (<c>compilerOptions.inlineSourceMap</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetInlineSourceMap(bool enabled)
    {
        InlineSourceMap = enabled;
        return this;
    }

    // --- JS interop / module support -------------------------------------------------------------

    /// <summary>Sets whether <c>.js</c> files can be imported and compiled alongside TypeScript (<c>compilerOptions.allowJs</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetAllowJs(bool enabled)
    {
        AllowJs = enabled;
        return this;
    }

    /// <summary>Sets whether type checking is performed on JavaScript files (<c>compilerOptions.checkJs</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetCheckJs(bool enabled)
    {
        CheckJs = enabled;
        return this;
    }

    /// <summary>Sets whether default imports are synthesized for modules with no default export (<c>compilerOptions.allowSyntheticDefaultImports</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetAllowSyntheticDefaultImports(bool enabled)
    {
        AllowSyntheticDefaultImports = enabled;
        return this;
    }

    /// <summary>Sets whether extra JS is emitted to ease CommonJS interop (<c>compilerOptions.esModuleInterop</c>); implies <see cref="SetAllowSyntheticDefaultImports"/>.</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetEsModuleInterop(bool enabled)
    {
        EsModuleInterop = enabled;
        return this;
    }

    /// <summary>Sets whether import casing must consistently match the file system (<c>compilerOptions.forceConsistentCasingInFileNames</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetForceConsistentCasingInFileNames(bool enabled)
    {
        ForceConsistentCasingInFileNames = enabled;
        return this;
    }

    /// <summary>Sets whether type checking of declaration (<c>.d.ts</c>) files is skipped (<c>compilerOptions.skipLibCheck</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetSkipLibCheck(bool enabled)
    {
        SkipLibCheck = enabled;
        return this;
    }

    /// <summary>Sets whether <c>.json</c> files can be imported as modules (<c>compilerOptions.resolveJsonModule</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetResolveJsonModule(bool enabled)
    {
        ResolveJsonModule = enabled;
        return this;
    }

    /// <summary>Sets whether each file must be safely transpilable without whole-program type information (<c>compilerOptions.isolatedModules</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetIsolatedModules(bool enabled)
    {
        IsolatedModules = enabled;
        return this;
    }

    /// <summary>Sets whether each file is enforced as an ES module with explicit import/export syntax preserved verbatim (<c>compilerOptions.verbatimModuleSyntax</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetVerbatimModuleSyntax(bool enabled)
    {
        VerbatimModuleSyntax = enabled;
        return this;
    }

    /// <summary>Sets whether UMD globals can be accessed from modules (<c>compilerOptions.allowUmdGlobalAccess</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetAllowUmdGlobalAccess(bool enabled)
    {
        AllowUmdGlobalAccess = enabled;
        return this;
    }

    /// <summary>Sets whether declaration emit is restricted to information available per-file, without cross-file analysis (<c>compilerOptions.isolatedDeclarations</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetIsolatedDeclarations(bool enabled)
    {
        IsolatedDeclarations = enabled;
        return this;
    }

    /// <summary>Sets whether imports of type-only bindings are preserved as value imports rather than elided (<c>compilerOptions.preserveValueImports</c>, legacy pre-<c>verbatimModuleSyntax</c> flag).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetPreserveValueImports(bool enabled)
    {
        PreserveValueImports = enabled;
        return this;
    }

    /// <summary>Sets whether emit helpers (e.g. <c>__extends</c>) are omitted from generated output (<c>compilerOptions.noEmitHelpers</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetNoEmitHelpers(bool enabled)
    {
        NoEmitHelpers = enabled;
        return this;
    }

    /// <summary>Sets whether helper functions are imported once from <c>tslib</c> instead of duplicated per-file (<c>compilerOptions.importHelpers</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetImportHelpers(bool enabled)
    {
        ImportHelpers = enabled;
        return this;
    }

    // --- Decorators -------------------------------------------------------------------------------

    /// <summary>Sets whether experimental (Stage 2 / legacy) decorator support is enabled (<c>compilerOptions.experimentalDecorators</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetExperimentalDecorators(bool enabled)
    {
        ExperimentalDecorators = enabled;
        return this;
    }

    /// <summary>Sets whether design-type metadata is emitted for decorated declarations, for use with <c>reflect-metadata</c> (<c>compilerOptions.emitDecoratorMetadata</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetEmitDecoratorMetadata(bool enabled)
    {
        EmitDecoratorMetadata = enabled;
        return this;
    }

    /// <summary>Sets whether class fields use <c>Object.defineProperty</c> semantics matching the JS spec, rather than simple assignment (<c>compilerOptions.useDefineForClassFields</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetUseDefineForClassFields(bool enabled)
    {
        UseDefineForClassFields = enabled;
        return this;
    }

    // --- Project / build ----------------------------------------------------------------------------

    /// <summary>Sets whether incremental compilation information is saved to speed up subsequent builds (<c>compilerOptions.incremental</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetIncremental(bool enabled)
    {
        Incremental = enabled;
        return this;
    }

    /// <summary>Sets whether the project is compiled as a composite project for use with project references (<c>compilerOptions.composite</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetComposite(bool enabled)
    {
        Composite = enabled;
        return this;
    }

    /// <summary>Sets whether TypeScript is disallowed from resolving files not explicitly part of the program (<c>compilerOptions.noResolve</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetNoResolve(bool enabled)
    {
        NoResolve = enabled;
        return this;
    }

    /// <summary>
    /// Sets whether modules can be imported using an explicit <c>.ts</c> extension (<c>compilerOptions.allowImportingTsExtensions</c>).
    /// Requires <see cref="SetNoEmit"/>(true) or a bundler-based <see cref="SetModuleResolution"/>.
    /// </summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetAllowImportingTsExtensions(bool enabled)
    {
        AllowImportingTsExtensions = enabled;
        return this;
    }

    // --- include / exclude / files / extends -----------------------------------------------------

    /// <summary>Adds a glob pattern of files to include in the program (top-level <c>include</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder AddInclude(string glob)
    {
        Includes.Add(glob);
        return this;
    }

    /// <summary>Adds multiple glob patterns of files to include in the program (top-level <c>include</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder AddIncludes(IEnumerable<string> globs)
    {
        Includes.AddRange(globs);
        return this;
    }

    /// <summary>Adds a glob pattern of files to exclude from the program (top-level <c>exclude</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder AddExclude(string glob)
    {
        Excludes.Add(glob);
        return this;
    }

    /// <summary>Adds multiple glob patterns of files to exclude from the program (top-level <c>exclude</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder AddExcludes(IEnumerable<string> globs)
    {
        Excludes.AddRange(globs);
        return this;
    }

    /// <summary>Adds a specific file to always include, independent of <see cref="Includes"/>/<see cref="Excludes"/> (top-level <c>files</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder AddFile(string file)
    {
        Files.Add(file);
        return this;
    }

    /// <summary>Sets the path of another tsconfig file to inherit settings from (top-level <c>extends</c>).</summary>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetExtends(string extends)
    {
        Extends = extends;
        return this;
    }

    // --- Escape hatch --------------------------------------------------------------------------

    /// <summary>
    /// Sets a raw, untyped <c>compilerOptions</c> entry for any option not covered by a named
    /// setter on this builder (e.g. obscure or newly-added TypeScript flags). Overwrites any
    /// previous raw value set for the same key. Takes precedence over named setters if the
    /// same logical option is set both ways, since raw options are applied last when this
    /// builder's output is serialized.
    /// </summary>
    /// <param name="key">The literal <c>compilerOptions</c> field name (e.g. <c>"emitBOM"</c>).</param>
    /// <param name="value">
    /// The value to assign. Use the natural CLR type for the target JSON value
    /// (e.g. <c>bool</c>, <c>string</c>, <c>List&lt;string&gt;</c>) so it serializes correctly.
    /// </param>
    /// <returns>The current <see cref="TsConfigBuilder"/> instance, for chaining.</returns>
    public TsConfigBuilder SetRaw(string key, object? value)
    {
        RawOptions[key] = value;
        return this;
    }

    // --- Serialization ---------------------------------------------------------------------

    /// <summary>Serializes this builder into a pretty-printed tsconfig.json document. Unset fields and empty collections are omitted.</summary>
    public override string ToString()
    {
        var entries = new List<string>();

        static string Str(string s) => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        static string Arr(IEnumerable<string> items) => "[" + string.Join(", ", items.Select(Str)) + "]";
        static string Tsv(Enum e) => e.ToString() switch
        {
            "EsNext" => "esnext", "NodeNext" => "nodenext", "ReactJsx" => "react-jsx", "ReactJsxDev" => "react-jsxdev",
            "ReactNative" => "react-native", "CommonJs" => "commonjs", "Crlf" => "crlf", "Lf" => "lf",
            var v => v.ToLowerInvariant(),
        };

        if (Extends is not null) entries.Add($"{Str("extends")}: {Str(Extends)}");

        var co = new List<string>();
        void B(string key, bool? v) { if (v is not null) co.Add($"{Str(key)}: {(v.Value ? "true" : "false")}"); }
        void S(string key, string? v) { if (v is not null) co.Add($"{Str(key)}: {Str(v)}"); }
        void A(string key, List<string> v) { if (v.Count > 0) co.Add($"{Str(key)}: {Arr(v)}"); }

        S("target", Target is null ? null : Tsv(Target.Value));
        S("module", Module is null ? null : Tsv(Module.Value));
        S("moduleResolution", ModuleResolution is null ? null : Tsv(ModuleResolution.Value));
        S("jsx", JsxMode is null ? null : Tsv(JsxMode.Value));
        S("jsxFactory", JsxFactory);
        S("jsxFragmentFactory", JsxFragmentFactory);
        S("jsxImportSource", JsxImportSource);
        S("newLine", NewLine is null ? null : Tsv(NewLine.Value));
        S("outDir", OutDir);
        S("rootDir", RootDir);
        S("outFile", OutFile);
        S("baseUrl", BaseUrl);
        A("lib", Lib);
        A("types", Types);
        A("typeRoots", TypeRoots);

        if (Paths.Count > 0)
            co.Add($"{Str("paths")}: {{\n      " + string.Join(",\n      ", Paths.Select(p => $"{Str(p.Key)}: {Arr(p.Value)}")) + "\n    }");

        B("strict", Strict);
        B("noImplicitAny", NoImplicitAny);
        B("noImplicitThis", NoImplicitThis);
        B("strictNullChecks", StrictNullChecks);
        B("strictFunctionTypes", StrictFunctionTypes);
        B("strictBindCallApply", StrictBindCallApply);
        B("strictPropertyInitialization", StrictPropertyInitialization);
        B("useUnknownInCatchVariables", UseUnknownInCatchVariables);
        B("noFallthroughCasesInSwitch", NoFallthroughCasesInSwitch);
        B("noUnusedLocals", NoUnusedLocals);
        B("noUnusedParameters", NoUnusedParameters);
        B("noUncheckedIndexedAccess", NoUncheckedIndexedAccess);
        B("noImplicitReturns", NoImplicitReturns);
        B("noImplicitOverride", NoImplicitOverride);
        B("declaration", Declaration);
        B("declarationMap", DeclarationMap);
        B("sourceMap", SourceMap);
        B("noEmit", NoEmit);
        B("noEmitOnError", NoEmitOnError);
        B("allowJs", AllowJs);
        B("checkJs", CheckJs);
        B("allowSyntheticDefaultImports", AllowSyntheticDefaultImports);
        B("esModuleInterop", EsModuleInterop);
        B("forceConsistentCasingInFileNames", ForceConsistentCasingInFileNames);
        B("skipLibCheck", SkipLibCheck);
        B("resolveJsonModule", ResolveJsonModule);
        B("isolatedModules", IsolatedModules);
        B("verbatimModuleSyntax", VerbatimModuleSyntax);
        B("allowUmdGlobalAccess", AllowUmdGlobalAccess);
        B("isolatedDeclarations", IsolatedDeclarations);
        B("experimentalDecorators", ExperimentalDecorators);
        B("emitDecoratorMetadata", EmitDecoratorMetadata);
        B("importHelpers", ImportHelpers);
        B("preserveValueImports", PreserveValueImports);
        B("noEmitHelpers", NoEmitHelpers);
        B("removeComments", RemoveComments);
        B("inlineSources", InlineSources);
        B("inlineSourceMap", InlineSourceMap);
        B("incremental", Incremental);
        B("composite", Composite);
        B("noResolve", NoResolve);
        B("useDefineForClassFields", UseDefineForClassFields);
        B("allowImportingTsExtensions", AllowImportingTsExtensions);

        // Raw options last, so they can override a named option above if set deliberately both ways.
        foreach (var (key, value) in RawOptions)
        {
            var json = value switch
            {
                null => "null",
                bool b => b ? "true" : "false",
                string s => Str(s),
                IEnumerable<string> list => Arr(list),
                int or long or float or double or decimal => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture),
                _ => Str(value.ToString() ?? ""),
            };
            co.Add($"{Str(key)}: {json}");
        }

        if (co.Count > 0)
            entries.Add($"{Str("compilerOptions")}: {{\n    " + string.Join(",\n    ", co) + "\n  }");

        if (Includes.Count > 0) entries.Add($"{Str("include")}: {Arr(Includes)}");
        if (Excludes.Count > 0) entries.Add($"{Str("exclude")}: {Arr(Excludes)}");
        if (Files.Count > 0) entries.Add($"{Str("files")}: {Arr(Files)}");

        return "{\n  " + string.Join(",\n  ", entries) + "\n}";
    }
}