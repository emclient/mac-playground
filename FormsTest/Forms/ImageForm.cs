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
    public partial class ImageForm : Form
    {
        public ImageForm()
        {
            InitializeComponent();

            // Mac layout debugging
            //this.DebugAllControls();
        }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

            var img0 = picturebox2.Image;
            var img1 = Resources.Providers.ResourceManager.GetObject("Windows Live Hotmail_Small");
            var img2 = Resources.Providers.Windows_Live_Hotmail_Small;

            picturebox3.ImageLocation = null;
            picturebox3.Image = img1 as Image;
            picturebox4.Image = img2;
		}
    }
}
//image = Resources.Providers.Providers.ResourceManager.GetObject(providerName + """", Resources.Providers.Providers.Culture) as System.Drawing.Bitmap;
//                        if (image != null)
//                        {
//                            image = CommonPaintUtils.ResizeImage(image, size);
//                            if (gray)
//                                image = PaintUtils.RecolorImage(image, Color.Black, Color.FromArgb(170, 170, 170));
//providerImageCache[cacheKey] = image;
                        //}
