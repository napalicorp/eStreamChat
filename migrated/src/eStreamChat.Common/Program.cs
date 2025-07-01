using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using eStreamChat.Common.Security;
using eStreamChat.Common.Services;
using eStreamChat.Interfaces;

namespace eStreamChat.Common;

public static class Program
{
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // Configure options
                services.Configure<UserCleanupOptions>(hostContext.Configuration.GetSection("UserCleanup"));

                // Register services
                services.AddSingleton<HashGenerator>();
                services.AddHostedService<UserCleanupService>();

                // Configure assembly resolution for providers
                var providersPath = hostContext.Configuration["ProvidersPath"] 
                    ?? throw new InvalidOperationException("ProvidersPath must be configured");

                AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) =>
                {
                    var assemblyName = eventArgs.Name.Split(',')[0];
                    var assemblyPath = Path.Combine(providersPath, $"{assemblyName}.dll");

                    return File.Exists(assemblyPath) 
                        ? Assembly.LoadFrom(assemblyPath)
                        : null;
                };

                // Add HTTP context accessor for legacy code that needs request context
                services.AddHttpContextAccessor();

                // Add logging
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                });
            });
}