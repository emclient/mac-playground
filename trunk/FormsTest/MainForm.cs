using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using System;
using System.Linq;
using MailClient.Common.UI;

namespace FormsTest
{
	using CocoaMessageBox = MessageBox;
	using FormsMessageBox = System.Windows.Forms.MessageBox;

	public partial class MainForm : Form
	{
		private string filter = String.Empty;

		public MainForm()
		{
            DisplaySettingsManager.Initialize();

			InitializeComponent();

			UpdateList();
		}

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
				return cp;
			}
		}

		void UpdateList()
		{
			listBox1.Items.Clear();
			ListFontFamilies(listBox1, filter);
		}

		void ListFontFamilies(ListBox listBox, string filter)
		{
			var fonts = new InstalledFontCollection();
			var families = fonts.Families.Select(x => x.Name).ToArray();

			Array.Sort(families);

			foreach (var family in families)
			{
				if (String.IsNullOrEmpty(filter) || -1 != family.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase))
					listBox.Items.Add(family);
			}
		}

		private void listBox1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			try
			{
				string name = listBox1.SelectedItem as string;
				label2.Font = new Font(name, label2.Font.Size);
				label3.Font = new Font(name, label3.Font.Size);
				label4.Font = new Font(name, label4.Font.Size);
				label5.Font = new Font(name, label5.Font.Size);
				label7.Font = new Font(name, label7.Font.Size);
			}
			catch
			{
			}
		}

		//		protected override void OnPaint (PaintEventArgs e)
		//		{
		//			NSView nsView = (NSView)MonoMac.ObjCRuntime.Runtime.GetNSObject(this.Handle);
		//			if (nsView != null && nsView.LockFocusIfCanDraw ()) {
		//				base.OnPaint (e);
		//				nsView.UnlockFocus ();
		//			}
		//		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			Application.Exit();
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			var form = new LayoutForm();
			form.Show();
		}

		private void button3_Click(object sender, System.EventArgs e)
		{
//			CocoaMessageBox.Show(loremIpsum);
//			FormsMessageBox.Show(loremIpsum);

//			CocoaMessageBox.Show(loremIpsum, "Bacha");
//			FormsMessageBox.Show(loremIpsum, "Bacha");

			CocoaMessageBox.Show(loremIpsum, "Bacha", MessageBoxButtons.OK, MessageBoxIcon.Error );
			FormsMessageBox.Show(loremIpsum, "Bacha", MessageBoxButtons.OK, MessageBoxIcon.Error );

//			CocoaMessageBox.Show(loremIpsum, "Bacha", MessageBoxButtons.OKCancel );
//			FormsMessageBox.Show(loremIpsum, "Bacha", MessageBoxButtons.OKCancel );

//			CocoaMessageBox.Show(loremIpsum, "Bacha", MessageBoxButtons.AbortRetryIgnore );
//			FormsMessageBox.Show(loremIpsum, "Bacha", MessageBoxButtons.AbortRetryIgnore );

			CocoaMessageBox.Show(loremIpsum, "Bacha", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation );
			FormsMessageBox.Show(loremIpsum, "Bacha", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation );

			CocoaMessageBox.Show(loremIpsum, "Bacha", MessageBoxButtons.YesNoCancel );
			FormsMessageBox.Show(loremIpsum, "Bacha", MessageBoxButtons.YesNoCancel );
		}

		static readonly string loremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum";

		private void filterTextBox_TextChanged(object sender, EventArgs e)
		{
			filter = filterTextBox.Text;
			UpdateList();
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			Console.WriteLine(e);

			base.OnKeyDown(e);
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg >= (int)Msg.WM_KEYFIRST && m.Msg <= (int)Msg.WM_KEYLAST)
				Console.WriteLine("MainForm.WndProc({0})", m);

			base.WndProc(ref m);
		}
	}
}
