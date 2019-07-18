namespace Advobot.Settings.GenerateResetValues
{
	internal sealed class NoParams<T> : IGenerateResetValue where T : new()
	{
		public object? GenerateResetValue(object currentValue)
			=> new T();
	}
}