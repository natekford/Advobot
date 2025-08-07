using Advobot.ParameterPreconditions;
using Advobot.ParameterPreconditions.Strings;
using Advobot.Quotes.Database;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Quotes.ParameterPreconditions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class QuoteName
	: StringLengthParameterPrecondition, IExistenceParameterPrecondition
{
	public ExistenceStatus Status => ExistenceStatus.MustNotExist;
	public override string StringType => "quote name";

	public QuoteName() : base(1, 100)
	{
	}

	protected override async Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		ParameterInfo parameter,
		IGuildUser invoker,
		string value,
		IServiceProvider services)
	{
		var result = await base.CheckPermissionsAsync(context, parameter, invoker, value, services).CAF();
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