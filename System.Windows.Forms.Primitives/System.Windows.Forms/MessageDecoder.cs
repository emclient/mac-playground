// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Windows.Forms
{
    /// <summary>
    ///  Decodes Windows messages. This is in a separate class from Message so we can avoid
    ///  loading it in the 99% case where we don't need it.
    /// </summary>
    internal static class MessageDecoder
    {
        /// <summary>
        ///  Returns the symbolic name of the msg value, or null if it isn't one of the
        ///  existing constants.
        /// </summary>
        private static string? MsgToString(int msg)
        {
            // FIXME
            return msg.ToString();
        }

        private static string Parenthesize(string? input)
        {
            if (input == null)
            {
                return string.Empty;
            }

            return " (" + input + ")";
        }

        public static string ToString(Message message)
        {
            return ToString(message.HWnd, message.Msg, message.WParam, message.LParam, message.Result);
        }

        public static string ToString(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam, IntPtr result)
        {
            string id = Parenthesize(MsgToString(msg));

            /*string lDescription = string.Empty;
            if (msg == (int)WM.PARENTNOTIFY)
            {
                lDescription = Parenthesize(MsgToString(PARAM.LOWORD(wparam)));
            }*/

            return
                "msg=0x" + Convert.ToString(msg, 16) + id +
                " hwnd=0x" + Convert.ToString((long)hWnd, 16) +
                " wparam=0x" + Convert.ToString((long)wparam, 16) +
                " lparam=0x" + Convert.ToString((long)lparam, 16) + //lDescription +
                " result=0x" + Convert.ToString((long)result, 16);
        }
    }
}
