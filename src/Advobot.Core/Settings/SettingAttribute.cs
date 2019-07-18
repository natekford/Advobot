using Advobot.Settings.GenerateResetValues;
using System;
using System.Linq;

namespace Advobot.Settings
{
	/// <summary>
	/// Indicates the marked property is a setting modifiable by the user.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	internal sealed class SettingAttribute : Attribute
	{
		/// <summary>
		/// The default value to use for this setting.
		/// If this is set, then this will be used over <see cref="ResetValueClass"/>.
		/// Use <see cref="Null"/> over setting this as null in order to have a reset value of null.
		/// </summary>
		public object? DefaultValue { get; set; }
		/// <summary>
		/// A class specifying how to reset the value this is applied to.
		/// The class must implement <see cref="IGenerateResetValue"/> and have a parameterless constructor.
		/// </summary>
		public Type? ResetValueClass
		{
			get => _ResetValueClass;
			set
			{
				if (value != null)
				{
					if (!value.GetInterfaces().Contains(typeof(IGenerateResetValue)))
					{
						throw new InvalidOperationException($"Does not implement {nameof(IGenerateResetValue)}.");
					}
					if (value.GetConstructor(Type.EmptyTypes) == null)
					{
						throw new InvalidOperationException($"Does not have a parameterless constructor.");
					}
				}
				_ResetValueClass = value;
			}
		}
		private Type? _ResetValueClass;
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
