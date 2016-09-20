using System;
using System.Windows.Forms;
using System.Threading;
using NUnit.Framework;
using MailClient.Common.UI.Controls.ControlToolStrip;

namespace GUITest
{
    // Note:
    // To avoid blocking the UI, all tests run in the background thread.
    // Therefore, you have to use UI.Perform() for invoking actions on UI elements.

    [TestFixture]
    public class MainTestSuite
	{
		[Test]
        public void NewMessageTest()
        {
            Thread.Sleep(2000);

            ControlToolStripButton button = null;
            UI.Perform(() => { button = UI.GetMember<ControlToolStripButton>("formMain.stripButton_New", null); });
            ThrowIfNull(button, "Failed locating New button (formMain.stripButton_New)");

            //UI.Perform(() => { button.PerformClick(); });
            UI.Click(button);

            var formSendMail = UI.WaitForForm("formSendMail");
            ThrowIfNull(formSendMail, "Failed opening formSendMail");
            
            UI.Type("volejnik@emclient.com{ENTER}{TAB}"); // {ENTER} closes the list of suggested addresses (if open).
            UI.Type("New Message Test{TAB}");
            UI.Type("Zdravi te GUITest.{ENTER}");
			UI.Type("`1234567890-=[]\\;',./{ENTER}");
			UI.Type("{~}!@#${%}{^}&*{(}{)}_{+}{{}{}}|:\"<>?{ENTER}"); // Special characters have to be escaped using curly braces: +^%{}()~

            // Try sending keystrokes using direct calls of Win32.SendInput - not implemented on the Mac yet.
            UI.Type('X');
            UI.Type('y');
            UI.Type('.');

            Thread.Sleep(1000);
			UI.Type("{ESC}"); // Invokes closing the window (with confirmation dialog))

			var taskForm = UI.WaitForForm("taskForm");
			ThrowIfNull(taskForm, "Confirmation dialog hasn't appeared.");

			var noButton = UI.FindControl(taskForm, "No");
			ThrowIfNull(noButton, "'No' button not found.");
			UI.Click(noButton);

			//UI.Type("{ENTER}"); // Confirms the save dialog and allows closing the e-mail window.

            ThrowIfNot(UI.WaitForFormClosed(formSendMail), "formSendMail failed to close.");
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
        
		public static void ThrowIfNull(object instance, string message = "Instance not found")
		{
			if (instance == null)
				throw new ApplicationException(message);
		}

        public static void ThrowIfNot(bool result, string message)
        {
            if (!result)
                throw new ApplicationException(message);
        }


    }
}