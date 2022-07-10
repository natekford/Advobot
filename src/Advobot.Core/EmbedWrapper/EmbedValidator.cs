using Advobot.Classes;

using System.Linq.Expressions;

namespace Advobot.EmbedWrapper;

internal sealed class EmbedValidator
{
	private readonly List<EmbedException> _Errors = new();
	private readonly List<EmbedException> _GlobalErrors;

	public EmbedValidator(List<EmbedException> globalErrors)
	{
		_GlobalErrors = globalErrors;
	}

	public EmbedPropertyValidator<TEmbed, T> Property<TEmbed, T>(
		Expression<Func<TEmbed, T>> p,
		T v)
		=> new(this, _GlobalErrors, _Errors, v, p);
}