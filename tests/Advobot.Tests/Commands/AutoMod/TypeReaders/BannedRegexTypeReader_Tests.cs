using Advobot.AutoMod.TypeReaders;

using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders.BannedPhraseTypeReaders
{
	[TestClass]
	public sealed class BannedRegexTypeReader_Tests : BannedPhraseTypeReader_Tests
	{
		protected override TypeReader Instance { get; } = new BannedRegexTypeReader();
		protected override bool IsName => false;
		protected override bool IsRegex => true;
		protected override bool IsString => false;
	}
}