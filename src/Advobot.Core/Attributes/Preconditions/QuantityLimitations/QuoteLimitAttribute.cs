using System;
using Advobot.Modules;
using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;
using AdvorangesUtils;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.Preconditions.QuantityLimitations
{
	/// <summary>
	/// Requires there to be less than the maximum amount of quotes.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class QuoteLimitAttribute : QuantityLimitAttribute
	{
		/// <inheritdoc />
		public override string QuantityName => nameof(IGuildSettings.Quotes).FormatTitle().ToLower();

		/// <summary>
		/// Creates an instance of <see cref="QuoteLimitAttribute"/>.
		/// </summary>
		/// <param name="action"></param>
		public QuoteLimitAttribute(QuantityLimitAction action) : base(action) { }

		/// <inheritdoc />
		public override int GetCurrent(AdvobotCommandContext context, IServiceProvider services)
			=> context.GuildSettings.Quotes.Count;
		/// <inheritdoc />
		public override int GetMaximumAllowed(AdvobotCommandContext context, IServiceProvider services)
			=> services.GetRequiredService<IBotSettings>().MaxQuotes;
	}
}
