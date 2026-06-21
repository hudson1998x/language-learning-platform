using LLE.Kernel.Registry;
using LLE.TypeScript.Builders;

namespace Tests;

public sealed class ApiBuilderTests
{
    public sealed class TestInput
    {
        public string Name { get; set; } = "";
    }

    public sealed class TestOutput
    {
        public int Id { get; set; }
    }

    private static FeatureDefinition CreateFeature(
        string route,
        HttpMethod method,
        Type inputType,
        Type outputType,
        string name,
        string group)
    {
        return new FeatureDefinition(
            Route: route,
            Method: method,
            InputType: inputType,
            OutputType: outputType,
            Executor: (_, _) => new ValueTask<object>(Activator.CreateInstance(outputType)!),
            ExceptionRules: new Dictionary<Type, FeatureExceptionRule<object>>(),
            FeatureName: name,
            FeatureGroup: group
        );
    }

    [Fact]
    public void AddFeature_Get_Should_Generate_Function_Without_Payload()
    {
        using var builder = new ApiBuilder();
        var feature = CreateFeature("/api/items", HttpMethod.Get, typeof(object), typeof(TestOutput), "getItems", "items");

        builder.AddFeature(feature);
        var result = builder.Build();

        Assert.Contains("items", result.Keys);
        var source = result["items"];
        Assert.Contains("getItems", source);
        Assert.Contains("Promise<TestOutput>", source);
        Assert.DoesNotContain("payload", source);
        Assert.DoesNotContain("Content-Type", source);
        Assert.Contains("method: \"GET\"", source);
    }

    [Fact]
    public void AddFeature_Post_Should_Generate_Function_With_Payload()
    {
        using var builder = new ApiBuilder();
        var feature = CreateFeature("/api/items", HttpMethod.Post, typeof(TestInput), typeof(TestOutput), "createItem", "items");

        builder.AddFeature(feature);
        var result = builder.Build();

        var source = result["items"];
        Assert.Contains("createItem", source);
        Assert.Contains("payload: TestInput", source);
        Assert.Contains("Promise<TestOutput>", source);
        Assert.Contains("Content-Type", source);
        Assert.Contains("JSON.stringify(payload)", source);
        Assert.Contains("method: \"POST\"", source);
    }

    [Fact]
    public void AddFeature_Should_Group_By_FeatureGroup()
    {
        using var builder = new ApiBuilder();

        var get = CreateFeature("/api/users", HttpMethod.Get, typeof(object), typeof(TestOutput), "getUsers", "users");
        var post = CreateFeature("/api/items", HttpMethod.Post, typeof(TestInput), typeof(TestOutput), "createItem", "items");

        builder.AddFeature(get);
        builder.AddFeature(post);
        var result = builder.Build();

        Assert.Equal(2, result.Count);
        Assert.Contains("users", result.Keys);
        Assert.Contains("items", result.Keys);
        Assert.Contains("getUsers", result["users"]);
        Assert.Contains("createItem", result["items"]);
    }

    [Fact]
    public void Build_Should_Return_Grouped_Source()
    {
        using var builder = new ApiBuilder();

        var feature1 = CreateFeature("/api/users", HttpMethod.Get, typeof(object), typeof(TestOutput), "getUsers", "users");
        var feature2 = CreateFeature("/api/users", HttpMethod.Post, typeof(TestInput), typeof(TestOutput), "createUser", "users");

        builder.AddFeature(feature1);
        builder.AddFeature(feature2);
        var result = builder.Build();

        Assert.Single(result);
        Assert.Contains("users", result.Keys);
        Assert.Contains("getUsers", result["users"]);
        Assert.Contains("createUser", result["users"]);
    }

    [Fact]
    public void Dispose_Should_Clear_State()
    {
        var builder = new ApiBuilder();
        var feature = CreateFeature("/api/items", HttpMethod.Get, typeof(object), typeof(TestOutput), "getItems", "items");
        builder.AddFeature(feature);

        builder.Dispose();
        var result = builder.Build();

        Assert.Empty(result);
    }

    [Fact]
    public void AddFeature_Post_Should_Emit_Input_Interface()
    {
        using var builder = new ApiBuilder();
        var feature = CreateFeature("/api/items", HttpMethod.Post, typeof(TestInput), typeof(TestOutput), "createItem", "items");

        builder.AddFeature(feature);
        var result = builder.Build();

        var source = result["items"];
        Assert.Contains("export interface TestInput {", source);
        Assert.Contains("export interface TestOutput {", source);
    }

    [Fact]
    public void AddFeature_Get_Should_Only_Emit_Output_Interface()
    {
        using var builder = new ApiBuilder();
        var feature = CreateFeature("/api/items", HttpMethod.Get, typeof(object), typeof(TestOutput), "getItems", "items");

        builder.AddFeature(feature);
        var result = builder.Build();

        var source = result["items"];
        Assert.Contains("export interface TestOutput {", source);
        Assert.DoesNotContain("export interface TestInput", source);
    }
}
