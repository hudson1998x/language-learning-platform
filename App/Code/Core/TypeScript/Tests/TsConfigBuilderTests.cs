using System.Text.Json;
using LLE.TypeScript.Builders;

namespace Tests;

public sealed class TsConfigBuilderTests
{
    [Fact]
    public void Default_Should_Have_Empty_CompilerOptions()
    {
        var builder = new TsConfigBuilder();
        Assert.NotNull(builder.CompilerOptions);
    }

    [Fact]
    public void Default_Should_Have_Empty_Include_And_Exclude()
    {
        var builder = new TsConfigBuilder();
        Assert.Empty(builder.Include);
        Assert.Empty(builder.Exclude);
    }

    [Fact]
    public void Output_Should_Contain_CompilerOptions()
    {
        var builder = new TsConfigBuilder();
        var doc = JsonDocument.Parse(builder.ToString());
        var _ = doc.RootElement.GetProperty("compilerOptions");
    }

    [Fact]
    public void Output_Should_Include_Include_When_Set()
    {
        var builder = new TsConfigBuilder();
        builder.Include.Add("src/**/*");
        var doc = JsonDocument.Parse(builder.ToString());
        var include = doc.RootElement.GetProperty("include");
        Assert.Equal("src/**/*", include[0].GetString());
    }

    [Fact]
    public void Output_Should_Include_Exclude_When_Set()
    {
        var builder = new TsConfigBuilder();
        builder.Exclude.Add("node_modules");
        var doc = JsonDocument.Parse(builder.ToString());
        var exclude = doc.RootElement.GetProperty("exclude");
        Assert.Equal("node_modules", exclude[0].GetString());
    }

    [Fact]
    public void Output_Should_Be_Valid_Json()
    {
        var builder = new TsConfigBuilder();
        builder.CompilerOptions.Target = TsTarget.ES2022;
        builder.Include.Add("src/**/*");
        var ex = Record.Exception(() => JsonDocument.Parse(builder.ToString()));
        Assert.Null(ex);
    }

    [Fact]
    public void CompilerOptions_Should_Omit_Null_Properties()
    {
        var builder = new TsConfigBuilder();
        var json = builder.ToString();
        Assert.DoesNotContain("target", json);
        Assert.DoesNotContain("strict", json);
        Assert.DoesNotContain("jsx", json);
    }

    [Fact]
    public void CompilerOptions_Target_Should_Be_Serialized()
    {
        var builder = new TsConfigBuilder();
        builder.CompilerOptions.Target = TsTarget.ES2022;
        var doc = JsonDocument.Parse(builder.ToString());
        Assert.Equal("eS2022", GetCompilerOption(doc, "target").GetString());
    }

    [Fact]
    public void CompilerOptions_Module_Should_Be_Serialized()
    {
        var builder = new TsConfigBuilder();
        builder.CompilerOptions.Module = TsModule.ESNext;
        var doc = JsonDocument.Parse(builder.ToString());
        Assert.Equal("eSNext", GetCompilerOption(doc, "module").GetString());
    }

    [Fact]
    public void CompilerOptions_ModuleResolution_Should_Be_Serialized()
    {
        var builder = new TsConfigBuilder();
        builder.CompilerOptions.ModuleResolution = TsModuleResolution.Bundler;
        var doc = JsonDocument.Parse(builder.ToString());
        Assert.Equal("bundler", GetCompilerOption(doc, "moduleResolution").GetString());
    }

    [Fact]
    public void CompilerOptions_Jsx_Should_Use_EnumMember_Value()
    {
        var builder = new TsConfigBuilder();
        builder.CompilerOptions.Jsx = JsxMode.ReactJsx;
        var doc = JsonDocument.Parse(builder.ToString());
        Assert.Equal("react-jsx", GetCompilerOption(doc, "jsx").GetString());
    }

    [Fact]
    public void CompilerOptions_JsxImportSource_Should_Be_Serialized()
    {
        var builder = new TsConfigBuilder();
        builder.CompilerOptions.JsxImportSource = "react";
        var doc = JsonDocument.Parse(builder.ToString());
        Assert.Equal("react", GetCompilerOption(doc, "jsxImportSource").GetString());
    }

    [Fact]
    public void CompilerOptions_Bool_Properties_Should_Be_Serialized()
    {
        var builder = new TsConfigBuilder();
        builder.CompilerOptions.Strict = true;
        builder.CompilerOptions.SkipLibCheck = false;
        builder.CompilerOptions.Declaration = true;

        var doc = JsonDocument.Parse(builder.ToString());
        var options = doc.RootElement.GetProperty("compilerOptions");

        Assert.True(options.GetProperty("strict").GetBoolean());
        Assert.False(options.GetProperty("skipLibCheck").GetBoolean());
        Assert.True(options.GetProperty("declaration").GetBoolean());
    }

    [Fact]
    public void CompilerOptions_All_Bool_Properties_Should_Be_Serialized()
    {
        var builder = new TsConfigBuilder();

        builder.CompilerOptions.ResolveJsonModule = true;
        builder.CompilerOptions.IsolatedModules = true;
        builder.CompilerOptions.AllowJs = true;
        builder.CompilerOptions.CheckJs = true;
        builder.CompilerOptions.SourceMap = true;
        builder.CompilerOptions.EsModuleInterop = true;
        builder.CompilerOptions.AllowSyntheticDefaultImports = true;

        var doc = JsonDocument.Parse(builder.ToString());
        var options = doc.RootElement.GetProperty("compilerOptions");

        Assert.True(options.GetProperty("resolveJsonModule").GetBoolean());
        Assert.True(options.GetProperty("isolatedModules").GetBoolean());
        Assert.True(options.GetProperty("allowJs").GetBoolean());
        Assert.True(options.GetProperty("checkJs").GetBoolean());
        Assert.True(options.GetProperty("sourceMap").GetBoolean());
        Assert.True(options.GetProperty("esModuleInterop").GetBoolean());
        Assert.True(options.GetProperty("allowSyntheticDefaultImports").GetBoolean());
    }

    [Fact]
    public void CompilerOptions_Paths_Should_Be_Serialized()
    {
        var builder = new TsConfigBuilder();
        builder.CompilerOptions.Paths["@api/*"] = ["./App/Api/*"];

        var doc = JsonDocument.Parse(builder.ToString());
        var paths = doc.RootElement.GetProperty("compilerOptions").GetProperty("paths");

        Assert.Equal("./App/Api/*", paths.GetProperty("@api/*")[0].GetString());
    }

    [Fact]
    public void CompilerOptions_Enum_Without_EnumMember_Should_Use_LowerCamelCase()
    {
        var builder = new TsConfigBuilder();
        builder.CompilerOptions.Target = TsTarget.ES2015;
        var doc = JsonDocument.Parse(builder.ToString());
        Assert.Equal("eS2015", GetCompilerOption(doc, "target").GetString());
    }

    private static JsonElement GetCompilerOption(JsonDocument doc, string name)
    {
        return doc.RootElement.GetProperty("compilerOptions").GetProperty(name);
    }
}
