using System;

namespace Advobot.Core.Classes.Attributes
{
	/// <summary>
	/// Indicates a field is a setting.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class SettingAttribute : Attribute
	{
		/// <summary>
		/// The default value of the setting.
		/// </summary>
		public object DefaultValue { get; }
		/// <summary>
		/// A non compile time default value. Used for lists and dictionaries.
		/// </summary>
		public NonCompileTimeDefaultValue NonCompileTimeDefaultValue { get; }

		/// <summary>
		/// A setting with a non compile time default value.
		/// </summary>
		/// <param name="nonCompileTimeDefaultValue"></param>
		public SettingAttribute(NonCompileTimeDefaultValue nonCompileTimeDefaultValue)
		{
			NonCompileTimeDefaultValue = nonCompileTimeDefaultValue;
		}
		/// <summary>
		/// A setting with a default value which is known at compile time.
		/// </summary>
		/// <param name="defaultValue"></param>
		public SettingAttribute(object defaultValue)
		{
			DefaultValue = defaultValue;
		}
	}

	/// <summary>
	/// A default value not known at compile time.
	/// </summary>
	public enum NonCompileTimeDefaultValue
	{
		/// <summary>
		/// There is not a non compile time default value.
		/// </summary>
		None = default,
		/// <summary>
		/// Create a new instance of the same type using a parameterless constructor.
		/// </summary>
		InstantiateDefaultParameterless,
		/// <summary>
		/// Clear all of the values from the dictionary, but keep the keys.
		/// </summary>
		ClearDictionaryValues,
	}
}
