using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormsTest
{
    public partial class DebugLayoutForm3 : Form
    {
        public DebugLayoutForm3()
        {
            InitializeComponent();

            // Mac layout debugging
            this.DebugAllControls();
        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
