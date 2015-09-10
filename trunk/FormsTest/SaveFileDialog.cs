#if MAC

using System;
using System.Windows.Forms;

namespace FormsTest
{
	public class SaveFileDialog : CommonDialog
	{
		#region implemented abstract members of CommonDialog

		public override void Reset ()
		{
		}

		protected override bool RunDialog (IntPtr hwndOwner)
		{
			return false;
		}

		#endregion

		public new DialogResult ShowDialog()
		{
			using (var context = new ModalDialogContext ()) {
				var panel = new MonoMac.AppKit.NSSavePanel ();
				panel.RunModal ();
				return new DialogResult ();
			}
		}
	}
}

#endif //MAC