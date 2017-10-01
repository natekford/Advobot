using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Advobot.Classes
{
	/// <summary>
	/// Explains why the bot is doing something.
	/// </summary>
	public class AutomaticModerationReason : ModerationReason
	{
		public AutomaticModerationReason(string reason) : base(null, reason)
		{
			User = null;
			Reason = reason == null ? "not specified" : reason.TrimEnd('.');
			HasReason = !String.IsNullOrWhiteSpace(reason);
			IsAutomatic = true;
		}

		public override string ToString()
		{
			return $"Automatic action. Trigger: {Reason}.";
		}
	}

	/// <summary>
	/// Explains why a mod is doing something.
	/// </summary>
	public class ModerationReason
	{
		public IUser User { get; protected set; }
		public string Reason { get; protected set; }
		public bool HasReason { get; protected set; }
		public bool IsAutomatic { get; protected set; }

		public ModerationReason(IUser user, string reason)
		{
			User = user;
			Reason = reason == null ? "not specified" : reason.TrimEnd('.');
			HasReason = !String.IsNullOrWhiteSpace(reason);
			IsAutomatic = false;
		}

		public RequestOptions CreateRequestOptions()
		{
			return new RequestOptions { AuditLogReason = this.ToString(), };
		}

		public override string ToString()
		{
			return $"Action by {User.FormatUser()}. Reason: {Reason}.";
		}
	}
}
