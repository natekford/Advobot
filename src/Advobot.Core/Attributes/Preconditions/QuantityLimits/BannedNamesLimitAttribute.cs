using System;
using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;

namespace Advobot.Attributes.Preconditions.QuantityLimits
{
	/// <summary>
	/// Requires specific amounts of items in commands adding or removing banned names.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class BannedNamesLimitAttribute : GuildSettingLimitAttribute
	{
		/// <inheritdoc />
		public override string QuantityName => "banned name";

		/// <summary>
		/// Creates an instance of <see cref="QuoteLimitAttribute"/>.
		/// </summary>
		/// <param name="action"></param>
		public BannedNamesLimitAttribute(QuantityLimitAction action) : base(action) { }

		/// <inheritdoc />
		protected override int GetCurrent(IGuildSettings settings)
			=> settings.BannedPhraseNames.Count;
		/// <inheritdoc />
		protected override int GetMaximumAllowed(IBotSettings settings)
			=> settings.MaxBannedNames;
	}
}
