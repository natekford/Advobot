using System;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Services.GuildSettings;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.ParameterPreconditions.Strings
{
	/// <summary>
	/// Makes sure the passed in string is not already being used for a quote name.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class QuoteNameAttribute
		: StringParameterPreconditionAttribute, IExistenceParameterPrecondition
	{
		/// <inheritdoc />
		public ExistenceStatus Status => ExistenceStatus.MustNotExist;

		/// <inheritdoc />
		public override string StringType => "quote name";

		/// <summary>
		/// Creates an instance of <see cref="QuoteNameAttribute"/>.
		/// </summary>
		public QuoteNameAttribute() : base(1, 100) { }

		/// <inheritdoc />
		protected override async Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			string value,
			IServiceProvider services)
		{
			var result = await base.SingularCheckPermissionsAsync(context, parameter, value, services).CAF();
			if (!result.IsSuccess)
			{
				return result;
			}

			var settingsFactory = services.GetRequiredService<IGuildSettingsFactory>();
			var settings = await settingsFactory.GetOrCreateAsync(context.Guild).CAF();
			var exists = settings.Quotes.Any(x => x.Name.CaseInsEquals(value));
			return this.FromExistence(exists, value, StringType);
		}
	}
}