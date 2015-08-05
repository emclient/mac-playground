//
// Test the generated API fields (e.g. against typos or unexisting values for the platform)
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

	[Preserve (AllMembers = true)]
	public abstract class ApiFieldTest : ApiBaseTest {
#if MONOMAC
		const string NSStringType = "MonoMac.Foundation.NSString";
#else
		const string NSStringType = "MonoTouch.Foundation.NSString";
#endif
		protected int Errors;

		/// <summary>
		/// Override if you want to skip testing the specified type.
		/// </summary>
		/// <param name="type">Type to be tested</param>
		protected virtual bool Skip (Type type)
		{
			return false;
		}

		/// <summary>
		/// Override if you want to skip testing the specified property.
		/// </summary>
		/// <param name="property">Property to be tested</param>
		protected virtual bool Skip (PropertyInfo property)
		{
			return false;
		}

		// check generated code, which are static properties, e.g.
		// [Field ("kCGImagePropertyIPTCObjectTypeReference")]
		// NSString IPTCObjectTypeReference { get; }
		bool CheckAgainstNull (PropertyInfo p, out string name)
		{
			name = String.Empty;
			var g = p.GetGetMethod (true);
			if (!g.IsStatic)
				return true;

			if (Skip (p) || SkipDueToAttribute (p))
				return true;
			
			try {
				// if it return nulls then it could be a typo...
				// or something not available in the executing version of iOS
				bool result = g.Invoke (null, null) != null;
				if (!result)
					name = p.DeclaringType.FullName + "." + p.Name;
				return result;
			}
			catch (Exception e) {
				Console.WriteLine ("[FAIL] Exception on '{0}' : {1}", p, e);
				name = p.DeclaringType.FullName + "." + p.Name;
				return false;
			}
		}
		
		[Test]
		public void NonNullNSStringFields ()
		{
			Errors = 0;
			int c = 0, n = 0;
			foreach (Type t in Assembly.GetTypes ()) {
				if (Skip (t) || SkipDueToAttribute (t))
					continue;

				if (LogProgress)
					Console.WriteLine ("{0}. {1}", c++, t.FullName);

				foreach (var p in t.GetProperties (BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
					// looking for properties with getters only
					if (p.CanWrite || !p.CanRead)
						continue;
					
					// FIXME: testing NSString only (vast majority but there are others)
					if (p.PropertyType.FullName != NSStringType)
						continue;

					string name;
					bool result = CheckAgainstNull (p, out name);
					if (!ContinueOnFailure)
						Assert.IsTrue (result, name);
					else if (!result) {
						Console.WriteLine ("[FAIL] {0}", name);
						Errors++;
					}
					n++;
				}
			}
			Assert.AreEqual (0, Errors, "{0} errors found in {1} fields validated", Errors, n);
		}
	}
}
