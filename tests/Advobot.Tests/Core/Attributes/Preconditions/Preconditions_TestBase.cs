using System;
using System.Threading.Tasks;

using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Utilities;

using Discord.Commands;

namespace Advobot.Tests.Core.Attributes.Preconditions
{
	public abstract class Preconditions_TestBase<T>
		: AttributeTestsBase<T>
		where T : PreconditionAttribute
	{
		public CommandInfo? Command { get; set; }
		public FakeCommandContext Context { get; set; } = FakeUtils.CreateContext();
		public IServiceProvider? Services { get; set; }

		protected Task<PreconditionResult> CheckAsync()
			=> Instance.CheckPermissionsAsync(Context, Command, Services);
	}
}