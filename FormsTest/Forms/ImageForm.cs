using System;
using System.Drawing;
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

            picturebox1.Image = Resources.Providers.iCloud;
            picturebox2.Image = Resources.Providers.iCloud_Small;
            picturebox3.Image = Resources.Providers.Windows_Live_Hotmail;
            picturebox4.Image = Resources.Providers.Windows_Live_Hotmail_Small;


		}

        protected void button1_Click(object sender, EventArgs e)
        {
            var wh = new int[] { -1, -1, 0, 0, 1, 1 };
            for (int i = 0; i < wh.Length; i += 2)
            {
                try
                {
                    var img = new Bitmap(wh[i], wh[1+i]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{wh[i]},{wh[1+i]}] {ex}");
                }
            }
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
