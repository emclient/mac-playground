using System;
using CoreGraphics;

namespace System.Drawing
{
	public interface IClientView
	{		
		CGRect ClientBounds { get; }
	}
}
