using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FormsTest
{
    public class SeparatorBox : GroupBox
    {
        public enum Style { Default, LineOnTop, Header }

        private Style style = Style.Default;
        private Font semibold = null;
        private Brush headerBrush = new SolidBrush(Color.FromArgb(230, 230, 230));

        [DefaultValue(typeof(Style), "Default")]
        public Style BoxStyle
        {
            get { return style; }
            set
            {
                if (style != value)
                {
                    style = value;
                    Invalidate();
                }
            }
        }

        public SeparatorBox()
        {
        }


        protected override Padding DefaultPadding
        {
            get
            {
                return new Padding(10, 6, 10, 10);
            }
        }

        protected override void OnFontChanged(System.EventArgs e)
        {
            base.OnFontChanged(e);
            semibold = null;
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            if (style == Style.Default)
                base.OnPaint(e);
            else if (style == Style.LineOnTop)
            {
                int separatorTop;
                if (!string.IsNullOrEmpty(this.Text))
                {
                    separatorTop = this.FontHeight / 2;
                    TextFormatFlags flags = 0;
                    if (!ShowKeyboardCues)
                        flags |= TextFormatFlags.HidePrefix;
                    MailClient.Common.UI.TextRendererEx.DrawText(e.Graphics, this.Text, this.Font, new Point(0, 0), SystemColors.ControlDarkDark, flags);
                    Size textSize = TextRenderer.MeasureText(e.Graphics, this.Text, this.Font);
                    e.Graphics.DrawLine(SystemPens.ControlDark, textSize.Width, separatorTop, this.Width, separatorTop);
                }
                else
                {
                    separatorTop = this.FontHeight / 2;
                    e.Graphics.DrawLine(SystemPens.ControlDark, 0, separatorTop, this.Width, separatorTop);
                }
            }
            else if (style == Style.Header)
            {
                if (semibold == null)
                    semibold = MailClient.Common.UI.FontCache.TryCreateSemiboldFont(this.Font);

                e.Graphics.FillRectangle(headerBrush, new Rectangle(0, 0, Width, MailClient.Common.UI.DisplaySettingsManager.Instance.Scale(20)));
                MailClient.Common.UI.TextRendererEx.DrawText(e.Graphics, this.Text, semibold, new Point(3, 3), Color.FromArgb(17, 82, 156));
            }
        }
    }
}
