using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Advobot.Classes;
using Advobot.Classes.ImageResizing;
using Advobot.Classes.Settings;
using Advobot.Classes.TypeReaders;
using Advobot.Enums;
using Advobot.Interfaces;
using AdvorangesSettingParser.Implementation;
using AdvorangesSettingParser.Implementation.Static;
using AdvorangesSettingParser.Interfaces;
using AdvorangesSettingParser.Results;
using AdvorangesSettingParser.Utils;
using Discord;
using ImageMagick;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Utilities
{
	/// <summary>
	/// Random utilities.
	/// </summary>
	public static class AdvobotUtils
	{
		/// <summary>
		/// Gets the file inside the bot directory.
		/// </summary>
		/// <param name="accessor"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static FileInfo GetBaseBotDirectoryFile(this IBotDirectoryAccessor accessor, string fileName)
			=> new FileInfo(Path.Combine(accessor.BaseBotDirectory.FullName, fileName));
		/// <summary>
		/// Gets the path of the object which implements both <see cref="IBotDirectoryAccessor"/> and <see cref="ISettingsBase"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static FileInfo GetFile<T>(this T obj) where T : IBotDirectoryAccessor, ISettingsBase
			=> obj.GetFile(obj);
		/// <summary>
		/// Saves the settings of the object which implements both <see cref="IBotDirectoryAccessor"/> and <see cref="ISettingsBase"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		public static void SaveSettings<T>(this T obj) where T : IBotDirectoryAccessor, ISettingsBase
			=> obj.SaveSettings(obj);
		/// <summary>
		/// Creates a provider and initializes all of its singletons.
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceProvider CreateProvider(this IServiceCollection services)
		{
			var provider = services.BuildServiceProvider();
			foreach (var service in services.Where(x => x.Lifetime == ServiceLifetime.Singleton))
			{
				provider.GetRequiredService(service.ServiceType);
			}
			return provider;
		}
		/// <summary>
		/// Joins the strings together after selecting them.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="seperator"></param>
		/// <param name="selector"></param>
		/// <returns></returns>
		public static string Join<T>(this IEnumerable<T> source, string seperator, Func<T, string> selector)
			=> string.Join(seperator, source.Select(selector));
		/// <summary>
		/// Joins the strings together with the seperator.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="seperator"></param>
		/// <returns></returns>
		public static string Join(this IEnumerable<string> source, string seperator)
			=> string.Join(seperator, source);
		/// <summary>
		/// Converts the enum a lowercase string.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="e"></param>
		/// <returns></returns>
		public static string ToLower<T>(this T e) where T : Enum
			=> e.ToString().ToLower();
		/// <summary>
		/// Attempts to get the first matching value. Will return default if no matches are found.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="predicate"></param>
		/// <param name="found"></param>
		/// <returns></returns>
		public static bool TryGetFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T found)
		{
			found = default;
			foreach (var item in source)
			{
				if (predicate(item))
				{
					found = item;
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// Attempts to get a single matching value. Will throw if more than one match is found. Will return default if no matches are found.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="predicate"></param>
		/// <param name="found"></param>
		/// <returns></returns>
		public static bool TryGetSingle<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T found)
		{
			found = default;
			var matches = 0;
			foreach (var item in source)
			{
				if (predicate(item))
				{
					if (matches > 0)
					{
						throw new InvalidOperationException("More than one match found.");
					}
					found = item;
					++matches;
				}
			}
			return matches == 1;
		}
		/// <summary>
		/// Registers the settings which need to be registered for the bot to work correctly.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<Type> RegisterStaticSettingParsers()
		{
			TryParserRegistry.Instance.Register<Uri>(TryParseUtils.TryParseUri);
			TryParserRegistry.Instance.RegisterNullable<Color>(ColorTypeReader.TryParseColor);
			TryParserRegistry.Instance.RegisterNullable<Percentage>(TryParseImageMagickPercentage);
			new StaticSettingParser<CustomField>
			{
				new StaticSetting<CustomField, string>(x => x.Name),
				new StaticSetting<CustomField, string>(x => x.Text),
				new StaticSetting<CustomField, bool>(x => x.Inline) { IsOptional = true, IsFlag = true },
			}.Register();
			new StaticSettingParser<CustomEmbed>
			{
				new StaticSetting<CustomEmbed, string>(x => x.Title) { IsOptional = true },
				new StaticSetting<CustomEmbed, string>(x => x.Description) { IsOptional = true },
				new StaticSetting<CustomEmbed, Uri>(x => x.ImageUrl) { IsOptional = true },
				new StaticSetting<CustomEmbed, Uri>(x => x.Url) { IsOptional = true },
				new StaticSetting<CustomEmbed, Uri>(x => x.ThumbUrl) { IsOptional = true },
				new StaticSetting<CustomEmbed, Color?>(x => x.Color) { IsOptional = true },
				new StaticSetting<CustomEmbed, string>(x => x.AuthorName) { IsOptional = true },
				new StaticSetting<CustomEmbed, Uri>(x => x.AuthorIconUrl) { IsOptional = true },
				new StaticSetting<CustomEmbed, Uri>(x => x.AuthorUrl) { IsOptional = true },
				new StaticSetting<CustomEmbed, string>(x => x.Footer) { IsOptional = true },
				new StaticSetting<CustomEmbed, Uri>(x => x.FooterIconUrl) { IsOptional = true },
				new StaticCollectionSetting<CustomEmbed, CustomField>(x => x.FieldInfo) { IsOptional = true },
			}.Register();
			new StaticSettingParser<GuildNotification>
			{
				new StaticSetting<GuildNotification, string>(x => x.Content),
				new StaticSetting<GuildNotification, CustomEmbed>(x => x.CustomEmbed) { IsOptional = true },
			}.Register();
			new StaticSettingParser<RuleFormatter>
			{
				new StaticSetting<RuleFormatter, char>(x => x.CharAfterNumbers) { IsOptional = true },
				new StaticSetting<RuleFormatter, RuleFormat>(x => x.RuleFormat) { IsOptional = true },
				new StaticCollectionSetting<RuleFormatter, MarkDownFormat>(x => x.TitleMarkDownFormat),
				new StaticCollectionSetting<RuleFormatter, MarkDownFormat>(x => x.RuleMarkDownFormat),
				new StaticCollectionSetting<RuleFormatter, RuleFormatOption>(x => x.Options),
			}.Register();
			new StaticSettingParser<NumberSearch>
			{
				new StaticSetting<NumberSearch, uint?>(x => x.Number),
				new StaticSetting<NumberSearch, CountTarget>(x => x.Method),
			}.Register();
			new StaticSettingParser<ListedInviteGatherer>
			{
				new StaticSetting<ListedInviteGatherer, string>(x => x.Code) { IsOptional = true },
				new StaticSetting<ListedInviteGatherer, string>(x => x.Name) { IsOptional = true },
				new StaticSetting<ListedInviteGatherer, bool>(x => x.HasGlobalEmotes) { IsOptional = true, IsFlag = true },
				new StaticSetting<ListedInviteGatherer, NumberSearch>(x => x.Users) {IsOptional = true},
				new StaticCollectionSetting<ListedInviteGatherer, string>(x => x.Keywords),
			}.Register();
			new StaticSettingParser<LocalInviteGatherer>
			{
				new StaticSetting<LocalInviteGatherer, ulong?>(x => x.UserId) { IsOptional = true },
				new StaticSetting<LocalInviteGatherer, ulong?>(x => x.ChannelId) { IsOptional = true },
				new StaticSetting<LocalInviteGatherer, NumberSearch>(x => x.Uses) { IsOptional = true },
				new StaticSetting<LocalInviteGatherer, NumberSearch>(x => x.Age) { IsOptional = true },
				new StaticSetting<LocalInviteGatherer, bool?>(x => x.IsTemporary) { IsOptional = true },
				new StaticSetting<LocalInviteGatherer, bool?>(x => x.NeverExpires) { IsOptional = true },
				new StaticSetting<LocalInviteGatherer, bool?>(x => x.NoMaxUses) { IsOptional = true },
			}.Register();
			new StaticSettingParser<UserProvidedImageArgs>
			{
				new StaticSetting<UserProvidedImageArgs, int>(x => x.ResizeTries) { IsOptional = true },
				new StaticSetting<UserProvidedImageArgs, Percentage>(x => x.ColorFuzzing) { IsOptional = true },
				new StaticSetting<UserProvidedImageArgs, double>(x => x.StartInSeconds) { IsOptional = true },
				new StaticSetting<UserProvidedImageArgs, double>(x => x.LengthInSeconds) { IsOptional = true },
			}.Register();
			new StaticSettingParser<SpamPrev>
			{
				new StaticSetting<SpamPrev, SpamType>(x => x.Type),
				new StaticSetting<SpamPrev, Punishment>(x => x.Punishment),
				new StaticSetting<SpamPrev, int>(x => x.SpamInstances) { Validation = x => IsError(nameof(SpamPrev.SpamInstances), x, 1, 25) },
				new StaticSetting<SpamPrev, int>(x => x.VotesForKick) { Validation = x => IsError(nameof(SpamPrev.VotesForKick), x, 1, 50) },
				new StaticSetting<SpamPrev, int>(x => x.SpamPerMessage) { Validation = x => IsError(nameof(SpamPrev.SpamPerMessage), x, 1, 2000) },
				new StaticSetting<SpamPrev, int>(x => x.TimeInterval) { Validation = x => IsError(nameof(SpamPrev.TimeInterval), x, 1, 180) },
			}.Register();
			new StaticSettingParser<RaidPrev>
			{
				new StaticSetting<RaidPrev, RaidType>(x => x.Type),
				new StaticSetting<RaidPrev, Punishment>(x => x.Punishment),
				new StaticSetting<RaidPrev, int>(x => x.UserCount) { Validation = x => IsError(nameof(RaidPrev.UserCount), x, 1, 25) },
				new StaticSetting<RaidPrev, int>(x => x.TimeInterval) { Validation = x => IsError(nameof(RaidPrev.TimeInterval), x, 1, 60) },
			}.Register();
			new StaticSettingParser<PersistentRole>
			{
				new StaticSetting<PersistentRole, ulong>(x => x.UserId),
				new StaticSetting<PersistentRole, ulong>(x => x.RoleId),
			}.Register();
			new StaticSettingParser<Slowmode>
			{
				new StaticSetting<Slowmode, int>(x => x.BaseMessages),
				new StaticSetting<Slowmode, int>(x => x.TimeInterval),
				new StaticCollectionSetting<Slowmode, ulong>(x => x.ImmuneRoleIds),
			}.Register();
			new StaticSettingParser<BotUser>
			{
				new StaticSetting<BotUser, ulong>(x => x.UserId),
				new StaticSetting<BotUser, ulong>(x => x.Permissions),
			}.Register();
			new StaticSettingParser<SelfAssignableRoles>
			{
				new StaticSetting<SelfAssignableRoles, int>(x => x.Group),
				new StaticCollectionSetting<SelfAssignableRoles, ulong>(x => x.Roles),
			}.Register();
			new StaticSettingParser<Quote>
			{
				new StaticSetting<Quote, string>(x => x.Name),
				new StaticSetting<Quote, string>(x => x.Description),
			}.Register();
			new StaticSettingParser<BannedPhrase>
			{
				new StaticSetting<BannedPhrase, string>(x => x.Phrase),
				new StaticSetting<BannedPhrase, Punishment>(x => x.Punishment),
			}.Register();
			new StaticSettingParser<BannedPhrasePunishment>
			{
				new StaticSetting<BannedPhrasePunishment, Punishment>(x => x.Punishment),
				new StaticSetting<BannedPhrasePunishment, ulong>(x => x.RoleId),
				new StaticSetting<BannedPhrasePunishment, int>(x => x.NumberOfRemoves),
				new StaticSetting<BannedPhrasePunishment, int>(x => x.Time),
			}.Register();
			return StaticSettingParserRegistry.Instance.RegisteredTypes;
		}
		private static IResult IsError(string name, int inputValue, int minValue, int maxValue)
		{
			if (inputValue > maxValue)
			{
				return Result.FromError($"The {name} must be less than or equal to `{maxValue}`.");
			}
			else if (inputValue < minValue)
			{
				return Result.FromError($"The {name} must be greater than or equal to `{minValue}`.");
			}
			return Result.FromSuccess("Successfully validated.");
		}
		private static bool TryParseImageMagickPercentage(string s, out Percentage result)
		{
			var success = double.TryParse(s, out var val);
			result = success ? new Percentage(val) : default;
			return success;
		}
	}
}