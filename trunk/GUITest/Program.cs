using System;

namespace GUITest
{
	class MainClass
	{
        [STAThread]
        public static void Main(string[] args)
		{
            TestRunner.Run("MailClient.Program,MailClient", "Main", args);
        }
    }
}
