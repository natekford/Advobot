namespace Advobot.SharedUI
{
	/// <summary>
	/// Targets to put colors on.
	/// This used to be an enum, but that doesn't work well with bindings in Avalonia.
	/// </summary>
	public static class ColorTargets
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public static string BaseBackground => nameof(BaseBackground);
		public static string BaseForeground => nameof(BaseForeground);
		public static string BaseBorder => nameof(BaseBorder);
		public static string ButtonBackground => nameof(ButtonBackground);
		public static string ButtonForeground => nameof(ButtonForeground);
		public static string ButtonBorder => nameof(ButtonBorder);
		public static string ButtonDisabledBackground => nameof(ButtonDisabledBackground);
		public static string ButtonDisabledForeground => nameof(ButtonDisabledForeground);
		public static string ButtonDisabledBorder => nameof(ButtonDisabledBorder);
		public static string ButtonMouseOverBackground => nameof(ButtonMouseOverBackground);
		public static string JsonDigits => nameof(JsonDigits);
		public static string JsonValue => nameof(JsonValue);
		public static string JsonParamName => nameof(JsonParamName);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}