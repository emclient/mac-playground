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
using System.Drawing.Imaging;
using CoreGraphics;
using MatrixOrder = System.Drawing.Drawing2D.MatrixOrder;

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
		
		public TextureBrush(Image image, Rectangle dstRect) : this(image, dstRect, null)
		{
		}
		
		public TextureBrush(Image image, RectangleF dstRect) : this(image, dstRect, null)
		{
		}
		
		public TextureBrush(Image image, WrapMode wrapMode, Rectangle dstRect)
		{
			if (image == null)
			{
				throw new ArgumentNullException("image");
			}

			throw new NotImplementedException();
		}
		
		public TextureBrush(Image image, WrapMode wrapMode, RectangleF dstRect)
		{
			if (image == null)
			{
				throw new ArgumentNullException("image");
			}

			throw new NotImplementedException();
		}
		
		public TextureBrush(Image image, Rectangle dstRect, ImageAttributes imageAttr)
		{
			if (image == null)
			{
				throw new ArgumentNullException("image");
			}

			throw new NotImplementedException();
		}
		
		public TextureBrush(Image image, RectangleF dstRect, ImageAttributes imageAttr)
		{
			if (image == null)
			{
				throw new ArgumentNullException("image");
			}

			throw new NotImplementedException();
		}

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
				return textureImage;
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
			var destRect = new CGRect(0, 0, textureImage.Width, textureImage.Height);
			//destRect = context.ConvertRectToUserSpace(destRect);

			// [top] [left] main quadrant
			context.ConcatCTM(textureImage.imageTransform);
			context.DrawImage(destRect, variation.NativeCGImage);
			context.ConcatCTM(textureImage.imageTransform.Invert());

			if (wrapMode == WrapMode.TileFlipX || wrapMode == WrapMode.TileFlipXY) 
			{
				// [top] right quadrant, flipped by the x axis
				var transform = new CGAffineTransform(-1, 0, 0, 1, 2 * destRect.Width, 0);
				context.ConcatCTM(transform);
				context.ConcatCTM (textureImage.imageTransform);
				context.DrawImage(destRect, variation.NativeCGImage);
				context.ConcatCTM (textureImage.imageTransform.Invert());
				context.ConcatCTM(transform.Invert());
			}

			if (wrapMode == WrapMode.TileFlipY || wrapMode == WrapMode.TileFlipXY) 
			{
				// bottom [left] quadrant, flipped by the y axis
				var transform = new CGAffineTransform(1, 0, 0, -1, 0, 2 * destRect.Height);
				context.ConcatCTM(transform);
				context.ConcatCTM (textureImage.imageTransform);
				context.DrawImage(destRect, variation.NativeCGImage);
				context.ConcatCTM (textureImage.imageTransform.Invert());
				context.ConcatCTM(transform.Invert());
			}


			if (wrapMode == WrapMode.TileFlipXY) 
			{
				// bottom-right quadrant, flipped by both the y and x axis
				var transform = new CGAffineTransform(-1, 0, 0, -1, 2 * destRect.Width, 2 * destRect.Height);
				context.ConcatCTM(transform);
				context.ConcatCTM (textureImage.imageTransform);
				context.DrawImage(destRect, variation.NativeCGImage);
				context.ConcatCTM (textureImage.imageTransform.Invert());
				context.ConcatCTM(transform.Invert());
			}
		}


		static float HALF_PIXEL_X = 0.5f;
		static float HALF_PIXEL_Y = 0.5f;

		Image variation;
		internal override void Setup (Graphics graphics, bool fill)
		{
			
			// if this is the same as the last that was set then return and no changes have been made
			// then return.
			if (graphics.LastBrush == this && !changed)
				return;

			variation = Graphics.GetRepresentation(graphics.context, textureImage, textureImage.Size);

			// obtain our width and height so we can set the pattern rectangle
			float textureWidth = textureImage.Width;
			float textureHeight = textureImage.Height;

			if (wrapMode == WrapMode.TileFlipX || wrapMode == WrapMode.TileFlipXY)
				textureWidth *= 2;

			if (wrapMode == WrapMode.TileFlipY || wrapMode == WrapMode.TileFlipXY) 
				textureHeight *= 2;

			if (wrapMode == WrapMode.Clamp)
			{
				textureHeight = graphics.ClipBounds.Height;
				textureWidth = graphics.ClipBounds.Width;
			}

			//choose the pattern to be filled based on the currentPattern selected
			var patternSpace = CGColorSpace.CreatePattern(null);
			graphics.context.SetFillColorSpace(patternSpace);
			patternSpace.Dispose();

			var sz = graphics.context.ConvertSizeToDeviceSpace(new CGSize(textureWidth, textureHeight));

			// Pattern default work variables
			//var patternRect = new CGRect(HALF_PIXEL_X,HALF_PIXEL_Y,
			//                                 textureWidth+HALF_PIXEL_X,
			//                                 textureHeight+HALF_PIXEL_Y);

			var patternRect = new CGRect(CGPoint.Empty, sz);
			var patternTransform = graphics.context.GetCTM();
			patternTransform = CGAffineTransform.Multiply(patternTransform, new CGAffineTransform(1f / graphics.screenScale, 0, 0, 1f / graphics.screenScale, 0, 0));
			patternTransform = CGAffineTransform.Multiply(textureTransform.transform, patternTransform);

			// DrawPattern callback which will be set depending on hatch style
			CGPattern.DrawPattern drawPattern;
			
			drawPattern = DrawTexture;

			//set the pattern as the Current Context’s fill pattern
			var pattern = new CGPattern(patternRect, 
			                            patternTransform,
			                            textureWidth,
			                            textureHeight,
			                            CGPatternTiling.NoDistortion,
			                            true, drawPattern);
			//we dont need to set any color, as the pattern cell itself has chosen its own color
			graphics.context.SetFillPattern(pattern, new nfloat[] { 1 });
			
			changed = false;

			graphics.LastBrush = this;
			// I am setting this to be used for Text coloring in DrawString
			//graphics.lastBrushColor = foreColor;
		}

		public override bool Equals(object obj)
		{
			return (obj is TextureBrush b)
				&& textureImage.Equals(b.textureImage)
				&& wrapMode.Equals(b.wrapMode)
				&& textureTransform.Equals(b.textureTransform);
		}
	}
}
