using System;
using System.Windows.Automation;
using System.Windows.Automation.Provider;

namespace WinApi
{
	public static partial class Win32
	{
		public static IntPtr UiaReturnRawElementProvider(IntPtr hwnd, IntPtr wParam, IntPtr lParam, IRawElementProviderSimple provider)
		{
			return IntPtr.Zero;
		}

		public static int UiaHostProviderFromHwnd(IntPtr handle, out IRawElementProviderSimple host)
		{
			host = null;
			return 0;
		}

		public static int UiaGetReservedNotSupportedValue(out object notSupportedValue)
		{
			notSupportedValue = null;
			return 0;
		}

		public static int UiaRaiseAutomationPropertyChangedEvent(IRawElementProviderSimple provider, int id, object oldValue, object newValue)
		{
			return 0;
		}

		public static int UiaRaiseAutomationEvent(IRawElementProviderSimple provider, int id)
		{
			return 0;
		}

		public static int UiaRaiseStructureChangedEvent(IRawElementProviderSimple provider, StructureChangeType structureChangeType, int[] runtimeId, int runtimeIdLen)
		{
			return 0;
		}

		public static int UiaRaiseAsyncContentLoadedEvent(IRawElementProviderSimple provider, AsyncContentLoadedState asyncContentLoadedState, double PercentComplete)
		{
			return 0;
		}

		public static bool UiaClientsAreListening()
		{
			return false;
		}

		public static bool UiaDisconnectProvider(IRawElementProviderSimple provider)
		{
			return true;
		}

		public static int UiaLookupId(AutomationIdentifierType type, ref Guid guid)
		{
			return 0;
		}

		public static int UiaGetReservedMixedAttributeValue(out object mixedAttributeValue)
		{
			mixedAttributeValue = null;
			return 0;
		}
	}
}
