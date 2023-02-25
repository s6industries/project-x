## VSCode Extensions

Name: .NET Core Test Explorer
Id: formulahendry.dotnet-test-explorer
Description: Test Explorer for .NET Core (MSTest, xUnit, NUnit)
Version: 0.7.8
Publisher: Jun Han
VS Marketplace Link: https://marketplace.visualstudio.com/items?itemName=formulahendry.dotnet-test-explorer

Name: C#
Id: ms-dotnettools.csharp
Description: C# for Visual Studio Code (powered by OmniSharp).
Version: 1.25.4
Publisher: Microsoft
VS Marketplace Link: https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp


<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="ActiveLogic" Version="1.3.1" />
    <PackageReference Include="AStarLite" Version="1.1.0" />
  </ItemGroup>

</Project>


