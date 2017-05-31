using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace WinApi
{
	public static partial class Win32
	{

		[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
		public struct SCRIPT_PROPERTIES
		{

			/// langid : 16
			///fNumeric : 1
			///fComplex : 1
			///fNeedsWordBreaking : 1
			///fNeedsCaretInfo : 1
			///bCharSet : 8
			///fControl : 1
			///fPrivateUseArea : 1
			///fNeedsCharacterJustify : 1
			///fInvalidGlyph : 1
			///fInvalidLogAttr : 1
			///fCDM : 1
			///fAmbiguousCharSet : 1
			///fClusterSizeVaries : 1
			///fRejectInvalid : 1
			public uint bitvector1;

			public uint langid
			{
				get
				{
					return ((uint)((this.bitvector1 & 65535u)));
				}
				set
				{
					this.bitvector1 = ((uint)((value | this.bitvector1)));
				}
			}

			public uint fNumeric
			{
				get
				{
					return ((uint)(((this.bitvector1 & 65536u)
								/ 65536)));
				}
				set
				{
					this.bitvector1 = ((uint)(((value * 65536)
								| this.bitvector1)));
				}
			}

			public uint fComplex
			{
				get
				{
					return ((uint)(((this.bitvector1 & 131072u)
								/ 131072)));
				}
				set
				{
					this.bitvector1 = ((uint)(((value * 131072)
								| this.bitvector1)));
				}
			}

			public uint fNeedsWordBreaking
			{
				get
				{
					return ((uint)(((this.bitvector1 & 262144u)
								/ 262144)));
				}
				set
				{
					this.bitvector1 = ((uint)(((value * 262144)
								| this.bitvector1)));
				}
			}

			public uint fNeedsCaretInfo
			{
				get
				{
					return ((uint)(((this.bitvector1 & 524288u)
								/ 524288)));
				}
				set
				{
					this.bitvector1 = ((uint)(((value * 524288)
								| this.bitvector1)));
				}
			}

			public uint bCharSet
			{
				get
				{
					return ((uint)(((this.bitvector1 & 267386880u)
								/ 1048576)));
				}
				set
				{
					this.bitvector1 = ((uint)(((value * 1048576)
								| this.bitvector1)));
				}
			}

			public uint fControl
			{
				get
				{
					return ((uint)(((this.bitvector1 & 268435456u)
								/ 268435456)));
				}
				set
				{
					this.bitvector1 = ((uint)(((value * 268435456)
								| this.bitvector1)));
				}
			}

			public uint fPrivateUseArea
			{
				get
				{
					return ((uint)(((this.bitvector1 & 536870912u)
								/ 536870912)));
				}
				set
				{
					this.bitvector1 = ((uint)(((value * 536870912)
								| this.bitvector1)));
				}
			}

			public uint fNeedsCharacterJustify
			{
				get
				{
					return ((uint)(((this.bitvector1 & 1073741824u)
								/ 1073741824)));
				}
				set
				{
					this.bitvector1 = ((uint)(((value * 1073741824)
								| this.bitvector1)));
				}
			}

			public uint fInvalidGlyph
			{
				get
				{
					return ((uint)(((this.bitvector1 & 2147483648u)
								/ 2147483648)));
				}
				set
				{
					this.bitvector1 = ((uint)(((value * 2147483648)
								| this.bitvector1)));
				}
			}

			public uint fInvalidLogAttr
			{
				get
				{
					return ((uint)(((this.bitvector1 & 1u)
								/ 4294967296)));
				}
				set
				{
					this.bitvector1 = ((uint)(((value * 4294967296)
								| this.bitvector1)));
				}
			}

			public uint fCDM
			{
				get
				{
					return ((uint)(((this.bitvector1 & 2u)
								/ 8589934592)));
				}
				set
				{
					this.bitvector1 = ((uint)(((value * 8589934592)
								| this.bitvector1)));
				}
			}

			public uint fAmbiguousCharSet
			{
				get
				{
					return ((uint)(((this.bitvector1 & 4u)
								/ 17179869184)));
				}
				set
				{
					this.bitvector1 = ((uint)(((value * 17179869184)
								| this.bitvector1)));
				}
			}

			public uint fClusterSizeVaries
			{
				get
				{
					return ((uint)(((this.bitvector1 & 8u)
								/ 34359738368)));
				}
				set
				{
					this.bitvector1 = ((uint)(((value * 34359738368)
								| this.bitvector1)));
				}
			}

			public uint fRejectInvalid
			{
				get
				{
					return ((uint)(((this.bitvector1 & 16u)
								/ 68719476736)));
				}
				set
				{
					this.bitvector1 = ((uint)(((value * 68719476736)
								| this.bitvector1)));
				}
			}
		}


	}
}
