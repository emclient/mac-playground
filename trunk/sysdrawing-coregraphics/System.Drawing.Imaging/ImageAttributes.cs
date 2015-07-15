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

		#region ICloneable implementation
		public object Clone ()
		{
			throw new NotImplementedException ();
		}
		#endregion
		
		#region IDisposable implementation
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
		#endregion
	}
}