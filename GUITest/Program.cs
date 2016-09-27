using System;
using System.Linq;

namespace GUITest
{
	class MainClass
	{
		internal const string DbFolderName = "eM Client - Test";

		internal static string DbLocationPath
		{
			get
			{
				return System.IO.Path.Combine(
							Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
							DbFolderName);
			}
		}

		[STAThread]
        public static int Main(string[] args)
		{
			string[] db_combined_args = args.Concat(new string[] { "/dblocation", DbLocationPath }).ToArray();

			return TestRunner.Run("MailClient.Program,MailClient", "Main", db_combined_args);
		}
    }
}
