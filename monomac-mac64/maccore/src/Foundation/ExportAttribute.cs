//
// ExportAttribute.cs: The Export attribute
//
// Authors:
//   Geoff Norton
//
// Copyright 2009, Novell, Inc.
// Copyright 2010, Novell, Inc.
// Copyright 2011, 2012 Xamarin Inc
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
//
using System;
using System.Globalization;
using System.Reflection;
using MonoMac.ObjCRuntime;

namespace MonoMac.Foundation {

	[AttributeUsage (AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property)]
	public class ExportAttribute : Attribute {
		string selector;
		ArgumentSemantic semantic;

		public ExportAttribute() {}
		public ExportAttribute(string selector) {
			this.selector = selector;
			this.semantic = ArgumentSemantic.None;
		}
		
		public ExportAttribute(string selector, ArgumentSemantic semantic) {
			this.selector = selector;
			this.semantic = semantic;
		}

		public string Selector {
			get { return this.selector; }
			set { this.selector = value; }
		}

		public ArgumentSemantic ArgumentSemantic {
			get { return this.semantic; }
			set { this.semantic = value; }
		}

		public bool IsVariadic {
			get;
			set;
		}

//#if MONOMAC
		public ExportAttribute ToGetter (PropertyInfo prop) {
			if (string.IsNullOrEmpty (Selector))
				Selector = prop.Name;
			return new ExportAttribute (selector, semantic);
		}

		public ExportAttribute ToSetter (PropertyInfo prop) {
			if (string.IsNullOrEmpty (Selector))
				Selector = prop.Name;
			return new ExportAttribute (string.Format ("set{0}{1}:", char.ToUpperInvariant (selector [0]), selector.Substring (1)), semantic); 
		}
//#endif
	}

	[AttributeUsage (AttributeTargets.Property)]
	public sealed class OutletAttribute : ExportAttribute {
		public OutletAttribute () {}
		public OutletAttribute (string name) : base (name) {}
	}

	[AttributeUsage (AttributeTargets.Method)]
	public sealed class ActionAttribute : ExportAttribute {
		public ActionAttribute () {}
		public ActionAttribute (string selector) : base (selector) {}
	}
}
