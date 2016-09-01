using System;

namespace GUITest
{
	class MainClass
	{
        [STAThread]
        public static void Main(string[] args)
		{
			var suite = new MainTestSuite().Initialize();

			FormsTest.Program.Main(args);

			suite.Done();
		}
	}
}
