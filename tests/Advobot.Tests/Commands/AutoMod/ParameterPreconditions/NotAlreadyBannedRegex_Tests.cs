using Advobot.AutoMod.Attributes.ParameterPreconditions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.AutoMod.ParameterPreconditions
{
	[TestClass]
	public sealed class NotAlreadyBannedRegex_Tests
		: NotAlreadyBannedPhraseAttribute_Tests<NotAlreadyBannedRegexAttribute>
	{
		protected override NotAlreadyBannedRegexAttribute Instance { get; } = new();
		protected override bool IsName => false;
		protected override bool IsRegex => true;
		protected override bool IsString => false;
	}
}