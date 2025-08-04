using System.Linq.Expressions;

namespace Advobot.Embeds;

internal sealed class EmbedValidator(Action setter, List<EmbedException> globalErrors)
{
	private readonly Action _Setter = setter;

	public List<EmbedException> Errors => field ??= [];
	public List<EmbedException> GlobalErrors { get; } = globalErrors;

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