using Advobot.Classes;

using System.Linq.Expressions;

namespace Advobot.EmbedWrapper;

internal sealed class EmbedPropertyValidator<TEmbed, T>
{
	private readonly List<EmbedException> _GlobalErrors;
	private readonly EmbedValidator _Parent;
	private readonly Expression<Func<TEmbed, T>> _Property;
	private readonly List<EmbedException> _PropertyErrors;
	private readonly string _PropertyPath;
	private readonly T _Value;

	public EmbedPropertyValidator(
		EmbedValidator parent,
		List<EmbedException> globalErrors,
		List<EmbedException> propertyErrors,
		T value,
		Expression<Func<TEmbed, T>> property)
	{
		_Parent = parent;
		_GlobalErrors = globalErrors;
		_PropertyErrors = propertyErrors;
		_Value = value;
		_Property = property;
		_PropertyPath = _Property.GetPropertyPath();
	}

	public IReadOnlyList<EmbedException> End()
		=> _PropertyErrors;

	public EmbedPropertyValidator<TEmbed2, T2> Property<TEmbed2, T2>(
		Expression<Func<TEmbed2, T2>> p,
		T2 v)
		=> _Parent.Property(p, v);

	public EmbedPropertyValidator<TEmbed, T> Rule(
		Func<T, bool> invalidation,
		Func<string> reason)
	{
		if (invalidation.Invoke(_Value))
		{
			var exception = new EmbedException(reason.Invoke(), _PropertyPath, _Value);
			_GlobalErrors.Add(exception);
			_PropertyErrors.Add(exception);
		}
		return this;
	}
}