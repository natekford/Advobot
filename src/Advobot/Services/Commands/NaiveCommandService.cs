using Advobot.Attributes;
using Advobot.CommandAssemblies;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.Services.BotConfig;
using Advobot.Services.Events;
using Advobot.Services.GuildSettings;
using Advobot.Services.Help;
using Advobot.TypeReaders;

using Discord;
using Discord.Commands;

using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace Advobot.Services.Commands;

/// <summary>
/// Handles user input commands.
/// </summary>
/// <param name="services"></param>
/// <param name="commandConfig"></param>
/// <param name="client"></param>
/// <param name="eventProvider"></param>
/// <param name="botConfig"></param>
/// <param name="guildSettings"></param>
/// <param name="help"></param>
public sealed class NaiveCommandService(
	IServiceProvider services,
	CommandServiceConfig commandConfig,
	IDiscordClient client,
	EventProvider eventProvider,
	IRuntimeConfig botConfig,
	IGuildSettingsService guildSettings,
	IHelpService help
) : StartableService
{
	// TODO: hook/remove the events in start/stop?
	private readonly Localized<CommandService> _CommandService = new(_ =>
	{
		var commands = new CommandService(commandConfig);
		commands.Log += eventProvider.Log.InvokeAsync;
		commands.CommandExecuted += eventProvider.CommandExecuted.InvokeAsync;
		return commands;
	});
	private readonly ConcurrentDictionary<ulong, byte> _GatheringUsers = new();

	/// <summary>
	/// Adds commands to this service.
	/// </summary>
	/// <param name="assemblies"></param>
	/// <returns></returns>
	/// <exception cref="InvalidCastException"></exception>
	public async Task AddCommandsAsync(IEnumerable<CommandAssembly> assemblies)
	{
		var commandServices = assemblies
			.SelectMany(x => x.SupportedCultures)
			.ToHashSet()
			.Select(_CommandService.Get)
			.ToArray();
		var types = assemblies
			.Select(x => x.Assembly)
			.Prepend(Assembly.GetExecutingAssembly())
			.SelectMany(x => x.GetTypes());
		foreach (var type in types)
		{
			var attr = type.GetCustomAttribute<TypeReaderTargetTypeAttribute>();
			if (attr is null)
			{
				continue;
			}
			if (Activator.CreateInstance(type) is not TypeReader instance)
			{
				throw new InvalidCastException($"{type} is not a {nameof(TypeReader)}.");
			}

			foreach (var commandService in commandServices)
			{
				foreach (var targetType in attr.TargetTypes)
				{
					commandService.AddTypeReader(targetType, instance, replaceDefault: true);
				}
			}
		}

		foreach (var assembly in assemblies)
		{
			foreach (var culture in assembly.SupportedCultures)
			{
				await AddCommandsAsync(culture, assembly.Assembly).ConfigureAwait(false);
			}
		}
	}

	/// <inheritdoc />
	protected override Task StartAsyncImpl()
	{
		eventProvider.MessageReceived.Add(OnMessageReceived);

		return Task.CompletedTask;
	}

	/// <inheritdoc />
	protected override Task StopAsyncImpl()
	{
		eventProvider.MessageReceived.Remove(OnMessageReceived);

		return base.StopAsyncImpl();
	}

	private async Task AddCommandsAsync(CultureInfo culture, Assembly assembly)
	{
		var commandService = _CommandService.Get(culture);
		var modules = await commandService.AddModulesAsync(assembly, services).ConfigureAwait(false);

		static IEnumerable<ModuleInfo> GetAllModules(ModuleInfo module)
		{
			yield return module;
			foreach (var submodule in module.Submodules)
			{
				foreach (var m in GetAllModules(submodule))
				{
					yield return m;
				}
			}
		}

		static void ThrowIfDuplicateId(
			ModuleInfo module,
			Dictionary<Guid, ModuleInfo> ids,
			MetaAttribute meta)
		{
			if (!ids.TryGetValue(meta.Guid, out var original))
			{
				ids.Add(meta.Guid, module);
				return;
			}

			var shouldThrow = true;
			for (var m = module; m != null; m = m.Parent)
			{
				if (m == original)
				{
					shouldThrow = false;
					break;
				}
			}

			if (shouldThrow)
			{
				throw new InvalidOperationException($"Duplicate id between {original.Name} and {module.Name}.");
			}
		}

		int commandCount = 0, helpEntryCount = 0;
		var ids = new Dictionary<Guid, ModuleInfo>();
		var categories = new HashSet<string>();
		foreach (var module in modules.SelectMany(GetAllModules))
		{
			var meta = module.Attributes.OfType<MetaAttribute>().FirstOrDefault();
			if (meta is null)
			{
				continue;
			}
			ThrowIfDuplicateId(module, ids, meta);

			++commandCount;
			if (!module.Attributes.Any(a => a is HiddenAttribute))
			{
				++helpEntryCount;

				var category = module.Attributes.OfType<CategoryAttribute>().First();
				categories.Add(category.Category);
				help.Add(new HelpModule(module, meta, category));
			}
		}

		await eventProvider.Log.InvokeAsync(new(
			severity: LogSeverity.Info,
			source: nameof(AddCommandsAsync),
			message: $"Successfully loaded {categories.Count} categories " +
				$"containing {commandCount} commands " +
				$"({helpEntryCount} were given help entries) " +
				$"from {assembly.GetName().Name} in the {culture} culture."
		)).ConfigureAwait(false);
	}

	private async Task OnMessageReceived(IMessage message)
	{
		if (botConfig.Pause
			|| message.Author.IsBot
			|| string.IsNullOrWhiteSpace(message.Content)
			|| botConfig.UsersIgnoredFromCommands.Contains(message.Author.Id)
			|| message is not IUserMessage msg
			|| msg.Author is not IGuildUser user
			|| msg.Channel is not ITextChannel _)
		{
			return;
		}

		if (_GatheringUsers.TryAdd(user.Guild.Id, 0))
		{
			_ = user.Guild.DownloadUsersAsync();
		}

		var argPos = -1;
		if (!msg.HasMentionPrefix(client.CurrentUser, ref argPos))
		{
			var prefix = await guildSettings.GetPrefixAsync(user.Guild).ConfigureAwait(false);
			if (!msg.HasStringPrefix(prefix, ref argPos))
			{
				return;
			}
		}

		var culture = await guildSettings.GetCultureAsync(user.Guild).ConfigureAwait(false);
		CultureInfo.CurrentUICulture = culture;
		CultureInfo.CurrentCulture = culture;
		var commands = _CommandService.Get(culture);
		var context = new GuildCommandContext(client, msg);
		await commands.ExecuteAsync(context, argPos, services).ConfigureAwait(false);
	}
}