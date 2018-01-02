using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MailClient.Common.UI;

namespace FormsTest
{
    public partial class TextBoxForm : Form
    {
        public TextBoxForm()
        {
            InitializeComponent();

            // Mac layout debugging
            //this.DebugAllControls();
        }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			var systemAssembly = typeof(System.Net.WebClient).Assembly;
			var loggingType = systemAssembly.GetType("System.Net.Logging", false);

			var nonPublicStaticProperties = loggingType.GetMembers(System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
			var nonPublicStaticFields = loggingType.GetMembers(System.Reflection.BindingFlags.SetField | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
			var nonPublicStaticMethods = loggingType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

			textbox4.AppendText("\n\nNon Public Static Properties:");
			foreach (var p in nonPublicStaticProperties)
				textbox4.AppendText($"\n{p.Name}");

			textbox4.AppendText("\n\nNon Public Static Fileds:");
			foreach (var f in nonPublicStaticFields)
				textbox4.AppendText($"\n{f.Name}");

			textbox4.AppendText("\n\nNon Public Static Methods:");
			foreach (var m in nonPublicStaticMethods)
				textbox4.AppendText($"\n{m.Name}()");

		}
    }
}
