using System;
using System.Windows.Forms;

namespace GUITest
{
	public static class ControlExtensions
	{
		public static void UIThread(this Control @this, Action code)
		{
			if (@this.InvokeRequired)
			{
				@this.Invoke(code);
			}
			else
			{
				code.Invoke();
			}
		}

        public static T UIThread<T>(this Control @this, Func<T> function)
        {
            if (@this.InvokeRequired)
            {
                return (T)@this.Invoke(function);
            }
            else
            {
                return function.Invoke();
            }
        }
    }
}
