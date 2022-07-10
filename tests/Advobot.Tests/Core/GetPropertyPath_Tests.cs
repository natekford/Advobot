using Discord;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq.Expressions;
using System.Runtime.CompilerServices;

using Advobot.Embeds;

namespace Advobot.Tests.Core;

public enum TestEnum
{
	EnumVal,
}

public sealed class ClassWithIndexerName
{
	public readonly ClassWithIndexerName Nest = new();

	[IndexerName("Dog")]
	public int this[int val]
		=> val;
}

[TestClass]
public sealed class GetPropertyPath_Tests
{
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
	public void ArrayAccessWithGetMethodOnVeryNestedClass_Test()
	{
		var expr = (Expression<Func<EmbedBuilder, string>>)(x => x.Fields[Nested.MegaNested.Create().Value].Name);
		var path = expr.GetPropertyPath();
		Assert.AreEqual("Fields[Nested.MegaNested.Create().Value].Name", path);
	}

	[TestMethod]
	public void ArrayAccessWithRenamedIndexer_Test()
	{
		var expr = (Expression<Func<ClassWithIndexerName, int>>)(x => x.Nest[2]);
		var path = expr.GetPropertyPath();
		Assert.AreEqual("Nest[2]", path);
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
		Assert.AreEqual("Fields[GetPropertyPath_Tests.GetNum2()].Name", path);
	}

	[TestMethod]
	public void Basic_Test()
	{
		var expr = (Expression<Func<EmbedFieldBuilder, string>>)(x => x.Name);
		var path = expr.GetPropertyPath();
		Assert.AreEqual("Name", path);
	}

	[TestMethod]
	public void Captured_Test()
	{
		var captured = new EmbedWrapper();
		var expr = (Expression<Func<string?>>)(() => captured.Description);
		var path = expr.GetPropertyPath();
		Assert.AreEqual("captured.Description", path);
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

	private static int GetNum2() => 2;

	private int GetNum() => 1;
}

public class Nested
{
	public readonly Nested Nest = new();
	public readonly int Value;

	public Nested this[Nested _]
		=> new();

	public Nested this[TestEnum _]
		=> new();

	public Nested this[string _]
		=> new();

	public static Nested Create()
		=> new();

	public int GetNum()
		=> 1;

	public sealed class MegaNested : Nested
	{
		public new static MegaNested Create()
			=> new();
	}
}