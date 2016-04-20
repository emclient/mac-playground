using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using System;
using System.Linq;
using MailClient.Common.UI;
using System.Diagnostics;

namespace FormsTest
{
    using CocoaMessageBox = MessageBox;
    using FormsMessageBox = System.Windows.Forms.MessageBox;

    public partial class MainForm : Form
    {
        const string title = "Title";
        const string loremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum";

        Pair[] Dialogs = {
            new Pair { Name = "Color Dialog", Action = delegate { new ColorDialog().ShowDialog(); } },
            new Pair { Name = "Open File", Action = delegate { new OpenFileDialog().ShowDialog(); } },
            new Pair { Name = "Save File", Action = delegate { new SaveFileDialog().ShowDialog(); } },
            new Pair { Name = "Choose Folder", Action = delegate { 
					var dlg = new FolderBrowserDialog();
					dlg.Description = "Vyber nějakou složku vosle";
					dlg.SelectedPath = "/Library";
					var result = dlg.ShowDialog();
					Debug.WriteLine(result == DialogResult.OK ? "Vybrals " + dlg.SelectedPath : "Nic nevybrals" );
			} },
            new Pair { Name = "Font Dialog", Action = delegate { new FontDialog().ShowDialog(); } },
            new Pair { Name = "Page Setup", Action = delegate { new PageSetupDialog().ShowDialog(); } },
            new Pair { Name = "MB Info OK", Action = delegate { MessageBox.Show(loremIpsum, title, MessageBoxButtons.OK, MessageBoxIcon.Information); } },
            new Pair { Name = "MB Warn OK/C", Action = delegate { MessageBox.Show(loremIpsum, title, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning); } },
            new Pair { Name = "MB Warn Y/N", Action = delegate { MessageBox.Show(loremIpsum, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning); } },
            new Pair { Name = "MB Error Y/N/C", Action = delegate { MessageBox.Show(loremIpsum, title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error); } },
            new Pair { Name = "MB Quest A/R/I", Action = delegate { MessageBox.Show(loremIpsum, title, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Question); } },
            new Pair { Name = "MB Error R/C", Action = delegate { MessageBox.Show(loremIpsum, title, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error); } },
			new Pair { Name = "MB Error C/R", Action = delegate { MessageBox.Show(loremIpsum, title, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2); } },
        };

        public delegate void Action();
        public class Pair
        {
            public string Name { get; set; }
            public Action Action { get; set; }
        }

        private string filter = String.Empty;

        public MainForm()
        {
            DisplaySettingsManager.Initialize();

            InitializeComponent();

            UpdateList();
            InitDialogCombo();

//			this.DebugAllControls();
//			this.DebugAllPanels();
			string[] names = {"MainForm", "panel1", "listBox1"};
			this.DebugNamedControls( names );
//			this.DebugNamedControl( "MainForm" );
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

        void InitDialogCombo()
        {
            dialogTypeCombo.ValueMember = "Action";
            dialogTypeCombo.DisplayMember = "Name";
            dialogTypeCombo.DataSource = Dialogs;
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
            //var form = new LayoutForm();
			var form = new WebForm();
            form.Show();
        }

        private void button3_Click(object sender, System.EventArgs e)
        {
            try
            {
                (dialogTypeCombo.SelectedValue as Action)();
            }
			catch (Exception x)
            {
				Debug.WriteLine(x.ToString());
            }
        }

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
