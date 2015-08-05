//
// Test the generated API selectors against typos or non-existing cases
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012-2013 Xamarin Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;
using NUnit.Framework;

#if MONOMAC
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
#else
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
#endif

namespace TouchUnit.Bindings {

	public abstract class ApiSignatureTest : ApiBaseTest {

		[DllImport ("/usr/lib/libobjc.dylib")]
		// note: the returned string is not ours to free
		static extern IntPtr objc_getClass (string name);

		[DllImport ("/usr/lib/libobjc.dylib")]
		// note: the returned string is not ours to free
		static extern IntPtr method_getTypeEncoding (IntPtr method);

		[DllImport ("/usr/lib/libobjc.dylib")]
		static extern IntPtr class_getClassMethod (IntPtr klass, IntPtr selector);

		[DllImport ("/usr/lib/libobjc.dylib")]
		static extern IntPtr class_getInstanceMethod (IntPtr klass, IntPtr selector);

		protected  int Errors;

		protected string[] Split (string encoded, out int size)
		{
			List<string> elements = new List<string> ();
			int pos = 0;
			string s = Next (encoded, ref pos);
			int end = pos;
			while (Char.IsDigit (encoded [end]))
				end++;

			size = Int32.Parse (encoded.Substring (pos, end - pos));

			if (encoded [end] != '@' || encoded [end + 1] != '0' || encoded [end + 2] != ':') {
				if (!ContinueOnFailure)
					Assert.Fail ("Unexpected format, missing '@0:', inside '{0}'", encoded);
				return null;
			}

			pos = end + 3;

			while (s != null) {
				elements.Add (s);
				s = Next (encoded, ref pos);
			}
			return elements.ToArray ();
		}

		static string Next (string encoded, ref int pos)
		{
			// skip digits
			while (pos < encoded.Length && Char.IsDigit (encoded [pos]))
				pos++;
			if (pos >= encoded.Length)
				return null;

			StringBuilder sb = new StringBuilder ();
			int acc = 0;
			char c = encoded [pos];
			while (!Char.IsDigit (c) || acc > 0) {
				sb.Append (c);
				if (c == '{' || c == '(')
					acc++;
				else if (c == '}' || c == ')')
					acc--;
				if (++pos >= encoded.Length)
					break;
				c = encoded [pos];
			}
			return sb.ToString ();
		}

		protected virtual int Size (Type t)
		{
			if (!t.IsValueType)
				return IntPtr.Size; // platform
			if (t.IsEnum) {
				// Gendarme code has better (and more complex) logic
				switch (t.Name) {
				case "NSAlignmentOptions":
				case "NSEventMask":
				case "NSTextCheckingType":
				case "NSTextCheckingTypes":
					return 8;	// [u]long
				default:
					return 4;
				}
			}
			int size = Marshal.SizeOf (t);
			return t.IsPrimitive && size < 4 ? 4 : size;
		}

		protected virtual bool Skip (Type type)
		{
			return false;
		}

		protected virtual bool Skip (Type type, MethodBase method, string selector)
		{
			return false;
		}

		public int CurrentParameter { get; private set; }

		public MethodBase CurrentMethod { get; private set; }

		public string CurrentSelector { get; private set; }

		public Type CurrentType { get; private set; }

		[Test]
		public void Signatures ()
		{
			int n = 0;
			Errors = 0;
			
			foreach (Type t in Assembly.GetTypes ()) {

				if (t.IsNested || !NSObjectType.IsAssignableFrom (t))
					continue;

				if (Skip (t))
					continue;

				CurrentType = t;

				FieldInfo fi = t.GetField ("class_ptr", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
				if (fi == null)
					continue; // e.g. *Delegate
				IntPtr class_ptr = (IntPtr) fi.GetValue (null);

				foreach (MethodBase m in t.GetMethods (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)) 
					CheckMemberSignature (m, t, class_ptr, ref n);
				foreach (MethodBase m in t.GetConstructors (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)) 
					CheckMemberSignature (m, t, class_ptr, ref n);
			}
			Assert.AreEqual (0, Errors, "{0} errors found in {1} signatures validated", Errors, n);
		}

		void CheckMemberSignature (MethodBase m, Type t, IntPtr class_ptr, ref int n)
		{				
			var methodinfo = m as MethodInfo;
			var constructorinfo = m as ConstructorInfo;

			if (methodinfo == null && constructorinfo == null)
				return;

			if (m.DeclaringType != t)
				return;
			
			CurrentMethod = m;
			
			foreach (object ca in m.GetCustomAttributes (true)) {
				if (ca is ExportAttribute) {
					string name = (ca as ExportAttribute).Selector;
					
					if (Skip (t, m, name))
						continue;
					
					CurrentSelector = name;
					
					IntPtr sel = Selector.GetHandle (name);
					IntPtr method;
					if (methodinfo != null)
						method = m.IsStatic ? class_getClassMethod (class_ptr, sel) :  class_getInstanceMethod (class_ptr, sel);
					else
						method = class_getInstanceMethod (class_ptr, sel);
					IntPtr tenc = method_getTypeEncoding (method);
					string encoded = Marshal.PtrToStringAuto (tenc);
					
					if (LogProgress)
						Console.WriteLine ("{0} {1} '{2} {3}' selector: {4} == {5}", ++n, t.Name, methodinfo != null ? methodinfo.IsStatic ? "static" : "instance" : "ctor", m, name, encoded);
					
					// NSObject has quite a bit of stuff that's not usable (except by some class that inherits from it)
					if (String.IsNullOrEmpty (encoded))
						continue;
					
					int encoded_size = -1;
					string [] elements = null;
					try {
						elements = Split (encoded, out encoded_size);
					}
					catch {
					}
					if (elements == null) {
						if (LogProgress)
							Console.WriteLine ("[WARNING] Could not parse encoded signature for {0} : {1}", name, encoded);
						continue;
					}

					bool result;
					CurrentParameter = 0;

					if (methodinfo != null) {
						// check return value

						result = Check (elements [CurrentParameter], methodinfo.ReturnType);
						if (!ContinueOnFailure)
							Assert.IsTrue (result, "Return Value");
						else if (!result) {
							Console.WriteLine ("[FAIL] Return Value: {0}", name);
							Errors++;
						}
					}
					
					int size = 2 * IntPtr.Size; // self + selector (@0:)
					
					var parameters = m.GetParameters ();
					foreach (var p in parameters) {
						CurrentParameter++;
						var pt = p.ParameterType;
						if (n == 719)
							Console.WriteLine ();
						result = Check (elements [CurrentParameter], pt);
						if (!ContinueOnFailure)
							Assert.IsTrue (result, "Parameter {0}", CurrentParameter);
						else if (!result) {
							Console.WriteLine ("[FAIL] Parameter {0}: {1}", CurrentParameter, name);
							Errors++;
						}
						size += Size (pt);
					}
					
					// also ensure the encoded size match what MT (or XM) provides
					// catch API errors (and should catch most 64bits issues as well)
					result = size == encoded_size;
					if (!ContinueOnFailure)
						Assert.IsTrue (result, "Size {0} != {1}", encoded_size, size);
					else if (!result) {
						Console.WriteLine ("[FAIL] Size {0} != {1} for {2}", encoded_size, size, name);
						Errors++;
					}
				}
			}
		}

		protected virtual bool IsValidStruct (Type type, string structName)
		{
			switch (structName) {
			// MKPolygon 'static MonoTouch.MapKit.MKPolygon _FromPoints(IntPtr, Int32)' selector: polygonWithPoints:count: == @16@0:4^{?=dd}8I12
			// NSValue 'static MonoTouch.Foundation.NSValue FromCMTime(CMTime)' selector: valueWithCMTime: == @32@0:4{?=qiIq}8
			case "?":
				return type.IsValueType; // || (type.FullName == "System.IntPtr");
			case "CGRect":
				return type.FullName == "System.Drawing.RectangleF";
			case "CGSize":
				return type.FullName == "System.Drawing.SizeF";
			case "CGPoint":
				return type.FullName == "System.Drawing.PointF";
			case "opaqueCMFormatDescription":
				structName = "CMFormatDescription";
				break;
			case "opaqueCMSampleBuffer":
				structName = "CMSampleBuffer";
				break;
			case "_NSRange":
				structName = "NSRange";
				break;
			// textureWithContentsOfFile:options:queue:completionHandler: == v24@0:4@8@12^{dispatch_queue_s=}16@?20
			case "dispatch_queue_s":
				structName = "DispatchQueue";
				break;
			case "OpaqueCMClock":
				structName = "CMClock";
				break;
			case "OpaqueCMTimebase":
				structName = "CMTimebase";
				break;
			case "__CFRunLoop":
				structName = "CFRunLoop";
				break;
			case "_GLKVector4":
				structName = "Vector4";
				break;
			case "_GLKVector3":
				structName = "Vector3";
				break;
			case "_GLKMatrix3":
				structName = "Matrix3";
				break;
			case "_GLKMatrix4":
				structName = "Matrix4";
				break;
			case "__CVPixelBufferPool":
				structName = "CVPixelBufferPool";
				break;
			case "opaqueMTAudioProcessingTap":
				structName = "MTAudioProcessingTap";
				break;
			case "OpaqueMIDIEndpoint":
				structName = "MidiEndpoint";
				break;
			case "__CFDictionary":
				structName = "NSDictionary";
				break;
			case "_CGLContextObject":
				structName = "CGLContext";
				break;
			case "_CGLPixelFormatObject":
				structName = "CGLPixelFormat";
				break;
			}
			return type.Name == structName;
		}

		static Type inativeobject = typeof (INativeObject);

		bool Check (string encodedType, Type type)
		{
			char c = encodedType [0];

			if (encodedType.Length == 1)
				return Check (c, type);

			switch (c) {
			// GLKBaseEffect 'instance Vector4 get_LightModelAmbientColor()' selector: lightModelAmbientColor == (_GLKVector4={?=ffff}{?=ffff}{?=ffff}[4f])8@0:4
			case '(':
			case '{':
				string struct_name = encodedType.Substring (1, encodedType.IndexOf ('=') - 1);
				return IsValidStruct (type, struct_name);
			case '@':
				switch (encodedType [1]) {
				case '?':
					return (type.Name == "NSAction") || type.BaseType.FullName == "System.MulticastDelegate";
				default:
					return false;
				}
			case '^':
				switch (encodedType [1]) {
				case 'v':
					// NSOpenGLContext 'instance MonoMac.OpenGL.CGLContext get_CGLContext()' selector: CGLContextObj == ^v8@0:4
					if ((CurrentType.Name == "NSOpenGLContext") && (type.Name == "CGLContext"))
						return true;
					// NSOpenGLPixelFormat 'instance MonoMac.OpenGL.CGLPixelFormat get_CGLPixelFormat()' selector: CGLPixelFormatObj == ^v8@0:4
					if ((CurrentType.Name == "NSOpenGLPixelFormat") && (type.Name == "CGLPixelFormat"))
						return true;
					return (type.FullName == "System.IntPtr");
				case 'd':
				case 'f':
				case 'I':
				case 'i':
				case 'c':
				case 'q':
				case 'Q':
					return (type.FullName == "System.IntPtr") || Check (encodedType.Substring (1), type.GetElementType ());
				// NSInputStream 'instance Boolean GetBuffer(IntPtr ByRef, UInt32 ByRef)' selector: getBuffer:length: == c16@0:4^*8^I12
				case '*':
				case '{':
				// 10.7 only: NSArray 'static MonoMac.Foundation.NSArray FromObjects(IntPtr, Int32)' selector: arrayWithObjects:count: == @16@0:4^r@8I12
				case 'r':
					if (type.FullName == "System.IntPtr")
						return true;
					return Check (encodedType.Substring (1), type.IsByRef ? type.GetElementType () : type);
				case '@':
					return Check ('@', type.IsByRef ? type.GetElementType () : type);
				case '^':
					return (type.FullName == "System.IntPtr");
				default:
					return false;
				}
			case 'r':
				// const -> ignore
				// e.g. vectorWithValues:count: == @16@0:4r^f8L12
			case 'o':
				// out -> ignore
				// e.g. validateValue:forKey:error: == c20@0:4N^@8@12o^@16
			case 'N':
				// inout -> ignore
				// e.g. validateValue:forKey:error: == c20@0:4N^@8@12o^@16
			case 'V':
				// oneway -> ignore
				// e.g. NSObject 'instance Void NativeRelease()' selector: release == Vv8@0:4
				return Check (encodedType.Substring (1), type);
			default:
				return false;
			}
		}

		/// <summary>
		/// Check that specified encodedType match the type and caller.
		/// </summary>
		/// <param name="encodedType">Encoded type from the ObjC signature.</param>
		/// <param name="type">Managed type representing the encoded type.</param>
		/// <param name="caller">Caller's type. Useful to limit any special case.</param>
		protected virtual bool Check (char encodedType, Type type)
		{
			switch (encodedType) {
			case '@':
				return (type.IsArray || 										// NSArray
					(type.Name == "NSArray") || 						// NSArray
					(type.FullName == "System.String") || 						// NSString
					(type.FullName == "System.IntPtr") || 						// unbinded, e.g. internal
					(type.BaseType.FullName == "System.MulticastDelegate") || 	// completion handler -> delegate
					NSObjectType.IsAssignableFrom (type)) ||					// NSObject derived
					inativeobject.IsAssignableFrom (type);						// e.g. CGImage
			case 'c': // char, used for C# bool
				switch (type.FullName) {
				case "System.Boolean":
				case "System.SByte":
					return true;
				default:
					return false;
				}
			case 'C':
				switch (type.FullName) {
				case "System.Byte":
				// GLKBaseEffect 'instance Boolean get_ColorMaterialEnabled()' selector: colorMaterialEnabled == C8@0:4
				case "System.Boolean":
					return true;
				default:
					return false;
				}
			case 'd':
				return type.FullName == "System.Double";
			case 'f':
				return type.FullName == "System.Single";
			case 'i':
				return type.FullName == "System.Int32" || type.BaseType.FullName == "System.Enum";
			case 'I':
				return type.FullName == "System.UInt32" || type.BaseType.FullName == "System.Enum";
			case 'l':
				return type.FullName == "System.Int32";
			case 'L':
				return type.FullName == "System.UInt32";
			case 'q':
				return type.FullName == "System.Int64";
			case 'Q':
				return type.FullName == "System.UInt64";
			case 's':
				return type.FullName == "System.Int16";
			// unsigned 16 bits
			case 'S':
				switch (type.FullName) {
				case "System.UInt16":
				// NSString 'instance Char _characterAtIndex(Int32)' selector: characterAtIndex: == S12@0:4I8
				case "System.Char":
					return true;
				default:
					return false;
				}
			case ':':
				return type.Name == "Selector";
			case 'v':
				return type.FullName == "System.Void";
			case '?':
				return type.BaseType.FullName == "System.MulticastDelegate";	// completion handler -> delegate
			case '#':
				return type.FullName == "System.IntPtr" || type.Name == "Class";
			// CAMediaTimingFunction 'instance Void GetControlPointAtIndex(Int32, IntPtr)' selector: getControlPointAtIndex:values: == v16@0:4L8[2f]12
			case '[':
				return type.FullName == "System.IntPtr";
			// const uint8_t * -> IntPtr
			// NSCoder 'instance Void EncodeBlock(IntPtr, Int32, System.String)' selector: encodeBytes:length:forKey: == v20@0:4r*8I12@16
			case '*':
				return type.FullName == "System.IntPtr";
			default:
				return false;
			}
		}
	}
}