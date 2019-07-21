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
		private const int VALUE = 1;

		private int GetNum() => 1;
		private static int GetNum2() => 2;

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
		public void ArrayAccessWithCapturedProperty_Test()
		{
			var num = GetNum();
			var expr = (Expression<Func<EmbedBuilder, string>>)(x => x.Fields[num].Name);
			var path = expr.GetPropertyPath();
			Assert.AreEqual("Fields[num].Name", path);
		}
		[TestMethod]
		public void ArrayAccessWithConstantPlusMethod_Test()
		{
			var expr = (Expression<Func<Nested, int>>)(x => x.Nest["abc".Substring(1)].Value);
			var path = expr.GetPropertyPath();
			Assert.AreEqual("Nest[\"abc\".Substring(1)].Value", path);
		}
		[TestMethod]
		public void ArrayAccessWithDeclaredConstant_Test()
		{
			var expr = (Expression<Func<EmbedBuilder, string>>)(x => x.Fields[VALUE].Name);
			var path = expr.GetPropertyPath();
			Assert.AreEqual("Fields[1].Name", path);
		}
		[TestMethod]
		public void ArrayAccessWithThisPlusMethod_Test()
		{
			var expr = (Expression<Func<EmbedBuilder, string>>)(x => x.Fields[GetNum()].Name);
			var path = expr.GetPropertyPath();
			Assert.AreEqual("Fields[GetNum()].Name", path);
		}
		[TestMethod]
		public void ArrayAccessWithThisPlusStaticMethod_Test()
		{
			var expr = (Expression<Func<EmbedBuilder, string>>)(x => x.Fields[GetNum2()].Name);
			var path = expr.GetPropertyPath();
			//There's no way to tell if the static class is 'this'
			//So the static class has to always be included
			Assert.AreEqual("Fields[GetPropertyPathTests.GetNum2()].Name", path);
		}
		[TestMethod]
		public void ArrayAccessWithGetMethodOnClass_Test()
		{
			var instance = new Nested();
			var expr = (Expression<Func<EmbedBuilder, string>>)(x => x.Fields[instance.GetNum()].Name);
			var path = expr.GetPropertyPath();
			Assert.AreEqual("Fields[instance.GetNum()].Name", path);
		}
		[TestMethod]
		public void ArrayAccessWithGetMethodOnVeryNestedClass_Test()
		{
			var expr = (Expression<Func<EmbedBuilder, string>>)(x => x.Fields[Nested.MegaNested.Create().Value].Name);
			var path = expr.GetPropertyPath();
			Assert.AreEqual("Fields[Nested.MegaNested.Create().Value].Name", path);
		}
		[TestMethod]
		public void ArrayAccessWithCtor_Test()
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
	}

	public class Nested
	{
		public readonly Nested Nest;
		public readonly int Value;

		public int GetNum()
			=> 1;

		public Nested this[Nested nest]
			=> new Nested();
		public Nested this[TestEnum e]
			=> new Nested();
		public Nested this[string val]
			=> new Nested();

		public static Nested Create()
			=> new Nested();

		public sealed class MegaNested : Nested
		{
			public new static MegaNested Create()
				=> new MegaNested();
		}
	}

	public sealed class ClassWithIndexerName
	{
		public readonly ClassWithIndexerName Nest;

		[IndexerName("Dog")]
		public int this[int val]
			=> val;
	}

	public enum TestEnum
	{
		EnumVal,
	}
}
