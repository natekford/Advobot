using Advobot.Services.Events;

using Discord;

using System.Globalization;
using System.Reflection;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Localization;
using YACCS.Parsing;
using YACCS.Trie;
using YACCS.TypeReaders;

namespace Advobot.Services.Commands;

/// <summary>
/// Handles command creation.
/// </summary>
public sealed class NaiveCommandService : CommandService
{
	private readonly IEnumerable<Assembly> _CommandAssemblies;
	private readonly Localized<CommandTrie> _Commands;
	private readonly EventProvider _EventProvider;
	private readonly Localized<Lazy<Task>> _Initialize;
	private readonly IServiceProvider _Services;

	/// <inheritdoc />
	public override ITrie<string, IImmutableCommand> Commands
		=> _Commands.GetCurrent();

	/// <summary>
	/// Creates an instance of <see cref="NaiveCommandService"/>.
	/// </summary>
	/// <param name="config"></param>
	/// <param name="argumentHandler"></param>
	/// <param name="readers"></param>
	/// <param name="services"></param>
	/// <param name="commandAssemblies"></param>
	/// <param name="eventProvider"></param>
	public NaiveCommandService(
		CommandServiceConfig config,
		IArgumentHandler argumentHandler,
		IReadOnlyDictionary<Type, ITypeReader> readers,
		IServiceProvider services,
		IEnumerable<Assembly> commandAssemblies,
		EventProvider eventProvider
	) : base(config, argumentHandler, readers)
	{
		_Services = services;
		_CommandAssemblies = commandAssemblies;
		_EventProvider = eventProvider;

		_Commands = new(_ => new CommandTrie(readers, config.Separator, config.CommandNameComparer));
		_Initialize = new(_ => new(() => PrivateInitialize()));
	}

	/// <summary>
	/// Creates commands using <see cref="CultureInfo.CurrentUICulture"/>.
	/// </summary>
	/// <returns></returns>
	public Task InitializeAsync()
		=> _Initialize.GetCurrent().Value;

	/// <inheritdoc />
	protected override async Task CommandExecutedAsync(CommandExecutedResult result)
	{
		await _EventProvider.CommandExecuted.InvokeAsync(result).ConfigureAwait(false);
		await base.CommandExecutedAsync(result).ConfigureAwait(false);
	}

	private async Task PrivateInitialize()
	{
		foreach (var assembly in _CommandAssemblies)
		{
			await foreach (var (_, command) in assembly.GetAllCommandsAsync(_Services))
			{
				Commands.Add(command);
			}
		}

		await _EventProvider.Log.InvokeAsync(new(
			severity: LogSeverity.Info,
			source: nameof(InitializeAsync),
			message: $"Successfully loaded {Commands.Count} commands."
		/*
		message: $"Successfully loaded {categories.Count} categories " +
			$"containing {commandCount} commands " +
			$"({helpEntryCount} were given help entries) " +
			$"from {assembly.GetName().Name} in the {culture} culture."*/
		)).ConfigureAwait(false);
	}
}