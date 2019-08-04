using System;
using System.Threading.Tasks;
using Advobot.Services.BotSettings;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.ParameterPreconditions.Strings
{
	/// <summary>
	/// Makes sure the passed in string is a valid bot setting name.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class BotSettingNameAttribute
		: StringParameterPreconditionAttribute, IExistenceParameterPrecondition
	{
		/// <inheritdoc />
		public ExistenceStatus Status => ExistenceStatus.MustExist;

		/// <summary>
		/// Creates an instance of <see cref="BotSettingNameAttribute"/>.
		/// </summary>
		public BotSettingNameAttribute() : base(1, int.MaxValue) { }

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

			var settings = services.GetRequiredService<IBotSettings>();
			var exists = settings.GetSettingNames().CaseInsContains(value);
			return this.FromExistence(exists, value, "bot setting name");
		}
		/// <inheritdoc />
		public override string ToString()
			=> $"Valid bot setting name";
	}
}
