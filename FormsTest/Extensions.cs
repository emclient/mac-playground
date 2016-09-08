using System.Windows.Forms;

namespace FormsTest.Extensions
{
	public static class EventArgsExt
	{
		public static string Dump(this KeyEventArgs e)
		{
			return string.Format(
				"{{code={0}, value={1}, data={2}, shift={3}, alt={4}, ctrl={5}, suppress={6}}}",
				e.KeyCode, e.KeyValue, e.KeyData, e.Shift, e.Alt, e.Control, e.SuppressKeyPress);
		}

		public static string Dump(this KeyPressEventArgs e)
		{
			return string.Format("{{char={0}}}", e.KeyChar);
		}
	}
}