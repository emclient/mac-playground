using System.Windows.Forms;

namespace FormsTest
{
    public partial class DebugLayoutForm : Form
    {
        public DebugLayoutForm()
        {
            InitializeComponent();

            // Mac layout debugging
			this.DebugAllControls();
        }
    }
}
