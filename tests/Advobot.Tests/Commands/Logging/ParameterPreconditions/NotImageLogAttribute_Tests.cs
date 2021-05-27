using System.Threading.Tasks;

using Advobot.Logging.Database;
using Advobot.Logging.Models;
using Advobot.Logging.ParameterPreconditions;
using Advobot.Tests.Fakes.Services.Logging;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Logging.ParameterPreconditions
{
	[TestClass]
	public sealed class NotImageLogAttribute_Tests : ParameterPreconditionTestsBase
	{
		private readonly LogChannels _Channels = new();

		protected override ParameterPreconditionAttribute Instance { get; }
			= new NotImageLogAttribute();

		[TestMethod]
		public async Task LogExisting_Test()
		{
			Context.Channel.Id = _Channels.ImageLogId = 73;

			var result = await CheckPermissionsAsync(Context.Channel).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task LogNotExisting_Test()
		{
			var result = await CheckPermissionsAsync(Context.Channel).CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		protected override void ModifyServices(IServiceCollection services)
		{
			services
				.AddSingleton(_Channels)
				.AddSingleton<ILoggingDatabase, FakeLoggingDatabase>();
		}
	}
}