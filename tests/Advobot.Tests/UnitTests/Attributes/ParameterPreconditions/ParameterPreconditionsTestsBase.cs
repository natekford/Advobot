using System;
using System.Threading.Tasks;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Utilities;
using Discord.Commands;

namespace Advobot.Tests.UnitTests.Attributes.ParameterPreconditions
{
	public abstract class ParameterPreconditionsTestsBase<T>
		: AttributeTestsBase<T>
		where T : ParameterPreconditionAttribute, new()
	{
		public FakeCommandContext Context { get; set; } = FakeUtils.CreateContext();
		public ParameterInfo Parameter { get; set; }
		public IServiceProvider Services { get; set; }

		protected Task<PreconditionResult> CheckAsync(object value)
			=> Instance.CheckPermissionsAsync(Context, Parameter, value, Services);
	}
}
