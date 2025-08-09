namespace Advobot.Interactivity;

/// <summary>
/// A result for interactivity.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class InteractiveResult<T> : IInteractiveResult
{
	/// <summary>
	/// A canceled result.
	/// </summary>
	public static InteractiveResult<T> Canceled { get; } = new(cancel: true);
	/// <summary>
	/// A timed out result.
	/// </summary>
	public static InteractiveResult<T> TimedOut { get; } = new(timeout: true);

	/// <inheritdoc />
	public bool HasBeenCanceled { get; }
	/// <inheritdoc />
	public bool HasTimedOut { get; }
	/// <inheritdoc />
	public bool HasValue { get; }
	/// <summary>
	/// The parsed value.
	/// </summary>
	/// <exception cref="InvalidOperationException">When <see cref="HasValue"/> is false.</exception>
	public T Value
	{
		get
		{
			if (HasValue)
			{
				return field;
			}
			throw new InvalidOperationException();
		}
	}
	object IInteractiveResult.Value => Value!;

	/// <summary>
	/// Creates an instance of <see cref="InteractiveResult{T}"/>.
	/// </summary>
	/// <param name="value"></param>
	public InteractiveResult(T value)
	{
		HasValue = true;
		Value = value;
	}

	private InteractiveResult(bool timeout = false, bool cancel = false)
	{
		HasValue = false;
		Value = default!;
		HasTimedOut = timeout;
		HasBeenCanceled = cancel;
	}

	/// <summary>
	/// Retrieves the value.
	/// </summary>
	/// <param name="value"></param>
	public static explicit operator T(InteractiveResult<T> value) => value.Value;

	/// <summary>
	/// Creates an instance of <see cref="InteractiveResult{T}"/> from <paramref name="value"/>.
	/// </summary>
	/// <param name="value"></param>
	public static implicit operator InteractiveResult<T>(T value) => new(value);

	/// <summary>
	/// Creates a generic instance of <see cref="InteractiveResult{T}"/> which is either timed out or canceled depending on the passed in value.
	/// </summary>
	/// <param name="result"></param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException">When <paramref name="result"/> has no errors.</exception>
	public static InteractiveResult<T> PropagateError(IInteractiveResult result)
	{
		if (result.HasBeenCanceled)
		{
			return InteractiveResult<T>.Canceled;
		}
		else if (result.HasTimedOut)
		{
			return InteractiveResult<T>.TimedOut;
		}
		throw new InvalidOperationException();
	}
}