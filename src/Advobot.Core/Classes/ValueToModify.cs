using Advobot.Interfaces;

namespace Advobot.Classes
{
	/// <summary>
	/// How to modify a command.
	/// </summary>
	public class ValueToModify
	{
		/// <summary>
		/// The name of the command to modify.
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// The value to set the command to.
		/// </summary>
		public bool? Value { get; set; }
		/// <summary>
		/// Whether this command can be modified.
		/// </summary>
		public bool CanModify { get; set; }

		/// <summary>
		/// Creates an instance of <see cref="ValueToModify"/>.
		/// </summary>
		/// <param name="help"></param>
		/// <param name="value"></param>
		public ValueToModify(IHelpEntry help, bool value) : this(help.Name, help.AbleToBeToggled, value) { }
		/// <summary>
		/// Creates an instance of <see cref="ValueToModify"/>.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="canModify"></param>
		/// <param name="value"></param>
		public ValueToModify(string name, bool canModify, bool value)
		{
			Name = name;
			CanModify = canModify;
			Value = value;
		}
	}
}