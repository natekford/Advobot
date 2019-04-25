using System;
using Advobot.Classes.Modules;
using Advobot.Classes.Results;
using Discord;
using Discord.Commands;

namespace Advobot.Commands.Responses
{
	public sealed partial class ResponsesFor : AdvobotResult
	{
		private ResponsesFor() : base(null, "") { }

		public abstract class CommandResponses
		{
			protected static readonly IFormatProvider Default = new ArgumentFormatter { UseCode = true, };
            protected static readonly IFormatProvider Title = new ArgumentFormatter { UseTitleCase = true, };
			protected static readonly IFormatProvider BigBlock = new ArgumentFormatter { UseBigCode = true, };

			protected static TimeSpan Time(int seconds)
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
}
