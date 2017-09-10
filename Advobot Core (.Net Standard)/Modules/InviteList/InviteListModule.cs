using Advobot.Actions;
using Advobot.Classes;
using Advobot.Interfaces;
using Discord;
using System.Collections.Generic;

namespace Advobot.Modules.InviteList
{
	public sealed class MyInviteListModule : IInviteListModule
	{
		private List<ListedInvite> _ListedInvites;
		public List<ListedInvite> ListedInvites => _ListedInvites ?? (_ListedInvites = new List<ListedInvite>());

		public void AddInvite(ListedInvite invite)
		{
			ListedInvites.ThreadSafeAdd(invite);
		}
		public void RemoveInvite(ListedInvite invite)
		{
			ListedInvites.ThreadSafeRemove(invite);
		}
		public void RemoveInvite(IGuild guild)
		{
			ListedInvites.ThreadSafeRemoveAll(x => x.Guild.Id == guild.Id);
		}
		public void BumpInvite(ListedInvite invite)
		{
			RemoveInvite(invite);
			AddInvite(invite);
			invite.UpdateLastBumped();
		}
	}
}
