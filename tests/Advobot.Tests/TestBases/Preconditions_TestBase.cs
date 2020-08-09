using System;
using System.Threading.Tasks;

using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Utilities;

using AdvorangesUtils;

using Discord.Commands;

namespace Advobot.Tests.TestBases
{
	public abstract class Preconditions_TestBase<T>
		: Attribute_TestsBase<T>
		where T : PreconditionAttribute
	{
		protected FakeCommandContext Context { get; set; } = FakeUtils.CreateContext();
		protected IServiceProvider? Services { get; set; }

		protected async Task<PreconditionResult> CheckAsync()
		{
			var command = await GetCommandInfoAsync().CAF();
			return await Instance.CheckPermissionsAsync(Context, command, Services).CAF();
		}

		protected virtual Task<CommandInfo?> GetCommandInfoAsync()
			=> Task.FromResult<CommandInfo?>(null);
	}
}