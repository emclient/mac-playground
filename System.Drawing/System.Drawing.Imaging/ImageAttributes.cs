using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace System.Drawing.Imaging 
{

	[StructLayout(LayoutKind.Sequential)]
	public sealed class ImageAttributes : ICloneable, IDisposable 
	{
		internal ColorMatrix colorMatrix;
		internal ColorMatrixFlag colorMatrixFlags;
		internal ColorAdjustType colorAdjustType;
		internal float gamma;

		internal bool isColorMatrixSet;
		internal bool isGammaSet;

		/// <summary>
		/// Clears the color matrix.
		/// </summary>
		public void ClearColorMatrix()
		{
			colorMatrix = null;
			colorMatrixFlags = ColorMatrixFlag.Default;
			colorAdjustType = ColorAdjustType.Default;
			isColorMatrixSet = false;
		}

		public void ClearColorMatrix(ColorAdjustType type)
		{
			// FIXME
		}

		/// <summary>
		/// Clears the gamma.
		/// </summary>
		public void ClearGamma()
		{
			this.ClearGamma(ColorAdjustType.Default);
		}

		/// <summary>
		/// Clears the gamma for the color adjust type.
		/// </summary>
		/// <param name="type">Type.</param>
		public void ClearGamma(ColorAdjustType type)
		{
			isGammaSet = false;
		}

		/// <summary>
		/// Sets the color matrix with the ColorMatrixFlag.Default.
		/// </summary>
		/// <param name="newColorMatrix">New color matrix.</param>
		public void SetColorMatrix(ColorMatrix newColorMatrix)
		{
			SetColorMatrix (newColorMatrix, ColorMatrixFlag.Default);
		}

		/// <summary>
		/// Sets the color matrix with specifed flagsß.
		/// </summary>
		/// <param name="newColorMatrix">New color matrix.</param>
		/// <param name="flags">Flags.</param>
		public void SetColorMatrix(ColorMatrix newColorMatrix, ColorMatrixFlag flags)
		{
			SetColorMatrix (newColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Default);
		}

		public void SetColorMatrix(ColorMatrix newColorMatrix, ColorMatrixFlag mode, ColorAdjustType type)
		{
			colorMatrix = newColorMatrix;
			colorMatrixFlags = mode;
			colorAdjustType = type;
			isColorMatrixSet = true;
		}

		/// <summary>
		/// Sets a color adjust matrix for image colors and a separate gray scale adjust matrix for gray scale values.
		/// </summary>
		public void SetColorMatrices(ColorMatrix newColorMatrix, ColorMatrix? grayMatrix)
		{
			SetColorMatrices(newColorMatrix, grayMatrix, ColorMatrixFlag.Default, ColorAdjustType.Default);
		}

		public void SetColorMatrices(ColorMatrix newColorMatrix, ColorMatrix? grayMatrix, ColorMatrixFlag flags)
		{
			SetColorMatrices(newColorMatrix, grayMatrix, flags, ColorAdjustType.Default);
		}

		public void SetColorMatrices(ColorMatrix newColorMatrix, ColorMatrix? grayMatrix, ColorMatrixFlag mode,
									 ColorAdjustType type)
		{
			throw new PlatformNotSupportedException ();
		}

		public void SetColorKey (Color colorLow, Color colorHigh)
		{
			SetColorKey (colorLow, colorHigh, ColorAdjustType.Default);
		}

		public void SetColorKey (Color colorLow, Color colorHigh, ColorAdjustType type)
		{
			// FIXME
		}

		public void ClearColorKey ()
		{
			// FIXME
		}

		public void ClearColorKey (ColorAdjustType type)
		{
			// FIXME
		}

		/// <summary>
		/// Sets the gamma.
		/// </summary>
		/// <param name="gamma">Gamma.</param>
		public void SetGamma(float gamma)
		{
			SetGamma (gamma, ColorAdjustType.Default);
		}

		public void SetGamma(float gamma, ColorAdjustType type)
		{
			this.gamma = gamma;
			colorAdjustType = type;
			isGammaSet = true;
		}

		public void SetWrapMode(WrapMode mode)
		{
			SetWrapMode(mode, new Color(), false);
		}

		public void SetWrapMode(WrapMode mode, Color color)
		{
			SetWrapMode(mode, color, false);
		}

		public void SetWrapMode(WrapMode mode, Color color, bool clamp)
		{
			// FIXME
		}

		public void SetRemapTable(ColorMap[] map)
		{
			SetRemapTable(map, ColorAdjustType.Default);
		}

		public void SetRemapTable(ColorMap[] map, ColorAdjustType type)
		{
			throw new PlatformNotSupportedException ();
		}

		public void ClearRemapTable()
		{
			ClearRemapTable(ColorAdjustType.Default);
		}

		public void ClearRemapTable(ColorAdjustType type)
		{
			throw new PlatformNotSupportedException ();
		}

		public void SetBrushRemapTable(ColorMap[] map)
		{
			SetRemapTable(map, ColorAdjustType.Brush);
		}

		public void ClearBrushRemapTable()
		{
			ClearRemapTable(ColorAdjustType.Brush);
		}

		public void SetNoOp() => SetNoOp(ColorAdjustType.Default);
		public void SetNoOp(ColorAdjustType type) => throw new NotImplementedException();
		public void ClearNoOp() => ClearNoOp(ColorAdjustType.Default);
		public void ClearNoOp(ColorAdjustType type) => throw new NotImplementedException();
		public void SetOutputChannel(ColorChannelFlag flags) => SetOutputChannel(flags, ColorAdjustType.Default);
		public void SetOutputChannel(ColorChannelFlag flags, ColorAdjustType type) => throw new NotImplementedException();
		public void ClearOutputChannel() => ClearOutputChannel(ColorAdjustType.Default);
		public void ClearOutputChannel(ColorAdjustType type)  => throw new NotImplementedException();
		public void SetOutputChannelColorProfile(string colorProfileFilename) => SetOutputChannelColorProfile(colorProfileFilename, ColorAdjustType.Default);
		public void SetOutputChannelColorProfile(string colorProfileFilename, ColorAdjustType type) => throw new NotImplementedException();
		public void ClearOutputChannelColorProfile() => ClearOutputChannelColorProfile(ColorAdjustType.Default);
		public void ClearOutputChannelColorProfile(ColorAdjustType type) => throw new NotImplementedException();
		public void SetThreshold(float threshold) => SetThreshold(threshold, ColorAdjustType.Default);
		public void SetThreshold(float threshold, ColorAdjustType type) => throw new NotImplementedException();
		public void ClearThreshold() => ClearThreshold(ColorAdjustType.Default);
		public void ClearThreshold(ColorAdjustType type) => throw new NotImplementedException();
		public void GetAdjustedPalette(ColorPalette palette, ColorAdjustType type) => throw new NotImplementedException();

		#region ICloneable implementation
		public object Clone ()
		{
			throw new NotImplementedException ();
		}
		#endregion
		
		#region IDisposable implementation
		public void Dispose ()
		{
		}
		#endregion
	}
}