using Advobot.Utilities;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Json;

using System.Globalization;

namespace Advobot.Serilog;

public static class LoggingExtensions
{
	public static IServiceCollection AddLogger<T>(
		this IServiceCollection services,
		string name)
	{
		var logger = CreateLogger<T>(name);
		return services.AddSingleton(logger);
	}

	public static ILogger<T> CreateLogger<T>(string name)
	{
		var logger = CreateSerilog(name);
		return new SerilogLoggerFactory(logger).CreateLogger<T>();
	}

	public static Logger CreateSerilog(string fileName)
	{
		return new LoggerConfiguration()
			.Enrich.FromLogContext()
			.MinimumLevel.Verbose()
			.WriteTo.File(
				formatter: new JsonFormatter(),
				path: $"{fileName}_Logs.txt"
			)
			.CreateLogger();
	}
}