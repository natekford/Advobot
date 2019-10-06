using System.Threading.Tasks;

using Advobot.Logging.Models;
using Advobot.Logging.Preconditions;
using Advobot.Logging.Service;
using Advobot.Tests.Fakes.Services.Logging;
using Advobot.Tests.PreconditionTestsBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Logging.Preconditions
{
	[TestClass]
	public sealed class RequireModLogAttribute_Tests
		: ParameterlessPreconditions_TestBase<RequireModLogAttribute>
	{
		private readonly LogChannels _Channels;

		public RequireModLogAttribute_Tests()
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
			_Channels.ModLogId = 0.ToString();

			var result = await CheckAsync().CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task HasLog_Test()
		{
			_Channels.ModLogId = 73.ToString();

			var result = await CheckAsync().CAF();
			Assert.IsTrue(result.IsSuccess);
		}
	}
}