using System.Text;
using LLE.Configuration.Providers;
using LLE.Dependencies.Providers;
using LLE.Dependencies.Utils;
using LLE.Frontend.Configurations;
using LLE.Frontend.Events;

namespace LLE.Frontend.Builders;

/// <summary>
/// Provides a fluent API for assembling the HTML markup of a page, including
/// its title, meta tags, stylesheets, and scripts. Use the <c>Set</c>/<c>Add</c>
/// methods to configure the page, then call <see cref="ToString"/> to render
/// the final HTML document as a string.
/// </summary>
public class PageBuilder
{
    /// <summary>Meta tag name/content pairs to be rendered in the document head.</summary>
    private readonly Dictionary<string, string> _metaTags = [];

    /// <summary>Script source URLs to be rendered before the closing <c>&lt;/html&gt;</c> tag.</summary>
    private readonly List<string> _scripts = [];

    /// <summary>Stylesheet href URLs to be rendered in the document head.</summary>
    private readonly List<string> _stylesheets = [];

    /// <summary>The page's <c>&lt;title&gt;</c> value. Defaults to "Untitled" if not set.</summary>
    private string _pageTitle = "Untitled";

    /// <summary>
    /// Sets the document title rendered inside the <c>&lt;title&gt;</c> tag.
    /// </summary>
    /// <param name="title">The title text to use for the page.</param>
    /// <returns>The current <see cref="PageBuilder"/> instance, for chaining.</returns>
    public PageBuilder SetTitle(string title)
    {
        _pageTitle = title;
        return this;
    }

    /// <summary>
    /// Adds a script source to be rendered as a <c>&lt;script&gt;</c> tag near the end of the document.
    /// </summary>
    /// <param name="script">The URL of the script to include.</param>
    /// <returns>The current <see cref="PageBuilder"/> instance, for chaining.</returns>
    public PageBuilder AddScript(string script)
    {
        _scripts.Add(script);
        return this;
    }

    /// <summary>
    /// Adds a stylesheet to be rendered as a <c>&lt;link rel="stylesheet"&gt;</c> tag in the document head.
    /// </summary>
    /// <param name="css">The URL of the stylesheet to include.</param>
    /// <returns>The current <see cref="PageBuilder"/> instance, for chaining.</returns>
    public PageBuilder AddStylesheet(string css)
    {
        _stylesheets.Add(css);
        return this;
    }

    /// <summary>
    /// Sets (or overwrites) a meta tag to be rendered in the document head as
    /// <c>&lt;meta name="tag" content="value"/&gt;</c>.
    /// </summary>
    /// <param name="tag">The value of the meta tag's <c>name</c> attribute.</param>
    /// <param name="value">The value of the meta tag's <c>content</c> attribute.</param>
    /// <returns>The current <see cref="PageBuilder"/> instance, for chaining.</returns>
    public PageBuilder SetMetaTag(string tag, string value)
    {
        _metaTags[tag] = value;
        return this;
    }

    /// <summary>
    /// Renders the fully assembled HTML document as a string, including title, meta
    /// tags, stylesheets, scripts, and any content contributed by subscribers to the
    /// <see cref="PageBuilderEvents.Head"/> and <see cref="PageBuilderEvents.Body"/> events.
    /// </summary>
    /// <returns>The complete HTML markup for the page.</returns>
    public override string ToString()
    {
        var source = new StringBuilder();

        // --- Document shell / head start ---
        source.Append("<html>");
        source.Append("<head>");
        source.Append("<title>" + _pageTitle + "</title>");

        // Render each registered meta tag as <meta name="..." content="..."/>
        foreach (var metaTag in _metaTags)
        {
            source.Append($"<meta name=\"{metaTag.Key}\" content=\"{metaTag.Value}\"/>");
        }

        // Give external subscribers a chance to inject additional markup into <head>
        // (e.g. extra meta tags, preload links, inline styles) before stylesheets are added.
        AsyncUtils.Await(
            Eventing.Eventing.Of<PageBuilderEvents>().Head.DispatchAsync(source)
        );

        // Resolve frontend configuration (e.g. global CSS/JS paths) from the DI/provider system.
        var configurationProvider = Provider.Get<ConfigurationProvider>();
        var frontendConfiguration = configurationProvider.Get<FrontendConfiguration>();

        // Include the globally configured frontend stylesheet, if one is set.
        if (!string.IsNullOrEmpty(frontendConfiguration.FrontendCssPath))
        {
            source.Append($"<link rel=\"stylesheet\" href=\"{frontendConfiguration.FrontendCssPath}\"/>");
        }

        // Include any stylesheets explicitly added via AddStylesheet().
        foreach (var href in _stylesheets)
        {
            source.Append($"<link rel=\"stylesheet\" href=\"{href}\"/>");
        }

        source.Append("</head>");

        // --- Body start ---
        source.Append("<body>");

        // Root container element that the frontend application mounts into.
        source.Append("<div id=\"app-container\"></div>");

        // Give external subscribers a chance to inject additional markup into <body>
        // (e.g. inline scripts, noscript fallbacks, analytics snippets).
        AsyncUtils.Await(
            Eventing.Eventing.Of<PageBuilderEvents>().Body.DispatchAsync(source)
        );

        source.Append("</body>");

        // Include the globally configured frontend script, if one is set.
        // NOTE: This is appended after </body> but before </html> (and after the
        // closing body tag), matching the original behavior of this method.
        if (!string.IsNullOrEmpty(frontendConfiguration.FrontendJsPath))
        {
            source.Append($"<script src=\"{frontendConfiguration.FrontendJsPath}\"></script>");
        }

        // Include any scripts explicitly added via AddScript().
        foreach (var script in _scripts)
        {
            source.Append($"<script src=\"{script}\"></script>");
        }

        source.Append("</html>");

        return source.ToString();
    }
}