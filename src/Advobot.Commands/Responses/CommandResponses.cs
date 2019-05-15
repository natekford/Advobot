using System;
using Advobot.Classes;
using Advobot.Classes.Results;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;

namespace Advobot.Commands.Responses
{
	public abstract class CommandResponses : AdvobotResult
	{
		protected static readonly IFormatProvider Default = new ArgumentFormatter { UseCode = true, };
		protected static readonly IFormatProvider Title = new ArgumentFormatter { UseTitleCase = true, };
		protected static readonly IFormatProvider BigBlock = new ArgumentFormatter { UseBigCode = true, Joiner = "\n", };
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
			_ => throw new InvalidOperationException("Invalid action."),
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
