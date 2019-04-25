using System;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;

namespace Advobot.Classes.DatabaseWrappers.MongoDB
{
	/// <summary>
	/// Lets dictionaries get serialized correctly.
	/// Source: https://stackoverflow.com/questions/28111846/bsonserializationexception-when-serializing-a-dictionarydatetime-t-to-bson/28111847#28111847
	/// </summary>
	public class DictionaryRepresentationConvention : ConventionBase, IMemberMapConvention
	{
		private readonly DictionaryRepresentation _DictionaryRepresentation;

		/// <summary>
		/// Creates an instance of <see cref="DictionaryRepresentationConvention"/>.
		/// </summary>
		/// <param name="dictionaryRepresentation"></param>
		public DictionaryRepresentationConvention(DictionaryRepresentation dictionaryRepresentation)
		{
			_DictionaryRepresentation = dictionaryRepresentation;
		}

		/// <inheritdoc />
		public void Apply(BsonMemberMap memberMap)
			=> memberMap.SetSerializer(ConfigureSerializer(memberMap.GetSerializer()));
		private IBsonSerializer ConfigureSerializer(IBsonSerializer serializer)
		{
			if (serializer is IDictionaryRepresentationConfigurable dictionaryRepresentationConfigurable)
			{
				serializer = dictionaryRepresentationConfigurable.WithDictionaryRepresentation(_DictionaryRepresentation);
			}
			if (serializer is IChildSerializerConfigurable childSerializerConfigurable)
			{
				return childSerializerConfigurable.WithChildSerializer(ConfigureSerializer(childSerializerConfigurable.ChildSerializer));
			}
			return serializer ?? throw new InvalidOperationException($"Unable to find a {nameof(IBsonSerializer)} for dictionaries.");
		}
	}
}