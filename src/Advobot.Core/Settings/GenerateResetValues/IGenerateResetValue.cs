namespace Advobot.Settings.GenerateResetValues
{
	internal interface IGenerateResetValue
	{
		public object? GenerateResetValue(object? currentValue);
	}
}
