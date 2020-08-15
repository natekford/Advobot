using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Services.GuildSettings;
using Advobot.Tests.Fakes.Services.GuildSettings;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Strings
{
	[TestClass]
	public sealed class RuleCategoryAttribute_Tests : ParameterPreconditionTestsBase
	{
		private readonly RuleCategoryAttribute _Instance = new RuleCategoryAttribute();
		private readonly GuildSettings _Settings = new GuildSettings();
		protected override ParameterPreconditionAttribute Instance => _Instance;

		[TestMethod]
		public async Task ErrorOnCategoryExistingFalse_Tests()
		{
			_Instance.Status = ExistenceStatus.MustExist;
			_Settings.Rules.Categories.Add("i exist", new List<string>());

			{
				var result = await CheckPermissionsAsync(_Settings.Rules.Categories.Keys.First()).CAF();
				Assert.IsTrue(result.IsSuccess);
			}

			{
				var result = await CheckPermissionsAsync("i dont exist").CAF();
				Assert.IsFalse(result.IsSuccess);
			}
		}

		[TestMethod]
		public async Task ErrorOnCategoryExistingTrue_Tests()
		{
			_Instance.Status = ExistenceStatus.MustNotExist;
			_Settings.Rules.Categories.Add("i exist", new List<string>());

			{
				var result = await CheckPermissionsAsync(_Settings.Rules.Categories.Keys.First()).CAF();
				Assert.IsFalse(result.IsSuccess);
			}

			{
				var result = await CheckPermissionsAsync("i dont exist").CAF();
				Assert.IsTrue(result.IsSuccess);
			}
		}

		[TestMethod]
		public async Task Standard_Test()
		{
			_Instance.Status = ExistenceStatus.None;

			var expected = new Dictionary<string, bool>
			{
				{ "", false },
				{ new string('a', 1), true },
				{ new string('a', 250), true },
				{ new string('a', 251), false },
			};
			foreach (var kvp in expected)
			{
				var result = await CheckPermissionsAsync(kvp.Key).CAF();
				Assert.AreEqual(kvp.Value, result.IsSuccess);
			}
		}

		protected override void ModifyServices(IServiceCollection services)
		{
			services
				.AddSingleton<IGuildSettingsFactory>(new FakeGuildSettingsFactory(_Settings));
		}
	}
}