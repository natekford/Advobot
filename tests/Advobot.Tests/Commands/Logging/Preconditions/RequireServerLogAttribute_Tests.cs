﻿using System.Threading.Tasks;

using Advobot.Logging.Models;
using Advobot.Logging.Preconditions;
using Advobot.Logging.Service;
using Advobot.Tests.Fakes.Services.Logging;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Logging.Preconditions
{
	[TestClass]
	public sealed class RequireServerLogAttribute_Tests
		: ParameterlessPreconditions_TestBase<RequireServerLogAttribute>
	{
		private readonly LogChannels _Channels;

		public RequireServerLogAttribute_Tests()
		{
			_Channels = new LogChannels();

			Services = new ServiceCollection()
				.AddSingleton(_Channels)
				.AddSingleton<ILoggingService, FakeLoggingService>()
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task DoesNotHaveLog_Test()
		{
			_Channels.ServerLogId = 0;

			var result = await CheckAsync().CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task HasLog_Test()
		{
			_Channels.ServerLogId = 73;

			var result = await CheckAsync().CAF();
			Assert.IsTrue(result.IsSuccess);
		}
	}
}