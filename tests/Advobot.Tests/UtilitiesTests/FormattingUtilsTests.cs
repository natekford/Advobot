using AdvorangesUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests
{
	[TestClass]
	public class FormattingUtilsTests
	{
		[TestMethod]
		public void JoinNonNullStrings_Test()
		{
			var strings = new[]
			{
				null,
				"dog",
				null,
				"fish",
				null,
				null,
				null,
				"dog"
			};

			Assert.AreEqual("dogfishdog", strings.JoinNonNullStrings(""));
			Assert.AreEqual("dog fish dog", strings.JoinNonNullStrings(" "));
		}
		[TestMethod]
		public void FormatNumberedList_Test()
		{
			var strings = new[]
			{
				"one",
				"two",
			};

			Assert.AreEqual("`1.` one\n`2.` two", strings.FormatNumberedList(x => x));
			Assert.AreEqual("`1.` \n`2.` ", strings.FormatNumberedList(x => ""));
			Assert.AreEqual("`1.` \n`2.` ", strings.FormatNumberedList(x => null));
		}
		[TestMethod]
		public void RemoveDuplicateNewLines_Test()
		{
			Assert.AreEqual("\n", "\r".RemoveDuplicateNewLines());
			Assert.AreEqual("\n", "\n\n\n\n\n\n\n\r\r\r\r\r\r\n".RemoveDuplicateNewLines());
			Assert.AreEqual("dog\ncat\nfish", "dog\n\n\rcat\r\r\nfish".RemoveDuplicateNewLines());
		}
		[TestMethod]
		public void FormatTitle_Test()
		{
			Assert.AreEqual("Testtest", "Testtest".FormatTitle());
			Assert.AreEqual("Test Test", "TestTest".FormatTitle());
			Assert.AreEqual("Test Test", "Test test".FormatTitle());
		}
	}
}