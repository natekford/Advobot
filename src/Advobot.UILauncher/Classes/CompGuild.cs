using Discord.WebSocket;
using System;

namespace Advobot.UILauncher.Classes
{
	internal class CompGuild : IComparable, IComparable<SocketGuild>
	{
		private SocketGuild _G;
		public CompGuild(SocketGuild guild)
		{
			_G = guild;
		}

		public int CompareTo(object obj)
		{
			return obj is SocketGuild g ? CompareTo(g) : 1;
		}
		public int CompareTo(SocketGuild other)
		{
			return _G.MemberCount == other.MemberCount ? _G.Name.CompareTo(other.Name) : _G.MemberCount.CompareTo(other.MemberCount);
		}
	}
}
