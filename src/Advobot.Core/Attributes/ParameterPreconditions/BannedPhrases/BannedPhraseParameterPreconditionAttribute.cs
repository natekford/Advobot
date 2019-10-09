using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.ParameterPreconditions.BannedPhrases
{
	/// <summary>
	/// Makes sure the passed in <see cref="string"/> is not already a banned phrase.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class BannedPhraseParameterPreconditionAttribute
		: AdvobotParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string Summary => $"Not already a banned {BannedPhraseName}";

		/// <summary>
		/// Gets the name of the banned phrase type.
		/// </summary>
		protected abstract string BannedPhraseName { get; }

		/// <summary>
		/// Gets the phrases this should look through.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected abstract IEnumerable<BannedPhrase> GetPhrases(IGuildSettings settings);

		/// <inheritdoc />
		protected override async Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services)
		{
			if (!(value is string regex))
			{
				return this.FromOnlySupports(typeof(string));
			}

			var settingsFactory = services.GetRequiredService<IGuildSettingsFactory>();
			var settings = await settingsFactory.GetOrCreateAsync(context.Guild).CAF();
			if (!GetPhrases(settings).Select(x => x.Phrase).Contains(regex))
			{
				return PreconditionResult.FromSuccess();
			}
			return PreconditionResult.FromError($"`{regex}` is already a banned regex.");
		}
	}
}