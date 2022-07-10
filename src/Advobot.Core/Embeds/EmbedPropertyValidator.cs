using System.Linq.Expressions;

namespace Advobot.Embeds;

internal sealed class EmbedPropertyValidator<TEmbed, T>
{
	private readonly string _PropertyPath;
	private readonly T _Value;

	public EmbedValidator Validator { get; }

	public EmbedPropertyValidator(
		EmbedValidator validator,
		Expression<Func<TEmbed, T>> property,
		T value)
	{
		Validator = validator;
		_Value = value;
		_PropertyPath = property.GetPropertyPath();
	}

	public EmbedPropertyValidator<TEmbed, T> Rule(
		Func<T, bool> validation,
		Func<string> reason)
	{
		if (!validation.Invoke(_Value))
		{
			var exception = new EmbedException(reason.Invoke(), _PropertyPath, _Value);
			Validator.GlobalErrors.Add(exception);
			Validator.Errors.Add(exception);
		}
		return this;
	}
}