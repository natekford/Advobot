using System;
using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;

namespace Advobot.Attributes.Preconditions.QuantityLimits
{
	/// <summary>
	/// Requires specific amounts of items in commands adding or removing banned regex.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class BannedRegexLimitAttribute : GuildSettingLimitAttribute
	{
		/// <inheritdoc />
		public override string QuantityName => "banned regex";

		/// <summary>
		/// Creates an instance of <see cref="QuoteLimitAttribute"/>.
		/// </summary>
		/// <param name="action"></param>
		public BannedRegexLimitAttribute(QuantityLimitAction action) : base(action) { }

		/// <inheritdoc />
		protected override int GetCurrent(IGuildSettings settings)
			=> settings.BannedPhraseRegex.Count;
		/// <inheritdoc />
		protected override int GetMaximumAllowed(IBotSettings settings)
			=> settings.MaxBannedRegex;
	}
}
