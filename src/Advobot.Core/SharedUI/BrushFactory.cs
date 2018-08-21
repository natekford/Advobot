namespace Advobot.SharedUI
{
	/// <summary>
	/// Specifies how to create a brush.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class BrushFactory<T>
	{
		/// <summary>
		/// Returns the default value of this brush type.
		/// </summary>
		public T Default => CreateBrush("#FF000000");

		/// <summary>
		/// Creates a brush from the input.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public T CreateBrush(string input)
		{
			return CreateBrush(SharedUIUtils.ParseColorBytes(input));
		}
		/// <summary>
		/// Attempts to create a brush from the input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="brush"></param>
		/// <returns></returns>
		public bool TryCreateBrush(string input, out T brush)
		{
			var success = SharedUIUtils.TryParseColorBytes(input, out var bytes);
			brush = success ? CreateBrush(bytes) : default;
			return success;
		}
		/// <summary>
		/// Creates a brush from ARGB.
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		protected abstract T CreateBrush(byte[] bytes);
		/// <summary>
		/// Gets the brush's ARGB bytes.
		/// </summary>
		/// <param name="brush"></param>
		/// <returns></returns>
		protected abstract byte[] GetBrushBytes(T brush);
		/// <summary>
		/// Returns the hex string representation of the brush.
		/// </summary>
		/// <param name="brush"></param>
		/// <returns></returns>
		public string FormatBrush(T brush)
		{
			var bytes = GetBrushBytes(brush);
			return $"#{bytes[0]:X2}{bytes[1]:X2}{bytes[2]:X2}{bytes[3]:X2}";
		}
	}
}