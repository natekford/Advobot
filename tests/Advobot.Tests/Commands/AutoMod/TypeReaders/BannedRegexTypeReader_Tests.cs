using Advobot.AutoMod.TypeReaders;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders.BannedPhraseTypeReaders;

[TestClass]
public sealed class BannedRegexTypeReader_Tests
	: BannedPhraseTypeReader_Tests<BannedRegexTypeReader>
{
	protected override BannedRegexTypeReader Instance { get; } = new();
	protected override bool IsName => false;
	protected override bool IsRegex => true;
	protected override bool IsString => false;
}