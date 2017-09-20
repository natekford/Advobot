using Advobot.Classes;
using Advobot.Enums;
using Discord;
using System;
using System.Threading.Tasks;

namespace Advobot.Actions
{
	public static class LogActions
	{
		/// <summary>
		/// Changes the log channel of the passed in type in guild settings to be the passed in channel.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="channelType"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		public static async Task SetChannel(MyCommandContext context, LogChannelType channelType, ITextChannel channel)
		{
			switch (channelType)
			{
				case LogChannelType.Server:
				{
					if (context.GuildSettings.ServerLog?.Id == channel.Id)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(context, $"That channel is already the current {channelType.EnumName().ToLower()} log.");
						return;
					}

					context.GuildSettings.ServerLog = channel;
					break;
				}
				case LogChannelType.Mod:
				{
					if (context.GuildSettings.ModLog?.Id == channel.Id)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(context, $"That channel is already the current {channelType.EnumName().ToLower()} log.");
						return;
					}

					context.GuildSettings.ModLog = channel;
					break;
				}
				case LogChannelType.Image:
				{
					if (context.GuildSettings.ImageLog?.Id == channel.Id)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(context, $"That channel is already the current {channelType.EnumName().ToLower()} log.");
						return;
					}

					context.GuildSettings.ImageLog = channel;
					break;
				}
				default:
				{
					throw new ArgumentException("Invalid channel type supplied.");
				}
			}

			await MessageActions.MakeAndDeleteSecondaryMessage(context, $"Successfully set the {channelType.EnumName().ToLower()} log as `{channel.FormatChannel()}`.");
		}
		/// <summary>
		/// Removes the log channel of the passed in type from the guild settings.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="channelType"></param>
		/// <returns></returns>
		public static async Task RemoveChannel(MyCommandContext context, LogChannelType channelType)
		{
			switch (channelType)
			{
				case LogChannelType.Server:
				{
					if (context.GuildSettings.ServerLog == null)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(context, $"The {channelType.EnumName().ToLower()} log is already off.");
						return;
					}

					context.GuildSettings.ServerLog = null;
					break;
				}
				case LogChannelType.Mod:
				{
					if (context.GuildSettings.ModLog == null)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(context, $"The {channelType.EnumName().ToLower()} log is already off.");
						return;
					}

					context.GuildSettings.ModLog = null;
					break;
				}
				case LogChannelType.Image:
				{
					if (context.GuildSettings.ImageLog == null)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(context, $"The {channelType.EnumName().ToLower()} log is already off.");
						return;
					}

					context.GuildSettings.ImageLog = null;
					break;
				}
				default:
				{
					throw new ArgumentException("Invalid channel type supplied.");
				}
			}

			await MessageActions.MakeAndDeleteSecondaryMessage(context, $"Successfully removed the {channelType.EnumName().ToLower()} log.");
		}
	}
}
