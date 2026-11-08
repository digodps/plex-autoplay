using NetArchTest.Rules;
using PlexAutoplay.Application;
using PlexAutoplay.Domain.Entities;
using PlexAutoplay.Domain.Ports;
using PlexAutoplay.Web.Endpoints;

namespace PlexAutoplay.ArchitectureTests;

public class LayerDependencyTests
{
    private static readonly System.Reflection.Assembly DomainAssembly =
        typeof(Marker).Assembly;

    private static readonly System.Reflection.Assembly ApplicationAssembly =
        typeof(IPlexApiClient).Assembly;

    private static readonly System.Reflection.Assembly WebAssembly =
        typeof(SettingsEndpoints).Assembly;

    private static void AssertPass(TestResult result, string what)
    {
        if (result.IsSuccessful)
        {
            return;
        }

        var offenders = result.FailingTypeNames is null
            ? "(none reported)"
            : string.Join(", ", result.FailingTypeNames);

        Assert.Fail($"{what}: {offenders}");
    }

    [Fact]
    public void Domain_does_not_depend_on_infrastructure_or_system_io_namespaces()
    {
        string[] forbidden =
        {
            "PlexAutoplay.Infrastructure",
            "System.Data",
            "System.Net.Http"
        };

        foreach (var dependency in forbidden)
        {
            var result = Types
                .InAssembly(DomainAssembly)
                .ShouldNot()
                .HaveDependencyOn(dependency)
                .GetResult();

            AssertPass(result, $"Domain depends on '{dependency}'");
        }
    }

    [Fact]
    public void Application_does_not_depend_on_web()
    {
        var result = Types
            .InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("PlexAutoplay.Web")
            .GetResult();

        AssertPass(result, "Application depends on Web");
    }

    [Fact]
    public void Web_does_not_implement_persistence_or_network_interfaces()
    {
        Type[] forbidden =
        {
            typeof(IMarkerRepository),
            typeof(ISeriesRepository),
            typeof(ILibraryRepository),
            typeof(IUserThemeRepository),
            typeof(IPlexApiClient),
            typeof(IImageProxyService),
            typeof(IAppSettingsStore),
            typeof(IScanJobStore),
            typeof(IPathResolver)
        };

        foreach (var contract in forbidden)
        {
            var result = Types
                .InAssembly(WebAssembly)
                .ShouldNot()
                .ImplementInterface(contract)
                .GetResult();

            AssertPass(result, $"Web implements '{contract.Name}'");
        }
    }
}
