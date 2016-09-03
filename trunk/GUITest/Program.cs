using System;

namespace GUITest
{
	class MainClass
	{
        [STAThread]
        public static void Main(string[] args)
		{
            TestRunner.Run(FormsTest.Program.Main, args, typeof(FormsTest.MainForm));
        }
    }
}
