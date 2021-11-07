using System.Diagnostics;

namespace FormsTest.Experiments
{
	public class PropertyParserTest
	{
		public static void Run()
		{
			var reader = new MailClient.Import.MailApp.Reader();
			reader.StringRead += (o, e) => { Debug.WriteLine(e.Value); };
			reader.ReadAll(TestResources.Condensed);
			Debug.WriteLine("-------");
			reader.ReadAll(TestResources.Regular);
			Debug.WriteLine("-------");

			var parser = new MailClient.Import.MailApp.MailAppPropertyParser();
			parser.Parse(TestResources.Regular);
		}
	}
}
