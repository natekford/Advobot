using System;
using System.Threading.Tasks;
using Advobot.Services.GuildSettings;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.ParameterPreconditions.Strings
{
	/// <summary>
	/// Validates the rule category by making sure it is between 1 and 250 characters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class RuleCategoryAttribute
		: StringParameterPreconditionAttribute, IExistenceParameterPrecondition
	{
		/// <inheritdoc />
		public override string StringType => "rule category name";
		/// <inheritdoc />
		public ExistenceStatus Status { get; set; } = ExistenceStatus.MustExist;

		/// <summary>
		/// Creates an instance of <see cref="RuleCategoryAttribute"/>.
		/// </summary>
		public RuleCategoryAttribute() : base(1, 250) { }

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
			var exists = settings.Rules?.Categories?.Keys?.CaseInsContains(value) ?? false;
			return this.FromExistence(exists, value, StringType);
		}
	}
}
