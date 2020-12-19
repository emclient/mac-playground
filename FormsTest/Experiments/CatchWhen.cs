using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

#if MAC
using Foundation;
#endif

namespace FormsTest.Experiments
{
	class MyException : ApplicationException
	{
		public int Code;
	}

	public class CatchWhen
	{
		public static void RunTest()
		{
			try
			{
				throw new MyException() { Code = 2 };
			}
			catch (MyException me) when (me.Code == 1)
			{
				Console.WriteLine($"catch (MyException me) when (me.Code == 1), me.Code:{me.Code}");
			}
			catch (MyException me) when (me.Code == 2)
			{
				Console.WriteLine($"(MyException me) when (me.Code == 2), me.Code:{me.Code}");
			}
			catch (MyException me)
			{
				Console.WriteLine($"(MyException me), me.Code:{me.Code}");
			}
		}
	}
}
