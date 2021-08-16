using System.Threading.Tasks;

using Advobot.Logging;
using Advobot.Logging.Database;
using Advobot.Logging.ParameterPreconditions;
using Advobot.Tests.Fakes.Services.Logging;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Logging.ParameterPreconditions
{
	[TestClass]
	public sealed class NotModLogAttribute_Tests
		: ParameterPreconditionTestsBase<NotModLogAttribute>
	{
		private readonly FakeLoggingDatabase _Db = new();

		protected override NotModLogAttribute Instance { get; } = new();

		[TestMethod]
		public async Task LogExisting_Test()
		{
			await _Db.UpsertLogChannelAsync(Log.Mod, Context.Guild.Id, Context.Channel.Id).CAF();
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
				.AddSingleton<ILoggingDatabase>(_Db);
		}
	}
}