using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Services.GuildSettings;
using Advobot.Tests.Fakes.Services.GuildSettings;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Strings
{
	[TestClass]
	public sealed class RuleCategoryAttribute_Tests
		: ParameterPreconditionsTestsBase<RuleCategoryAttribute>
	{
		private const string _ExistingCategory = "i exist";
		private const string _NonExistentCategory = "i dont exist";

		private readonly IGuildSettings _Settings;

		public RuleCategoryAttribute_Tests()
		{
			_Settings = new GuildSettings();

			Services = new ServiceCollection()
				.AddSingleton<IGuildSettingsFactory>(new FakeGuildSettingsFactory(_Settings))
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task ErrorOnCategoryExistingFalse_Tests()
		{
			Instance.Status = ExistenceStatus.MustExist;
			_Settings.Rules.Categories.Add(_ExistingCategory, new List<string>());

			{
				var result = await CheckAsync(_ExistingCategory).CAF();
				Assert.AreEqual(true, result.IsSuccess);
			}

			{
				var result = await CheckAsync(_NonExistentCategory).CAF();
				Assert.AreEqual(false, result.IsSuccess);
			}
		}

		[TestMethod]
		public async Task ErrorOnCategoryExistingTrue_Tests()
		{
			Instance.Status = ExistenceStatus.MustNotExist;
			_Settings.Rules.Categories.Add(_ExistingCategory, new List<string>());

			{
				var result = await CheckAsync(_ExistingCategory).CAF();
				Assert.AreEqual(false, result.IsSuccess);
			}

			{
				var result = await CheckAsync(_NonExistentCategory).CAF();
				Assert.AreEqual(true, result.IsSuccess);
			}
		}

		[TestMethod]
		public async Task FailsOnNotString_Test()
			=> await AssertPreconditionFailsOnInvalidType(CheckAsync(1)).CAF();

		[TestMethod]
		public async Task Standard_Test()
		{
			Instance.Status = ExistenceStatus.None;

			var expected = new Dictionary<string, bool>
			{
				{ "", false },
				{ new string('a', 1), true },
				{ new string('a', 250), true },
				{ new string('a', 251), false },
			};
			foreach (var kvp in expected)
			{
				var result = await CheckAsync(kvp.Key).CAF();
				Assert.AreEqual(kvp.Value, result.IsSuccess);
			}
		}
	}
}