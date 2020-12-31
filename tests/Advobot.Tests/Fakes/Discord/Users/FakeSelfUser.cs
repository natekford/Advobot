using System;
using System.Threading.Tasks;

using Discord;

namespace Advobot.Tests.Fakes.Discord.Users
{
	public sealed class FakeSelfUser : FakeUser, ISelfUser
	{
		public string Email { get; set; }
		public UserProperties Flags { get; set; }
		public bool IsMfaEnabled { get; set; }
		public bool IsVerified { get; set; }
		public string Locale { get; set; }
		public PremiumType PremiumType { get; set; }

		public Task ModifyAsync(Action<SelfUserProperties> func, RequestOptions? options = null)
			=> throw new NotImplementedException();
	}
}