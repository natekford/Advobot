﻿using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles
{
	[TestClass]
	public sealed class NotManagedAttribute_Tests
		: ParameterPreconditionTestsBase<NotManagedAttribute>
	{
		protected override NotManagedAttribute Instance { get; } = new();

		[TestMethod]
		public async Task RoleIsManaged_Test()
		{
			var result = await CheckPermissionsAsync(new FakeRole(Context.Guild)
			{
				IsManaged = true
			}).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task RoleIsNotManaged_Test()
		{
			var result = await CheckPermissionsAsync(new FakeRole(Context.Guild)
			{
				IsManaged = false
			}).CAF();
			Assert.IsTrue(result.IsSuccess);
		}
	}
}