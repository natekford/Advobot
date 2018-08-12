using System;
using Advobot.Enums;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Indicates a property is a setting.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class SettingAttribute : Attribute
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
}
