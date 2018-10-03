namespace MailClient.UI.Controls
{
	partial class ControlStatusBar
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.timerInfinite = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// timerInfinite
			// 
			this.timerInfinite.Interval = 25;
			this.timerInfinite.Tick += new System.EventHandler(this.timerInfinite_Tick);
			// 
			// ControlStatusBar
			// 
			this.Name = "ControlStatusBar";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Timer timerInfinite;
	}
}
