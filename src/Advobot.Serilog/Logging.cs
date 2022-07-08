using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Json;

namespace Advobot.Serilog;

/// <summary>
/// Utilities for adding Serilog to a project.
/// </summary>
public static class LoggingExtensions
{
	/// <summary>
	/// Adds a logger for <typeparamref name="T"/> to <paramref name="services"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="services"></param>
	/// <param name="name"></param>
	/// <returns></returns>
	public static IServiceCollection AddLogger<T>(
		this IServiceCollection services,
		string name)
	{
		var logger = CreateLogger<T>(name);
		return services.AddSingleton(logger);
	}

	/// <summary>
	/// Creates a logger for <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="name"></param>
	/// <returns></returns>
	public static ILogger<T> CreateLogger<T>(string name)
	{
		var logger = CreateSerilog(name);
		return new SerilogLoggerFactory(logger).CreateLogger<T>();
	}

	/// <summary>
	/// Creates a Serilog logger.
	/// </summary>
	/// <param name="fileName"></param>
	/// <returns></returns>
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