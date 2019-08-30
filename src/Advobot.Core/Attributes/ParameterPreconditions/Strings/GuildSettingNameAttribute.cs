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
	/// Makes sure the passed in string is a valid guild setting name.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class GuildSettingNameAttribute
		: StringParameterPreconditionAttribute, IExistenceParameterPrecondition
	{
		/// <inheritdoc />
		public ExistenceStatus Status => ExistenceStatus.MustExist;

		/// <inheritdoc />
		public override string StringType => "guild setting name";

		/// <summary>
		/// Creates an instance of <see cref="BotSettingNameAttribute"/>.
		/// </summary>
		public GuildSettingNameAttribute() : base(1, int.MaxValue) { }

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
			var exists = settings.GetSettingNames().CaseInsContains(value);
			return this.FromExistence(exists, value, StringType);
		}
	}
}