using System;
using System.Threading.Tasks;

using Advobot.Invites.Preconditions;
using Advobot.Invites.Service;
using Advobot.Services.Time;
using Advobot.Tests.Fakes.Services.InviteList;
using Advobot.Tests.Fakes.Services.Time;
using Advobot.Tests.PreconditionTestsBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Invites.Preconditions
{
	[TestClass]
	public sealed class RequireNotRecentlyBumpedAttribute_Tests
		: ParameterlessPreconditions_TestBase<RequireNotRecentlyBumpedAttribute>
	{
		private readonly FakeInviteListService _Invites;
		private readonly MutableTime _Time;

		public RequireNotRecentlyBumpedAttribute_Tests()
		{
			_Time = new MutableTime();
			_Invites = new FakeInviteListService(_Time);

			Services = new ServiceCollection()
				.AddSingleton<ITime>(_Time)
				.AddSingleton<IInviteListService>(_Invites)
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task NoInvite_Test()
		{
			var result = await CheckAsync().CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task NotRecentlyBumped_Test()
		{
			var invite = await Context.Channel.CreateInviteAsync().CAF();
			await _Invites.AddAsync(invite).CAF();

			_Time.UtcNow += TimeSpan.FromHours(3);

			var result = await CheckAsync().CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		[TestMethod]
		public async Task RecentlyBumped_Test()
		{
			var invite = await Context.Channel.CreateInviteAsync().CAF();
			await _Invites.AddAsync(invite).CAF();

			var result = await CheckAsync().CAF();
			Assert.IsFalse(result.IsSuccess);
		}
	}
}