using System;

namespace GUITest
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			var suite = new MainTestSuite().Initialize();

			FormsTest.Program.Main(args);

			suite.Done();
		}
	}
}
