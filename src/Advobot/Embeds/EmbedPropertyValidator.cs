using System.Linq.Expressions;

namespace Advobot.Embeds;

internal sealed class EmbedPropertyValidator<TEmbed, T>(
	EmbedValidator validator,
	Expression<Func<TEmbed, T>> property,
	T value)
{
	private readonly string _PropertyPath = property.GetPropertyPath();
	private readonly T _Value = value;

	public EmbedValidator Validator { get; } = validator;

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