using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;

namespace Advobot.Serilog;

/// <summary>
/// Utilities for Serilog.
/// </summary>
public static class LoggingUtils
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
		var serilog = CreateSerilog(name);
		return new SerilogLoggerFactory(serilog).CreateLogger<T>();
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
			.MinimumLevel.Debug()
			.Destructure.With(DiscordObjectDestructuringPolicy.Instance)
			.WriteTo.File(
				formatter: new JsonFormatter(),
				path: Path.Combine("Logs", $"{fileName}.txt")
			)
			.WriteTo.Console(
				restrictedToMinimumLevel: LogEventLevel.Warning,
				theme: AnsiConsoleTheme.Code
			)
			.CreateLogger();
	}

	private sealed class DiscordObjectDestructuringPolicy : IDestructuringPolicy
	{
		public static DiscordObjectDestructuringPolicy Instance { get; } = new();

		public bool TryDestructure(
			object value,
			ILogEventPropertyValueFactory propertyValueFactory,
			out LogEventPropertyValue? result)
		{
			// Guild, Channel, User, Message
			if (value is IEntity<ulong> entity)
			{
				result = propertyValueFactory.CreatePropertyValue(entity.Id);
				return true;
			}
			// Invite
			else if (value is IEntity<string> entity2)
			{
				result = propertyValueFactory.CreatePropertyValue(entity2.Id);
				return true;
			}

			result = null;
			return false;
		}
	}
}