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
	public sealed class RequireModLogAttribute_Tests : PreconditionTestsBase
	{
		private readonly LogChannels _Channels = new LogChannels();

		protected override PreconditionAttribute Instance { get; }
			= new RequireModLogAttribute();

		[TestMethod]
		public async Task DoesNotHaveLog_Test()
		{
			_Channels.ModLogId = 0;

			var result = await CheckPermissionsAsync().CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task HasLog_Test()
		{
			_Channels.ModLogId = 73;

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