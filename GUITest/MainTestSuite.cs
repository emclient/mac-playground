using System;
using System.Windows.Forms;
using System.Threading;
using NUnit.Framework;
using MailClient.Common.UI.Controls.ControlToolStrip;
using WinApi;
using System.Collections.Generic;

namespace GUITest
{
    [TestFixture]
    public class MainTestSuite
	{
		private List<INPUT> createSingleInput(char character)
		{
			VirtualKeyShort virtualKeyCode = (VirtualKeyShort)Win32.VkKeyScan(character);
			List<INPUT> inputs = new List<INPUT>();
			inputs.Add(new INPUT()
			{
				type = InputType.KEYBOARD,
				U = new InputUnion()
				{
					ki = new KEYBDINPUT()
					{
						wVk = virtualKeyCode
					}
				}
			});
			inputs.Add(new INPUT()
			{
				type = InputType.KEYBOARD,
				U = new InputUnion()
				{
					ki = new KEYBDINPUT()
					{
						wVk = virtualKeyCode,
						dwFlags = KEYEVENTF.SCANCODE | KEYEVENTF.KEYUP
					}
				}
			});
			return inputs;
		}

		private void SendInputTest()
		{
			List<INPUT> inputs = new List<INPUT>();
			inputs.AddRange(createSingleInput('X'));
			inputs.AddRange(createSingleInput('X'));

			Thread.Sleep(2500);
			Win32.SendInput((uint)inputs.Count, inputs.ToArray(), INPUT.Size);
			Thread.Sleep(2500);
		}

		[Test]
        public void OpenNewMessageWindow()
        {
            Thread.Sleep(10000);

            ControlToolStripButton button = null;
            UI.Perform(() => { button = UI.GetMember<ControlToolStripButton>("formMain.stripButton_New", null); });
            ThrowIfNull(button, "Failed locating New button (formMain.stripButton_New)");

            UI.Perform(() => { button.PerformClick(); });

            var formSendMail = UI.WaitForForm("formSendMail");
            ThrowIfNull(formSendMail, "Failed opening formSendMail");

            UI.Type("volejnik@emclient.com");
            UI.Type("{ENTER}");
            UI.Type("{TAB}");
            UI.Type("OpenNewMessageWindowTest");
            UI.Type("{TAB}");
            UI.Type("Hezky den vam preje GUITest.");
            Thread.Sleep(1000);
            UI.Type("{ESC}");
            Thread.Sleep(1000);
            UI.Type("{ENTER}");
            Thread.Sleep(2000);
        }

        //[Test]
        public void BrowserTest()
		{
            var button = (Button)UI.GetMember("MainForm.button2", typeof(Button));
            UI.Perform(() => button.PerformClick());

            var webForm = UI.WaitForForm("WebForm");
            ThrowIfNull(webForm, "WebForm not found");

            Thread.Sleep(1000);
            UI.Perform(() => webForm.Close());
        }

        //[Test]
        public void DialogTest()
		{
			var button = (Button)UI.GetMember("MainForm.button3", typeof(Button));
            UI.Delay(1.0, () => { SendKeys.SendWait("{ESC}");  });
            UI.Perform(() => button.PerformClick());
		}

        //[Test]
        public void OpenFileDialogTest()
        {
            var combo = (ComboBox)UI.GetMember("MainForm.dialogTypeCombo", typeof(ComboBox));
            UI.Perform(() => combo.Focus());
            SendKeys.SendWait("{F4}");
            SendKeys.SendWait("{DOWN}");
            SendKeys.SendWait("{ENTER}");
            Thread.Sleep(500);

            var button = (Button)UI.GetMember("MainForm.button3", typeof(Button));
            UI.Delay(1.5, () => { SendKeys.SendWait("{ESC}"); });
            UI.Perform(() => button.PerformClick());
            Thread.Sleep(2000);
        }
        
		void ThrowIfNull(object instance, string message = "Instance not found")
		{
			if (instance == null)
				throw new ApplicationException(message);
		}
	}
}