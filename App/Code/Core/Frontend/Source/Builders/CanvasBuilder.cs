using LLE.Frontend.Events;
using LLE.UiIR;

namespace LLE.Frontend.Builders;

public static class CanvasBuilder
{
    public static async Task<HtmlBuilder> CreateHtmlBuilder(Func<VNode, Task<VNode>> rootNodeHandler)
    {
        // create the root node.
        var rootVNode = VNode.Create("div", [], []);
        
        // allow a creator to modify the VNode asynchronously. 
        rootVNode = await rootNodeHandler.Invoke(rootVNode);
        
        // emit the created VNode.
        rootVNode = await Eventing.Eventing.Of<CanvasBuilderEvents>().Created.DispatchAsync(rootVNode);

        var builder = await HtmlBuilder.Create();
        await builder.Initialize();
        
        // inject it into the page.
        builder.AddSnippet("<script>window.canvasState = " + rootVNode + ";</script>");

        return builder;
    }
}
