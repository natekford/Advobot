using Advobot.Core.Utilities.Formatting;
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
		[TestMethod]
		public void FormatPlural_Test()
		{
			Assert.AreEqual("", Formatting.FormatPlural(1));
			Assert.AreEqual("s", Formatting.FormatPlural(2));

			Assert.AreEqual("", Formatting.FormatPlural(1U));
			Assert.AreEqual("s", Formatting.FormatPlural(2U));

			Assert.AreEqual("", Formatting.FormatPlural(1UL));
			Assert.AreEqual("s", Formatting.FormatPlural(2UL));

			Assert.AreEqual("s", Formatting.FormatPlural(0.9d));
			Assert.AreEqual("", Formatting.FormatPlural(1.0d));
			Assert.AreEqual("s", Formatting.FormatPlural(2.0d));

			Assert.AreEqual("s", Formatting.FormatPlural(0.9f));
			Assert.AreEqual("", Formatting.FormatPlural(1.0f));
			Assert.AreEqual("s", Formatting.FormatPlural(2.0f));

			Assert.AreEqual("", Formatting.FormatPlural(0b01));
			Assert.AreEqual("s", Formatting.FormatPlural(0b10));
		}
	}
}