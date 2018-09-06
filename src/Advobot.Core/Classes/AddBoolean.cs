using System.Collections.Generic;
using AdvorangesUtils;

namespace Advobot.Classes
{
	/// <summary>
	/// Easy way to allow other words than 'true' and 'false' to have boolean values when considering user input.
	/// </summary>
	/// <remarks>
	/// This was made mainly because when I would modify a list setting, instead of typing 'true' for when I intended
	/// to add to the list, I would usually write 'add' by accident.
	/// </remarks>
	public struct AddBoolean
	{
		/// <summary>
		/// The value of the boolean.
		/// </summary>
		public readonly bool Value;

		private static readonly IEnumerable<string> TrueVals = new[] { "true", "add", "enable" };
		private static readonly IEnumerable<string> FalseVals = new[] { "false", "remove", "disable" };

		private AddBoolean(bool value)
		{
			Value = value;
		}

		/// <summary>
		/// Attempts to match the input to a word indicating 'true' or 'false'.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="value"></param>
		/// <param name="trueVals">Default values are 'true' and 'add'</param>
		/// <param name="falseVals">Default values are 'false' and 'remove'</param>
		/// <returns></returns>
		public static bool TryCreate(string input, out AddBoolean value, IEnumerable<string> trueVals = null, IEnumerable<string> falseVals = null)
		{
			switch (input)
			{
				//Implicit conversion to bool lets me do this stupid return stuff
				case string add when (trueVals ?? TrueVals).CaseInsContains(add):
					return (value = new AddBoolean(true)) || true;
				case string remove when (falseVals ?? FalseVals).CaseInsContains(remove):
					return (value = new AddBoolean(false)) || true;
				default:
					return (value = default) && false;
			}
		}

		/// <summary>
		/// Returns the instance's value.
		/// </summary>
		/// <param name="instance"></param>
		public static implicit operator bool(AddBoolean instance)
		{
			return instance.Value;
		}
	}
}