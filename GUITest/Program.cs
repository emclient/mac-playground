using System;

namespace GUITest
{
	class MainClass
	{
        [STAThread]
        public static int Main(string[] args)
		{
            return TestRunner.Run("MailClient.Program,MailClient", "Main", args);
        }
    }
}
