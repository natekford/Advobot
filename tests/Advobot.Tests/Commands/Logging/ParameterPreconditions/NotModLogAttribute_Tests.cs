using System.Threading.Tasks;

using Advobot.Logging.Models;
using Advobot.Logging.ParameterPreconditions;
using Advobot.Logging.Service;
using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Fakes.Services.Logging;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Logging.ParameterPreconditions
{
	[TestClass]
	public sealed class NotModLogAttribute_Tests
		: ParameterlessParameterPreconditions_TestsBase<NotModLogAttribute>
	{
		private const ulong CHANNEL_ID = 1;
		private readonly LogChannels _Channels;
		private readonly ITextChannel _FakeChannel;

		public NotModLogAttribute_Tests()
		{
			_Channels = new LogChannels();
			_FakeChannel = new FakeTextChannel(Context.Guild)
			{
				Id = CHANNEL_ID
			};

			Services = new ServiceCollection()
				.AddSingleton(_Channels)
				.AddSingleton<ILoggingService, FakeLoggingService>()
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task FailsOnNotUlong_Test()
			=> await AssertPreconditionFailsOnInvalidType(CheckAsync("")).CAF();

		[TestMethod]
		public async Task LogExisting_Test()
		{
			_Channels.ModLogId = CHANNEL_ID.ToString();

			var result = await CheckAsync(_FakeChannel).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task LogNotExisting_Test()
		{
			var result = await CheckAsync(_FakeChannel).CAF();
			Assert.IsTrue(result.IsSuccess);
		}
	}
}