using Advobot.Classes.Attributes;
using Advobot.Utilities;
using Discord;
using Discord.Commands;
using System;
using System.Linq;

namespace Advobot.Classes
{
	/// <summary>
	/// Shorter way to write the used modulebase and also has every command go through the <see cref="CommandRequirementAttribute"/> first.
	/// </summary>
	[CommandRequirement]
	[RequireContext(ContextType.Guild)]
	public abstract class AdvobotModuleBase : ModuleBase<AdvobotCommandContext>
	{
		/// <summary>
		/// Gets a request options that mainly is used for the reason in the audit log.
		/// </summary>
		/// <param name="reason"></param>
		/// <returns></returns>
		public RequestOptions GetRequestOptions(string reason = "")
		{
			return String.IsNullOrWhiteSpace(reason)
				? ClientUtils.CreateRequestOptions($"Action by {Context.User.Format()}.")
				: ClientUtils.CreateRequestOptions($"Action by {Context.User.Format()}. Reason: {reason}.");
		}
		/// <inheritdoc />
		protected override void AfterExecute(CommandInfo command)
		{
			var attributes = command.Attributes.Concat(command.Module.Attributes);
			if (attributes.Any(x => x.GetType() == typeof(SaveGuildSettingsAttribute)))
			{
				Context.GuildSettings.SaveSettings(Context.Config);
			}
			if (attributes.Any(x => x.GetType() == typeof(SaveBotSettingsAttribute)))
			{
				Context.BotSettings.SaveSettings(Context.Config);
			}
			if (attributes.Any(x => x.GetType() == typeof(SaveLowLevelConfigAttribute)))
			{
				Context.Config.SaveSettings();
			}
			base.AfterExecute(command);
		}
	}
}