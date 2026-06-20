using System.IO.Pipelines;
using LLE.Frontend.Events;
using LLE.Frontend.Writers;

namespace LLE.Frontend.Builders;

public class HtmlBuilder
{
    private readonly Dictionary<string, string> _metaTags = [];
    private string _pageTitle = string.Empty;
    private readonly List<string> _scripts = [];
    private readonly List<string> _css = [];

    private HtmlBuilder()
    {
        // can only be invoked by the factory.
    }

    public static async Task<HtmlBuilder> Create()
    {
        var builder = new HtmlBuilder();
        await builder.Initialize();
        return builder;
    }

    public async Task Initialize()
    {
        await Eventing.Eventing.Of<HtmlBuilderEvents>().Created.DispatchAsync(this);
    }

    public HtmlBuilder WithTitle(string title)
    {
        _pageTitle = title;
        return this;
    }

    public HtmlBuilder WithScript(string script)
    {
        _scripts.Add(script);
        return this;
    }

    public HtmlBuilder WithCss(string css)
    {
        _css.Add(css);
        return this;
    }

    public HtmlBuilder WithMetaTag(string metaTag, string value)
    {
        _metaTags.Add(metaTag, value);
        return this;
    }

    public async Task WriteToStreamAsync(PipeWriter bodyWriter)
    {
        var stringBuilder = new HtmlSink(bodyWriter);

        stringBuilder.Append("<html>");
        stringBuilder.Append("<head>");
        stringBuilder.Append("<title>" + _pageTitle + "</title>");
        
        foreach (var metaTag in _metaTags)
        {
            stringBuilder.Append($"<meta name=\"{metaTag.Key}\" content=\"{metaTag.Value}\"/>");
        }

        foreach (var stylesheet in _css)
        {
            stringBuilder.Append($"<link rel=\"stylesheet\" href=\"{stylesheet}\"/>");
        }
        
        await Eventing.Eventing.Of<HtmlBuilderEvents>().Head.DispatchAsync(stringBuilder);
        
        stringBuilder.Append("</head>");
        stringBuilder.Append("<body>");
        
        await Eventing.Eventing.Of<HtmlBuilderEvents>().Body.DispatchAsync(stringBuilder);
        
        foreach (var script in _scripts)
        {
            stringBuilder.Append($"<script src=\"{script}\"></script>");
        }
        
        stringBuilder.Append("</body>");
        stringBuilder.Append("</html>");
    }
}