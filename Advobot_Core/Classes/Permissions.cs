using Advobot.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes
{
	public class BotImplementedPermissions : ISetting
	{
		[JsonProperty]
		public ulong UserId { get; }
		[JsonProperty]
		public ulong Permissions { get; private set; }

		public BotImplementedPermissions(ulong userID, ulong permissions)
		{
			UserId = userID;
			Permissions = permissions;
		}

		public void AddPermissions(ulong flags)
		{
			Permissions |= flags;
		}
		public void RemovePermissions(ulong flags)
		{
			Permissions &= ~flags;
		}

		public override string ToString()
		{
			return $"**User:** `{UserId}`\n**Permissions:** `{Permissions}`";
		}
		public string ToString(SocketGuild guild)
		{
			return $"**User:** `{guild.GetUser(UserId).FormatUser()}`\n**Permissions:** `{Permissions}`";
		}
	}

	/// <summary>
	/// Holds a guild permission name and value.
	/// </summary>
	public struct BotGuildPermission : IPermission
	{
		public string Name { get; }
		public ulong Value { get; }

		public BotGuildPermission(string name, int position)
		{
			Name = name;
			Value = (1U << position);
		}
	}

	/// <summary>
	/// Holds a channel permission name and value. Also holds booleans describing whether or not the permissions is on text/voice/both channels.
	/// </summary>
	public struct BotChannelPermission : IPermission
	{
		public string Name { get; }
		public ulong Value { get; }
		public bool General { get; }
		public bool Text { get; }
		public bool Voice { get; }

		public BotChannelPermission(string name, int position, bool gen = false, bool text = false, bool voice = false)
		{
			Name = name;
			Value = (1U << position);
			General = gen;
			Text = text;
			Voice = voice;
		}
	}
}
