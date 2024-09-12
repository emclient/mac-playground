using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms
{
	public sealed class DpiChangedEventArgs : CancelEventArgs
	{
		internal unsafe DpiChangedEventArgs(int old, Message m)
        {
            DeviceDpiOld = old;
			DeviceDpiNew = m.WParam.ToInt32() & 0xffff;
			var rect = ((XplatUIWin32.RECT*)m.LParam.ToPointer())[0];
			SuggestedRectangle = rect.ToRectangle();
        }

		public int DeviceDpiOld { get; }

		public int DeviceDpiNew { get; }

		public Rectangle SuggestedRectangle { get; }
	}
}
