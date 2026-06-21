using System.Collections;
using System.Reflection;
using System.Text;

namespace LLE.TypeScript.Builders;

/// <summary>
/// Reflects over C# DTO/entity/enum types and emits matching TypeScript
/// interface/enum declarations, recursively walking referenced types.
/// </summary>
public class TypeScriptTypeMapper
{
    // Tracks every type we've already emitted (or are in the process of emitting)
    // per feature group, so shared types (e.g. User referenced by five DTOs)
    // only get written once and circular references don't recurse forever.
    private readonly Dictionary<string, HashSet<Type>> _emittedPerGroup = new();

    /// <summary>
    /// Ensures the TS declaration for <paramref name="type"/> (and everything
    /// it transitively references) has been appended to <paramref name="builder"/>
    /// for the given feature group. Safe to call repeatedly with the same type.
    /// </summary>
    public void EmitTypeAndDependencies(StringBuilder builder, string featureGroup, Type type)
    {
        var emitted = GetEmittedSet(featureGroup);
        EmitRecursive(builder, emitted, type);
    }

    /// <summary>
    /// Returns the TS type reference for use inline (e.g. in a function signature)
    /// without necessarily emitting a new declaration — primitives map directly,
    /// complex types resolve to their interface/enum name.
    /// </summary>
    public string GetTypeReference(Type type) => MapTypeReference(type);

    private HashSet<Type> GetEmittedSet(string featureGroup)
    {
        if (!_emittedPerGroup.TryGetValue(featureGroup, out var set))
        {
            set = new HashSet<Type>();
            _emittedPerGroup[featureGroup] = set;
        }

        return set;
    }

    private void EmitRecursive(StringBuilder builder, HashSet<Type> emitted, Type type)
    {
        type = UnwrapNullable(type);

        if (IsPrimitive(type) || type == typeof(object) || type == typeof(void))
        {
            return;
        }

        if (type.IsGenericParameter)
        {
            return;
        }

        if (IsCollection(type, out var elementType))
        {
            EmitRecursive(builder, emitted, elementType!);
            return;
        }

        if (IsDictionary(type, out var keyType, out var valueType))
        {
            EmitRecursive(builder, emitted, keyType!);
            EmitRecursive(builder, emitted, valueType!);
            return;
        }

        // For generic types, emit concrete type arguments first (e.g. Role in ApiResponse<Role>)
        // so their interfaces appear before the generic definition.
        if (type.IsGenericType)
        {
            foreach (var arg in type.GetGenericArguments())
            {
                EmitRecursive(builder, emitted, arg);
            }
        }

        // Track and emit by the open generic definition (e.g. ApiResponse<>), not the closed one,
        // so the generic interface is written only once per feature group.
        var effectiveType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;

        if (!emitted.Add(effectiveType))
        {
            return;
        }

        if (effectiveType.IsEnum)
        {
            EmitEnum(builder, effectiveType);
            return;
        }

        // Recurse into property types BEFORE emitting this interface, so dependencies
        // appear earlier in the file (purely cosmetic, but reads naturally top-down).
        var properties = GetTsProperties(effectiveType);
        foreach (var prop in properties)
        {
            EmitRecursive(builder, emitted, prop.PropertyType);
        }

        EmitInterface(builder, effectiveType, properties);
    }

    private static void EmitEnum(StringBuilder builder, Type enumType)
    {
        builder.AppendLine($"export enum {enumType.Name} {{");
        var names = Enum.GetNames(enumType);
        foreach (var name in names)
        {
            builder.AppendLine($"    {name} = \"{name}\",");
        }
        builder.AppendLine("}");
        builder.AppendLine();
    }

    private void EmitInterface(StringBuilder builder, Type type, List<PropertyInfo> properties)
    {
        var interfaceName = type.Name;
        if (type.IsGenericType)
        {
            interfaceName = StripGenericArity(interfaceName);
            var typeParams = type.GetGenericArguments().Select(p => p.Name);
            interfaceName += "<" + string.Join(", ", typeParams) + ">";
        }

        builder.AppendLine($"export interface {interfaceName} {{");

        foreach (var prop in properties)
        {
            var propName = ToCamelCase(prop.Name);
            var isNullable = IsNullableProperty(prop);
            var tsType = MapTypeReference(prop.PropertyType);

            builder.AppendLine(isNullable
                ? $"    {propName}?: {tsType} | null;"
                : $"    {propName}: {tsType};");
        }

        builder.AppendLine("}");
        builder.AppendLine();
    }

    /// <summary>
    /// Public instance properties, flattened across the inheritance chain
    /// (e.g. ContentWithId's Id property shows up on User).
    /// </summary>
    private static List<PropertyInfo> GetTsProperties(Type type)
    {
        return type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .GroupBy(p => p.Name) // avoid duplicate entries when a property is overridden/shadowed
            .Select(g => g.First())
            .ToList();
    }

    private static string MapTypeReference(Type type)
    {
        type = UnwrapNullable(type);

        if (type == typeof(string) || type == typeof(char) || type == typeof(Guid)
            || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(DateOnly)
            || type == typeof(TimeOnly) || type == typeof(TimeSpan))
        {
            // Dates/Guids serialize as ISO strings over the wire.
            return "string";
        }

        if (type == typeof(bool))
        {
            return "boolean";
        }

        if (IsNumeric(type))
        {
            return "number";
        }

        if (type == typeof(object))
        {
            return "unknown";
        }

        if (IsDictionary(type, out var keyType, out var valueType))
        {
            return $"Record<{MapTypeReference(keyType!)}, {MapTypeReference(valueType!)}>";
        }

        if (IsCollection(type, out var elementType))
        {
            var inner = MapTypeReference(elementType!);
            // Wrap union types so `T | null` arrays read as (T | null)[] not T | null[].
            return inner.Contains(' ') ? $"({inner})[]" : $"{inner}[]";
        }

        // Generic type parameter (T, U, etc.) — use its name directly.
        if (type.IsGenericParameter)
        {
            return type.Name;
        }

        // Closed generic type (e.g. ApiResponse<Role>) — strip the .NET backtick suffix
        // and format as ApiResponse<Role>.
        if (type.IsGenericType)
        {
            var name = StripGenericArity(type.GetGenericTypeDefinition().Name);
            var args = type.GetGenericArguments().Select(MapTypeReference);
            return $"{name}<{string.Join(", ", args)}>";
        }

        // Fallback: non-generic complex type — reference it by name;
        // EmitRecursive is responsible for ensuring the declaration exists.
        return type.Name;
    }

    private static bool IsPrimitive(Type type)
    {
        return type == typeof(string) || type == typeof(char) || type == typeof(Guid)
            || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(DateOnly)
            || type == typeof(TimeOnly) || type == typeof(TimeSpan)
            || type == typeof(bool) || IsNumeric(type);
    }

    private static bool IsNumeric(Type type)
    {
        return type == typeof(byte) || type == typeof(sbyte)
            || type == typeof(short) || type == typeof(ushort)
            || type == typeof(int) || type == typeof(uint)
            || type == typeof(long) || type == typeof(ulong)
            || type == typeof(float) || type == typeof(double)
            || type == typeof(decimal);
    }

    private static bool IsCollection(Type type, out Type? elementType)
    {
        if (type != typeof(string) && type.IsArray)
        {
            elementType = type.GetElementType();
            return true;
        }

        if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
        {
            // Dictionaries are technically IEnumerable<KeyValuePair<,>> — handled separately.
            if (IsDictionary(type, out _, out _))
            {
                elementType = null;
                return false;
            }

            var enumerableInterface = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                ? type
                : type.GetInterfaces().FirstOrDefault(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (enumerableInterface != null)
            {
                elementType = enumerableInterface.GetGenericArguments()[0];
                return true;
            }
        }

        elementType = null;
        return false;
    }

    private static bool IsDictionary(Type type, out Type? keyType, out Type? valueType)
    {
        var dictInterface = type.IsGenericType &&
                             (type.GetGenericTypeDefinition() == typeof(IDictionary<,>) ||
                              type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            ? type
            : type.GetInterfaces().FirstOrDefault(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));

        if (dictInterface != null)
        {
            var args = dictInterface.GetGenericArguments();
            keyType = args[0];
            valueType = args[1];
            return true;
        }

        keyType = null;
        valueType = null;
        return false;
    }

    private static Type UnwrapNullable(Type type)
    {
        return Nullable.GetUnderlyingType(type) ?? type;
    }

    private static bool IsNullableProperty(PropertyInfo prop)
    {
        // Value-type nullability (int?, DateTime?, etc.)
        if (Nullable.GetUnderlyingType(prop.PropertyType) != null)
        {
            return true;
        }

        // Reference-type nullability via NullabilityInfoContext (works for `string?`,
        // requires nullable reference types enabled, which LLE's DTOs use given `required` is present).
        if (!prop.PropertyType.IsValueType)
        {
            var context = new NullabilityInfoContext();
            var info = context.Create(prop);
            return info.WriteState == NullabilityState.Nullable;
        }

        return false;
    }

    /// <summary>
    /// Strips the .NET generic arity suffix, e.g. "ApiResponse`1" → "ApiResponse".
    /// </summary>
    private static string StripGenericArity(string name)
    {
        var tick = name.IndexOf('`');
        return tick > 0 ? name[..tick] : name;
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name) || char.IsLower(name[0]))
        {
            return name;
        }

        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}