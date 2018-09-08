namespace Advobot.Interfaces
{
	/// <summary>
	/// Specifies how to get and set a setting.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ISetting<T>
	{
		/// <summary>
		/// The name of the setting.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Sets the setting to its default value.
		/// </summary>
		void Reset();
		/// <summary>
		/// Gets the current value of the setting.
		/// </summary>
		/// <returns></returns>
		T GetValue();
		/// <summary>
		/// Sets the setting to the specified value.
		/// </summary>
		/// <param name="newValue"></param>
		void SetValue(T newValue);
	}

	/// <summary>
	/// Specifies how to get and set a setting.
	/// </summary>
	public interface ISetting
	{
		/// <summary>
		/// The name of the setting.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Sets the setting to its default value.
		/// </summary>
		void Reset();
		/// <summary>
		/// Gets the current value of the setting.
		/// </summary>
		/// <returns></returns>
		object GetValue();
		/// <summary>
		/// Sets the setting to the specified value.
		/// </summary>
		/// <param name="newValue"></param>
		void SetValue(object newValue);
	}
}