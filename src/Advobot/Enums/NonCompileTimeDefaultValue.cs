namespace Advobot.Enums
{
	/// <summary>
	/// A default value not known at compile time.
	/// </summary>
	public enum NonCompileTimeDefaultValue
	{
		/// <summary>
		/// There is not a non compile time default value.
		/// </summary>
		None,
		/// <summary>
		/// Create a new instance of the same type using a parameterless constructor.
		/// </summary>
		Default,
		/// <summary>
		/// Clear all of the values from the dictionary, but keep the keys.
		/// </summary>
		ClearDictionaryValues,
		/// <summary>
		/// Removes every entry from the list.
		/// </summary>
		ClearList,
	}
}