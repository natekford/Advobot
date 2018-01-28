using Advobot.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Advobot.Tests
{
	[TestClass]
    public class EnumUtilsTests
    {
		[Flags]
		public enum TestEnum : ulong
		{
			None = 0,
			A = 1,
			B = 2,
			C = A | B, //3
			D = 4,
			E = 8,
			F = C | D, //12
			All = E | F, //15
		}

		[TestMethod]
		public void GetFlags_Test()
		{
			Assert.AreEqual(1, EnumUtils.GetFlagNames(TestEnum.All, true).Count());
			Assert.AreEqual(8, EnumUtils.GetFlagNames(TestEnum.All, false).Count());
			try
			{
				EnumUtils.GetFlagNames(0).ToList();
				Assert.Fail("Should have thrown an argument exception.");
			}
			catch (ArgumentException) { }

			Assert.AreEqual(1, EnumUtils.GetFlags(TestEnum.All, true).Count());
			Assert.AreEqual(8, EnumUtils.GetFlags(TestEnum.All, false).Count());
			try
			{
				EnumUtils.GetFlags(0).ToList();
				Assert.Fail("Should have thrown an argument exception.");
			}
			catch (ArgumentException) { }
		}
		[TestMethod]
		public void TryParse_Test()
		{
			var input = new[]
			{
				nameof(TestEnum.None),
				"Dog",
				nameof(TestEnum.A),
				nameof(TestEnum.E),
				"Fish",
				"Cat",
			};

			{
				Assert.IsFalse(EnumUtils.TryParseMultiple<TestEnum>(input, out var validInput, out var invalidInput));
				Assert.AreEqual(3, validInput.Count);
				Assert.AreEqual(3, invalidInput.Count);

				try
				{
					EnumUtils.TryParseMultiple<int>(input, out var throwArayValid, out var throwAwayInvalid);
					Assert.Fail("Should have thrown an argument exception.");
				}
				catch (ArgumentException) { }
			}

			{
				Assert.IsFalse(EnumUtils.TryParseFlags(input, out TestEnum value, out var invalidInput));
				Assert.AreEqual(TestEnum.None | TestEnum.A | TestEnum.E, value);
				Assert.AreEqual(3, invalidInput.Count);

				try
				{
					EnumUtils.TryParseFlags(input, out int throwArayValid, out var throwAwayInvalid);
					Assert.Fail("Should have thrown an argument exception.");
				}
				catch (ArgumentException) { }
			}
		}
	}
}