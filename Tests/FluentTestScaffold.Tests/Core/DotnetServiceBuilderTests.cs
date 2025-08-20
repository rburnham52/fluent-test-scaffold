using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using FluentTestScaffold.Core;
using FluentTestScaffold.Sample.Data;
using FluentTestScaffold.Sample.Services;
using FluentTestScaffold.Tests.CustomBuilder;
using FluentTestScaffold.Tests.CustomBuilder.Autofac;
using FluentTestScaffold.Tests.CustomBuilder.DataTemplates;
using FluentTestScaffold.Tests.CustomBuilder.DefaultIoc;
using FluentTestScaffold.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace FluentTestScaffold.Tests.Core;

[TestFixture]
public class DotnetServiceBuilderTests
{
    [Test]
    public void TestScaffold_IOC_Uses_Dot_Net_IOC_By_Default()
    {
        // Setup
        var user = new User(
            Guid.NewGuid(),
            "John",
            "John@test.com",
            "SuperSecret",
            new DateTime(1990, 03, 30));

        var mockAuthService = new Mock<IAuthService>();
        mockAuthService.Setup(c => c.AuthenticateUser(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(() => user);

        // Ensure the default IOC can be added to and resolved from
        var testScaffold = new TestScaffold()
            .UseIoc(ctx =>
            {
                ctx.Container.AddSingleton<IAuthService>(_ => mockAuthService.Object)
                    .AddSingleton<IUserRequestContext, UserRequestContext>();
            });

        var requestContext = testScaffold.Resolve<IUserRequestContext>();
        requestContext.AuthenticateUser(user.Email, user.Password);

        Assert.AreEqual(requestContext.CurrentUser?.Id, user.Id);
    }

    //Scoped

    [Test]
    public void DotnetServiceBuilder_RegisterScoped()
    {
        var testScaffold = new TestScaffold()
            .UseIoc(ctx => ctx.Container.AddScoped<MockService>());

        var resolved1 = testScaffold.Resolve<MockService>();
        var resolved2 = testScaffold.Resolve<MockService>();

        using var scope = testScaffold.ServiceProvider!.CreateScope();
        var resolved3 = scope.ServiceProvider.GetService<MockService>();
        resolved3?.Increment();

        Assert.AreSame(resolved1, resolved2, "Object reference should match when resolved from the same scope");
        Assert.AreNotSame(resolved1, resolved3, "Object reference should not match when resolved from different scopes");
    }

    [Test]
    public void DotnetServiceBuilder_RegisterScoped_With_Ctor()
    {
        var testScaffold = new TestScaffold()
            .UseIoc(ctx =>
            {
                ctx.Container.AddScoped(_ =>
                {
                    var service = new MockService();
                    service.Increment();
                    return service;
                });
            });

        var resolved1 = testScaffold.Resolve<MockService>();
        var resolved2 = testScaffold.Resolve<MockService>();

        using var scope = testScaffold.ServiceProvider!.CreateScope();
        var resolved3 = scope.ServiceProvider.GetService<MockService>();

        Assert.AreSame(resolved1, resolved2, "Object reference should match when resolved from the same scope");
        Assert.AreNotSame(resolved1, resolved3, "Object reference should not match when resolved from different scopes");
        Assert.AreEqual(1, resolved1.Count, "Constructor State is not replicated");
    }

    [Test]
    public void DotnetServiceBuilder_RegisterScopedAs_As_Type()
    {
        var testScaffold = new TestScaffold()
            .UseIoc(ctx =>
            {
                ctx.Container.AddScoped<IMockService, MockService>();
            });

        var resolved1 = testScaffold.Resolve<IMockService>();
        var resolved2 = testScaffold.Resolve<IMockService>();

        using var scope = testScaffold.ServiceProvider!.CreateScope();
        var resolved3 = scope.ServiceProvider.GetService<MockService>();
        var resolved4 = scope.ServiceProvider.GetService<MockService>();

        Assert.AreSame(resolved1, resolved2, "Object reference should match when resolved from the same scope");
        Assert.AreNotSame(resolved1, resolved3, "Object reference should not match when resolved from different scopes");
        Assert.IsNull(resolved4, "Base type should not be registered");
    }

    [Test]
    public void DotnetServiceBuilder_RegisterScopedAs_With_Ctor()
    {
        var testScaffold = new TestScaffold()
            .UseIoc(ctx =>
            {
                ctx.Container.AddScoped<IMockService>(_ =>
                {
                    var service = new MockService();
                    service.Increment();
                    return service;
                });
            });


        var resolved1 = testScaffold.Resolve<IMockService>();
        var resolved2 = testScaffold.Resolve<IMockService>();

        using var scope = testScaffold.ServiceProvider!.CreateScope();
        var resolved3 = scope.ServiceProvider.GetService<MockService>();
        var resolved4 = scope.ServiceProvider.GetService<MockService>();

        Assert.AreSame(resolved1, resolved2, "Object reference should match when resolved from the same scope");
        Assert.AreNotSame(resolved1, resolved3, "Object reference should not match when resolved from different scopes");
        Assert.AreEqual(1, resolved1.Count, "Constructor State is not replicated");
        Assert.IsNull(resolved4, "Base type should not be registered");
    }

    // Transient
    [Test]
    public void DotnetServiceBuilder_RegisterTransient()
    {
        var testScaffold = new TestScaffold()
            .UseIoc(ctx =>
            {
                ctx.Container.AddTransient<MockService>();
            });

        var resolved1 = testScaffold.Resolve<MockService>();
        var resolved2 = testScaffold.Resolve<MockService>();

        Assert.AreNotSame(resolved1, resolved2, "Object reference should not match");
    }

    [Test]
    public void DotnetServiceBuilder_RegisterTransient_With_Ctor()
    {
        var testScaffold = new TestScaffold()
            .UseIoc(ctx =>
            {
                ctx.Container.AddTransient(_ =>
                {
                    var service = new MockService();
                    service.Increment();
                    return service;
                });
            });

        var resolved1 = testScaffold.Resolve<MockService>();
        var resolved2 = testScaffold.Resolve<MockService>();

        Assert.AreNotSame(resolved1, resolved2, "Object reference should not match");
        Assert.AreEqual(1, resolved1.Count, "Constructor State is not replicated");
        Assert.AreEqual(1, resolved2.Count, "Constructor State is not replicated");
    }

    [Test]
    public void DotnetServiceBuilder_RegisterTransientAs_As_Type()
    {
        var testScaffold = new TestScaffold()
            .UseIoc(ctx => ctx.Container.AddTransient<IMockService, MockService>());

        var resolved1 = testScaffold.Resolve<IMockService>();
        var resolved2 = testScaffold.Resolve<IMockService>();

        Assert.AreNotSame(resolved1, resolved2, "Object reference should not match");
        Assert.Catch<InvalidOperationException>(() => testScaffold.Resolve<MockService>());
    }

    [Test]
    public void DotnetServiceBuilder_RegisterTransientAs_With_Ctor()
    {
        var testScaffold = new TestScaffold()
            .UseIoc(ctx => ctx.Container.AddTransient<IMockService>(_ =>
            {
                var service = new MockService();
                service.Increment();
                return service;
            }));

        var resolved1 = testScaffold.Resolve<IMockService>();
        var resolved2 = testScaffold.Resolve<IMockService>();

        Assert.AreNotSame(resolved1, resolved2, "Object reference should not match");
        Assert.Catch<InvalidOperationException>(() => testScaffold.Resolve<MockService>());
    }

    //Singleton

    [Test]
    public void DotnetServiceBuilder_RegisterSingleton()
    {
        var testScaffold = new TestScaffold()
            .UseIoc(ctx => ctx.Container.AddSingleton<MockService>());

        var resolved1 = testScaffold.Resolve<MockService>();
        var resolved2 = testScaffold.Resolve<MockService>();

        Assert.AreSame(resolved1, resolved2, "Object reference should match");
    }

    [Test]
    public void DotnetServiceBuilder_RegisterSingleton_With_Ctor()
    {
        var testScaffold = new TestScaffold()
            .UseIoc(ctx => ctx.Container.AddSingleton(_ =>
            {
                var service = new MockService();
                service.Increment();
                return service;
            }));

        var resolved1 = testScaffold.Resolve<MockService>();
        var resolved2 = testScaffold.Resolve<MockService>();

        Assert.AreSame(resolved1, resolved2, "Object reference should match");
        Assert.AreEqual(1, resolved1.Count, "Constructor State is not replicated");
    }

    [Test]
    public void DotnetServiceBuilder_RegisterSingletonAs_As_Type()
    {
        var testScaffold = new TestScaffold()
            .UseIoc(ctx => ctx.Container.AddSingleton<IMockService, MockService>());

        var resolved1 = testScaffold.Resolve<IMockService>();
        var resolved2 = testScaffold.Resolve<IMockService>();

        Assert.AreSame(resolved1, resolved2, "Object reference should match");
        Assert.Catch<InvalidOperationException>(() => testScaffold.Resolve<MockService>());
    }

    [Test]
    public void DotnetServiceBuilder_RegisterSingletonAs_With_Ctor()
    {
        var testScaffold = new TestScaffold()
            .UseIoc(ctx => ctx.Container.AddSingleton<IMockService>(_ =>
            {
                var service = new MockService();
                service.Increment();
                return service;
            }));

        var resolved1 = testScaffold.Resolve<IMockService>();
        var resolved2 = testScaffold.Resolve<IMockService>();

        using var scope = testScaffold.ServiceProvider!.CreateScope();
        var resolved3 = scope.ServiceProvider.GetService<IMockService>();

        Assert.AreSame(resolved1, resolved2, "Object reference should match");
        Assert.AreSame(resolved1, resolved3, "Object reference should match");
        Assert.Catch<InvalidOperationException>(() => testScaffold.Resolve<MockService>());
    }

    [Test]
    public void DotnetServiceBuilder_RegisterBuilder()
    {
        var testScaffold = new TestScaffold()
            .UseIoc();

        var builder = testScaffold.UsingBuilder<MockBuilder>();

        Assert.NotNull(builder, "Could not resolver builder from IOC container");
    }

    [Test]
    public void DotnetServiceBuilder_CreateBuilder_Returns_Builder_From_Context()
    {
        var context = new DotnetServiceBuilder();
        Assert.AreSame(context.Container, context.CreateBuilder(null!));
    }

    [Test]
    public void DotnetServiceBuilder_RegisterBuilder_Multiple_At_Once()
    {
        var dbContext = TestDbContextFactory.Create();
        var testScaffold = new TestScaffold()
            .UseIoc(ctx =>
            {
                ctx.Container.AddSingleton(_ => dbContext);
            });

        var mockBuilder = testScaffold.Resolve<MockBuilder>();
        var inventoryBuilder = testScaffold.Resolve<InventoryBuilder>();

        Assert.IsNotNull(mockBuilder);
        Assert.IsNotNull(inventoryBuilder);
    }

    [Test]
    public void DotnetServiceBuilder_With_DotnetServiceBuilder()
    {
        //Register service as instance per lifetime scope using autofac builder.
        var testScaffold = new TestScaffold()
            .UseIoc(ctx => ctx.Container.AddScoped<ITimeService, TimeService>());


        var timeService1 = testScaffold.Resolve<ITimeService>();
        var timeService2 = testScaffold.Resolve<ITimeService>();

        Assert.Multiple(() =>
        {
            Assert.NotNull(timeService1);
            Assert.NotNull(timeService2);

            //should return the same object reference
            Assert.AreSame(timeService1, timeService2, "Object reference should match for InstancePerLifetimeScope.");
        });
    }

    [Test]
    public void DefaultIoc_WithCustomServiceBuilder()
    {
        var user = new User(Guid.NewGuid(), "Joe", "joe@test.com", "superSecret", new DateTime(2020, 03, 20));
        var testScaffold = new TestScaffold()
            .UseIoc(
                new DotnetIocAppServicesBuilder(),
                ctx => ctx.RegisterAppServices())
            .UsingBuilder<UserBuilder>()
            .WithUser(user)
            .Build();

        var requestContext = testScaffold.Resolve<IUserRequestContext>();
        requestContext.AuthenticateUser(user.Email, user.Password);

        Assert.AreEqual(user, requestContext.CurrentUser);
    }

    [Test]
    public void DefaultIoc_WithAutoDiscovery_None()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            new TestScaffold(new ConfigOptions() { AutoDiscovery = AutoDiscovery.None })
                .UseIoc()
                .Resolve<InventoryBuilder>();
        });


        //Service is still initialized but should have nothing cached
        var testScaffold = new TestScaffold(new ConfigOptions() { AutoDiscovery = AutoDiscovery.None })
            .UseIoc();
        Assert.Throws<InvalidOperationException>(() => testScaffold.Resolve<TestScaffoldDataTemplates>());
    }

    [Test]
    public void DefaultIoc_WithAutoDiscovery_Builders()
    {
        Assert.DoesNotThrow(() =>
        {
            new TestScaffold(new ConfigOptions() { AutoDiscovery = AutoDiscovery.Builders })
                .UseIoc()
                .Resolve<InventoryBuilder>();
        });
    }

    [Test]
    public void DefaultIoc_WithAutoDiscovery_DataTemplate()
    {
        Assert.DoesNotThrow(() =>
        {
            var testScaffold = new TestScaffold(new ConfigOptions() { AutoDiscovery = AutoDiscovery.DataTemplates })
                .UseIoc();

            var dataTemplate = testScaffold.Resolve<TestScaffoldDataTemplates>();
            Assert.IsNotNull(dataTemplate);
        });
    }

    [Test]
    public void DefaultIoc_WithAutoDiscovery_All()
    {
        Assert.DoesNotThrow(() =>
        {
            var testScaffold = new TestScaffold(new ConfigOptions() { AutoDiscovery = AutoDiscovery.All })
                .UseIoc();

            var dataTemplate = testScaffold.Resolve<TestScaffoldDataTemplates>();
            Assert.IsNotNull(dataTemplate);
        });
    }

    [Test]
    public void DefaultIoc_WithAutoDiscovery_WithSpecificAssemblies()
    {
        var options = new ConfigOptions()
        {
            // Not expecting any custom builders or data templates to be found in this assembly.
            Assemblies = new List<Assembly> { typeof(TestScaffold).Assembly },
            AutoDiscovery = AutoDiscovery.All
        };

        Assert.Throws<InvalidOperationException>(() =>
        {
            new TestScaffold(options)
                .UseIoc()
                .Resolve<InventoryBuilder>();
        });

        //Service is still initialized but should have nothing cached
        Assert.Throws<InvalidOperationException>(() =>
        {
            new TestScaffold(options)
                .UseIoc()
                .Resolve<TestScaffoldDataTemplates>();
        });
    }

    [Test]
    public void DefaultIoc_WithMock()
    {
        Assert.DoesNotThrow(() =>
        {
            var timeString = "12:51:01";
            var timeService = new TestScaffold()
                .UseIoc(serviceBuilder =>
                {
                    serviceBuilder.WithMock<ITimeService>(mock =>
                    {
                        mock.Setup(c => c.GetTime()).Returns(TimeOnly.Parse(timeString, CultureInfo.CurrentCulture));
                        return mock;
                    });
                })
                .Resolve<ITimeService>();

            var time = timeService.GetTime();
            Assert.AreEqual(TimeOnly.Parse(timeString, CultureInfo.CurrentCulture), time);
        });
    }
}
