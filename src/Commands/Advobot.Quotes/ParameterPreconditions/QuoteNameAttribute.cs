using System;
using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Quotes.Database;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Quotes.ParameterPreconditions
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class QuoteNameAttribute
		: StringRangeParameterPreconditionAttribute, IExistenceParameterPrecondition
	{
		public ExistenceStatus Status => ExistenceStatus.MustNotExist;
		public override string StringType => "quote name";

		public QuoteNameAttribute() : base(1, 100)
		{
		}

		protected override async Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			IGuildUser invoker,
			string value,
			IServiceProvider services)
		{
			var result = await base.SingularCheckPermissionsAsync(context, parameter, invoker, value, services).CAF();
			if (!result.IsSuccess)
			{
				return result;
			}

			var db = services.GetRequiredService<IQuoteDatabase>();
			var quote = await db.GetQuoteAsync(context.Guild.Id, value).CAF();
			var exists = quote != null;
			return this.FromExistence(exists, value, StringType);
		}
	}
}