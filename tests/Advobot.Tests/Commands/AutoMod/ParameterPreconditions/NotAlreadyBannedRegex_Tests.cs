using Advobot.AutoMod.Attributes.ParameterPreconditions;

using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.AutoMod.ParameterPreconditions
{
	[TestClass]
	public sealed class NotAlreadyBannedRegex_Tests : NotAlreadyBannedPhraseAttribute_Tests
	{
		protected override ParameterPreconditionAttribute Instance { get; } = new NotAlreadyBannedRegexAttribute();
		protected override bool IsName => false;
		protected override bool IsRegex => true;
		protected override bool IsString => false;
	}
}