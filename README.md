# mac-playground

This is a mix of code that was developed during our effort to port eM Client to macOS. It is a hodgepodge of various projects and experiments. The notable ones are listed below. The code is released as-is.

To test the experiments:
* Open MacPlayground.sln in Visual Studio for Mac
* Set solution configuration to Xamarin - Debug
* Set default project to FormsTest.app
* Run

## System.Drawing

The `sysdrawing-coregraphics` directory contains a fork of the https://github.com/mono/sysdrawing-coregraphics project. We have enhanced the API surface to be compatible enough with System.Drawing to run System.Windows.Forms on top of it. In addition we have implemented some missing APIs and fixed compatibility issues with pixel rounding.

## System.Windows.Forms

The `mono/mcs` directory contains a fork of the Mono System.Windows.Forms implementation. It contains a Cocoa backend to allow applications run on 64-bit macOS systems. Layout code was heavily overhauled and debugged on both a test application and a full UI of eM Client. Further experiments were made with replacing some controls with their native counterparts (akin to https://github.com/Clancey/MonoMac.Windows.Form), which can be done on per-control basis.

## License

The code is released under the MIT X11 license unless noted otherwise in a specific source file. The original sysdrawing-coregraphics project didn't explicitely state the license. Any changes in this repository to the original code are published under the MIT X11 license.

### MIT X11 License

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.