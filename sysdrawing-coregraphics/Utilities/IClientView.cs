using System;
#if MONOMAC
using MonoMac.CoreGraphics;
#else
using CoreGraphics;
#endif

namespace System.Drawing
{
	public interface IClientView
	{		
		CGRect ClientBounds { get; }
	}
}
