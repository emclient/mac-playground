// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez	(jordi@ximian.com)
//	Benjamin Dasnois	(benjamin.dasnois@gmail.com)
//	Robert Thompson		(rmt@corporatism.org)
//	Peter Bartok		(pbartok@novell.com)
//
// TODO:
//	- Add support for MessageBoxOptions and MessageBoxDefaultButton.
//


// NOT COMPLETE

using System;
using System.Drawing;
using System.Globalization;
using System.Resources;

#if MONOMAC || XAMARINMAC
using MessageBoxForm = System.Windows.Forms.MessageBoxFormMac;
#else
using MessageBoxForm = System.Windows.Forms.MessageBoxFormNative;
#endif

namespace System.Windows.Forms
{
	public class MessageBox
	{
		#region	Constructors
		private MessageBox ()
		{
		}
		#endregion	// Constructors

		#region Public Static Methods
		public static DialogResult Show (string text)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None);

			return form.RunDialog ();
		}

		public static DialogResult Show (IWin32Window owner, string text)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None);
				
			return form.RunDialog ();
		}

		public static DialogResult Show (string text, string caption)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None);

			return form.RunDialog ();
		}

		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption, buttons, MessageBoxIcon.None);
				
			return form.RunDialog ();
		}

		public static DialogResult Show (IWin32Window owner, string text, string caption,
						 MessageBoxButtons buttons)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption, buttons, MessageBoxIcon.None);
				
			return form.RunDialog ();
		}

		public static DialogResult Show (IWin32Window owner, string text, string caption,
						 MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption, buttons, icon);
				
			return form.RunDialog ();
		}


		public static DialogResult Show (IWin32Window owner, string text, string caption)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None);
				
			return form.RunDialog ();
		}


		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons,
				MessageBoxIcon icon)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption, buttons, icon);
				
			return form.RunDialog ();
		}

		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons,
						 MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{

			MessageBoxForm form = new MessageBoxForm (null, text, caption, buttons,
								  icon, defaultButton, MessageBoxOptions.DefaultDesktopOnly, false);
				
			return form.RunDialog ();
		}

		public static DialogResult Show (IWin32Window owner, string text, string caption,
						 MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption, buttons,
								  icon, defaultButton, MessageBoxOptions.DefaultDesktopOnly, false);
				
			return form.RunDialog ();
		}

		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption, buttons,
								  icon, defaultButton, options, false);
				
			return form.RunDialog ();
		}

		public static DialogResult Show (IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption, buttons,
								  icon, defaultButton, options, false);
				
			return form.RunDialog ();
		}
		#endregion	// Public Static Methods

		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton, MessageBoxOptions options,
						 bool displayHelpButton)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption, buttons,
								  icon, defaultButton, options, displayHelpButton);
			return form.RunDialog ();
		}
		
		[MonoTODO ("Help is not implemented")]
		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton, MessageBoxOptions options,
						 string helpFilePath)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption, buttons,
								  icon, defaultButton, options, true);
			form.SetHelpData (helpFilePath, null, HelpNavigator.TableOfContents, null);
			return form.RunDialog ();
		}
		
		[MonoTODO ("Help is not implemented")]
		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton, MessageBoxOptions options,
						 string helpFilePath, string keyword)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption, buttons,
								  icon, defaultButton, options, true);
			form.SetHelpData (helpFilePath, keyword, HelpNavigator.TableOfContents, null);
			return form.RunDialog ();
		}
		
		[MonoTODO ("Help is not implemented")]
		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton, MessageBoxOptions options,
						 string helpFilePath, HelpNavigator navigator)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption, buttons,
								  icon, defaultButton, options, true);
			form.SetHelpData (helpFilePath, null, navigator, null);
			return form.RunDialog ();
		}
		
		[MonoTODO ("Help is not implemented")]
		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton, MessageBoxOptions options,
						 string helpFilePath, HelpNavigator navigator, object param)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption, buttons,
								  icon, defaultButton, options, true);
			form.SetHelpData (helpFilePath, null, navigator, param);
			return form.RunDialog ();
		}
		
		[MonoTODO ("Help is not implemented")]
		public static DialogResult Show (IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton, MessageBoxOptions options,
						 string helpFilePath)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption, buttons,
								  icon, defaultButton, options, true);
			form.SetHelpData (helpFilePath, null, HelpNavigator.TableOfContents, null);
			return form.RunDialog ();
		}
		
		[MonoTODO ("Help is not implemented")]
		public static DialogResult Show (IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton, MessageBoxOptions options,
						 string helpFilePath, string keyword)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption, buttons,
								  icon, defaultButton, options, true);
			form.SetHelpData (helpFilePath, keyword, HelpNavigator.TableOfContents, null);
			return form.RunDialog ();
		}
		
		[MonoTODO ("Help is not implemented")]
		public static DialogResult Show (IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton, MessageBoxOptions options,
						 string helpFilePath, HelpNavigator navigator)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption, buttons,
								  icon, defaultButton, options, true);
			form.SetHelpData (helpFilePath, null, navigator, null);
			return form.RunDialog ();
		}
		
		[MonoTODO ("Help is not implemented")]
		public static DialogResult Show (IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton, MessageBoxOptions options,
						 string helpFilePath, HelpNavigator navigator, object param)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption, buttons,
								  icon, defaultButton, options, true);
			form.SetHelpData (helpFilePath, null, navigator, param);
			return form.RunDialog ();
		}
	}
}

