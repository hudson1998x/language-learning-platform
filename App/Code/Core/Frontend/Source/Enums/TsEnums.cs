using LLE.Frontend.Builders;

namespace LLE.Frontend.Enums;

/// <summary>
/// The ECMAScript (or ESNext) language version that TypeScript compiles down to.
/// Mirrors the allowed values of <c>compilerOptions.target</c> in <c>tsconfig.json</c>.
/// </summary>
public enum TsTarget
{
    Es3,
    Es5,
    Es6,
    Es2015,
    Es2016,
    Es2017,
    Es2018,
    Es2019,
    Es2020,
    Es2021,
    Es2022,
    Es2023,
    Es2024,
    EsNext,
}

/// <summary>
/// The module system TypeScript emits for compiled output. Mirrors the allowed
/// values of <c>compilerOptions.module</c> in <c>tsconfig.json</c>.
/// </summary>
public enum TsModule
{
    None,
    CommonJs,
    Amd,
    Umd,
    System,
    Es6,
    Es2015,
    Es2020,
    Es2022,
    EsNext,
    Node16,
    Node18,
    NodeNext,
    Preserve,
}

/// <summary>
/// The strategy TypeScript uses to resolve module imports to files on disk.
/// Mirrors the allowed values of <c>compilerOptions.moduleResolution</c> in <c>tsconfig.json</c>.
/// </summary>
public enum TsModuleResolution
{
    Classic,
    Node10,
    Node16,
    NodeNext,
    Bundler,
}

/// <summary>
/// How TypeScript should transform JSX syntax found in <c>.tsx</c> files.
/// Mirrors the allowed values of <c>compilerOptions.jsx</c> in <c>tsconfig.json</c>.
/// </summary>
/// <remarks>
/// This is intentionally framework-agnostic: <see cref="ReactJsx"/> and <see cref="ReactJsxDev"/>
/// work for any library providing a compatible automatic JSX runtime (not just React), while
/// <see cref="React"/> is the classic runtime that relies on a configurable pragma
/// (see <see cref="TsConfigBuilder.SetJsxFactory"/>).
/// </remarks>
public enum TsJsxMode
{
    /// <summary>JSX is left untouched in the output for a downstream tool to process.</summary>
    Preserve,

    /// <summary>Classic transform; JSX compiles to calls against a configurable factory function.</summary>
    React,

    /// <summary>Like <see cref="React"/>, but leaves the JSX output untransformed so a downstream tool (e.g. Metro/Babel) finishes the transform.</summary>
    ReactNative,

    /// <summary>Automatic runtime (TS 4.1+); injects the JSX factory import automatically. Production output.</summary>
    ReactJsx,

    /// <summary>Same as <see cref="ReactJsx"/> but includes extra debug information for development builds.</summary>
    ReactJsxDev,
}

/// <summary>
/// The line-ending style TypeScript uses when emitting output files.
/// Mirrors the allowed values of <c>compilerOptions.newLine</c> in <c>tsconfig.json</c>.
/// </summary>
public enum TsNewLine
{
    Crlf,
    Lf,
}