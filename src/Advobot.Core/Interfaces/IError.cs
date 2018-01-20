namespace Advobot.Core.Interfaces
{
	/// <summary>
	/// An error which has a reason.
	/// </summary>
	public interface IError
    {
		string Reason { get; }
    }
}
