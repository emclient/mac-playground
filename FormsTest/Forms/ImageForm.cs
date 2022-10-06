using System;
using System.IO;
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
            using var dlg = new OpenFileDialog();
            if (DialogResult.OK == dlg.ShowDialog())
            {
                var path = dlg.FileName;
                var ext = Path.GetExtension(path);
                switch (ext)
                {
                    case ".txt":
                        if (ImageFromTextFile(path) is Image image)
                            picturebox1.Image = image;
                        break;
                    case ".png":
                    case ".jpg":
                        SaveImageAsText(path, 74, 27, 1);
                        break;
                }
            }
        }

        void SaveImageAsText(string path, int lineLength = 60, int firstLineLength = 60, int padding = 0, string lineEnd = "\x0d\x0a")
        {
            var image = Image.FromFile(path);

            path = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path)) + ".txt";

            using var m = new MemoryStream();
            image.Save(m, image.RawFormat);
            var bytes = m.ToArray();
            var base64 = Convert.ToBase64String(bytes);


            string prefix = "";
            int len = firstLineLength;
            for (int pos = 0; pos < base64.Length; )
            {
                var n = Math.Min(len, base64.Length - pos);
                var s = base64.Substring(pos, n);

                File.AppendAllText(path, prefix);
                File.AppendAllText(path, s);
                File.AppendAllText(path, lineEnd);

                pos += n;
                len = lineLength;
                if (prefix.Length != padding)
                    prefix = new string(' ', padding);
            }

            // File.WriteAllText(path, base64);
        }

        public Image ImageFromTextFile(string path)
        {
            var lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; ++i)
                lines[i] = lines[i].Trim();

            var text = string.Join("", lines);
            var pad = text.Length % 4;
            for (int i = 0; i < pad; ++i)
                text += "=";

            byte[] bytes = Convert.FromBase64String(text);
            using (var ms = new System.IO.MemoryStream(bytes))
                return Image.FromStream(ms);
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
