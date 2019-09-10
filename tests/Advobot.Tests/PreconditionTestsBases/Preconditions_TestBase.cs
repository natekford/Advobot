using System;
using System.Threading.Tasks;

using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Utilities;

using AdvorangesUtils;

using Discord.Commands;

namespace Advobot.Tests.PreconditionTestsBases
{
	public abstract class Preconditions_TestBase<T>
		: AttributeTestsBase<T>
		where T : PreconditionAttribute
	{
		public FakeCommandContext Context { get; set; } = FakeUtils.CreateContext();
		public IServiceProvider? Services { get; set; }

		protected async Task<PreconditionResult> CheckAsync()
		{
			var command = await GetCommandInfoAsync().CAF();
			return await Instance.CheckPermissionsAsync(Context, command, Services).CAF();
		}

		protected virtual Task<CommandInfo?> GetCommandInfoAsync()
			=> Task.FromResult<CommandInfo?>(null);
	}
}