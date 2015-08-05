//
// Base test fixture for introspection tests
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

#if MONOMAC
using MonoMac.Foundation;
#else
using MonoTouch.Foundation;
#endif

namespace TouchUnit.Bindings {
	
	public abstract class ApiBaseTest {
		
		/// <summary>
		/// Gets or sets a value indicating whether this test fixture will continue after failures.
		/// </summary>
		/// <value>
		/// <c>true</c> if continue on failure; otherwise, <c>false</c>.
		/// </value>
		public bool ContinueOnFailure { get; set; }
		
		/// <summary>
		/// Gets or sets a value indicating whether this test fixture will log it's progress.
		/// </summary>
		/// <value>
		/// <c>true</c> if log progress; otherwise, <c>false</c>.
		/// </value>
		public bool LogProgress { get; set; }
		
		static protected Type NSObjectType = typeof (NSObject);

		protected virtual bool Skip (Attribute attribute)
		{
			return false;
		}

		protected bool SkipDueToAttribute (MemberInfo member)
		{
			if (member == null)
				return false;

			foreach (Attribute attr in member.GetCustomAttributes (true)) {
				if (Skip (attr))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Gets the assembly on which the test fixture will reflect the NSObject-derived types.
		/// The default implementation returns the assembly where NSObject is defined, e.g.
		/// monotouch.dll or xammac.dll. 
		/// You need to override this method to return the binding assembly you wish to test.
		/// </summary>
		/// <value>
		/// The assembly on which the fixture will execute it's tests.
		/// </value>
		protected virtual Assembly Assembly {
			get { return NSObjectType.Assembly; }
		}
	}
}
