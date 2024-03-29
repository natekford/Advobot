﻿using Advobot.Preconditions;
using Advobot.Services.BotSettings;
using Advobot.Tests.Fakes.Services.BotSettings;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Preconditions;

[TestClass]
public sealed class RequireAllowedToDmBotOwner_Tests
	: Precondition_Tests<RequireAllowedToDmBotOwner>
{
	private readonly FakeBotSettings _BotSettings = new();
	protected override RequireAllowedToDmBotOwner Instance { get; } = new();

	[TestMethod]
	public async Task CanDmBotOwner_Test()
	{
		var result = await CheckPermissionsAsync().CAF();
		Assert.IsTrue(result.IsSuccess);
	}

	[TestMethod]
	public async Task CannotDmBotOwner_Test()
	{
		_BotSettings.UsersUnableToDmOwner.Add(Context.User.Id);

		var result = await CheckPermissionsAsync().CAF();
		Assert.IsFalse(result.IsSuccess);
	}

	protected override void ModifyServices(IServiceCollection services)
	{
		services
			.AddSingleton<IBotSettings>(_BotSettings);
	}
}