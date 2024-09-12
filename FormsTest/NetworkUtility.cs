using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace FormsTest
{
	public class NetworkUtility
	{
		private static readonly object staticLock = new object();
		private static event EventHandler<EventArgs>? networkAvailabilityChanged;
		private static bool oldIsNetworkAvailable;

#if !MAC
		private static class NativeMethods
		{
			[DllImport("wininet", SetLastError = true)]
			public extern static bool InternetGetConnectedState(out int lpdwFlags, int dwReserved);
		}

		private static bool RealIsNetworkAvailable
		{
			get
			{
				try
				{
					foreach (NetworkInterface iface in NetworkInterface.GetAllNetworkInterfaces())
					{
						if (iface.OperationalStatus == OperationalStatus.Up &&
							iface.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
							iface.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
						    iface.NetworkInterfaceType != NetworkInterfaceType.Unknown) // On OS X some tunnel interfaces (eg. utun0) are reported as Unknown.
						{
							var ipProperties = iface.GetIPProperties();
							bool hasValidAddress = false;
							if (ipProperties.GatewayAddresses.Count > 0)
							{
								bool isDnsEligibleFailed = false;
								foreach (var uni in ipProperties.UnicastAddresses)
								{
									// Skip 169.254.xxx.xxx addresses
									if (uni.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
									{
										byte[] addr = uni.Address.GetAddressBytes();
										if (addr.Length == 4 && (addr[0] != 169 || addr[1] != 254))
										{
											hasValidAddress = true;
											break;
										}
									}
									else
									{
										try
										{
											if (!isDnsEligibleFailed && uni.IsDnsEligible) // throws on .net6-macos
											{
												//Logic for detecting IsDnsEligible in System.Net.NetworkInformation.SystemIPAddressInformation is incorrect, 
												// -> use it just as fallback
												hasValidAddress = true;
												break;
											}
										}
										catch
										{
											isDnsEligibleFailed = true;
										}
									}
								}

								if (isDnsEligibleFailed)
									hasValidAddress |= ipProperties.HasNonLoopbackDns();

								if (hasValidAddress)
									return true;
							}
						}
					}
				}
				catch (Exception e)
				{
					if (e is NetworkInformationException || // Something is screwed up really bad. 
						e is NullReferenceException) // Old .NET Framework
					{
						int flags;
						return NativeMethods.InternetGetConnectedState(out flags, 0);
					}
				}
				return false;
			}
		}

		static void AddNetworkChangeHandler()
		{
			NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
		}

		static void RemoveNetworkChangeHandler()
		{
			NetworkChange.NetworkAddressChanged -= NetworkChange_NetworkAddressChanged;
		}

		static void NetworkChange_NetworkAddressChanged(object? sender, EventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("NetworkChange_NetworkAddressChanged");
			bool isNetworkAvailable = RealIsNetworkAvailable;
			if (oldIsNetworkAvailable != isNetworkAvailable)
			{
				oldIsNetworkAvailable = isNetworkAvailable;
				networkAvailabilityChanged?.Invoke(sender, isNetworkAvailable ? new OnlineStatusEventArgs() : new OnlineStatusEventArgs(OfflineReason.NetworkAvailability));
			}
		}
#endif

		public static bool IsNetworkAvailable
		{
			get
			{
				lock (staticLock)
				{
					if (networkAvailabilityChanged == null)
						return RealIsNetworkAvailable;
					return oldIsNetworkAvailable;
				}
			}
		}

		public static event EventHandler<EventArgs> NetworkAvailabilityChanged
		{
			add
			{
				lock (staticLock)
				{
					if (networkAvailabilityChanged == null)
					{
						oldIsNetworkAvailable = RealIsNetworkAvailable;
						AddNetworkChangeHandler();
					}
					networkAvailabilityChanged += value;
				}
			}
			remove
			{
				lock (staticLock)
				{
					networkAvailabilityChanged -= value;
					if (networkAvailabilityChanged == null)
					{
						try
						{
							RemoveNetworkChangeHandler();
						}
						catch (NullReferenceException)
						{
							//NullReferenceException bugs are reported when removing the delegate - just don't crash..
							System.Diagnostics.Debug.Assert(false);
						}
					}
				}
			}
		}

#if MAC
		static SystemConfiguration.NetworkReachability? reachability;

		static void AddNetworkChangeHandler()
		{
			if (reachability == null)
				reachability = new SystemConfiguration.NetworkReachability("www.apple.com");

			reachability.SetNotification(ReachabilityDidChange);
			reachability.Schedule(CoreFoundation.CFRunLoop.Main, CoreFoundation.CFRunLoop.ModeDefault);
		}

		static void RemoveNetworkChangeHandler()
		{
			if (reachability != null)
				reachability.Unschedule(CoreFoundation.CFRunLoop.Main, CoreFoundation.CFRunLoop.ModeDefault);
		}

		static void ReachabilityDidChange(SystemConfiguration.NetworkReachabilityFlags flags)
		{
			System.Diagnostics.Debug.WriteLine($"ReachabilityDidChange:{flags}");
			var reachable = 0 != (flags & SystemConfiguration.NetworkReachabilityFlags.Reachable);
			if (oldIsNetworkAvailable != reachable)
			{
				oldIsNetworkAvailable = reachable;
				networkAvailabilityChanged?.Invoke(reachability, EventArgs.Empty);
			}
		}

		private static bool RealIsNetworkAvailable
		{
			get
			{
				if (reachability == null)
					reachability = new SystemConfiguration.NetworkReachability("www.apple.com");

				reachability.GetFlags(out var flags);
				return 0 != (flags & SystemConfiguration.NetworkReachabilityFlags.Reachable);
			}
		}
#endif
	}

	static class Extensions
	{
		public static bool HasNonLoopbackDns(this IPInterfaceProperties ipProperties)
		{
			var dnsAddresses = ipProperties.DnsAddresses;
			if (dnsAddresses?.Count > 0)
				foreach (var dns in dnsAddresses)
					if (!IPAddress.IsLoopback(dns))
						return true;
			return false;
		}
	}
}
