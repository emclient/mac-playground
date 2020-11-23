using System.Drawing;

namespace System.Windows.Forms
{
	static class Hwnd
	{
		static Graphics cachedGraphicsContext;

		public static Graphics GraphicsContext
		{
			get
			{
				if (cachedGraphicsContext != null)
					return cachedGraphicsContext;
				return cachedGraphicsContext = Graphics.FromHwnd(IntPtr.Zero);
			}
		}
	}
}
