using Advobot.AutoMod.TypeReaders;

using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders.BannedPhraseTypeReaders;

[TestClass]
public sealed class BannedNameTypeReader_Tests : BannedPhraseTypeReader_Tests
{
	protected override TypeReader Instance { get; } = new BannedNameTypeReader();
	protected override bool IsName => true;
	protected override bool IsRegex => false;
	protected override bool IsString => false;
}