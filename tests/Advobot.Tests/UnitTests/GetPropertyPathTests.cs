using Advobot.Classes;
using Discord;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace Advobot.Tests.UnitTests
{
	[TestClass]
	public sealed class GetPropertyPathTests
	{
		private int GetNum() => 1;

		[TestMethod]
		public void Captured_Test()
		{
			var captured = new EmbedWrapper();
			var expr = (Expression<Func<string>>)(() => captured.Description);
			var path = expr.GetPropertyPath();
			Assert.AreEqual("captured.Description", path);
		}
		[TestMethod]
		public void Basic_Test()
		{
			var expr = (Expression<Func<EmbedFieldBuilder, string>>)(x => x.Name);
			var path = expr.GetPropertyPath();
			Assert.AreEqual("Name", path);
		}
		[TestMethod]
		public void Cast_Test()
		{
			var expr = (Expression<Func<EmbedFieldBuilder, string>>)(x => (string)x.Value);
			var path = expr.GetPropertyPath();
			Assert.AreEqual("Value", path);
		}
		[TestMethod]
		public void Concat_Test()
		{
			var expr = (Expression<Func<EmbedFieldBuilder, string>>)(x => x.Name + (string)x.Value);
			var path = expr.GetPropertyPath();
			Assert.AreEqual("Name + Value", path);
		}
		[TestMethod]
		public void Nested_Test()
		{
			var expr = (Expression<Func<Nested, int>>)(x => x.Nest.Nest.Nest.Nest.Nest.Value);
			var path = expr.GetPropertyPath();
			Assert.AreEqual("Nest.Nest.Nest.Nest.Nest.Value", path);
		}
		[TestMethod]
		public void ArrayAccess_Test()
		{
			var expr = (Expression<Func<EmbedBuilder, string>>)(x => x.Fields[1].Name);
			var path = expr.GetPropertyPath();
			Assert.AreEqual("Fields[1].Name", path);
		}
		[TestMethod]
		public void ArrayAccessWithProperty_Test()
		{
			var num = GetNum();
			var expr = (Expression<Func<EmbedBuilder, string>>)(x => x.Fields[num].Name);
			var path = expr.GetPropertyPath();
			Assert.AreEqual("Fields[num].Name", path);
		}
		[TestMethod]
		public void ArrayAccessWithClass_Test()
		{
			var expr = (Expression<Func<Nested, int>>)(x => x.Nest[new Nested()].Value);
			var path = expr.GetPropertyPath();
			Assert.AreEqual("Nest[new Nested()].Value", path);
		}
		[TestMethod]
		public void ArrayAccessWithEnum_Test()
		{
			var expr = (Expression<Func<Nested, int>>)(x => x.Nest[TestEnum.EnumVal].Value);
			var path = expr.GetPropertyPath();
			Assert.AreEqual("Nest[EnumVal].Value", path);
		}
		[TestMethod]
		public void ArrayAccessWithRenamedIndexer_Test()
		{
			var expr = (Expression<Func<ClassWithIndexerName, int>>)(x => x.Nest[2]);
			var path = expr.GetPropertyPath();
			Assert.AreEqual("Nest[2]", path);
		}

		private sealed class Nested
		{
			public readonly Nested Nest;
			public readonly int Value;

			public Nested this[Nested nest]
				=> new Nested();
			public Nested this[TestEnum e]
				=> new Nested();
		}

		private sealed class ClassWithIndexerName
		{
			public readonly ClassWithIndexerName Nest;

			[IndexerName("Dog")]
			public int this[int val]
				=> val;
		}

		private enum TestEnum
		{
			EnumVal,
		}
	}
}
