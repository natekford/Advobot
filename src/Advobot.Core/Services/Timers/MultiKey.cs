namespace Advobot.Core.Services.Timers
{
	/// <summary>
	/// A way to store more than one bit of identifying information for a dictionary key.
	/// </summary>
	/// <typeparam name="TFirst"></typeparam>
	/// <typeparam name="TSecond"></typeparam>
	internal struct MultiKey<TFirst, TSecond>
	{
		public readonly TFirst FirstValue;
		public readonly TSecond SecondValue;

		public MultiKey(TFirst firstValue, TSecond secondValue)
		{
			FirstValue = firstValue;
			SecondValue = secondValue;
		}

		public override bool Equals(object obj)
		{
			return obj is MultiKey<TFirst, TSecond> key && key.FirstValue.Equals(FirstValue) && key.SecondValue.Equals(SecondValue);
		}
		public override int GetHashCode()
		{
			//Source: https://stackoverflow.com/a/263416
			unchecked // Overflow is fine, just wrap
			{
				var hash = (int)2166136261;
				// Suitable nullity checks etc, of course :)
				hash = (hash * 16777619) ^ FirstValue.GetHashCode();
				hash = (hash * 16777619) ^ SecondValue.GetHashCode();
				return hash;
			}
		}

		public static bool operator ==(MultiKey<TFirst, TSecond> left, MultiKey<TFirst, TSecond> right)
		{
			return left.Equals(right);
		}
		public static bool operator !=(MultiKey<TFirst, TSecond> left, MultiKey<TFirst, TSecond> right)
		{
			return !left.Equals(right);
		}
	}
}
