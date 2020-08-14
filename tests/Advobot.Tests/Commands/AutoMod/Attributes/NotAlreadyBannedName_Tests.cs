using Advobot.AutoMod.Attributes.ParameterPreconditions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.AutoMod.Attributes
{
	[TestClass]
	public sealed class NotAlreadyBannedName_Tests
		: NotAlreadyBannedPhraseAttribute_Tests<NotAlreadyBannedNameAttribute>
	{
		protected override bool IsName => true;
		protected override bool IsRegex => false;
		protected override bool IsString => false;
	}
}