//
// Copyright 2009, Novell, Inc.
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
using MonoMac.ObjCRuntime;
using MonoMac.Foundation;
using System;
using System.Collections.Generic;

namespace MonoMac.Foundation {

	[Register]
	internal class InternalNSNotificationHandler : NSObject  {
		NSNotificationCenter notificationCenter;
		Action<NSNotification> notify;
		
		public InternalNSNotificationHandler (NSNotificationCenter notificationCenter, Action<NSNotification> notify)
		{
			this.notificationCenter = notificationCenter;
			this.notify = notify;
		}
		
		[Export ("post:")]
		[Preserve (Conditional = true)]
		public void Post (NSNotification s)
		{
			notify (s);
			s.Dispose ();
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing && notificationCenter != null){
				notificationCenter.RemoveObserver (this);
				notificationCenter = null;
			}
			base.Dispose (disposing);
		}
	}

	// The C# overloads
	public partial class NSNotificationCenter {
		const string postSelector = "post:";

		class ObservedData 
		{
			public NSObject Observer;
			public string Name;
			public NSObject Object;
		}

		List <ObservedData> __mt_ObserverList_var = new List <ObservedData> ();

		[Advice ("Use AddObserver(NSString, Action<NSNotification>, NSObject)")]
		public NSObject AddObserver (string aName, Action<NSNotification> notify, NSObject fromObject)
		{
			return AddObserver (new NSString (aName), notify, fromObject);
		}
		
		public NSObject AddObserver (NSString aName, Action<NSNotification> notify, NSObject fromObject)
		{
			if (notify == null)
				throw new ArgumentNullException ("notify");
			
			var proxy = new InternalNSNotificationHandler (this, notify);
			
			AddObserver (proxy, new Selector (postSelector), aName, fromObject);

			return proxy;
		}

		public NSObject AddObserver (NSString aName, Action<NSNotification> notify)
		{
			return AddObserver (aName, notify, null);
		}

		[Advice ("Use AddObserver(NSString, Action<NSNotification>) instead")]
		public NSObject AddObserver (string aName, Action<NSNotification> notify)
		{
			return AddObserver (aName, notify, null);
		}

		[Advice ("Use AddObserver(NSObject, Selector, NSString, NSObject) instead")]
		public void AddObserver (NSObject observer, Selector aSelector, string aname, NSObject anObject)
		{
			AddObserver (observer, aSelector, new NSString (aname), anObject);
		}

		public void RemoveObservers (IEnumerable<NSObject> keys)
		{
			if (keys == null)
				return;
			foreach (var k in keys)
				RemoveObserver (k);
		}

		void AddObserverToList (NSObject observer, string aName, NSObject anObject)
		{
			__mt_ObserverList_var.Add (new ObservedData { Observer = observer, Name = aName, Object = anObject });
		}

		void RemoveObserversFromList (NSObject observer, string aName, NSObject anObject)
		{
			for (int i = __mt_ObserverList_var.Count - 1; i >= 0; i--) {
				ObservedData od = __mt_ObserverList_var [i];

				if (observer != od.Observer)
					continue;

				if (aName != null && aName != od.Name)
					continue;

				if (anObject != null && anObject != od.Object)
					continue;

				__mt_ObserverList_var.RemoveAt (i);
			}
		}
	}

	public class NSNotificationEventArgs : EventArgs {
		public NSNotification Notification { get; private set; }
		public NSNotificationEventArgs (NSNotification notification)
		{
			Notification = notification;
		}
	}
}
