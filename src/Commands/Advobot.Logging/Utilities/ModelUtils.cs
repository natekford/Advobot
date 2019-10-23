namespace Advobot.Logging.Utilities
{
	public static class ModelUtils
	{
		public static ulong ToId(this string? value)
		{
			if (value == null)
			{
				return 0;
			}
			return ulong.Parse(value);
		}
	}
}