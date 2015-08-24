using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FormsTest.Extensions;
//using MonoMac.AppKit;
using System.Runtime.InteropServices;

namespace FormsTest
{
	public class MyTextBox : TextBox
	{
		protected override void OnKeyDown(KeyEventArgs e)
		{
			Console.WriteLine("MyTextBox.OnKeyDown({0})", e.Dump());
			base.OnKeyDown(e);
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			Console.WriteLine("MyTextBox.OnKeyPress({0})", e.Dump());
			base.OnKeyPress(e);
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg >= (int)Msg.WM_KEYFIRST && m.Msg <= (int)Msg.WM_KEYLAST)
				Console.WriteLine("MyTextBox.WndProc({0})", m);

			base.WndProc(ref m);
		}

		protected override void OnMouseClick (MouseEventArgs e)
		{
			base.OnMouseClick (e);

			//EmulateKeypress ();
		}


		void EmulateKeypress ()
		{
//			MyTextBox.WndProc(msg=0x100 (WM_KEYDOWN) hwnd=0x70184 wparam=0x10 lparam=0x402a0001 result=0x0)
//			MyTextBox.OnKeyDown({code=ShiftKey, value=16, data=ShiftKey, Shift, shift=True, alt=False, ctrl=False, suppress=False})
			Message m;
			m = new Message () {
				HWnd = this.Handle,
				Msg = (int)Msg.WM_KEYDOWN,
				WParam = (IntPtr)0x10,
				LParam = (IntPtr)0 //0x402a0001
			};
			this.WndProc (ref m);

//			MyTextBox.WndProc(msg=0x100 (WM_KEYDOWN) hwnd=0x70184 wparam=0xbb lparam=0xd0001 result=0x0)
//			MyTextBox.OnKeyDown({code=Oemplus, value=187, data=Oemplus, Shift, shift=True, alt=False, ctrl=False, suppress=False})
			m = new Message () {
				HWnd = this.Handle,
				Msg = (int)Msg.WM_KEYDOWN,
				WParam = (IntPtr)0xbb,
				LParam = (IntPtr)0//0xd0001
			};
			this.WndProc (ref m);

//			MyTextBox.WndProc(msg=0x103 (WM_DEADCHAR) hwnd=0x70184 wparam=0x2c7 lparam=0xd0001 result=0x0)
			m = new Message () {
				HWnd = this.Handle,
				Msg = (int)Msg.WM_DEADCHAR,
				WParam = (IntPtr)0x2c7,
				LParam = (IntPtr)0xd0001
			};
			this.WndProc (ref m);

//			MyTextBox.WndProc(msg=0x101 (WM_KEYUP) hwnd=0x70184 wparam=0xbb lparam=0xffffffffc00d0001 result=0x0)
			m = new Message () {
				HWnd = this.Handle,
				Msg = (int)Msg.WM_KEYUP,
				WParam = (IntPtr)0xbb,
				LParam = (IntPtr)0//0xffffffffc00d0001
			};
			this.WndProc (ref m);

//			MyTextBox.WndProc(msg=0x101 (WM_KEYUP) hwnd=0x70184 wparam=0x10 lparam=0xffffffffc02a0001 result=0x0)
			m = new Message () {
				HWnd = this.Handle,
				Msg = (int)Msg.WM_KEYUP,
				WParam = (IntPtr)0x10,
				LParam = (IntPtr)0//0xffffffffc02a0001
			};
			this.WndProc (ref m);

//			MyTextBox.WndProc(msg=0x100 (WM_KEYDOWN) hwnd=0x70184 wparam=0x53 lparam=0x1f0001 result=0x0)
//			MyTextBox.OnKeyDown({code=S, value=83, data=S, shift=False, alt=False, ctrl=False, suppress=False})
			m = new Message () {
				HWnd = this.Handle,
				Msg = (int)Msg.WM_KEYDOWN,
				WParam = (IntPtr)0x43,
				LParam = (IntPtr)0//0x1f0001
			};
			this.WndProc (ref m);

//			MyTextBox.WndProc(msg=0x102 (WM_CHAR) hwnd=0x70184 wparam=0x161 lparam=0x1f0001 result=0x0)
//			MyTextBox.OnKeyPress({char=š})
			m = new Message () {
				HWnd = this.Handle,
				Msg = (int)Msg.WM_CHAR,
				WParam = (IntPtr)0x10d,//0x161,
				LParam = (IntPtr)0//0x1f0001
			};
			this.WndProc (ref m);

//			MyTextBox.WndProc(msg=0x101 (WM_KEYUP) hwnd=0x70184 wparam=0x53 lparam=0xffffffffc01f0001 result=0x0)
			m = new Message () {
				HWnd = this.Handle,
				Msg = (int)Msg.WM_KEYUP,
				WParam = (IntPtr)0x43,
				LParam = (IntPtr)0//0xffffffffc01f0001
			};
			this.WndProc (ref m);
		}
	}
}

//WndProc(msg=0x100 (WM_KEYDOWN) hwnd=0x70184 wparam=0xbb lparam=0xd0001 result=0x0)
//OnKeyDown({code=Oemplus, value=187, data=Oemplus, Shift, shift=True, alt=False, ctrl=False, suppress=False})
//WndProc(msg=0x103 (WM_DEADCHAR) hwnd=0x70184 wparam=0x2c7 lparam=0xd0001 result=0x0)
//WndProc(msg=0x101 (WM_KEYUP) hwnd=0x70184 wparam=0xbb lparam=0xffffffffc00d0001 result=0x0)
//WndProc(msg=0x101 (WM_KEYUP) hwnd=0x70184 wparam=0x10 lparam=0xffffffffc02a0001 result=0x0)
//WndProc(msg=0x100 (WM_KEYDOWN) hwnd=0x70184 wparam=0x43 lparam=0x2e0001 result=0x0)
//OnKeyDown({code=C, value=67, data=C, shift=False, alt=False, ctrl=False, suppress=False})
//WndProc(msg=0x102 (WM_CHAR) hwnd=0x70184 wparam=0x10d lparam=0x2e0001 result=0x0)
//OnKeyPress({char=c})
//WndProc(msg=0x101 (WM_KEYUP) hwnd=0x70184 wparam=0x43 lparam=0xffffffffc02e0001 result=0x0)
