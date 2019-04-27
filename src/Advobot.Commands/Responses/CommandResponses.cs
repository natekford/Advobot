using System;
using Advobot.Classes.Results;
using Discord;

namespace Advobot.Commands.Responses
{
	public abstract class CommandResponses : AdvobotResult
	{
		protected static readonly IFormatProvider Default = new ArgumentFormatter { UseCode = true, };
		protected static readonly IFormatProvider Title = new ArgumentFormatter { UseTitleCase = true, };
		protected static readonly IFormatProvider BigBlock = new ArgumentFormatter { UseBigCode = true, };

		protected CommandResponses() : base(null, "") { }

		protected static TimeSpan CreateTime(int seconds)
			=> TimeSpan.FromSeconds(seconds);
		protected static string GetAction(PermValue action) => action switch
		{
			PermValue.Allow => "allowed",
			PermValue.Inherit => "inherited",
			PermValue.Deny => "denied",
			_ => throw new InvalidOperationException("Invalid action."),
		};
		protected static string GetEnabled(bool add)
			=> add ? "enabled" : "disabled";
	}
}
