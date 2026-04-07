using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentTestScaffold.Sample.AutofacWebApp;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace FluentTestScaffold.Sample.AutofacWebApp;

public class AutofacProgram
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        builder.Host.ConfigureContainer<ContainerBuilder>(cb =>
        {
            cb.RegisterType<RealOverrideService>().As<IOverrideTestService>().SingleInstance();
            cb.RegisterType<RealOtherService>().As<IOtherTestService>().SingleInstance();
        });

        var app = builder.Build();

        app.MapGet("/", () => "Autofac Sample");

        app.Run();
    }
}
