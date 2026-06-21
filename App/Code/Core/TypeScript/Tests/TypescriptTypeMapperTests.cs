using System.Text;
using LLE.TypeScript.Builders;

namespace Tests;

public sealed class TypescriptTypeMapperTests
{
    public sealed class SimpleDto
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }

    public sealed class NestedDto
    {
        public SimpleDto Child { get; set; } = new();
    }

    public enum Color
    {
        Red,
        Green,
        Blue
    }

    public sealed class NullableDto
    {
        public string? NullableString { get; set; }
        public int? NullableInt { get; set; }
        public string RequiredString { get; set; } = "";
    }

    public sealed class ApiResponse<T>
    {
        public T Data { get; set; } = default!;
        public string Message { get; set; } = "";
    }

    public sealed class DictionaryDto
    {
        public Dictionary<string, int> Scores { get; set; } = new();
    }

    public sealed class ListDto
    {
        public List<string> Items { get; set; } = new();
    }

    [Fact]
    public void GetTypeReference_Should_Map_String_Types()
    {
        var mapper = new TypeScriptTypeMapper();
        Assert.Equal("string", mapper.GetTypeReference(typeof(string)));
        Assert.Equal("string", mapper.GetTypeReference(typeof(char)));
        Assert.Equal("string", mapper.GetTypeReference(typeof(Guid)));
        Assert.Equal("string", mapper.GetTypeReference(typeof(DateTime)));
        Assert.Equal("string", mapper.GetTypeReference(typeof(DateTimeOffset)));
        Assert.Equal("string", mapper.GetTypeReference(typeof(DateOnly)));
        Assert.Equal("string", mapper.GetTypeReference(typeof(TimeOnly)));
        Assert.Equal("string", mapper.GetTypeReference(typeof(TimeSpan)));
    }

    [Fact]
    public void GetTypeReference_Should_Map_Bool()
    {
        var mapper = new TypeScriptTypeMapper();
        Assert.Equal("boolean", mapper.GetTypeReference(typeof(bool)));
    }

    [Fact]
    public void GetTypeReference_Should_Map_Numeric_Types()
    {
        var mapper = new TypeScriptTypeMapper();
        Assert.Equal("number", mapper.GetTypeReference(typeof(byte)));
        Assert.Equal("number", mapper.GetTypeReference(typeof(sbyte)));
        Assert.Equal("number", mapper.GetTypeReference(typeof(short)));
        Assert.Equal("number", mapper.GetTypeReference(typeof(ushort)));
        Assert.Equal("number", mapper.GetTypeReference(typeof(int)));
        Assert.Equal("number", mapper.GetTypeReference(typeof(uint)));
        Assert.Equal("number", mapper.GetTypeReference(typeof(long)));
        Assert.Equal("number", mapper.GetTypeReference(typeof(ulong)));
        Assert.Equal("number", mapper.GetTypeReference(typeof(float)));
        Assert.Equal("number", mapper.GetTypeReference(typeof(double)));
        Assert.Equal("number", mapper.GetTypeReference(typeof(decimal)));
    }

    [Fact]
    public void GetTypeReference_Should_Map_Object_To_Unknown()
    {
        var mapper = new TypeScriptTypeMapper();
        Assert.Equal("unknown", mapper.GetTypeReference(typeof(object)));
    }

    [Fact]
    public void GetTypeReference_Should_Map_Void_To_PascalCase()
    {
        var mapper = new TypeScriptTypeMapper();
        Assert.Equal("Void", mapper.GetTypeReference(typeof(void)));
    }

    [Fact]
    public void GetTypeReference_Should_Map_Dictionary_To_Record()
    {
        var mapper = new TypeScriptTypeMapper();
        Assert.Equal("Record<string, number>", mapper.GetTypeReference(typeof(Dictionary<string, int>)));
        Assert.Equal("Record<string, string>", mapper.GetTypeReference(typeof(Dictionary<string, string>)));
    }

    [Fact]
    public void GetTypeReference_Should_Map_IDictionary_To_Record()
    {
        var mapper = new TypeScriptTypeMapper();
        Assert.Equal("Record<string, boolean>", mapper.GetTypeReference(typeof(System.Collections.Generic.IDictionary<string, bool>)));
    }

    [Fact]
    public void GetTypeReference_Should_Map_Collections_To_Array()
    {
        var mapper = new TypeScriptTypeMapper();
        Assert.Equal("string[]", mapper.GetTypeReference(typeof(List<string>)));
        Assert.Equal("number[]", mapper.GetTypeReference(typeof(int[])));
        Assert.Equal("boolean[]", mapper.GetTypeReference(typeof(bool[])));
    }

    [Fact]
    public void GetTypeReference_Should_Unwrap_Nullable()
    {
        var mapper = new TypeScriptTypeMapper();
        Assert.Equal("number", mapper.GetTypeReference(typeof(int?)));
        Assert.Equal("boolean", mapper.GetTypeReference(typeof(bool?)));
        Assert.Equal("string", mapper.GetTypeReference(typeof(string))); // ref types erased at runtime
    }

    [Fact]
    public void GetTypeReference_Should_Map_Enum_To_Name()
    {
        var mapper = new TypeScriptTypeMapper();
        Assert.Equal(nameof(Color), mapper.GetTypeReference(typeof(Color)));
    }

    [Fact]
    public void GetTypeReference_Should_Map_Closed_Generic_Type()
    {
        var mapper = new TypeScriptTypeMapper();
        var result = mapper.GetTypeReference(typeof(ApiResponse<Color>));
        Assert.Equal("ApiResponse<Color>", result);
    }

    [Fact]
    public void GetTypeReference_Should_Map_Nested_Generic_Type()
    {
        var mapper = new TypeScriptTypeMapper();
        var result = mapper.GetTypeReference(typeof(ApiResponse<ApiResponse<string>>));
        Assert.Equal("ApiResponse<ApiResponse<string>>", result);
    }

    [Fact]
    public void EmitTypeAndDependencies_Should_Emit_Enum()
    {
        var mapper = new TypeScriptTypeMapper();
        var sb = new StringBuilder();
        mapper.EmitTypeAndDependencies(sb, "test", typeof(Color));
        var result = sb.ToString();

        Assert.Contains("export enum Color {", result);
        Assert.Contains("Red = \"Red\"", result);
        Assert.Contains("Green = \"Green\"", result);
        Assert.Contains("Blue = \"Blue\"", result);
    }

    [Fact]
    public void EmitTypeAndDependencies_Should_Emit_Interface()
    {
        var mapper = new TypeScriptTypeMapper();
        var sb = new StringBuilder();
        mapper.EmitTypeAndDependencies(sb, "test", typeof(SimpleDto));
        var result = sb.ToString();

        Assert.Contains("export interface SimpleDto {", result);
        Assert.Contains("name: string;", result);
        Assert.Contains("age: number;", result);
    }

    [Fact]
    public void EmitTypeAndDependencies_Should_CamelCase_Property_Names()
    {
        var mapper = new TypeScriptTypeMapper();
        var sb = new StringBuilder();
        mapper.EmitTypeAndDependencies(sb, "test", typeof(SimpleDto));
        var result = sb.ToString();

        Assert.Contains("name", result);
        Assert.DoesNotContain("Name", result);
        Assert.Contains("age", result);
        Assert.DoesNotContain("Age", result);
    }

    [Fact]
    public void EmitTypeAndDependencies_Should_Mark_Nullable_Properties()
    {
        var mapper = new TypeScriptTypeMapper();
        var sb = new StringBuilder();
        mapper.EmitTypeAndDependencies(sb, "test", typeof(NullableDto));
        var result = sb.ToString();

        Assert.Contains("nullableString?: string | null;", result);
        Assert.Contains("nullableInt?: number | null;", result);
        Assert.Contains("requiredString: string;", result);
    }

    [Fact]
    public void EmitTypeAndDependencies_Should_Emit_Nested_Types()
    {
        var mapper = new TypeScriptTypeMapper();
        var sb = new StringBuilder();
        mapper.EmitTypeAndDependencies(sb, "test", typeof(NestedDto));
        var result = sb.ToString();

        Assert.Contains("export interface SimpleDto {", result);
        Assert.Contains("export interface NestedDto {", result);
        Assert.Contains("child: SimpleDto;", result);
    }

    [Fact]
    public void EmitTypeAndDependencies_Should_Deduplicate_In_Same_Group()
    {
        var mapper = new TypeScriptTypeMapper();
        var sb = new StringBuilder();

        mapper.EmitTypeAndDependencies(sb, "group1", typeof(SimpleDto));
        mapper.EmitTypeAndDependencies(sb, "group1", typeof(SimpleDto));

        var occurrences = CountOccurrences(sb.ToString(), "export interface SimpleDto");
        Assert.Equal(1, occurrences);
    }

    [Fact]
    public void EmitTypeAndDependencies_Should_Emit_In_Different_Groups()
    {
        var mapper = new TypeScriptTypeMapper();
        var sb1 = new StringBuilder();
        var sb2 = new StringBuilder();

        mapper.EmitTypeAndDependencies(sb1, "group1", typeof(SimpleDto));
        mapper.EmitTypeAndDependencies(sb2, "group2", typeof(SimpleDto));

        Assert.Contains("export interface SimpleDto", sb1.ToString());
        Assert.Contains("export interface SimpleDto", sb2.ToString());
    }

    [Fact]
    public void EmitTypeAndDependencies_Should_Emit_Generic_Interface()
    {
        var mapper = new TypeScriptTypeMapper();
        var sb = new StringBuilder();
        mapper.EmitTypeAndDependencies(sb, "test", typeof(ApiResponse<Color>));
        var result = sb.ToString();

        Assert.Contains("export enum Color {", result);
        Assert.Contains("export interface ApiResponse<T> {", result);
        Assert.Contains("message: string;", result);
    }

    [Fact]
    public void EmitTypeAndDependencies_Should_Handle_Dictionary_Properties()
    {
        var mapper = new TypeScriptTypeMapper();
        var sb = new StringBuilder();
        mapper.EmitTypeAndDependencies(sb, "test", typeof(DictionaryDto));
        var result = sb.ToString();

        Assert.Contains("export interface DictionaryDto {", result);
        Assert.Contains("scores: Record<string, number>;", result);
    }

    [Fact]
    public void EmitTypeAndDependencies_Should_Handle_List_Properties()
    {
        var mapper = new TypeScriptTypeMapper();
        var sb = new StringBuilder();
        mapper.EmitTypeAndDependencies(sb, "test", typeof(ListDto));
        var result = sb.ToString();

        Assert.Contains("export interface ListDto {", result);
        Assert.Contains("items: string[];", result);
    }

    [Fact]
    public void EmitTypeAndDependencies_Should_Not_Emit_Primitives()
    {
        var mapper = new TypeScriptTypeMapper();
        var sb = new StringBuilder();
        mapper.EmitTypeAndDependencies(sb, "test", typeof(string));
        Assert.Empty(sb.ToString());
    }

    [Fact]
    public void EmitTypeAndDependencies_Should_Not_Emit_Void()
    {
        var mapper = new TypeScriptTypeMapper();
        var sb = new StringBuilder();
        mapper.EmitTypeAndDependencies(sb, "test", typeof(void));
        Assert.Empty(sb.ToString());
    }

    private static int CountOccurrences(string text, string pattern)
    {
        var count = 0;
        var i = 0;
        while ((i = text.IndexOf(pattern, i, StringComparison.Ordinal)) != -1)
        {
            count++;
            i += pattern.Length;
        }
        return count;
    }
}
