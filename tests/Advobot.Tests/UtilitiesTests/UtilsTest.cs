using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Advobot.Core.Utilities;

namespace Advobot.Tests
{
	[TestClass]
	public class UtilsTest
	{
		[TestMethod]
		public void CaseIns_Tests()
		{
			Assert.IsTrue("ABC".CaseInsEquals("abc"));
			Assert.IsTrue("ABC".CaseInsEquals("ABC"));
			Assert.IsFalse("ABC".CaseInsEquals("AB"));

			Assert.IsTrue("ABC".CaseInsContains("a"));
			Assert.IsTrue("ABC".CaseInsContains("A"));
			Assert.IsFalse("ABC".CaseInsContains("Q"));

			Assert.IsTrue("ABC".CaseInsIndexOf("b", out int firstPos));
			Assert.AreEqual(1, firstPos);
			Assert.IsTrue("ABC".CaseInsIndexOf("C", out int secondPos));
			Assert.AreEqual(2, secondPos);
			Assert.IsFalse("ABC".CaseInsIndexOf("Q", out int thirdPos));
			Assert.AreEqual(-1, thirdPos);

			Assert.IsTrue("ABC".CaseInsEquals("abc"));
			Assert.IsTrue("ABC".CaseInsEquals("ABC"));
			Assert.IsFalse("ABC".CaseInsEquals("AB"));

			Assert.IsTrue("ABC".CaseInsEquals("abc"));
			Assert.IsTrue("ABC".CaseInsEquals("ABC"));
			Assert.IsFalse("ABC".CaseInsEquals("AB"));
		}
	}
}
