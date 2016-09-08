//
// CoreLocation2.cs: Extra support for CoreLocation
//
// Authors:
//   Miguel de Icaza (miguel@gnome.org)
//
// The class can be either constructed from a string (from user code)
// or from a handle (from iphone-sharp.dll internal calls).  This
// delays the creation of the actual managed string until actually
// required
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using MonoMac.ObjCRuntime;

namespace MonoMac.CoreLocation {

	public partial class CLLocation {
		public static readonly double AccuracyBest;
		public static readonly double AccuracyNearestTenMeters;
		public static readonly double AccuracyHundredMeters;
		public static readonly double AccuracyKilometer;
		public static readonly double AccuracyThreeKilometers;
		public static readonly double AccurracyBestForNavigation;

		static CLLocation ()
		{
			var handle = Libraries.CoreLocation.Handle;
			if (handle == IntPtr.Zero)
				return;

			AccurracyBestForNavigation = Dlfcn.GetDouble (handle, "kCLLocationAccuracyBestForNavigation");
			AccuracyBest = Dlfcn.GetDouble (handle, "kCLLocationAccuracyBest");
			AccuracyNearestTenMeters = Dlfcn.GetDouble (handle, "kCLLocationAccuracyNearestTenMeters");
			AccuracyHundredMeters = Dlfcn.GetDouble (handle, "kCLLocationAccuracyHundredMeters");
			AccuracyKilometer = Dlfcn.GetDouble (handle, "kCLLocationAccuracyKilometer");
			AccuracyThreeKilometers = Dlfcn.GetDouble (handle, "kCLLocationAccuracyThreeKilometers");
		}
	}
}
