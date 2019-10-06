using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

using Advobot.Databases.Abstract;
using Advobot.Resources;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Services.GuildSettings.UserInformation;
using Advobot.Settings;
using Advobot.Settings.GenerateResetValues;
using Advobot.Utilities;

using Newtonsoft.Json;

namespace Advobot.Services.GuildSettings
{
	/// <summary>
	/// Holds settings for a guild.
	/// </summary>
	internal sealed class GuildSettings : SettingsBase, IGuildSettings, IDatabaseEntry
	{
		private readonly List<BannedPhraseUserInfo> _BannedPhraseUsers = new List<BannedPhraseUserInfo>();

		private string _Culture = "en-US";

		private GuildNotification? _GoodbyeMessage;

		private ulong _MuteRoleId;

		private bool _NonVerboseErrors;

		private GuildSettingsFactory? _Parent;

		private string? _Prefix;

		private GuildNotification? _WelcomeMessage;

		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.BannedPhraseNames), ResetValueClass = typeof(ClearList))]
		[JsonProperty("BannedPhraseNames")]
		public IList<BannedPhrase> BannedPhraseNames { get; set; } = new ObservableCollection<BannedPhrase>();

		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.BannedPhrasePunishments), ResetValueClass = typeof(ClearList))]
		[JsonProperty("BannedPhrasePunishments")]
		public IList<BannedPhrasePunishment> BannedPhrasePunishments { get; set; } = new ObservableCollection<BannedPhrasePunishment>();

		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.BannedPhraseRegex), ResetValueClass = typeof(ClearList))]
		[JsonProperty("BannedPhraseRegex")]
		public IList<BannedPhrase> BannedPhraseRegex { get; set; } = new ObservableCollection<BannedPhrase>();

		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.BannedPhraseStrings), ResetValueClass = typeof(ClearList))]
		[JsonProperty("BannedPhraseStrings")]
		public IList<BannedPhrase> BannedPhraseStrings { get; set; } = new ObservableCollection<BannedPhrase>();

		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.BotUsers), ResetValueClass = typeof(ClearList))]
		[JsonProperty("BotUsers")]
		public IList<BotUser> BotUsers { get; set; } = new ObservableCollection<BotUser>();

		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.CommandSettings), ResetValueClass = typeof(NoParams<CommandSettings>))]
		[JsonProperty("CommandSettings")]
		public CommandSettings CommandSettings { get; set; } = new CommandSettings();

		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.GuildCulture), DefaultValue = "en-US")]
		[JsonProperty("Culture")]
		public string Culture
		{
			get => _Culture;
			set => ThrowIfElseSet(ref _Culture, value, x => CultureInfo.GetCultureInfo(x) == null, "Invalid culture provided.");
		}

		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.GoodbyeMessage), ResetValueClass = typeof(Null))]
		[JsonProperty("GoodbyeMessage")]
		public GuildNotification? GoodbyeMessage
		{
			get => _GoodbyeMessage;
			set => SetValue(ref _GoodbyeMessage, value);
		}

		/// <inheritdoc />
		[JsonIgnore]
		public ulong GuildId { get; set; }

		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.IgnoredCommandChannels), ResetValueClass = typeof(ClearList))]
		[JsonProperty("IgnoredCommandChannels")]
		public IList<ulong> IgnoredCommandChannels { get; set; } = new ObservableSet<ulong>();

		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.IgnoredXpChannels), ResetValueClass = typeof(ClearList))]
		[JsonProperty("IgnoredXpChannels")]
		public IList<ulong> IgnoredXpChannels { get; set; } = new ObservableSet<ulong>();

		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.ImageOnlyChannels), ResetValueClass = typeof(ClearList))]
		[JsonProperty("ImageOnlyChannels")]
		public IList<ulong> ImageOnlyChannels { get; set; } = new ObservableSet<ulong>();

		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.MuteRole), DefaultValue = 0)]
		[JsonProperty("MuteRole")]
		public ulong MuteRoleId
		{
			get => _MuteRoleId;
			set => SetValue(ref _MuteRoleId, value);
		}

		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.NonVerboseErrors), DefaultValue = false)]
		[JsonProperty("NonVerboseErrors")]
		public bool NonVerboseErrors
		{
			get => _NonVerboseErrors;
			set => SetValue(ref _NonVerboseErrors, value);
		}

		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.PersistentRoles), ResetValueClass = typeof(ClearList))]
		[JsonProperty("PersistentRoles")]
		public IList<PersistentRole> PersistentRoles { get; set; } = new ObservableCollection<PersistentRole>();

		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.Prefix), ResetValueClass = typeof(Null))]
		[JsonProperty("Prefix")]
		public string? Prefix
		{
			get => _Prefix;
			set => SetValue(ref _Prefix, value);
		}

		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.Quotes), ResetValueClass = typeof(ClearList))]
		[JsonProperty("Quotes")]
		public IList<Quote> Quotes { get; set; } = new ObservableCollection<Quote>();

		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.RaidPrevention), ResetValueClass = typeof(ClearList))]
		[JsonProperty("RaidPrevention")]
		public IList<RaidPrev> RaidPrevention { get; set; } = new ObservableCollection<RaidPrev>();

		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.Rules), ResetValueClass = typeof(NoParams<RuleHolder>))]
		[JsonProperty("Rules")]
		public RuleHolder Rules { get; set; } = new RuleHolder();

		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.SelfAssignableGroups), ResetValueClass = typeof(ClearList))]
		[JsonProperty("SelfAssignableGroups")]
		public IList<SelfAssignableRoles> SelfAssignableGroups { get; set; } = new ObservableCollection<SelfAssignableRoles>();

		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.SpamPrevention), ResetValueClass = typeof(ClearList))]
		[JsonProperty("SpamPrevention")]
		public IList<SpamPrev> SpamPrevention { get; set; } = new ObservableCollection<SpamPrev>();

		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.WelcomeMessage), ResetValueClass = typeof(Null))]
		[JsonProperty("WelcomeMessage")]
		public GuildNotification? WelcomeMessage
		{
			get => _WelcomeMessage;
			set => SetValue(ref _WelcomeMessage, value);
		}

		//IDatabaseEntry
		object IDatabaseEntry.Id { get => GuildId; set => GuildId = (ulong)value; }

		/// <inheritdoc />
		public IList<BannedPhraseUserInfo> GetBannedPhraseUsers()
			=> _BannedPhraseUsers;

		/// <inheritdoc />
		public override void Save()
		{
			if (_Parent == null)
			{
				throw new InvalidOperationException("Unable to save due to parent not being set.");
			}
			_Parent.Save(this);
		}

		/// <summary>
		/// Stores the factory so <see cref="Save"/> can be called solely from this object.
		/// </summary>
		/// <param name="parent"></param>
		public void StoreGuildSettingsFactory(GuildSettingsFactory parent)
			=> _Parent = parent;

		/// <inheritdoc />
		protected override string GetLocalizedName(SettingAttribute attr)
			=> GuildSettingNames.ResourceManager.GetStringEnsured(attr.UnlocalizedName);

		/// <summary>
		/// Observable collection but only allows one of each matching item in.
		/// Gotten from https://stackoverflow.com/a/527000.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		private class ObservableSet<T> : ObservableCollection<T>
		{
			/// <inheritdoc />
			protected override void InsertItem(int index, T item)
			{
				if (Contains(item))
				{
					return;
				}

				base.InsertItem(index, item);
			}

			/// <inheritdoc />
			protected override void SetItem(int index, T item)
			{
				var i = IndexOf(item);
				if (i >= 0 && i != index)
				{
					return;
				}
				base.SetItem(index, item);
			}
		}
	}
}