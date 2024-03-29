﻿using Advobot.Invites.Preconditions;
using Advobot.Invites.Service;
using Advobot.Services.Time;
using Advobot.Tests.Fakes.Services.Time;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Invites.Preconditions;

[TestClass]
public sealed class RequireNotRecentlyBumped_Tests
	: Precondition_Tests<RequireNotRecentlyBumped>
{
	private readonly MutableTime _Time = new();
	protected override RequireNotRecentlyBumped Instance { get; } = new();

	[TestMethod]
	public async Task NoInvite_Test()
	{
		var result = await CheckPermissionsAsync().CAF();
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task NotRecentlyBumped_Test()
	{
		await BumpAsync().CAF();
		_Time.UtcNow += TimeSpan.FromHours(3);

		var result = await CheckPermissionsAsync().CAF();
		Assert.IsTrue(result.IsSuccess);
	}

	[TestMethod]
	public async Task RecentlyBumped_Test()
	{
		await BumpAsync().CAF();

		var result = await CheckPermissionsAsync().CAF();
		Assert.IsFalse(result.IsSuccess);
	}

	protected override void ModifyServices(IServiceCollection services)
	{
		services
			.AddSingleton<ITime>(_Time)
			.AddSingleton<IInviteListService, FakeInviteListService>();
	}

	private async Task BumpAsync()
	{
		var invite = await Context.Channel.CreateInviteAsync().CAF();
		var db = Services.Value.GetRequiredService<IInviteListService>();
		await db.AddInviteAsync(invite).CAF();
	}
}