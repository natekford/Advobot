using System;
using System.Collections.Generic;
using System.Linq;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;

namespace Advobot.Formatting
{
	/// <summary>
	/// Holds a semi 2d collection of <see cref="Information"/>.
	/// </summary>
	public sealed class InformationMatrix
	{
		/// <summary>
		/// The rows of this matrix.
		/// </summary>
		public IReadOnlyList<InformationCollection> Collections => _Collections.AsReadOnly();

		private readonly List<InformationCollection> _Collections = new List<InformationCollection>();

		/// <summary>
		/// Creates an <see cref="InformationCollection"/> for time created and adds it to this matrix.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public InformationCollection AddTimeCreatedCollection(ISnowflakeEntity e)
			=> AddTimeCreatedCollection(e.Id.ToString(), e.CreatedAt.UtcDateTime);
		/// <summary>
		/// Creates an <see cref="InformationCollection"/> for time created and adds it to this matrix.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="dt"></param>
		/// <returns></returns>
		public InformationCollection AddTimeCreatedCollection(string id, DateTime dt)
		{
			var diff = (DateTime.UtcNow - dt).TotalDays;
			var collection = CreateCollection();
			collection.Add("Id", id);
			collection.Add("Created At", $"{dt.ToReadable()} ({diff:0.00} days ago)");
			return collection;
		}
		/// <summary>
		/// Creates an <see cref="InformationCollection"/> and adds it to this matrix.
		/// </summary>
		/// <returns></returns>
		public InformationCollection CreateCollection()
		{
			var collection = new InformationCollection();
			_Collections.Add(collection);
			return collection;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			//Any collections with no information in them dont need to be added
			var valid = Collections.Where(x => x.Information.Any());
			return valid.ToDelimitedString(x => x.ToString(), "\n\n");
		}
	}
}
