﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
#if MAC
using Foundation;
using UniformTypeIdentifiers;
#endif
namespace FormsTest
{
	using System.Collections.Generic;
	using FormsTest.Experiments;
	//using MailClient.UI.Forms;
	//using CocoaMessageBox = MessageBox;
	using MessageBox = System.Windows.Forms.MessageBox;

	public partial class MainForm : Form
    {
        const string title = "Title";
        const string loremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum";

        Pair[] Dialogs = {
            new Pair { Name = "Color Dialog", Action = delegate { new ColorDialog().ShowDialog(); } },
            new Pair { Name = "Open File", Action = delegate { new OpenFileDialog().ShowDialog(); } },
            new Pair { Name = "Open File w Filter", Action = OpenFileDialogWithFilter1 },
            new Pair { Name = "Open File w Filter 2", Action = OpenFileDialogWithFilter2 },
            new Pair { Name = "Save File", Action = delegate { new SaveFileDialog().ShowDialog(); } },
            new Pair { Name = "Save File w Filter", Action = SaveFileDialogWithFilter1 },
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
            //DisplaySettingsManager.Initialize();

            InitializeComponent();

            UpdateList();
            InitDialogCombo();

            // Mac layout debugging
            //this.DebugAllControls();

            this.filterTextBox.KeyDown += FilterTextBox_KeyDown;

			AddButton("WebForm", () => { new WebForm().Show(); });
            AddButton("ImageForm", () => { new ImageForm().Show(); });
			AddButton("TextBoxes", () => { new TextBoxForm().Show(); });
			//AddButton("ImapOptions", () => { new ImapOptionsForm().Show(); });
			//AddButton("MailOptions", () => { new MailOptionsForm().Show(); });
			/*AddButton("FormInputBox", () => { new FormInputBox(
				"Nazdar", 
				"Přihlášení k webovému kalendáři",
			    "Zadej url kalendáře, ke kterému se chceš přihlásit, třeba http://calndar.org-mycalndar.ics"
			).Show(); });*/
			AddButton("Layout 1", () => { new DebugLayoutForm().Show(); });
			AddButton("Layout 2", () => { new DebugLayoutForm2().Show(); });
			AddButton("Layout 3", () => { new DebugLayoutForm3().Show(); });
			AddButton("Layout 4", () => { new DebugLayoutForm4().Show(); });
			AddButton("Tables", () => { new TablesForm().Show(); });
			/*AddButton("Editor", () =>
			{
				var f = new Form() { Size = new Size(500, 300), Text = "Editor" };

				f.Controls.Add(new HtmlEditorControl.HtmlEditorBrowser { Dock = DockStyle.Fill });
				f.Controls.Add(new MailClient.Common.UI.Controls.ControlTextBox.ControlTextBox { Dock = DockStyle.Top });
				var toolStrip = new MailClient.Common.UI.Controls.ControlToolStrip.ControlToolStrip { Dock = DockStyle.Top, AutoSize = true, Height = 40 };
				f.Controls.Add(toolStrip);
				toolStrip.Items.Add(new MailClient.Common.UI.Controls.ToolStripControls.ControlToolStripButtonFontSelector());
				f.Show();
			});*/
			/*AddButton("FormLinkMessageBox", () =>
			{
				new FormLinkMessageBox("Hello", "Zkušební doba vypršela. Platnou licenci pro eM Client můžete získat na http://cz.emclient.com/prehled-cenovych-moznosti\r\nNyní budete mít ke svým datům přístup pouze v režimu off-line. Synchronizace a odesílání jakýchkoliv zpráv bude nyní zablokována. Jakmile bude zadána validní licence, všechny funkce aplikace budou okamžitě obnoveny.").Show();
			});*/
			/*AddButton("Recurrence", () =>
			{
				var f = new Form() { Size = new Size(500, 300), Text = "Recurrence" };
				f.Controls.Add(new MailClient.UI.Controls.ControlRecurrence());
				f.Show();
			});
			AddButton("Confirmations", () =>
			{
				var f = new Form() { Size = new Size(400, 500), Text = "Confirmations" };
				f.Controls.Add(new MailClient.UI.Controls.SettingsControls.ControlSettingsConfirmations() { Dock = DockStyle.Top, AutoSize = true });
				f.DebugAllControls();
				f.Show();
			});
			AddButton("Spell checker", () =>
			{
				var f = new Form() { Size = new Size(400, 500), Text = "Spell checker" };
				f.Controls.Add(new MailClient.UI.Controls.SettingsControls.ControlSettingsSpellChecker() { Dock = DockStyle.Top, AutoSize = true });
				f.Show();
			});
			AddButton("Autodiscover", () =>
			{
				var f = new Form() { Size = new Size(400, 500), Text = "Autodiscover" };
				f.Controls.Add(new MailClient.UI.Controls.WizardControls.ControlExpandablePanelAutodiscover() { Dock = DockStyle.Top, AutoSize = true, Expanded = true });
				f.Show();
			});*/

			AddButton("Animations", () => { new AnimationsForm().Show(); });
			AddButton("Print dialog", () => { ShowPrintDialog(); });
			AddButton("Toolstrip form", () => { new ToolstripForm().Show(); });
#if MAC
			AddButton("Bg activity", () => { ToggleBgActivity(); });
			AddButton("Swizzle (cls)", () => { SwizzleCls(); });
			AddButton("NSException", () => { NativeException(); });
			AddButton("QuickLook panel", () => { ToggleQuickLookPanel(); });
			AddButton("NC Preferences", () => { ReadNotificationCenterPreferences(); });
			AddButton("Back color", () => { ChangeBackColor(); });
			AddButton("Text Extraction", () => { TextExtractionTest(); });
			AddButton("Core Data Test", () => { CoreDataTest(); });
			AddButton("RAM Disk Test", () => { RamDiskTest(); });
			//AddButton("UTType Test", () => { UTTypeTest(); });
			AddButton("UNNotifications", () => { new NotificationsForm().Show(); });
			//AddButton("NSUserDefaults", () => { TestNSUserDefaults(); });
#endif

			//AddButton("Data Grid", () => { new DataGridForm().Show(); });
			AddButton("Font Dialog", () => { FontDialogTest(); });
			AddButton("Date Time", () => { new DateTimeForm().Show(); });
			AddButton("Texture Brush", () => { new TextureBrushForm().Show(); });
			AddButton("File Descriptors", () => { (fd = new FileDescriptors()).RunTest(); });
			AddButton("Catch When", () => { CatchWhen.RunTest(); });
			AddButton("Print Preview", () => { PrintPreview(); });
			//AddButton("AutoSize Form", () => { ShowAutoSizeForm(); });

			AddButton("DrawString Form", () => { new DrawStringForm().Show(); });
			AddButton("DrawText Form", () => { new DrawTextForm().Show(); });
			AddButton("Label Form", () => { new LabelForm().Show(); });
			AddButton("ScrollBar", () => { new ScrollBarsForm().Show(); });
			AddButton("Regions", () => { new RegionsForm().Show(); });

			AddButton("Cancellation", () => { new CancellationForm().Show(); });
		}

		FileDescriptors fd;

		List<Button> buttons = new List<Button>();
		void AddButton(string text, Action a)
		{
			const int gap = 5;

			var b = new Button();
			b.AutoSize = true;
			b.Anchor = AnchorStyles.Right;
			b.Click += (sender, e) => { a(); };
			b.Text = text;
			b.SetBounds(panel2.Width - b.Width - gap, gap + buttons.Count * 28, 0, 0, BoundsSpecified.Y | BoundsSpecified.X);

			buttons.Add(b);
			panel2.Controls.Add(b);
		}

		private void FilterTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine($"KeyDown:[KeyCode={e.KeyCode},KeyValue={e.KeyValue},Modifiers={e.Modifiers}]");
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
            ListFontFamilies(listBox1, filter);
        }

		FontCollection fonts = new InstalledFontCollection();

		void ListFontFamilies(ListBox listBox, string filter)
        {
            var families = fonts.Families.Select(x => x.Name).ToArray();

            Array.Sort(families);

			listBox.BeginUpdate();
            listBox1.Items.Clear();
			foreach (var family in families)
            {
                if (String.IsNullOrEmpty(filter) || -1 != family.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase))
                    listBox.Items.Add(family);
            }
			listBox.EndUpdate();
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
            var form = new MainForm();
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

		void ChangeBackColor()
		{
#if MAC
			var view = ObjCRuntime.Runtime.GetNSObject<AppKit.NSView>(Handle);
			var window = view.Window;

			if (!window.TitlebarAppearsTransparent)
			{
				window.TitlebarAppearsTransparent = true;
				//window.StyleMask |= AppKit.NSWindowStyle.TexturedBackground;
				window.BackgroundColor = AppKit.NSColor.Red;

				//var box = new AppKit.NSBox(CoreGraphics.CGRect.FromLTRB(0, 0, 100, 20));
				//box.FillColor = AppKit.NSColor.Clear;
				//tbc = new AppKit.NSTitlebarAccessoryViewController();
				//tbc.View = box;
				//window.AddTitlebarAccessoryViewController(tbc);
			}
			else
			{	
				window.TitlebarAppearsTransparent = false;
				//window.StyleMask &= ~AppKit.NSWindowStyle.TexturedBackground;
				window.BackgroundColor = AppKit.NSColor.WindowBackground;
				//window.RemoveTitlebarAccessoryViewControllerAtIndex(0);
			}
#endif
		}

		void FontDialogTest()
		{
			using (var dialog = new FontDialog())
			{
				dialog.ShowColor = true;
				dialog.ShowEffects = true;
				dialog.Color = Color.Green;
				dialog.Font = new Font("Geneva", 12);

				if (DialogResult.OK == dialog.ShowDialog())
				{
					Debug.WriteLine($"Selected font:{dialog.Font}");
					Debug.WriteLine($"Selected color:{dialog.Color}");
				}
			}
		}

#if MAC
		void UTTypeTest()
		{
			const string NSPasteboardTypeWebArchive = "Apple Web Archive pasteboard type";
			var src = NSPasteboardTypeWebArchive;
			var dst = CreateDynamicTypeFor(src);
			Console.WriteLine($"CreateDynamicType({src}) -> {dst}");
		}

		string? CreateDynamicTypeFor(string identifier)
		{
			const string tag = "kUTTagClassNSPboardType";
			using var tagClass = new NSString(identifier);
			return UTType.GetType(tag, tagClass, UTTypes.Data)?.Identifier;
		}

		void TestNSUserDefaults()
		{
			var defaults = new NSUserDefaults();
			var domain = "com.parallels.Parallels Desktop";
			var dict = defaults.PersistentDomainForName(domain);
			Console.WriteLine($"{dict}");
		}

#endif

		public void ShowAutoSizeForm()
		{
			new AutoSizeForm().Show();
		}

		static void OpenFileDialogWithFilter1()
		{
			using var dlg = new OpenFileDialog();
			dlg.Filter = "PNG Files|*.png|JPEG Files|*.jpg;*.jpeg";
			dlg.FilterIndex = 2;
			dlg.ShowDialog();
		}

		static void OpenFileDialogWithFilter2()
		{
			using var dlg = new OpenFileDialog();
			dlg.Filter = "PNG Files|*.png|JPEG Files|*.jpg;*.jpeg|All files|*.*";
			dlg.ShowDialog();
		}

		static void SaveFileDialogWithFilter1()
		{
			using var dlg = new SaveFileDialog();
			dlg.Filter = "PNG Files|*.png|JPEG Files|*.jpg;*.jpeg";
			dlg.ShowDialog();
		}
	}
}
