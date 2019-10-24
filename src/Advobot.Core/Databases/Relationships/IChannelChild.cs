namespace Advobot.Databases.Relationships
{
	/// <summary>
	/// Represents an object which belongs to a channel.
	/// </summary>
	public interface IChannelChild : IGuildChild
	{
		/// <summary>
		/// The channel's id.
		/// </summary>
		ulong ChannelId { get; }
	}
}