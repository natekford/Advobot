using System.Collections.ObjectModel;

using Advobot.Services.BotSettings;
using Advobot.UI.ValidationAttributes;

using Discord;

namespace Advobot.UI.ViewModels
{
	public sealed class BotSettingsViewModel : SettingsViewModel
	{
		private readonly IBotSettings _BotSettings;

		private string _Prefix = "";

		private string? _Stream;

		public bool AlwaysDownloadUsers
		{
			get => _BotSettings.AlwaysDownloadUsers;
			set => _BotSettings.AlwaysDownloadUsers = value;
		}

		public string? Game
		{
			get => _BotSettings.Game;
			set => _BotSettings.Game = value;
		}

		public LogSeverity LogLevel
		{
			get => _BotSettings.LogLevel;
			set => _BotSettings.LogLevel = value;
		}

		public int MaxBannedNames
		{
			get => _BotSettings.MaxBannedNames;
			set => _BotSettings.MaxBannedNames = value;
		}

		public int MaxBannedPunishments
		{
			get => _BotSettings.MaxBannedPunishments;
			set => _BotSettings.MaxBannedPunishments = value;
		}

		public int MaxBannedRegex
		{
			get => _BotSettings.MaxBannedRegex;
			set => _BotSettings.MaxBannedRegex = value;
		}

		public int MaxBannedStrings
		{
			get => _BotSettings.MaxBannedStrings;
			set => _BotSettings.MaxBannedStrings = value;
		}

		public int MaxMessageGatherSize
		{
			get => _BotSettings.MaxMessageGatherSize;
			set => _BotSettings.MaxMessageGatherSize = value;
		}

		public int MaxQuotes
		{
			get => _BotSettings.MaxQuotes;
			set => _BotSettings.MaxQuotes = value;
		}

		public int MaxRuleCategories
		{
			get => _BotSettings.MaxRuleCategories;
			set => _BotSettings.MaxRuleCategories = value;
		}

		public int MaxRulesPerCategory
		{
			get => _BotSettings.MaxRulesPerCategory;
			set => _BotSettings.MaxRulesPerCategory = value;
		}

		public int MaxSelfAssignableRoleGroups
		{
			get => _BotSettings.MaxSelfAssignableRoleGroups;
			set => _BotSettings.MaxSelfAssignableRoleGroups = value;
		}

		public int MaxUserGatherCount
		{
			get => _BotSettings.MaxUserGatherCount;
			set => _BotSettings.MaxUserGatherCount = value;
		}

		public int MessageCacheSize
		{
			get => _BotSettings.MessageCacheSize;
			set => _BotSettings.MessageCacheSize = value;
		}

		//When validation is used these weird methods and useless field have to be used.
		[PrefixValidation]
		public string Prefix
		{
			get => IsValid() ? _BotSettings.Prefix : _Prefix;
			set => RaiseAndSetIfChangedAndValid(v => _BotSettings.Prefix = v, ref _Prefix, value, new PrefixValidationAttribute());
		}

		[TwitchStreamValidation]
		public string? Stream
		{
			get => IsValid() ? _BotSettings.Stream : _Stream;
			set => RaiseAndSetIfChangedAndValid(v => _BotSettings.Stream = v, ref _Stream, value, new TwitchStreamValidationAttribute());
		}

		public ObservableCollection<ulong> TrustedUsers
			=> (ObservableCollection<ulong>)_BotSettings.TrustedUsers;

		public ObservableCollection<ulong> UsersIgnoredFromCommands
			=> (ObservableCollection<ulong>)_BotSettings.UsersIgnoredFromCommands;

		public ObservableCollection<ulong> UsersUnableToDmOwner
					=> (ObservableCollection<ulong>)_BotSettings.UsersUnableToDmOwner;

		public BotSettingsViewModel(IBotSettings botSettings) : base(botSettings)
		{
			_BotSettings = botSettings;
		}
	}
}