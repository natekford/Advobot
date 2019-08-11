using System;
using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;

namespace Advobot.Attributes.Preconditions.QuantityLimits
{
	/// <summary>
	/// Requires specific amounts of items in commands adding or removing banned strings.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class BannedStringsLimitAttribute : GuildSettingLimitAttribute
	{
		/// <inheritdoc />
		public override string QuantityName => "banned string";

		/// <summary>
		/// Creates an instance of <see cref="QuoteLimitAttribute"/>.
		/// </summary>
		/// <param name="action"></param>
		public BannedStringsLimitAttribute(QuantityLimitAction action) : base(action) { }

		/// <inheritdoc />
		protected override int GetCurrent(IGuildSettings settings)
			=> settings.BannedPhraseStrings.Count;
		/// <inheritdoc />
		protected override int GetMaximumAllowed(IBotSettings settings)
			=> settings.MaxBannedStrings;
	}
}
