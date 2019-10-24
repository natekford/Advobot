namespace Advobot.Databases.Relationships
{
	/// <summary>
	/// Represents an object which belongs to a message.
	/// </summary>
	public interface IMessageChild : IChannelChild
	{
		/// <summary>
		/// The message's id.
		/// </summary>
		ulong MessageId { get; }
	}
}