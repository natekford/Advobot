using System;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Indicates the marked property is a setting modifiable by the user.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	internal sealed class SettingAttribute : Attribute
	{
		/// <summary>
		/// The unlocalized name for this setting.
		/// </summary>
		public string UnlocalizedName { get; }

		/// <summary>
		/// Creates an instance of <see cref="SettingAttribute"/>.
		/// </summary>
		/// <param name="unlocalizedName"></param>
		public SettingAttribute(string unlocalizedName)
		{
			UnlocalizedName = unlocalizedName;
		}
	}
}
