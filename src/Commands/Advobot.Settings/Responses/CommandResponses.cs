using System;
using System.Collections.Generic;
using Advobot.Classes;
using Advobot.Formatting;
using Advobot.Modules;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;

namespace Advobot.Settings.Responses
{
	public abstract class CommandResponses : AdvobotResult
	{
		protected const string CODE = ArgumentFormattingUtils.CODE;
		protected const string BIG_CODE = ArgumentFormattingUtils.BIG_CODE;

		protected static readonly IFormatProvider Default = new ArgumentFormatter
		{
			Formats = new List<FormatApplier>
			{
				new FormatApplier(true, ArgumentFormattingUtils.CODE, s => $"`{s}`"),
				new FormatApplier(false, ArgumentFormattingUtils.BIG_CODE, s => $"```{s}```"),
				new FormatApplier(false, ArgumentFormattingUtils.BOLD, s => $"**{s}**"),
				new FormatApplier(false, ArgumentFormattingUtils.ITALICS, s => $"_{s}_"),
				new FormatApplier(false, ArgumentFormattingUtils.UNDERLINE, s => $"__{s}__"),
				new FormatApplier(false, ArgumentFormattingUtils.STRIKETHROUGH, s => $"~~{s}~~"),
			},
		};
		protected static readonly IFormatProvider Markdown = new ArgumentFormatter
		{
			Formats = new List<FormatApplier>
			{
				new FormatApplier(false, ArgumentFormattingUtils.CODE, s => $"`{s}`"),
				new FormatApplier(false, ArgumentFormattingUtils.BIG_CODE, s => $"```{s}```"),
				new FormatApplier(false, ArgumentFormattingUtils.BOLD, s => $"**{s}**"),
				new FormatApplier(false, ArgumentFormattingUtils.ITALICS, s => $"_{s}_"),
				new FormatApplier(false, ArgumentFormattingUtils.UNDERLINE, s => $"__{s}__"),
				new FormatApplier(false, ArgumentFormattingUtils.STRIKETHROUGH, s => $"~~{s}~~"),
			},
		};
		protected static readonly IFormatProvider Title = new ArgumentFormatter
		{
			Formats = new List<FormatApplier>
			{
				new FormatApplier(true, "title", s => s.FormatTitle()),
			},
		};
		protected static readonly IFormatProvider BigBlock = new ArgumentFormatter
		{
			Joiner = "\n",
			Formats = new List<FormatApplier>
			{
				new FormatApplier(true, ArgumentFormattingUtils.BIG_CODE, s => $"```{s}```"),
			},
		};
		protected static readonly TimeSpan DefaultTime = CreateTime(5);

		protected CommandResponses() : base(null, "") { }

		public static AdvobotResult DisplayEnumValues<T>() where T : Enum
		{
			return Success(new EmbedWrapper
			{
				Title = typeof(T).Name.FormatTitle(),
				Description = Default.FormatInterpolated($"{Enum.GetNames(typeof(T))}"),
			});
		}
		protected static TimeSpan CreateTime(int seconds)
			=> TimeSpan.FromSeconds(seconds);
		protected static RuntimeFormattedObject GetAction(PermValue action) => action switch
		{
			PermValue.Allow => "allowed".NoFormatting(),
			PermValue.Inherit => "inherited".NoFormatting(),
			PermValue.Deny => "denied".NoFormatting(),
			_ => throw new ArgumentOutOfRangeException(nameof(action)),
		};
		protected static RuntimeFormattedObject GetEnabled(bool enabled)
			=> (enabled ? "enabled" : "disabled").NoFormatting();
		protected static RuntimeFormattedObject GetIgnored(bool ignored)
			=> (ignored ? "ignored" : "unignored").NoFormatting();
		protected static RuntimeFormattedObject GetAdded(bool added)
			=> (added ? "added" : "removed").NoFormatting();
		protected static RuntimeFormattedObject GetAllowed(bool allowed)
			=> (allowed ? "allowed" : "denied").NoFormatting();
		protected static RuntimeFormattedObject GetHoisted(bool hoisted)
			=> (hoisted ? "hoisted" : "unhoisted").NoFormatting();
		protected static RuntimeFormattedObject GetMentionability(bool mentionability)
			=> (mentionability ? "mentionable" : "unmentionable").NoFormatting();
		protected static RuntimeFormattedObject GetCreated(bool created)
			=> (created ? "created" : "deleted").NoFormatting();
	}
}
