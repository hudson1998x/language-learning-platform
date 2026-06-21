using System.Text.Json;
using LLE.TypeScript.Builders;

namespace Tests;

public sealed class PackageJsonBuilderTests
{
    [Fact]
    public void Default_Should_Produce_Minimal_Json()
    {
        var builder = new PackageJsonBuilder();
        var doc = JsonDocument.Parse(builder.ToString());
        Assert.Equal("module", doc.RootElement.GetProperty("type").GetString());
    }

    [Fact]
    public void WithName_Should_Set_Name()
    {
        var builder = new PackageJsonBuilder();
        builder.WithName("my-package");
        var doc = JsonDocument.Parse(builder.ToString());
        Assert.Equal("my-package", doc.RootElement.GetProperty("name").GetString());
    }

    [Fact]
    public void WithVersion_Should_Set_Version()
    {
        var builder = new PackageJsonBuilder();
        builder.WithVersion("1.2.3");
        var doc = JsonDocument.Parse(builder.ToString());
        Assert.Equal("1.2.3", doc.RootElement.GetProperty("version").GetString());
    }

    [Fact]
    public void WithDescription_Should_Set_Description()
    {
        var builder = new PackageJsonBuilder();
        builder.WithDescription("A test package");
        var doc = JsonDocument.Parse(builder.ToString());
        Assert.Equal("A test package", doc.RootElement.GetProperty("description").GetString());
    }

    [Fact]
    public void WithMain_Should_Set_Main()
    {
        var builder = new PackageJsonBuilder();
        builder.WithMain("dist/index.js");
        var doc = JsonDocument.Parse(builder.ToString());
        Assert.Equal("dist/index.js", doc.RootElement.GetProperty("main").GetString());
    }

    [Fact]
    public void Fluent_Methods_Should_Return_Same_Instance()
    {
        var builder = new PackageJsonBuilder();
        var result = builder
            .WithName("pkg")
            .WithVersion("1.0")
            .WithDescription("desc")
            .WithMain("index.js");
        Assert.Same(builder, result);
    }

    [Fact]
    public void AddScript_Should_Add_Script()
    {
        var builder = new PackageJsonBuilder();
        builder.AddScript("build", "tsc");
        var doc = JsonDocument.Parse(builder.ToString());
        Assert.Equal("tsc", doc.RootElement.GetProperty("scripts").GetProperty("build").GetString());
    }

    [Fact]
    public void AddScript_Should_Overwrite_Existing_Script()
    {
        var builder = new PackageJsonBuilder();
        builder.AddScript("build", "tsc");
        builder.AddScript("build", "tsc --watch");
        var doc = JsonDocument.Parse(builder.ToString());
        Assert.Equal("tsc --watch", doc.RootElement.GetProperty("scripts").GetProperty("build").GetString());
    }

    [Fact]
    public void AddDependency_App_Should_Go_Under_Dependencies()
    {
        var builder = new PackageJsonBuilder();
        builder.AddDependency("express", "^4.18", Dependencies.App);
        var doc = JsonDocument.Parse(builder.ToString());
        Assert.Equal("^4.18", doc.RootElement.GetProperty("dependencies").GetProperty("express").GetString());
        Assert.False(doc.RootElement.TryGetProperty("devDependencies", out _));
    }

    [Fact]
    public void AddDependency_Dev_Should_Go_Under_DevDependencies()
    {
        var builder = new PackageJsonBuilder();
        builder.AddDependency("typescript", "^5.0", Dependencies.Dev);
        var doc = JsonDocument.Parse(builder.ToString());
        Assert.Equal("^5.0", doc.RootElement.GetProperty("devDependencies").GetProperty("typescript").GetString());
        Assert.False(doc.RootElement.TryGetProperty("dependencies", out _));
    }

    [Fact]
    public void AddDependency_Both_Should_Go_Under_Both_Sections()
    {
        var builder = new PackageJsonBuilder();
        builder.AddDependency("lodash", "^4.17", Dependencies.App | Dependencies.Dev);
        var doc = JsonDocument.Parse(builder.ToString());
        Assert.Equal("^4.17", doc.RootElement.GetProperty("dependencies").GetProperty("lodash").GetString());
        Assert.Equal("^4.17", doc.RootElement.GetProperty("devDependencies").GetProperty("lodash").GetString());
    }

    [Fact]
    public void AddDependency_Without_Version_Should_Use_Wildcard()
    {
        var builder = new PackageJsonBuilder();
        builder.AddDependency("react", Dependencies.App);
        var doc = JsonDocument.Parse(builder.ToString());
        Assert.Equal("*", doc.RootElement.GetProperty("dependencies").GetProperty("react").GetString());
    }

    [Fact]
    public void AddScript_Should_Return_Same_Instance()
    {
        var builder = new PackageJsonBuilder();
        var result = builder.AddScript("test", "jest");
        Assert.Same(builder, result);
    }

    [Fact]
    public void AddDependency_Should_Return_Same_Instance()
    {
        var builder = new PackageJsonBuilder();
        var result = builder.AddDependency("chalk", Dependencies.App);
        Assert.Same(builder, result);
    }

    [Fact]
    public void Output_Should_Be_Valid_Json()
    {
        var builder = new PackageJsonBuilder();
        builder.WithName("test")
               .WithVersion("1.0.0")
               .AddScript("build", "tsc")
               .AddDependency("react", "^18", Dependencies.App)
               .AddDependency("typescript", "^5", Dependencies.Dev);
        var ex = Record.Exception(() => JsonDocument.Parse(builder.ToString()));
        Assert.Null(ex);
    }
}
