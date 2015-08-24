using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;


// http://blogs.msdn.com/b/jfoscoding/archive/2005/11/14/492554.aspx


namespace FormsTest
{
    /// <summary>
    /// A check box that can wrap its text onto multiple lines as needed.
    /// </summary>
    public class WrappingCheckBox : System.Windows.Forms.CheckBox
    {
        System.Drawing.Size cachedSizeOfOneLineOfText = System.Drawing.Size.Empty;
        Dictionary<Size, Size> preferredSizeHash = new Dictionary<Size, Size>(3); // typically we've got three different constraints.

        [DefaultValue(true)]
        public override bool AutoSize
        {
            get
            {
                return base.AutoSize;
            }
            set
            {
                base.AutoSize = value;
            }
        }

        [DefaultValue(ContentAlignment.TopLeft)]
        public new ContentAlignment CheckAlign
        {
            get
            {
                return base.CheckAlign;
            }
            set
            {
                base.CheckAlign = value;
            }
        }

        public WrappingCheckBox()
        {
            this.AutoSize = true;
            this.CheckAlign = ContentAlignment.TopLeft;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            CacheTextSize();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            CacheTextSize();
        }

        private void CacheTextSize()
        {
            //When the text has changed, the preferredSizeHash is invalid...
            preferredSizeHash.Clear();

            if (String.IsNullOrEmpty(this.Text))
            {
                cachedSizeOfOneLineOfText = System.Drawing.Size.Empty;
            }
            else
            {
                cachedSizeOfOneLineOfText = TextRenderer.MeasureText(this.Text, this.Font, new Size(Int32.MaxValue, Int32.MaxValue), TextFormatFlags.WordBreak);
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size prefSize = base.GetPreferredSize(proposedSize);
            if ((prefSize.Width > proposedSize.Width) && (!String.IsNullOrEmpty(this.Text) && !proposedSize.Width.Equals(Int32.MaxValue) || !proposedSize.Height.Equals(Int32.MaxValue)))
            {
                // we have the possiblility of wrapping... back out the single line of text
                Size bordersAndPadding = prefSize - cachedSizeOfOneLineOfText;
                // add back in the text size, subtract baseprefsize.width and 3 from proposed size width so they wrap properly
                Size newConstraints = proposedSize - bordersAndPadding - new Size(3, 0);
                if (!preferredSizeHash.ContainsKey(newConstraints))
                {
                    prefSize = bordersAndPadding + TextRenderer.MeasureText(this.Text, this.Font, newConstraints, TextFormatFlags.WordBreak);
                    preferredSizeHash[newConstraints] = prefSize;
                }
                else
                {
                    prefSize = preferredSizeHash[newConstraints];
                }
            }
            return prefSize;
        }
    }
}
