using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Registration;
using FluentTestScaffold.Core;
using FluentTestScaffold.Tests.CustomBuilder;
using FluentTestScaffold.Tests.CustomBuilder.Autofac;
using FluentTestScaffold.Tests.CustomBuilder.DefaultIoc;
using FluentTestScaffold.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System.Globalization;
using System.Linq;
using FluentTestScaffold.Sample.Data;
using FluentTestScaffold.Sample.Services;
using FluentTestScaffold.Tests.CustomBuilder.DataTemplates;

namespace FluentTestScaffold.Tests.Core;

[TestFixture]
public class AutofacServiceBuilderTests
{
    [Test]
    public void AutofacServiceBuilder_UseAutofac()
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
            .UseAutofac(ctx =>
            {
                ctx.Container.Register<IAuthService>(_ => mockAuthService.Object).SingleInstance();
                ctx.Container.RegisterType<UserRequestContext>().As<IUserRequestContext>().SingleInstance();
            });

        var requestContext = testScaffold.Resolve<IUserRequestContext>();
        requestContext.AuthenticateUser(user.Email, user.Password);

        Assert.AreEqual(requestContext.CurrentUser?.Id, user.Id);
    }

    [Test]
    public void AutofacServiceBuilder_DefaultConfig()
    {
        //Default config should enable auto discovery
        var testScaffold = new TestScaffold()
            .UseAutofac(new AutofacServiceBuilder());

        Assert.NotNull( testScaffold.Resolve<InventoryBuilder>());
    }
    [Test]
    public void AutofacServiceBuilder_CustomConfig()
    {
        var testScaffold = new TestScaffold()
            .UseAutofac(new AutofacServiceBuilder(new ConfigOptions()
            {
                AutoDiscovery = AutoDiscovery.None
            }));

        Assert.Throws<ComponentNotRegisteredException>(() => testScaffold.Resolve<InventoryBuilder>());
    }
    
    //Scoped
    
    [Test]
    public void AutofacServiceBuilder_RegisterScoped()
    {
        var testScaffold = new TestScaffold()
            .UseAutofac(ctx => ctx.Container.RegisterType<MockService>().InstancePerLifetimeScope());
        
        var resolved1 = testScaffold.Resolve<MockService>();
        var resolved2 = testScaffold.Resolve<MockService>();
        
        using var scope = testScaffold.ServiceProvider!.CreateScope();
        var resolved3 = scope.ServiceProvider.GetService<MockService>();
        resolved3?.Increment();

        Assert.AreSame(resolved1, resolved2, "Object reference should match when resolved from the same scope");
        Assert.AreNotSame(resolved1, resolved3, "Object reference should not match when resolved from different scopes");
    }

    [Test]
    public void AutofacServiceBuilder_RegisterScoped_With_Ctor()
    {
        var testScaffold = new TestScaffold()
            .UseAutofac(ctx =>
            {
                ctx.Container.Register(_ =>
                    {
                        var service = new MockService();
                        service.Increment();
                        return service;
                    }).InstancePerLifetimeScope();
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
    public void AutofacServiceBuilder_RegisterScopedAs_As_Type()
    {
        var testScaffold = new TestScaffold()
            .UseAutofac(ctx =>
            {
                ctx.Container.RegisterType<MockService>().As<IMockService>().InstancePerLifetimeScope();
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
    public void AutofacServiceBuilder_RegisterScopedAs_With_Ctor()
    {
        var testScaffold = new TestScaffold()
            .UseAutofac(ctx =>
            {
                ctx.Container.Register<IMockService>(_ =>
                {
                    var service = new MockService();
                    service.Increment();
                    return service;
                }).InstancePerLifetimeScope();
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
    public void AutofacServiceBuilder_RegisterTransient()
    {
        var testScaffold = new TestScaffold()
            .UseAutofac(ctx =>
            {
                ctx.Container.RegisterType<MockService>().InstancePerDependency();
            });

        var resolved1 = testScaffold.Resolve<MockService>();
        var resolved2 = testScaffold.Resolve<MockService>();

        Assert.AreNotSame(resolved1, resolved2, "Object reference should not match");
    }

    [Test]
    public void AutofacServiceBuilder_RegisterTransient_With_Ctor()
    {
        var testScaffold = new TestScaffold()
            .UseAutofac(ctx =>
            {
                ctx.Container.Register(_ =>
                {
                    var service = new MockService();
                    service.Increment();
                    return service;
                }).InstancePerDependency();
            });

        var resolved1 = testScaffold.Resolve<MockService>();
        var resolved2 = testScaffold.Resolve<MockService>();

        Assert.AreNotSame(resolved1, resolved2, "Object reference should not match");
        Assert.AreEqual(1, resolved1.Count, "Constructor State is not replicated");
        Assert.AreEqual(1, resolved2.Count, "Constructor State is not replicated");
    }
    
    [Test]
    public void AutofacServiceBuilder_RegisterTransientAs_As_Type()
    {
        var testScaffold = new TestScaffold()
            .UseAutofac(ctx => ctx.Container.RegisterType<MockService>().As<IMockService>().InstancePerDependency());

        var resolved1 = testScaffold.Resolve<IMockService>();
        var resolved2 = testScaffold.Resolve<IMockService>();

        Assert.AreNotSame(resolved1, resolved2, "Object reference should not match");
        Assert.Catch<ComponentNotRegisteredException>(() => testScaffold.Resolve<MockService>());
    }

    [Test]
    public void AutofacServiceBuilder_RegisterTransientAs_With_Ctor()
    {
        var testScaffold = new TestScaffold()
            .UseAutofac(ctx => ctx.Container.Register<IMockService>(_ =>
            {
                var service = new MockService();
                service.Increment();
                return service;
            }).InstancePerDependency());

        var resolved1 = testScaffold.Resolve<IMockService>();
        var resolved2 = testScaffold.Resolve<IMockService>();

        Assert.AreNotSame(resolved1, resolved2, "Object reference should not match");
        Assert.Catch<ComponentNotRegisteredException>(() => testScaffold.Resolve<MockService>());
    }
    
    //Singleton

      [Test]
    public void AutofacServiceBuilder_RegisterSingleton()
    {
        var testScaffold = new TestScaffold()
            .UseAutofac(ctx => ctx.Container.RegisterType<MockService>().SingleInstance());

        var resolved1 = testScaffold.Resolve<MockService>();
        var resolved2 = testScaffold.Resolve<MockService>();

        Assert.AreSame(resolved1, resolved2, "Object reference should match");
    }

    [Test]
    public void AutofacServiceBuilder_RegisterSingleton_With_Ctor()
    {
        var testScaffold = new TestScaffold()
            .UseAutofac(ctx => ctx.Container.Register(_ =>
            {
                var service = new MockService();
                service.Increment();
                return service;
            }).SingleInstance());

        var resolved1 = testScaffold.Resolve<MockService>();
        var resolved2 = testScaffold.Resolve<MockService>();

        Assert.AreSame(resolved1, resolved2, "Object reference should match");
        Assert.AreEqual(1, resolved1.Count, "Constructor State is not replicated");
    }
    
    [Test]
    public void AutofacServiceBuilder_RegisterSingletonAs_As_Type()
    {
        var testScaffold = new TestScaffold()
            .UseAutofac(ctx => ctx.Container.RegisterType<MockService>().As<IMockService>().SingleInstance());

        var resolved1 = testScaffold.Resolve<IMockService>();
        var resolved2 = testScaffold.Resolve<IMockService>();

        Assert.AreSame(resolved1, resolved2, "Object reference should match");
        Assert.Catch<ComponentNotRegisteredException>(() => testScaffold.Resolve<MockService>());
    }

    [Test]
    public void AutofacServiceBuilder_RegisterSingletonAs_With_Ctor()
    {
        var testScaffold = new TestScaffold()
            .UseAutofac(ctx => ctx.Container.Register<IMockService>(_ =>
            {
                var service = new MockService();
                service.Increment();
                return service;
            }).SingleInstance());

        var resolved1 = testScaffold.Resolve<IMockService>();
        var resolved2 = testScaffold.Resolve<IMockService>();

        using var scope = testScaffold.ServiceProvider!.CreateScope();
        var resolved3 = scope.ServiceProvider.GetService<IMockService>();

        Assert.AreSame(resolved1, resolved2, "Object reference should match");
        Assert.AreSame(resolved1, resolved3, "Object reference should match");
        Assert.Catch<ComponentNotRegisteredException>(() => testScaffold.Resolve<MockService>());
    }
    
    [Test]
    public void AutofacServiceBuilder_RegisterBuilder()
    {
        var messages = new List<string>();
        
        new TestScaffold()
            .UseAutofac()
            .UsingBuilder<MockBuilder>()
            .WithMockData(messages);
        
        Assert.Contains("Hello World!", messages);
    }
    
    [Test]
    public void AutofacServiceBuilder_RegisterBuilder_Multiple_At_Once()
    {
        var dbContext = TestDbContextFactory.Create();
        var testScaffold = new TestScaffold()
            .UseAutofac(ctx =>
            {
                ctx.Container.Register(_ => dbContext);
            });
        
        var mockBuilder = testScaffold.Resolve<MockBuilder>();
        var inventoryBuilder = testScaffold.Resolve<InventoryBuilder>();
        
        Assert.IsNotNull(mockBuilder);
        Assert.IsNotNull(inventoryBuilder);
    }
    
    [Test]
    public void AutofacServiceBuilder_With_DefaultServiceBuilder()
    {
        //Register service as instance per lifetime scope using autofac builder.
        var testScaffold = new TestScaffold()
            .UseAutofac(ctx => ctx.Container.RegisterType<TimeService>().As<ITimeService>().InstancePerLifetimeScope());


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
    public void AutofacServiceBuilder_WithCustomServiceBuilder()
    {
        var user = new User(Guid.NewGuid(), "Joe", "joe@test.com", "superSecret", new DateTime(2020, 03, 20));
        var testScaffold = new TestScaffold()
            .UseAutofac(new AutofacAppServicesBuilder(),
                ctx =>
                {
                    ctx.RegisterAppServices();
                    ctx.AutoDiscover();
                })
            .UsingBuilder<UserBuilder>()
            .WithUser(user)
            .Build();

        var requestContext = testScaffold.Resolve<IUserRequestContext>();
        requestContext.AuthenticateUser(user.Email, user.Password);

        Assert.AreEqual(user, requestContext.CurrentUser);
    }
    
    
    [Test]
    public void AutofacServiceBuilder_WithAutoDiscovery_None()
    {
        Assert.Throws<ComponentNotRegisteredException>(() =>
        {
            new TestScaffold(new ConfigOptions() {AutoDiscovery = AutoDiscovery.None})
                .UseAutofac()
                .Resolve<InventoryBuilder>();
        });


        //Service is still initialized but should have nothing cached
        var dataTemplateService = new TestScaffold(new ConfigOptions() {AutoDiscovery = AutoDiscovery.None})
            .UseAutofac()
            .Resolve<DataTemplateService>();
        Assert.Throws<MissingMethodException>(() => dataTemplateService.FindByName(TestScaffoldDataTemplates.TemplateAttributeName));
    }

    [Test]
    public void AutofacServiceBuilder_WithAutoDiscovery_Builders()
    {
        Assert.DoesNotThrow(() =>
        {
            new TestScaffold(new ConfigOptions() {AutoDiscovery = AutoDiscovery.Builders})
                .UseAutofac()
                .Resolve<InventoryBuilder>();
        });
    }
    
    [Test]
    public void AutofacServiceBuilder_WithAutoDiscovery_DataTemplate()
    {
        Assert.DoesNotThrow(() =>
        {
            var dataTemplateService = new TestScaffold(new ConfigOptions() {AutoDiscovery = AutoDiscovery.DataTemplates})
                .UseAutofac()
                .Resolve<DataTemplateService>();
            
            var dataTemplate = dataTemplateService.FindByName(TestScaffoldDataTemplates.TemplateAttributeName);
            Assert.IsNotNull(dataTemplate);
        });
    }
    
    [Test]
    public void AutofacServiceBuilder_AutoDiscovery_WithDefaultConfigOptions()
    {
        Assert.DoesNotThrow(() =>
        {
            var dataTemplateService = new TestScaffold()
                .UseAutofac()
                .Resolve<DataTemplateService>();
            
            var dataTemplate = dataTemplateService.FindByName(TestScaffoldDataTemplates.TemplateAttributeName);
            Assert.IsNotNull(dataTemplate);
        });
    }
    
    [Test]
    public void AutofacServiceBuilder_AutoDiscovery_WithIgnoredAssemblies()
    {
        Assert.DoesNotThrow(() =>
        {
            var configOptions = new ConfigOptions(new[] { "FluentTestScaffold.Tests", "Microsoft.*", "System.*" })
            {
                AutoDiscovery = AutoDiscovery.All,
                Assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).ToList(),
            };
            var testScaffold = new TestScaffold(configOptions)
                .UseAutofac();

            Assert.IsTrue(configOptions.Assemblies.All(a =>
                !a.FullName.StartsWith("FluentTestScaffold.Tests")
                || !a.FullName.StartsWith("Microsoft")
                || !a.FullName.StartsWith("System")));
            
            var dataTemplateService = testScaffold.Resolve<DataTemplateService>();
            Assert.Throws<MissingMethodException>(() => 
            {
                var dataTemplate = dataTemplateService.FindByName(TestScaffoldDataTemplates.TemplateAttributeName);
            });
        });
    }
    
    [Test]
    public void AutofacServiceBuilder_WithAutoDiscovery_All()
    {
        Assert.DoesNotThrow(() =>
        {
            var dataTemplateService = new TestScaffold(new ConfigOptions() {AutoDiscovery = AutoDiscovery.All})
                .UseAutofac()
                .Resolve<DataTemplateService>();
            
            var dataTemplate = dataTemplateService.FindByName(TestScaffoldDataTemplates.TemplateAttributeName);
            Assert.IsNotNull(dataTemplate);
        });
    }
    
    [Test]
    public void AutofacServiceBuilder_WithAutoDiscovery_WithSpecificAssemblies()
    {
        var options = new ConfigOptions()
        {
            // Not expecting any custom builders or data templates to be found in this assembly.
            Assemblies = new List<Assembly> {typeof(TestScaffold).Assembly},
            AutoDiscovery = AutoDiscovery.All
        };
        
        Assert.Throws<ComponentNotRegisteredException>(() =>
        {
            new TestScaffold(options)
                .UseAutofac()
                .Resolve<InventoryBuilder>();
        });
        
        //Service is still initialized but should have nothing cached
        var dataTemplateService = new TestScaffold(options)
            .UseAutofac()
            .Resolve<DataTemplateService>();
        Assert.Throws<MissingMethodException>(() => dataTemplateService.FindByName(TestScaffoldDataTemplates.TemplateAttributeName));
    }
    
        [Test]
        public void AutofacServiceBuilder_WithMock()
        {
            Assert.DoesNotThrow(() =>
            {
                var timeString = "12:51:01";
                var timeService = new TestScaffold()
                    .UseAutofac(serviceBuilder =>
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