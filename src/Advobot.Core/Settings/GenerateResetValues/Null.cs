namespace Advobot.Settings.GenerateResetValues
{
	internal sealed class Null : IGenerateResetValue
	{
		public object? GenerateResetValue(object? currentValue)
			=> null;
	}
}
