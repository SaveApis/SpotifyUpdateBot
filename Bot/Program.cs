// See https://aka.ms/new-console-template for more information

using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Bot.Infrastructure.Spotify;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.ConfigureContainer(new AutofacServiceProviderFactory(),
    containerBuilder => containerBuilder.RegisterAssemblyModules(Assembly.GetExecutingAssembly()));

var app = builder.Build();
await app.RunAsync();