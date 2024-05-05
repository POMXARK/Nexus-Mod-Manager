using System.Windows.Forms;
using Nexus.Client.Util;

namespace Nexus.Client.Settings.UI
{
	/// <summary>
	/// A view allowing the editing of mod options.
	/// </summary>
	public partial class ModOptionsPage : UserControl, ISettingsGroupView
	{
		#region Constructors
		
		#endregion

		#region ISettingsGroupView Members

		/// <summary>
		/// Gets the <see cref="SettingsGroup"/> whose settings will be editable with this view.
		/// </summary>
		/// <value>The <see cref="SettingsGroup"/> whose settings will be editable with this view.</value>
		public SettingsGroup SettingsGroup { get; private set; }

		#endregion
	}
}
