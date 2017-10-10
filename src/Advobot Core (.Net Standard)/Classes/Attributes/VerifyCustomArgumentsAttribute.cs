using Discord.Commands;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Limits the arguments in a <see cref="CustomArguments"/> to the set names.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public class VerifyCustomArgumentsAttribute : ParameterPreconditionAttribute
	{
		public ImmutableList<string> SearchTerms { get; }

		public VerifyCustomArgumentsAttribute(params string[] searchTerms)
		{
			SearchTerms = searchTerms.ToImmutableList();
		}

		public override Task<PreconditionResult> CheckPermissions(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			if (value is CustomArguments args)
			{
				args.LimitArgsToTheseNames(SearchTerms);
				return Task.FromResult(PreconditionResult.FromSuccess());
			}
			else
			{
				throw new ArgumentException($"{nameof(value)} must be {nameof(CustomArguments)}.");
			}
		}

		public override string ToString()
		{
			return $"({String.Join("|", SearchTerms)})";
		}
	}
}
