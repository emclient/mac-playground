//
// Test the generated API `init` selectors are usable by the binding consumers
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
#else
using MonoTouch.Foundation;
#endif

namespace TouchUnit.Bindings {

	public abstract class ApiCtorInitTest : ApiBaseTest {

		string instance_type_name;
		protected int Errors;

		/// <summary>
		/// Gets or sets a value indicating whether this test fixture will log untested types.
		/// </summary>
		/// <value><c>true</c> if log untested types; otherwise, <c>false</c>.</value>
		public bool LogUntestedTypes { get; set; }

		/// <summary>
		/// Override this method if you want the test to skip some specific types.
		/// By default types decorated with [Model] will be skipped.
		/// </summary>
		/// <param name="type">The Type to be tested</param>
		protected virtual bool Skip (Type type)
		{
			// skip delegate (and other protocol references)
			foreach (object ca in type.GetCustomAttributes (false)) {
				if (ca is ModelAttribute)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Checks that the Handle property of the specified NSObject instance is not null (not IntPtr.Zero).
		/// </summary>
		/// <param name="obj">NSObject instance to validate</param>
		protected virtual void CheckHandle (NSObject obj)
		{
			bool result = obj.Handle != IntPtr.Zero;
			if (!ContinueOnFailure)
				Assert.IsTrue (result, instance_type_name + ".Handle");
			else if (!result) {
				Console.WriteLine ("[FAIL] {0} : Handle", instance_type_name);
				Errors++;
			}
		}

		/// <summary>
		/// Checks that ToString does not return null (not helpful for debugging) and that it does not crash.
		/// </summary>
		/// <param name="obj">NSObject instance to validate</param>
		protected virtual void CheckToString (NSObject obj)
		{
			bool result = obj.ToString () != null;
			if (!ContinueOnFailure)
				Assert.IsTrue (result, instance_type_name + ".ToString");
			else if (!result) {
				Console.WriteLine ("[FAIL] {0} : ToString", instance_type_name);
				Errors++;
			}
		}

		/// <summary>
		/// Dispose the specified NSObject instance. In some cases objects cannot be disposed safely.
		/// Override this method to keep them alive while the remaining tests execute.
		/// </summary>
		/// <param name="obj">NSObject instance to dispose</param>
		/// <param name="type">Type of the object, to be used if special logic is required.</param>
		protected virtual void Dispose (NSObject obj, Type type)
		{
			obj.Dispose ();
		}

		[Test]
		public void DefaultCtorAllowed ()
		{
			Errors = 0;
			int n = 0;
			
			foreach (Type t in Assembly.GetTypes ()) {
				if (t.IsAbstract || !NSObjectType.IsAssignableFrom (t))
					continue;
				
				if (Skip (t) || SkipDueToAttribute (t))
					continue;
				
				var ctor = t.GetConstructor (Type.EmptyTypes);
				if ((ctor == null) || ctor.IsAbstract) {
					if (LogUntestedTypes)
						Console.WriteLine ("[WARNING] {0} was skipped because it had no default constructor", t);
					continue;
				}
				
				instance_type_name = t.FullName;
				if (LogProgress)
						Console.WriteLine ("{0}. {1}", n, instance_type_name);

				NSObject obj = null;
				try {
					obj = ctor.Invoke (null) as NSObject;
					CheckHandle (obj);
					CheckToString (obj);
					Dispose (obj, t);
				}
				catch (TargetInvocationException tie) {
					// Objective-C exception thrown
					if (!ContinueOnFailure)
						throw;

					Console.WriteLine ("[FAIL] {0} : {1}", instance_type_name, tie.InnerException);
					Errors++;
				}
				n++;
			}
			Assert.AreEqual (0, Errors, "{0} potential errors found in {1} default ctor validated", Errors, n);
		}
	}
}
