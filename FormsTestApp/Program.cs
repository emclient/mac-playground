using System;
using System.Reflection;
using MacApi.Posix;

[assembly: AssemblyTitle("FormsTest")]
[assembly: AssemblyProduct("FormsTest")]
[assembly: AssemblyCopyright("Copyright © 2006-2017 eM Client s.r.o.")]
[assembly: AssemblyCompany("eM Client s.r.o.")]

namespace FormsTestApp
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var code = FormsTest.Program.Main(args);
			LibC.exit(code);
		}
	}
}
