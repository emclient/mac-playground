using System;
using System.Windows.Forms;

namespace FormsTest
{
    public partial class ToolstripForm : Form
    {
        public ToolstripForm()
        {
            InitializeComponent();

			var dd = new ToolStripDropDownButton() { Text = "AAA" };
			dd.DropDownItems.Add("Menu 1");
			this.toolStrip1.Items.Add(dd);
        }
    }
}
