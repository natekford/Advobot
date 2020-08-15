using Advobot.AutoMod.TypeReaders;

using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders.BannedPhraseTypeReaders
{
	[TestClass]
	public sealed class BannedStringTypeReader_Tests : BannedPhraseTypeReader_Tests
	{
		protected override TypeReader Instance { get; } = new BannedStringTypeReader();
		protected override bool IsName => false;
		protected override bool IsRegex => false;
		protected override bool IsString => true;
	}
}