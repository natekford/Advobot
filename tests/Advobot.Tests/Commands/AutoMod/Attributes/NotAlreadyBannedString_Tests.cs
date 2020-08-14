using Advobot.AutoMod.Attributes.ParameterPreconditions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.AutoMod.Attributes
{
	[TestClass]
	public sealed class NotAlreadyBannedString_Tests
		: NotAlreadyBannedPhraseAttribute_Tests<NotAlreadyBannedStringAttribute>
	{
		protected override bool IsName => false;
		protected override bool IsRegex => false;
		protected override bool IsString => true;
	}
}