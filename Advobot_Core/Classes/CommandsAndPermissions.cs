using Advobot.Actions;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Linq;

namespace Advobot.Classes
{
	public class CommandOverride : ISetting
	{
		[JsonProperty]
		public string Name { get; }
		[JsonProperty]
		public ulong Id { get; }

		public CommandOverride(string name, ulong id)
		{
			Name = name;
			Id = id;
		}

		public override string ToString()
		{
			return $"**Command:** `{Name}`\n**ID:** `{Id}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}

	public class CommandSwitch : ISetting
	{
		[JsonProperty]
		public string Name { get; }
		[JsonIgnore]
		public string[] Aliases { get; }
		[JsonProperty]
		public bool Value { get; private set; }
		[JsonIgnore]
		public string ValAsString { get => Value ? "ON" : "OFF"; }
		[JsonIgnore]
		public int ValAsInteger { get => Value ? 1 : -1; }
		[JsonIgnore]
		public bool ValAsBoolean { get => Value; }
		[JsonProperty]
		public CommandCategory Category { get; }
		[JsonIgnore]
		public string CategoryName { get => Category.EnumName(); }
		[JsonIgnore]
		public int CategoryValue { get => (int)Category; }
		[JsonIgnore]
		private HelpEntry _HelpEntry;

		public CommandSwitch(string name, bool value)
		{
			_HelpEntry = Constants.HELP_ENTRIES.FirstOrDefault(x => x.Name.Equals(name));
			if (_HelpEntry == null)
			{
				//TODO: uncomment this when all commands have been put back in
				//throw new ArgumentException("Command name does not have a help entry.");
				return;
			}

			Name = name;
			Value = value;
			Category = _HelpEntry.Category;
			Aliases = _HelpEntry.Aliases;
		}

		public void ToggleEnabled()
		{
			Value = !Value;
		}

		public override string ToString()
		{
			return $"`{ValAsString.PadRight(3)}` `{Name}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}

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
}
