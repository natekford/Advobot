using AdvorangesUtils;

using Discord;

namespace Advobot.Formatting;

/// <summary>
/// Holds a 2d collection of <see cref="Info"/>.
/// </summary>
public sealed class InfoMatrix
{
	/// <summary>
	/// The rows of this matrix.
	/// </summary>
	public List<InfoCollection> Collections { get; set; } = [];

	/// <summary>
	/// Creates an <see cref="InfoCollection"/> for time created and adds it to this matrix.
	/// </summary>
	/// <param name="e"></param>
	/// <returns></returns>
	public InfoCollection AddTimeCreatedCollection(ISnowflakeEntity e)
		=> AddTimeCreatedCollection(e.Id.ToString(), e.CreatedAt.UtcDateTime);

	/// <summary>
	/// Creates an <see cref="InfoCollection"/> for time created and adds it to this matrix.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="dt"></param>
	/// <returns></returns>
	public InfoCollection AddTimeCreatedCollection(string id, DateTimeOffset dt)
	{
		var diff = (DateTimeOffset.UtcNow - dt).TotalDays;
		var collection = CreateCollection();
		collection.Add("Id", id);
		collection.Add("Created At", $"{dt.DateTime.ToReadable()} ({diff:0.00} days ago)");
		return collection;
	}

	/// <summary>
	/// Creates an <see cref="InfoCollection"/> and adds it to this matrix.
	/// </summary>
	/// <returns></returns>
	public InfoCollection CreateCollection()
	{
		var collection = new InfoCollection();
		Collections.Add(collection);
		return collection;
	}

	/// <inheritdoc />
	public override string ToString()
		=> ToString(InfoFormattingArgs.Default);

	/// <inheritdoc />
	public string ToString(InfoFormattingArgs args)
	{
		//Any collections with no information in them dont need to be added
		var valid = Collections.Where(x => x.Information.Count > 0);
		return valid.Join(x => x.ToString(args), args.CollectionSeparator);
	}
}