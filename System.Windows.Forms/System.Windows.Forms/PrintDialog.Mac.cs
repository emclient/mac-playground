#if MACDIALOGS
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
// Copyright (c) 2005-2006 Novell, Inc.
//
// Authors:
//	Someone
//	Jonathan Chambers (jonathan.chambers@ansys.com)
//	Jordi Mas i Hernandez (jordimash@gmail.com)
//

using System;
using System.ComponentModel;
using System.Drawing.Printing;
using AppKit;
using System.Windows.Forms.CocoaInternal;

namespace System.Windows.Forms
{
	[DefaultProperty("Document")]
	public sealed class PrintDialog : CommonDialog
	{
		PrintDocument document;
		PrinterSettings settings;

		public PrintDialog()
		{
			Reset();
		}

		public override void Reset()
		{
			settings = null;
			AllowPrintToFile = true;
			AllowSelection = false;
			AllowSomePages = false;
			PrintToFile = false;
			ShowHelp = false;
			ShowNetwork = true;
		}

		[DefaultValue(false)]
		public bool AllowCurrentPage
		{
			get;
			set;
		}

		[DefaultValue(true)]
		public bool AllowPrintToFile
		{
			get;
			set;
		}

		[DefaultValue(false)]
		public bool AllowSelection
		{
			get;
			set;
		}

		[DefaultValue(false)]
		public bool AllowSomePages
		{
			get;
			set;
		}

		[DefaultValue(null)]
		public PrintDocument Document
		{
			get
			{
				return document;
			}

			set
			{
				document = value;
				settings = (value == null) ? new PrinterSettings() : value.PrinterSettings;
			}
		}

		[Browsable(false)]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public PrinterSettings PrinterSettings
		{
			get
			{
				if (settings == null)
					settings = new PrinterSettings();
				return settings;
			}

			set
			{
				if (value != null && value == settings)
					return;

				settings = (value == null) ? new PrinterSettings() : value;
				document = null;
			}
		}

		[DefaultValue(false)]
		public bool PrintToFile
		{
			get;
			set;
		}

		[DefaultValue(true)]
		public bool ShowNetwork
		{
			get;
			set;
		}

		[DefaultValue(false)]
		public bool ShowHelp
		{
			get;
			set;
		}

		[MonoTODO("Stub, not implemented, will always use default dialog")]
		[DefaultValue(false)]
		public bool UseEXDialog
		{
			get;
			set;
		}

		protected override bool RunDialog(IntPtr hwndOwner)
		{
			var currentDirectory = Environment.CurrentDirectory;
			try
			{
				using (var context = new ModalDialogContext())
				{
					using (var panel = new NSPrintPanel())
					{
						var info = NSPrintInfo.SharedPrintInfo ?? new NSPrintInfo();
						if (NSPanelButtonType.Ok != (NSPanelButtonType)(int)panel.RunModalWithPrintInfo(info))
							return false;
					}
					return true;
				}
			}
			finally
			{
			}
		}
	}
}
#endif
