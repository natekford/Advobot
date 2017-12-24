using Discord;

namespace Advobot.Core.Utilities.Formatting
{
	/// <summary>
	/// Formatting for various Discord objects.
	/// </summary>
	public static class DiscordObjectFormatting
    {
		/// <summary>
		/// Returns a string that better describes the object than its ToString() method.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string FormatDiscordObject(object obj)
		{
			if (obj is IUser user)
			{
				return user.FormatUser();
			}
			else if (obj is IChannel channel)
			{
				return channel.FormatChannel();
			}
			else if (obj is IRole role)
			{
				return role.FormatRole();
			}
			else if (obj is IGuild guild)
			{
				return guild.FormatGuild();
			}
			else
			{
				return obj.ToString();
			}
		}
		/// <summary>
		/// Returns a string with the user's name, discriminator, and id.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static string FormatUser(this IUser user)
			=> user != null
			? $"'{user.Username.EscapeBackTicks().CaseInsReplace("discord.gg", Constants.FAKE_DISCORD_LINK)}#{user.Discriminator}' ({user.Id})"
			: "Irretrievable User";
		/// <summary>
		/// Returns a string with the role's name and id.
		/// </summary>
		/// <param name="role"></param>
		/// <returns></returns>
		public static string FormatRole(this IRole role)
			=> role != null
			? $"'{role.Name.EscapeBackTicks()}' ({role.Id})"
			: "Irretrievable Role";
		/// <summary>
		/// Returns a string with the channel's name and id.
		/// </summary>
		/// <param name="channel"></param>
		/// <returns></returns>
		public static string FormatChannel(this IChannel channel)
			=> channel != null
			? $"'{channel.Name.EscapeBackTicks()}' ({(channel is IMessageChannel ? "text" : "voice")}) ({channel.Id})"
			: "Irretrievable Channel";
		/// <summary>
		/// Returns a string with the guild's name and id.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public static string FormatGuild(this IGuild guild)
			=> guild != null
			? $"'{guild.Name.EscapeBackTicks()}' ({guild.Id})"
			: "Irretrievable Guild";
		/// <summary>
		/// Replaces everyone/here mentions with a non pinging version and removes \tts.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		public static string FormatMessageContentForNotBeingAnnoying(IGuild guild, string content)
			=> content
			.CaseInsReplace(guild.EveryoneRole.Mention, Constants.FAKE_EVERYONE) //Everyone and Here have the same role.
			.CaseInsReplace("@everyone", Constants.FAKE_EVERYONE)
			.CaseInsReplace("@here", Constants.FAKE_HERE)
			.CaseInsReplace("\tts", Constants.FAKE_TTS);
		/// <summary>
		/// Returns the game's name or stream name/url.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static string FormatGame(IUser user)
		{
			if (user.Activity is StreamingGame sg)
			{
				switch (sg.StreamType)
				{
					case StreamType.Twitch:
					{
						return $"**Current Stream:** [{sg.Name.EscapeBackTicks()}]({sg.Url})";
					}
				}
			}
			else if (user.Activity is Game g)
			{
				return $"**Current Game:** `{g.Name.EscapeBackTicks()}`";
			}

			return "**Current Game:** `N/A`";
		}
	}
}
