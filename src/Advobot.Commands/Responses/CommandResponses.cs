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
		protected static readonly IFormatProvider BigBlock = new ArgumentFormatter { UseBigCode = true, };
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
		protected static string GetAction(PermValue action) => action switch
		{
			PermValue.Allow => "allowed",
			PermValue.Inherit => "inherited",
			PermValue.Deny => "denied",
			_ => throw new InvalidOperationException("Invalid action."),
		};
		protected static string GetEnabled(bool enabled)
			=> enabled ? "enabled" : "disabled";
		protected static string GetIgnored(bool ignored)
			=> ignored ? "ignored" : "unignored";
		protected static string GetAdded(bool added)
			=> added ? "added" : "removed";
		protected static string GetAllowed(bool allowed)
			=> allowed ? "allowed" : "denied";
	}
}
