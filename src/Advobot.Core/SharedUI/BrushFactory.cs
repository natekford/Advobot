namespace Advobot.SharedUI
{
	/// <summary>
	/// Specifies how to create a brush.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class BrushFactory<T>
	{
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
	}
}