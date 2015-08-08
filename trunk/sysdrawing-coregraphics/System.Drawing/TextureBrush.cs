//
// HatchBrush.cs: HatchBrush implementation for MonoTouch and MonoMac
//
// Authors:
//   Kenneth Pouncey (kjpou@pt.lu)
//
// Copyright 2012
//
using System;
using System.Drawing.Drawing2D;
#if MONOMAC
using MonoMac.CoreGraphics;
#else
using MonoTouch.CoreGraphics;
#endif

using nfloat = System.Single;

namespace System.Drawing 
{
	/// <summary>
	/// Summary description for TextureBrush.
	/// </summary>
	public sealed class TextureBrush : Brush 
	{
		Image textureImage;
		Matrix textureTransform = new Matrix();
		WrapMode wrapMode = WrapMode.Tile;
		new bool changed = false;
		
		public TextureBrush(Image bitmap) : this(bitmap, WrapMode.Tile)
		{
		}
		
		public TextureBrush(Image image, WrapMode wrapMode)
		{
			if (image == null)
			{
				throw new ArgumentNullException("image");
			}

			textureImage = image;
			this.wrapMode = wrapMode;
		}
		
//		public TextureBrush(Image image, Rectangle dstRect) : this(image, dstRect, null)
//		{
//		}
//		
//		public TextureBrush(Image image, RectangleF dstRect) : this(image, dstRect, null)
//		{
//		}
		
		public TextureBrush(Image image, WrapMode wrapMode, Rectangle dstRect)
		{
			if (image == null)
			{
				throw new ArgumentNullException("image");
			}
		}
		
		public TextureBrush(Image image, WrapMode wrapMode, RectangleF dstRect)
		{
			if (image == null)
			{
				throw new ArgumentNullException("image");
			}
		}
		
//		public TextureBrush(Image image, Rectangle dstRect, ImageAttributes imageAttr)
//		{
//			if (image == null)
//			{
//				throw new ArgumentNullException("image");
//			}
//		}
//		
//		public TextureBrush(Image image, RectangleF dstRect, ImageAttributes imageAttr)
//		{
//			if (image == null)
//			{
//				throw new ArgumentNullException("image");
//			}
//		}

		public override object Clone()
		{
			// Implement clone
			return null;
		}
		
		public void MultiplyTransform(Matrix matrix)
		{
			this.MultiplyTransform(matrix, MatrixOrder.Prepend);
		}
		
		public void MultiplyTransform(Matrix matrix, MatrixOrder order)
		{
			if (matrix == null)
			{
				throw new ArgumentNullException("matrix");
			}
			textureTransform.Multiply(matrix, order);
			changed = true;
		}
		
		public void ResetTransform()
		{
			textureTransform.Reset();
			changed = true;
		}
		
		public void RotateTransform(float angle)
		{
			this.RotateTransform(angle, MatrixOrder.Prepend);
		}
		
		public void RotateTransform(float angle, MatrixOrder order)
		{
			textureTransform.Rotate(angle, order);
			changed = true;
		}
		
		public void ScaleTransform(float sx, float sy)
		{
			this.ScaleTransform(sx, sy, MatrixOrder.Prepend);
		}
		
		public void ScaleTransform(float sx, float sy, MatrixOrder order)
		{
			textureTransform.Scale(sx, sy, order);
			changed = true;
		}
		
		public void TranslateTransform(float dx, float dy)
		{
			this.TranslateTransform(dx, dy, MatrixOrder.Prepend);
		}
		
		public void TranslateTransform(float dx, float dy, MatrixOrder order)
		{
			textureTransform.Translate(dx, dy, order);
			changed = true;
		}
		
		// Properties
		public Image Image
		{
			get
			{
				return null;
			}
		}

		public Matrix Transform
		{
			get
			{
				return textureTransform;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				textureTransform = value.Clone();
				changed = true;
			}
		}
		
		public WrapMode WrapMode
		{
			get
			{
				return wrapMode;
			}
			set
			{
				if (wrapMode != value)
				{
					wrapMode = value;
					changed = true;
				}
			}
		}

		// test draw pattern
		private void DrawTexture (CGContext context)
		{
			var destRect = new CGRect(0,0,textureImage.Width,textureImage.Height);
			context.DrawImage(destRect, textureImage.NativeCGImage);

			if (wrapMode == WrapMode.TileFlipX) 
			{
				context.ConcatCTM(CGAffineTransform.MakeScale(-1,1));
				context.DrawImage(destRect, textureImage.NativeCGImage);
			}

			if (wrapMode == WrapMode.TileFlipY) 
			{
				var transformY = new CGAffineTransform(1, 0, 0, -1, 
				                                       textureImage.Width, 
				                                       textureImage.Height);
				context.ConcatCTM(transformY);
				context.DrawImage(destRect, textureImage.NativeCGImage);
			}


			if (wrapMode == WrapMode.TileFlipXY) 
			{
				// draw the original
				var transform = new CGAffineTransform(1, 0, 0, 1, 
				                                       0, textureImage.Height);
				context.ConcatCTM(transform);
				context.DrawImage(destRect, textureImage.NativeCGImage);

				// reset the transform
				context.ConcatCTM (context.GetCTM().Invert());

				// draw next to original one that is flipped by x axis
				transform = new CGAffineTransform(-1, 0, 0, 1, 
				                                  textureImage.Width * 2, textureImage.Height);
				context.ConcatCTM(transform);
				context.DrawImage(destRect, textureImage.NativeCGImage);


				// reset the transform
				context.ConcatCTM (context.GetCTM().Invert());

				// draw one that is flipped by Y axis under the oricinal
				transform = new CGAffineTransform(1, 0, 0, -1, 
				                                  0, textureImage.Height);
				context.ConcatCTM(transform);
				context.DrawImage(destRect, textureImage.NativeCGImage);

				// draw the last one of the quadrant which is flipped by both the y and x axis
				context.ConcatCTM (context.GetCTM().Invert());
				transform = new CGAffineTransform(-1, 0, 0, -1, 
				                                  textureImage.Width * 2, textureImage.Height);
				context.ConcatCTM(transform);
				context.DrawImage(destRect, textureImage.NativeCGImage);
			}
		}


		static float HALF_PIXEL_X = 0.5f;
		static float HALF_PIXEL_Y = 0.5f;

		internal override void Setup (Graphics graphics, bool fill)
		{
			
			// if this is the same as the last that was set then return and no changes have been made
			// then return.
			if (graphics.LastBrush == this && !changed)
				return;
			
			// obtain our width and height so we can set the pattern rectangle
			float textureWidth = textureImage.Width;
			float textureHeight = textureImage.Height;

			if (wrapMode == WrapMode.TileFlipX || wrapMode == WrapMode.TileFlipY)
				textureWidth *= 2;

			if (wrapMode == WrapMode.TileFlipXY) 
			{
				textureWidth *= 2;
				textureHeight *= 2;
			}

			// this is here for testing only
			var textureOffset = new PointF(0,-0);

			//choose the pattern to be filled based on the currentPattern selected
			var patternSpace = CGColorSpace.CreatePattern(null);
			graphics.context.SetFillColorSpace(patternSpace);
			patternSpace.Dispose();
			
			// Pattern default work variables
			var patternRect = new CGRect(HALF_PIXEL_X,HALF_PIXEL_Y,
			                                 textureWidth+HALF_PIXEL_X,
			                                 textureHeight+HALF_PIXEL_Y);
			var patternTransform = CGAffineTransform.MakeIdentity();
			
			// We need to take into account the orientation of the graphics object
#if MONOMAC
			if (!graphics.isFlipped)
				patternTransform = new CGAffineTransform(1, 0, 0, -1, 
				                                         textureOffset.X, 
				                                         textureHeight + textureOffset.Y);
#endif
#if MONOTOUCH
			if (graphics.isFlipped)
				patternTransform = new CGAffineTransform(1, 0, 0, -1, 
				                                         textureOffset.X, 
				                                         textureHeight + textureOffset.Y);
#endif

			patternTransform = CGAffineTransform.Multiply(patternTransform, textureTransform.transform);

			// DrawPattern callback which will be set depending on hatch style
			CGPattern.DrawPattern drawPattern;
			
			drawPattern = DrawTexture;

			//set the pattern as the Current Context’s fill pattern
			var pattern = new CGPattern(patternRect, 
			                            patternTransform,
			                            textureWidth,
			                            textureHeight,
			                            //textureHeight,
			                            CGPatternTiling.NoDistortion,
			                            true, drawPattern);
			//we dont need to set any color, as the pattern cell itself has chosen its own color
			graphics.context.SetFillPattern(pattern, new nfloat[] { 1 });
			
			changed = false;

			graphics.LastBrush = this;
			// I am setting this to be used for Text coloring in DrawString
			//graphics.lastBrushColor = foreColor;
		}
		
	}
}
