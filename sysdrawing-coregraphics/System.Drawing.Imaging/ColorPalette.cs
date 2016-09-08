using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Drawing.Imaging
{
	public sealed class ColorPalette {
		// 0x1: the color values in the array contain alpha information
		// 0x2: the color values are grayscale values.
		// 0x4: the colors in the array are halftone values.

		private int flags;
		private Color [] entries;

		//
		// There is no public constructor, this will be used somewhere in the
		// drawing code
		//
		internal ColorPalette ()
		{
			entries = new Color [0];
		}

		internal ColorPalette (int flags, Color[] colors) {
			this.flags = flags;
			entries = colors;
		}

		public Color [] Entries {
			get {
				return entries;
			}
		}

		public int Flags {
			get {
				return flags;
			}
		}
	}
}
