using System.Collections;

namespace Advobot.Settings.GenerateResetValues
{
	internal sealed class ClearList : IGenerateResetValue
	{
		public object? GenerateResetValue(object? currentValue)
		{
			((IList?)currentValue)?.Clear();
			return currentValue;
		}
	}
}