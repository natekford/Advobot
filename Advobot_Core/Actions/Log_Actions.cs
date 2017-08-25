using Advobot.Enums;
using Advobot.NonSavedClasses;
using Discord;
using System;
using System.Threading.Tasks;

namespace Advobot
{
	namespace Actions
	{
		public static class LogActions
		{
			public static async Task SetChannel(MyCommandContext context, LogChannelType channelType, ITextChannel channel)
			{
				switch (channelType)
				{
					case LogChannelType.Server:
					{
						if (context.GuildSettings.ServerLog?.Id == channel.Id)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(context, $"That channel is already the current {0} log.", channelType.EnumName().ToLower()));
							return;
						}

						context.GuildSettings.ServerLog = channel;
						break;
					}
					case LogChannelType.Mod:
					{
						if (context.GuildSettings.ModLog?.Id == channel.Id)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(context, $"That channel is already the current {0} log.", channelType.EnumName().ToLower()));
							return;
						}

						context.GuildSettings.ModLog = channel;
						break;
					}
					case LogChannelType.Image:
					{
						if (context.GuildSettings.ImageLog?.Id == channel.Id)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(context, $"That channel is already the current {0} log.", channelType.EnumName().ToLower()));
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

				await MessageActions.MakeAndDeleteSecondaryMessage(context, $"Successfully set the {0} log as `{1}`.", channelType.EnumName().ToLower(), channel.FormatChannel()));
			}
			public static async Task RemoveChannel(MyCommandContext context, LogChannelType channelType)
			{
				switch (channelType)
				{
					case LogChannelType.Server:
					{
						if (context.GuildSettings.ServerLog == null)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(context, $"The {0} log is already off.", channelType.EnumName().ToLower()));
							return;
						}

						context.GuildSettings.ServerLog = null;
						break;
					}
					case LogChannelType.Mod:
					{
						if (context.GuildSettings.ModLog == null)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(context, $"The {0} log is already off.", channelType.EnumName().ToLower()));
							return;
						}

						context.GuildSettings.ModLog = null;
						break;
					}
					case LogChannelType.Image:
					{
						if (context.GuildSettings.ImageLog == null)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(context, $"The {0} log is already off.", channelType.EnumName().ToLower()));
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

				await MessageActions.MakeAndDeleteSecondaryMessage(context, $"Successfully removed the {0} log.", channelType.EnumName().ToLower()));
			}
		}
	}
}
