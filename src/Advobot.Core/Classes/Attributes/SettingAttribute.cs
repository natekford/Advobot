using System;

namespace Advobot.Core.Classes.Attributes
{
	[AttributeUsage(AttributeTargets.Field)]
	public class SettingAttribute : Attribute
	{
		public object DefaultValue { get; }
		public NonCompileTimeDefaultValue NonCompileTimeDefaultValue { get; }
		public bool NonCompileTime { get; }

		public SettingAttribute(NonCompileTimeDefaultValue nonCompileTimeDefaultValue)
		{
			NonCompileTimeDefaultValue = nonCompileTimeDefaultValue;
			NonCompileTime = true;
		}
		public SettingAttribute(object defaultValue)
		{
			DefaultValue = defaultValue;
			NonCompileTime = false;
		}
	}

	public enum NonCompileTimeDefaultValue
	{
		InstantiateDefaultParameterless,
		ClearDictionaryValues,
	}
}
