﻿using Advobot.Logging;
using Advobot.Logging.Database;
using Advobot.Logging.ParameterPreconditions;
using Advobot.Tests.Fakes.Services.Logging;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Logging.ParameterPreconditions;

[TestClass]
public sealed class NotServerLogAttribute_Tests
	: ParameterPreconditionTestsBase<NotServerLogAttribute>
{
	private readonly FakeLoggingDatabase _Db = new();

	protected override NotServerLogAttribute Instance { get; } = new();

	[TestMethod]
	public async Task LogExisting_Test()
	{
		await _Db.UpsertLogChannelAsync(Log.Server, Context.Guild.Id, Context.Channel.Id).CAF();

		await AssertFailureAsync(Context.Channel).CAF();
	}

	[TestMethod]
	public async Task LogNotExisting_Test()
		=> await AssertSuccessAsync(Context.Channel).CAF();

	protected override void ModifyServices(IServiceCollection services)
	{
		services
			.AddSingleton<ILoggingDatabase>(_Db);
	}
}