using System.Windows.Forms;

namespace FormsTest
{
    public partial class DebugLayoutForm2 : Form
    {
        public DebugLayoutForm2()
        {
            InitializeComponent();

            // Mac layout debugging
            this.DebugAllControls();
        }
    }
}
