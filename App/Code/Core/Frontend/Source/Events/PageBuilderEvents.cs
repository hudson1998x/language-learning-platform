using System.Text;
using LLE.Eventing;

namespace LLE.Frontend.Events;

/// <summary>
/// Defines the set of events raised by <see cref="LLE.Frontend.Builders.PageBuilder"/>
/// while rendering a page, allowing other parts of the application to inject
/// additional HTML markup at specific points in the document.
/// </summary>
/// <remarks>
/// Each event provides subscribers with the in-progress <see cref="StringBuilder"/>
/// being used to assemble the page's HTML, so handlers can append (or otherwise mutate)
/// markup directly into the output at the point the event is dispatched.
/// </remarks>
public class PageBuilderEvents : EventTable
{
    /// <summary>
    /// Raised while building the <c>&lt;head&gt;</c> section, after meta tags have
    /// been written but before stylesheets are appended. Subscribers can use this
    /// to inject additional head content (e.g. extra meta tags, preload links,
    /// inline styles).
    /// </summary>
    public readonly EventCollection<StringBuilder> Head = new();

    /// <summary>
    /// Raised while building the <c>&lt;body&gt;</c> section, immediately after the
    /// <c>#app-container</c> element has been written. Subscribers can use this to
    /// inject additional body content (e.g. inline scripts, noscript fallbacks,
    /// analytics snippets).
    /// </summary>
    public readonly EventCollection<StringBuilder> Body = new();
}