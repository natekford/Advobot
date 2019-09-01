using System;
using System.Threading.Tasks;

using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Utilities;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions
{
	public abstract class ParameterPreconditions_TestsBase<T>
		: AttributeTestsBase<T>
		where T : ParameterPreconditionAttribute
	{
		public FakeCommandContext Context { get; set; } = FakeUtils.CreateContext();
		public ParameterInfo? Parameter { get; set; }
		public IServiceProvider? Services { get; set; }

		protected async Task AssertPreconditionFailsOnInvalidType(Task<PreconditionResult> task)
		{
			var result = await task.CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		protected Task<PreconditionResult> CheckAsync(object value)
			=> Instance.CheckPermissionsAsync(Context, Parameter, value, Services);
	}
}