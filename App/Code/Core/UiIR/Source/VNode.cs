using System.Text.Json.Serialization;

namespace LLE.UiIR;

/// <summary>
/// Represents a node in a virtual UI tree, capturing a component's type,
/// properties, and child nodes for later rendering or diffing.
/// </summary>
public class VNode
{
    private string _component;
    private Dictionary<string, object> _properties;
    private readonly List<VNode> _children = [];

    /// <summary>
    /// Gets or sets the component identifier.
    /// </summary>
    public string Component { get => _component; set => _component = value; }

    /// <summary>
    /// Gets or sets the component properties.
    /// </summary>
    public Dictionary<string, object> Properties { get => _properties; set => _properties = value; }

    /// <summary>
    /// Gets or sets the child VNodes.
    /// </summary>
    public List<VNode> Children
    {
        get => _children;
        set
        {
            _children.Clear();
            _children.AddRange(value);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VNode"/> class.
    /// </summary>
    /// <param name="component">The name or identifier of the component this node represents.</param>
    /// <param name="props">A dictionary of property names to values to apply to the component.</param>
    /// <param name="children">The child nodes nested within this node, if any.</param>
    private VNode(string component, Dictionary<string, object> props, params VNode[] children)
    {
        _component = component;
        _properties = props;
        _children.AddRange(children);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VNode"/> class from deserialized JSON.
    /// </summary>
    /// <param name="component">The component identifier.</param>
    /// <param name="properties">The component properties.</param>
    /// <param name="children">The child VNodes.</param>
    [JsonConstructor]
    public VNode(string component, Dictionary<string, object> properties, List<VNode> children)
    {
        _component = component;
        _properties = properties;
        _children = children ?? [];
    }
    
    /// <summary>
    /// Change the component
    /// </summary>
    /// <param name="newComponent"></param>
    public void ChangeComponent(string newComponent)
    {
        _component = newComponent;
    }
    
    /// <summary>
    /// Add a child to the VNode tree.
    /// </summary>
    /// <param name="child"></param>
    public void AddChild(VNode child)
    {
        _children.Add(child);
    }

    /// <summary>
    /// Creates a new <see cref="VNode"/> instance representing a UI component.
    /// </summary>
    /// <param name="component">The name or identifier of the component this node represents.</param>
    /// <param name="props">A dictionary of property names to values to apply to the component.</param>
    /// <param name="children">The child nodes nested within this node, if any.</param>
    /// <returns>A new <see cref="VNode"/> instance.</returns>
    public static VNode Create(string component, Dictionary<string, object> props, params VNode[] children)
    {
        return new(component, props, children);
    }

    public override string ToString()
    {
        var props = string.Join(",", (_properties ?? []).Select(kvp =>
            $"\"{EscapeJson(char.ToLowerInvariant(kvp.Key[0]) + kvp.Key[1..])}\":{FormatValue(kvp.Value)}"));

        var children = string.Join(",", (_children ?? []).Select(c => c.ToString()));

        return $"{{\"t\":\"{EscapeJson(_component)}\",\"p\":{{{props}}},\"c\":[{children}]}}";
    }

    private static string? FormatValue(object value)
    {
        return value switch
        {
            null => "null",
            string s => $"\"{EscapeJson(s)}\"",
            bool b => b ? "true" : "false",
            int or long or short or byte => value.ToString(),
            float or double or decimal => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture),
            _ => $"\"{EscapeJson(value.ToString())}\""
        };
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

    public void Change(VNode canvasNode)
    {
        _component = canvasNode._component;
        _children.Clear();
        _children.AddRange(canvasNode._children);
        _properties.Clear();
        _properties = canvasNode._properties;
    }
}