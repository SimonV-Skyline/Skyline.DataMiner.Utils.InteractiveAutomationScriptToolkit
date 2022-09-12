namespace Skyline.DataMiner.InteractiveAutomationToolkit
{
	using System;
	using System.ComponentModel;

	using Skyline.DataMiner.Automation;

	/// <summary>
	///     Represents a text box for passwords.
	/// </summary>
	public interface IPasswordBox : IInteractiveWidget, IValidate
	{
		/// <summary>
		///     Triggered when the password changes.
		/// </summary>
		event EventHandler<PasswordBox.ChangedEventArgs> Changed;

		/// <summary>
		///     Gets or sets a value indicating whether the peek icon to reveal the password is shown.
		///     Default: <c>false</c>.
		/// </summary>
		bool HasPeekIcon { get; set; }

		/// <summary>
		///     Gets or sets the password set in the password box.
		/// </summary>
		string Password { get; set; }

		/// <summary>
		///     Gets or sets the text that should be displayed as a placeholder.
		/// </summary>
		/// <remarks>Available from DataMiner Feature Release 10.0.5 and Main Release 10.1.0 onwards.</remarks>
		string PlaceHolder { get; set; }

		/// <summary>
		///     Gets or sets the tooltip.
		/// </summary>
		string Tooltip { get; set; }
	}
}