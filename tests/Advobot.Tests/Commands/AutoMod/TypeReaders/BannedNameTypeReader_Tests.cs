using System.Threading.Tasks;

using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.AutoMod.TypeReaders;
using Advobot.Punishments;
using Advobot.Tests.Commands.AutoMod.Fakes;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders.BannedPhraseTypeReaders
{
	[TestClass]
	public sealed class BannedNameTypeReader_Tests
		: BannedPhraseTypeReader_Tests<BannedNameTypeReader>
	{
		protected override bool IsName => true;
		protected override bool IsRegex => false;
		protected override bool IsString => false;
	}
}