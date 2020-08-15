using Advobot.AutoMod.Attributes.ParameterPreconditions;

using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.AutoMod.Attributes
{
	[TestClass]
	public sealed class NotAlreadyBannedString_Tests : NotAlreadyBannedPhraseAttribute_Tests
	{
		protected override ParameterPreconditionAttribute Instance { get; } = new NotAlreadyBannedStringAttribute();
		protected override bool IsName => false;
		protected override bool IsRegex => false;
		protected override bool IsString => true;
	}
}