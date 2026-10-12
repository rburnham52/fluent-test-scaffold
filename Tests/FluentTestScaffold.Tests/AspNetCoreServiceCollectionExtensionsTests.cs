using System;
using System.Linq;
using FluentTestScaffold.Sample.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceCollectionExtensions = FluentTestScaffold.AspNetCore.ServiceCollectionExtensions;

namespace FluentTestScaffold.Tests;

[TestFixture]
public class AspNetCoreServiceCollectionExtensionsTests
{
    [Test]
    public void ReplaceServiceWithMock_Replaces_Existing_Registration()
    {
        var expectedTime = new TimeOnly(12, 51, 01);
        var mock = new Mock<ITimeService>();
        mock.Setup(c => c.GetTime()).Returns(expectedTime);

        var services = new ServiceCollection();
        services.AddTransient<ITimeService, TimeService>();

        var result = ServiceCollectionExtensions.ReplaceServiceWithMock(services, mock);

        using var serviceProvider = services.BuildServiceProvider();
        var timeService = serviceProvider.GetRequiredService<ITimeService>();

        Assert.AreSame(services, result);
        Assert.AreEqual(1, services.Count(sd => sd.ServiceType == typeof(ITimeService)));
        Assert.AreSame(mock.Object, timeService);
        Assert.AreEqual(expectedTime, timeService.GetTime());
    }

    [Test]
    public void ReplaceServiceWithMock_Leaves_Other_Registrations_Untouched()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TestService>();
        services.AddTransient<ITimeService, TimeService>();

        ServiceCollectionExtensions.ReplaceServiceWithMock(services, new Mock<ITimeService>());

        using var serviceProvider = services.BuildServiceProvider();
        Assert.IsNotNull(serviceProvider.GetService<TestService>());
    }

    [Test]
    public void ReplaceServiceWithMock_Throws_When_Service_Not_Registered()
    {
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() =>
            ServiceCollectionExtensions.ReplaceServiceWithMock(services, new Mock<ITimeService>()));
    }
}
