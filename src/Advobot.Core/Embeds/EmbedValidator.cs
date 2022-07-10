using System.Linq.Expressions;

namespace Advobot.Embeds;

internal sealed class EmbedValidator
{
	private readonly Action _Setter;
	private List<EmbedException>? _Errors;

	public List<EmbedException> Errors => _Errors ??= new();
	public List<EmbedException> GlobalErrors { get; }

	public EmbedValidator(Action setter, List<EmbedException> globalErrors)
	{
		GlobalErrors = globalErrors;
		_Setter = setter;
	}

	public bool Finalize(out IReadOnlyList<EmbedException> errors)
	{
		errors = Errors;

		var success = errors.Count == 0;
		if (success)
		{
			_Setter.Invoke();
		}
		return success;
	}

	public EmbedPropertyValidator<TEmbed, T> Property<TEmbed, T>(
		Expression<Func<TEmbed, T>> property,
		T value)
		=> new(this, property, value);
}