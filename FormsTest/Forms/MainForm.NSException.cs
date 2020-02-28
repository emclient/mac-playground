#if MAC
using System;
using System.Windows.Forms;
using Foundation;
using ObjCRuntime;

namespace FormsTest
{
	public partial class MainForm
	{
		public void NativeException()
		{
			try
			{
				new NSMutableDictionary().LowlevelSetObject(IntPtr.Zero, IntPtr.Zero);

				var nat = new NSException("Beautiful exception", "Just so", null);
				MacApi.LibObjc.bool_objc_msgSend(nat.Handle, Selector.GetHandle("raise"));
			}
			catch (Exception e)
			{
				Console.WriteLine($"Native exception caught:{e}");
			}
			finally
			{
				Console.WriteLine($"Finally block called");
			}
		}
	}
}
#endif
