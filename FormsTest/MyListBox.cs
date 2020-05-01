using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FormsTest.Extensions;

namespace FormsTest
{
	public class MyListBox : ListBox
	{
		protected override void OnKeyDown(KeyEventArgs e)
		{
			Console.WriteLine("MyListBox.OnKeyDown({0})", e.Dump());
			base.OnKeyDown(e);
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			Console.WriteLine("MyListBox.OnKeyPress({0})", e.Dump());
			base.OnKeyPress(e);
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg >= (int)Msg.WM_KEYFIRST && m.Msg <= (int)Msg.WM_KEYLAST)
				Console.WriteLine("MyListBox.WndProc({0})", m);

			base.WndProc(ref m);
		}
	}
}
