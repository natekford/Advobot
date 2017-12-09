using Discord.WebSocket;
using System;

namespace Advobot.UILauncher.Classes
{
	internal class CompGuild : IComparable, IComparable<SocketGuild>
	{
		private SocketGuild _G;
		public CompGuild(SocketGuild guild)
		{
			this._G = guild;
		}

		public int CompareTo(object obj) => obj is SocketGuild g ? CompareTo(g) : 1;
		public int CompareTo(SocketGuild other)
			=> this._G.MemberCount == other.MemberCount ? this._G.Name.CompareTo(other.Name) : this._G.MemberCount.CompareTo(other.MemberCount);
	}
}
