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
using System.Reflection;
using NUnit.Framework;

#if MONOMAC
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
#else
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
#endif

namespace TouchUnit.Bindings {

	public abstract class ApiSelectorTest : ApiBaseTest {
		
		protected int Errors;

		// not everything should be even tried
		
		protected virtual bool Skip (Type type)
		{
			// skip delegate (and other protocol references)
			foreach (object ca in type.GetCustomAttributes (false)) {
				if (ca is ModelAttribute)
					return true;
			}
			return false;
		}

		protected virtual bool Skip (Type type, string selectorName)
		{
			return false;
		}

		protected virtual bool CheckResponse (bool value, Type actualType, Type declaredType, ref string name)
		{
			if (value)
				return true;
			
			name = actualType.FullName + " : " + name;
			return false;
		}
		
		[Test]
		public void InstanceMethods ()
		{
			int n = 0;
			
			IntPtr responds_handle = Selector.GetHandle ("instancesRespondToSelector:");
			
			foreach (Type t in Assembly.GetTypes ()) {
				if (t.IsNested || !NSObjectType.IsAssignableFrom (t))
					continue;

				if (Skip (t) || SkipDueToAttribute (t))
					continue;
				
				FieldInfo fi = t.GetField ("class_ptr", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
				if (fi == null)
					continue; // e.g. *Delegate
				IntPtr class_ptr = (IntPtr) fi.GetValue (null);

				foreach (var m in t.GetMethods (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
					
					if (m.DeclaringType != t || SkipDueToAttribute (m))
						continue;

					foreach (object ca in m.GetCustomAttributes (true)) {
						ExportAttribute export = (ca as ExportAttribute);
						if (export == null)
							continue;

						string name = export.Selector;
						if (Skip (t, name))
							continue;
						
						bool result = Messaging.bool_objc_msgSend_IntPtr (class_ptr, responds_handle, Selector.GetHandle (name));
						bool response = CheckResponse (result, t, m.DeclaringType, ref name);
						if (!ContinueOnFailure)
							Assert.IsTrue (response, name);
						else if (!response) {
							CheckResponse (result, t, m.DeclaringType, ref name);
							Console.WriteLine ("[FAIL] {0}", name);
							Errors++;
						}
						n++;
					}
				}
			}
			Assert.AreEqual (0, Errors, "{0} errors found in {1} instance selector validated", Errors, n);
		}
		
		protected virtual void Dispose (NSObject obj, Type type)
		{
			obj.Dispose ();
		}
		
		// funny, this is how I envisioned the instance version... before hitting run :|
		protected virtual bool CheckStaticResponse (bool value, Type actualType, Type declaredType, ref string name)
		{
			if (value)
				return true;
			
			name = actualType.FullName + " : " + name;
			return false;
		}

		[Test]
		public void StaticMethods ()
		{
			Errors = 0;
			int n = 0;
			
			IntPtr responds_handle = Selector.GetHandle ("respondsToSelector:");
			
			foreach (Type t in Assembly.GetTypes ()) {
				if (t.IsNested || !NSObjectType.IsAssignableFrom (t))
					continue;

				if (Skip (t) || SkipDueToAttribute (t))
					continue;

				FieldInfo fi = t.GetField ("class_ptr", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
				if (fi == null)
					continue; // e.g. *Delegate
				IntPtr class_ptr = (IntPtr) fi.GetValue (null);
				
				foreach (var m in t.GetMethods (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)) {
					if (SkipDueToAttribute (m))
						continue;

					foreach (object ca in m.GetCustomAttributes (true)) {
						if (ca is ExportAttribute) {
							string name = (ca as ExportAttribute).Selector;
							bool result = Messaging.bool_objc_msgSend_IntPtr (class_ptr, responds_handle, Selector.GetHandle (name));
							bool response = CheckStaticResponse (result, t, m.DeclaringType, ref name);
							if (!ContinueOnFailure)
								Assert.IsTrue (response, name);
							else if (!response) {
								Console.WriteLine ("[FAIL] {0}", name);
								Errors++;
							}
							n++;
						}
					}
				}
			}
			Assert.AreEqual (0, Errors, "{0} errors found in {1} static selector validated", Errors, n);
		}
		
		protected virtual bool HasNoSetter (PropertyInfo p)
		{
			return false;
		}

		[Test]
		public void MissingSetters ()
		{
			Errors = 0;
			int n = 0;
			
			IntPtr responds_handle = Selector.GetHandle ("instancesRespondToSelector:");

			foreach (Type t in Assembly.GetTypes ()) {
				if (t.IsNested || !NSObjectType.IsAssignableFrom (t))
					continue;

				if (Skip (t))
					continue;

				// static properties
				
				FieldInfo fi = t.GetField ("class_ptr", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
				if (fi == null)
					continue; // e.g. *Delegate
				IntPtr class_ptr = (IntPtr) fi.GetValue (null);
				
				foreach (var p in t.GetProperties (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)) {
					
					if (p.DeclaringType != t || SkipDueToAttribute (p))
						continue;

					var mg = p.GetGetMethod ();
					var ms = p.GetSetMethod ();
					if (HasNoSetter (p) || (mg == null) || (ms != null))
						continue;

					if (SkipDueToAttribute (mg) || SkipDueToAttribute (ms))
						continue;

					foreach (object ca in mg.GetCustomAttributes (true)) {
						if (ca is ExportAttribute) {
							// if getter has [Export] assume it's valid (there's a test for this)
							string name = (ca as ExportAttribute).Selector;
							if (!Messaging.bool_objc_msgSend_IntPtr (class_ptr, responds_handle, Selector.GetHandle (name)))
								continue;

							n++;
							string setter_selector = String.Format ("set{0}{1}:", Char.ToUpperInvariant (name [0]), name.Substring (1));
							if (LogProgress)
								Console.WriteLine ("{0} {1} '{2} {3}' selector: {4}", n, t.Name, mg.IsStatic ? "static" : "instance", p, setter_selector);

							bool result = !Messaging.bool_objc_msgSend_IntPtr (class_ptr, responds_handle, Selector.GetHandle (setter_selector));
							if (!ContinueOnFailure)
								Assert.IsTrue (result, t.Name + " - " + setter_selector);
							else if (!result) {
								Console.WriteLine ("[FAIL] {0} {1}", t, setter_selector);
								Errors++;
							}
						}
					}
				}
			}
			Assert.AreEqual (0, Errors, "{0} potential errors found in {1} setters validated", Errors, n);
		}
	}
}
