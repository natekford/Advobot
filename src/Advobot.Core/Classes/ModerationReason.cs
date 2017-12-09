using Advobot.Core.Actions.Formatting;
using Discord;
using System;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Explains why the bot is doing something.
	/// </summary>
	public class AutomaticModerationReason : ModerationReason
	{
		public AutomaticModerationReason(string reason) : base(null, reason)
		{
			this.User = null;
			this.Reason = reason == null ? "not specified" : reason.TrimEnd('.');
			this.HasReason = !String.IsNullOrWhiteSpace(reason);
			this.IsAutomatic = true;
		}

		public override string ToString() => $"Automatic action. Trigger: {this.Reason}.";
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
			this.User = user;
			this.Reason = reason == null ? "not specified" : reason.TrimEnd('.');
			this.HasReason = !String.IsNullOrWhiteSpace(reason);
			this.IsAutomatic = false;
		}

		public RequestOptions CreateRequestOptions() => new RequestOptions { AuditLogReason = this.ToString(), };

		public override string ToString() => $"Action by {this.User.FormatUser()}. Reason: {this.Reason}.";
	}
}
