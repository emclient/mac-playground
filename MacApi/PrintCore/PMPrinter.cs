using System;
using System.Linq;
using PrintCore;

namespace MacApi.PrintCore
{
	public static class PMPrinterEx
	{
		public static PMPrinter PrinterWithName(string name)
		{
			if (0 == PMServer.CreatePrinterList(out PMPrinter[] printers))
				return printers.FirstOrDefault(x => x.Name == name);
			return null;
		}

		public static PMPrinter DefaultPrinter()
		{
			if (0 == PMServer.CreatePrinterList(out PMPrinter[] printers))
				return printers.FirstOrDefault(x => x.IsDefault);
			return null;
		}
	}
}
