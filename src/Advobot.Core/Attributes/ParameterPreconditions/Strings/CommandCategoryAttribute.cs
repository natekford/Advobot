using System;
using System.Threading.Tasks;

using Advobot.Services.HelpEntries;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.ParameterPreconditions.Strings
{
	/// <summary>
	/// Makes sure the passed in string is a valid command category.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class CommandCategoryAttribute
		: StringParameterPreconditionAttribute, IExistenceParameterPrecondition
	{
		/// <inheritdoc />
		public ExistenceStatus Status => ExistenceStatus.MustExist;

		/// <inheritdoc />
		public override string StringType => "command category";

		/// <summary>
		/// Creates an instance of <see cref="BotSettingNameAttribute"/>.
		/// </summary>
		public CommandCategoryAttribute() : base(1, int.MaxValue) { }

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

			var helpEntries = services.GetRequiredService<IHelpEntryService>();
			var exists = helpEntries.GetCategories().CaseInsContains(value);
			return this.FromExistence(exists, value, StringType);
		}
	}
}