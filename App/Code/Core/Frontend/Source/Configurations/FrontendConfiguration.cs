using LLE.Configuration.Attributes;

namespace LLE.Frontend.Configurations;

/// <summary>
/// Configuration settings for the frontend application's static assets.
/// Decorated with <see cref="ConfigurationAttribute"/> so it can be resolved
/// via the configuration provider system (see <c>ConfigurationProvider.Get&lt;T&gt;</c>).
/// </summary>
[Configuration]
public class FrontendConfiguration
{
    /// <summary>
    /// The path or URL to the main frontend JavaScript bundle, rendered as a
    /// <c>&lt;script&gt;</c> tag by <see cref="LLE.Frontend.Builders.PageBuilder"/>.
    /// Defaults to <c>"/app.js"</c>. Set to <c>null</c> or empty to omit the tag.
    /// </summary>
    public string FrontendJsPath = "/app.js";

    /// <summary>
    /// The path or URL to the main frontend stylesheet, rendered as a
    /// <c>&lt;link rel="stylesheet"&gt;</c> tag by <see cref="LLE.Frontend.Builders.PageBuilder"/>.
    /// Defaults to <c>"/app.css"</c>. Set to <c>null</c> or empty to omit the tag.
    /// </summary>
    public string FrontendCssPath = "/app.css";
}