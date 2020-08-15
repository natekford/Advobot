using Advobot.AutoMod.Attributes.ParameterPreconditions;

using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.AutoMod.Attributes
{
	[TestClass]
	public sealed class NotAlreadyBannedName_Tests : NotAlreadyBannedPhraseAttribute_Tests
	{
		protected override ParameterPreconditionAttribute Instance { get; }
			= new NotAlreadyBannedNameAttribute();
		protected override bool IsName => true;
		protected override bool IsRegex => false;
		protected override bool IsString => false;
	}
}