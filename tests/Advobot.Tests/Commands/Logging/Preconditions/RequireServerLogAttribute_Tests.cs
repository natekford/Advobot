using System.Threading.Tasks;

using Advobot.Logging.Models;
using Advobot.Logging.Preconditions;
using Advobot.Logging.Service;
using Advobot.Tests.Fakes.Services.Logging;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Logging.Preconditions
{
	[TestClass]
	public sealed class RequireServerLogAttribute_Tests : PreconditionTestsBase
	{
		private readonly LogChannels _Channels = new LogChannels();

		protected override PreconditionAttribute Instance { get; }
			= new RequireServerLogAttribute();

		[TestMethod]
		public async Task DoesNotHaveLog_Test()
		{
			_Channels.ServerLogId = 0;

			var result = await CheckPermissionsAsync().CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task HasLog_Test()
		{
			_Channels.ServerLogId = 73;

			var result = await CheckPermissionsAsync().CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		protected override void ModifyServices(IServiceCollection services)
		{
			services
				.AddSingleton(_Channels)
				.AddSingleton<ILoggingService, FakeLoggingService>();
		}
	}
}