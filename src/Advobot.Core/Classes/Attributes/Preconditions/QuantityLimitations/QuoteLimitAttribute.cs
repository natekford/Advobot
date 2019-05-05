using System;
using Advobot.Classes.Modules;
using Advobot.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Classes.Attributes.Preconditions.QuantityLimitations
{
	/// <summary>
	/// Requires there to be less than the maximum amount of quotes in the 
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class QuoteLimitAttribute : QuantityLimitAttribute
	{
		/// <inheritdoc />
		public override string QuantityName => nameof(IGuildSettings.Quotes).ToLower();

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
