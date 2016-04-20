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
    public partial class DebugLayoutForm : Form
    {
        public DebugLayoutForm()
        {
            InitializeComponent();

			this.DebugAllControls();
        }
    }
}
