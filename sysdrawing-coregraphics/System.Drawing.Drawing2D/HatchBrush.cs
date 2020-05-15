//
// HatchBrush.cs: HatchBrush implementation for MonoTouch and MonoMac
//
// Authors:
//   Kenneth Pouncey (kjpou@pt.lu)
//
// Copyright 2012
//
using System;
using System.Drawing.Mac;
#if XAMARINMAC
using CoreGraphics;
#elif MONOMAC
using MonoMac.CoreGraphics;
#else
using MonoTouch.CoreGraphics;
#endif

#if MAC64
using nfloat = System.Double;
#else
using nfloat = System.Single;
#endif

namespace System.Drawing.Drawing2D 
{
	/// <summary>
	/// Summary description for HatchBrush.
	/// </summary>
	public sealed class HatchBrush : Brush 
	{
		Color backColor;
		Color foreColor;
		HatchStyle hatchStyle;
		
		public HatchBrush (HatchStyle hatchStyle, Color foreColor)
			: this (hatchStyle, foreColor, Color.Black)
		{
		}
		
		public HatchBrush(HatchStyle hatchStyle, Color foreColor, Color backColor)
		{
			this.hatchStyle = hatchStyle;
			this.foreColor = foreColor;
			this.backColor = backColor;
		}
		
		public Color BackgroundColor {
			get {
				return backColor;
			}
		}
		
		public Color ForegroundColor {
			get {
				return foreColor;
			}
		}
		
		public HatchStyle HatchStyle {
			get {
				return hatchStyle;
			}
		}
		
		public override void Dispose (bool disposing)
		{
			if (disposing){
			}
		}

		public override object Clone ()
		{
			return new HatchBrush (hatchStyle, foreColor, backColor);
		}

		// our default area size and line width
		static float HATCH_SIZE = 8;
		static float LINE_WIDTH = 1;

		static float[][] hatches_const = new float[][] {
			/* HatchStyleHorizontal */		new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyleVertical */		new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyleForwardDiagonal */		new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyleBackwardDiagonal */	new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyleCross */			new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyleDiagonalCross */		new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyle05Percent */		new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyle10Percent */		new float[]{ HATCH_SIZE, HATCH_SIZE - 3.0f, LINE_WIDTH },
			/* HatchStyle20Percent */		new float[]{ 2.0f, 2.0f, LINE_WIDTH },
			/* HatchStyle25Percent */		new float[]{ 4.0f, 2.0f, LINE_WIDTH },
			/* HatchStyle30Percent */		new float[]{ 4.0f, 4.0f, LINE_WIDTH },
			/* HatchStyle40Percent */		new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyle50Percent */		new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyle60Percent */		new float[]{ 4.0f, 4.0f, LINE_WIDTH },
			/* HatchStyle70Percent */		new float[]{ 4.0f, 2.0f, LINE_WIDTH },
			/* HatchStyle75Percent */		new float[]{ 4.0f, 4.0f, LINE_WIDTH },
			/* HatchStyle80Percent */		new float[]{ HATCH_SIZE, 4.0f, LINE_WIDTH },
			/* HatchStyle90Percent */		new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyleLightDownwardDiagonal */	new float[]{ HATCH_SIZE * 0.5f, HATCH_SIZE * 0.5f, LINE_WIDTH },
			/* HatchStyleLightUpwardDiagonal */	new float[]{ HATCH_SIZE * 0.5f, HATCH_SIZE * 0.5f, LINE_WIDTH },
			/* HatchStyleDarkDownwardDiagonal */	new float[]{ HATCH_SIZE * 0.5f, HATCH_SIZE * 0.5f, LINE_WIDTH + 1 },
			/* HatchStyleDarkUpwardDiagonal */	new float[]{ HATCH_SIZE * 0.5f, HATCH_SIZE * 0.5f, LINE_WIDTH + 1 },
			/* HatchStyleWideDownwardDiagonal */	new float[]{ (HATCH_SIZE), (HATCH_SIZE), LINE_WIDTH + 2 },
			/* HatchStyleWideUpwardDiagonal */	new float[]{ (HATCH_SIZE), (HATCH_SIZE), LINE_WIDTH + 2 },
			/* HatchStyleLightVertical */		new float[]{ HATCH_SIZE * 0.5f, HATCH_SIZE * 0.5f, LINE_WIDTH },
			/* HatchStyleLightHorizontal */		new float[]{ HATCH_SIZE * 0.5f, HATCH_SIZE * 0.5f, LINE_WIDTH },
			/* HatchStyleNarrowVertical */		new float[]{ 2.0f, 2.0f, LINE_WIDTH },
			/* HatchStyleNarrowHorizontal */	new float[]{ 2.0f, 2.0f, LINE_WIDTH },
			/* HatchStyleDarkVertical */		new float[]{ HATCH_SIZE * 0.5f, HATCH_SIZE * 0.5f, LINE_WIDTH * 2.0f },
			/* HatchStyleDarkHorizontal */		new float[]{ HATCH_SIZE * 0.5f, HATCH_SIZE * 0.5f, LINE_WIDTH * 2.0f },
			/* HatchStyleDashedDownwardDiagonal */	new float[]{ HATCH_SIZE * 0.5f, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyleDashedUpwardDiagonal */	new float[]{ HATCH_SIZE * 0.5f, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyleDashedHorizontal */	new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyleDashedVertical */		new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyleSmallConfetti */		new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyleLargeConfetti */		new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyleZigZag */			new float[]{ HATCH_SIZE, 4.0f, LINE_WIDTH },
			/* HatchStyleWave */			new float[]{ HATCH_SIZE, HATCH_SIZE * 0.50f, LINE_WIDTH },
			/* HatchStyleDiagonalBrick */		new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyleHorizontalBrick */		new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyleWeave */			new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStylePlaid */			new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyleDivot */			new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyleDottedGrid */		new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyleDottedDiamond */		new float[]{ HATCH_SIZE, (HATCH_SIZE), LINE_WIDTH },
			/* HatchStyleShingle */			new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyleTrellis */			new float[]{ 4.0f, 4.0f, LINE_WIDTH },
			/* HatchStyleSphere */			new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH  },
			/* HatchStyleSmallGrid */		new float[]{ HATCH_SIZE * 0.5f, HATCH_SIZE * 0.5f, LINE_WIDTH },
			/* HatchStyleSmallCheckerBoard */	new float[]{ 4.0f, 4.0f, LINE_WIDTH },
			/* HatchStyleLargeCheckerBoard */	new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyleOutlinedDiamond */		new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH },
			/* HatchStyleSolidDiamond */		new float[]{ HATCH_SIZE, HATCH_SIZE, LINE_WIDTH }

		};

		float getHatchWidth (HatchStyle hbr)
		{

			return hatches_const[(int)hbr][0];
		}

		float getHatchHeight (HatchStyle hbr)
		{
			
			return hatches_const[(int)hbr][1];
		}

		float getLineWidth (HatchStyle hbr)
		{
			
			return hatches_const[(int)hbr][2];
		}

		void drawBackground(CGContext context, Color color, float width, float height) 
		{
			context.SetFillColor(color);
			context.FillRect(new CGRect(HALF_PIXEL_X, HALF_PIXEL_Y, width+HALF_PIXEL_X, height+HALF_PIXEL_Y));
			context.FillPath();
		}

		void initializeContext(CGContext context, float size, bool antialias) 
		{
			initializeContext (context, size, size, antialias);
		}

		void initializeContext(CGContext context, float width, float height, bool antialias) 
		{

			// Do any initialization for the context that needs to be done.
			context.SetShouldAntialias(antialias);
		}

		static float HALF_PIXEL_X = 0.5f;
		static float HALF_PIXEL_Y = 0.5f;

		private void HatchHorizontal (CGContext context)
		{
			var hatchSize = getHatchWidth (hatchStyle);
			var lineWidth = getLineWidth (hatchStyle);

			initializeContext(context, hatchSize, false);

			/* draw background */
			drawBackground (context, backColor, hatchSize, hatchSize);

			/* draw horizontal line in the foreground color */
			context.SetStrokeColor(foreColor);
			context.SetLineWidth(lineWidth);
			context.SetLineCap(CGLineCap.Square);

			// draw a horizontal line
			context.MoveTo(0,0);
			context.AddLineToPoint(hatchSize, 0);

			context.StrokePath();
		}

		private void HatchVertical (CGContext context)
		{
			var hatchSize = getHatchWidth (hatchStyle);
			var lineWidth = getLineWidth (hatchStyle);
			
			initializeContext(context, hatchSize, false);
			
			/* draw background */
			drawBackground (context, backColor, hatchSize, hatchSize);
			
			/* draw horizontal line in the foreground color */
			context.SetStrokeColor(foreColor);
			context.SetLineWidth(lineWidth);
			context.SetLineCap(CGLineCap.Square);

			//float halfMe = hatchSize / 2.0f;

			// draw a horizontal line
			context.MoveTo(0, 0);
			context.AddLineToPoint(0, hatchSize);
			
			context.StrokePath();
		}

		private void HatchGrid (CGContext context)
		{
			var hatchSize = getHatchWidth (hatchStyle);
			var lineWidth = getLineWidth (hatchStyle);

			initializeContext(context, hatchSize, false);

			/* draw background */
			drawBackground (context, backColor, hatchSize, hatchSize);

			/* draw lines in the foreground color */
			context.SetStrokeColor(foreColor);
			context.SetLineWidth(lineWidth);
			context.SetLineCap(CGLineCap.Square);

			hatchSize -=HALF_PIXEL_X;
			float yoffset = 0;

			if (hatchStyle == HatchStyle.DottedGrid)
			{
				yoffset = 1;
				nfloat[] dash = new nfloat[] { 1, 1};
				context.SetLineDash(2,dash);

			}

			/* draw a horizontal line */
			context.MoveTo(0,yoffset);
			context.AddLineToPoint(0,hatchSize);
			context.StrokePath();
			/* draw a vertical line */
			context.MoveTo(0,hatchSize);
			context.AddLineToPoint(hatchSize, hatchSize);

			context.StrokePath();
		}

		/**
		 * Percentage patterns were obtained by creating a screen shot from a windows fill
		 * and looking at the patterns at the pixel level.  A little tedious to say the least
		 * but they do seem correct and recreate the same pattern from windows to here.
		 */
		private void HatchPercentage (CGContext context)
		{
			var hatchWidth = getHatchWidth (hatchStyle);
			var hatchHeight = getHatchHeight (hatchStyle);
			//var lineWidth = getLineWidth (hatchStyle);

			initializeContext(context, hatchHeight, false);
			

			/* some patterns require us to reverse the colors */
			switch (hatchStyle) {
			case HatchStyle.Percent05:
			case HatchStyle.Percent10:
			case HatchStyle.Percent20:
			case HatchStyle.Percent25:
			case HatchStyle.Percent30:
				drawBackground (context, backColor, hatchWidth, hatchWidth);
				context.SetFillColor(foreColor);
				break;
			default:
				drawBackground (context, foreColor, hatchWidth, hatchWidth);
				context.SetFillColor(backColor);
				break;
			}

			// create a work rectangle for setting pixels
			CGRect rect = new CGRect(0,0,1,1);

			// Only set the pixels for some
			if (hatchStyle != HatchStyle.Percent50 &&
			    hatchStyle != HatchStyle.Percent40 &&
			    hatchStyle != HatchStyle.Percent30 &&
			    hatchStyle != HatchStyle.Percent60) 
			{
				rect.X = 0;
				rect.Y = (int)(hatchHeight / 2.0f);
				setPixels(context, rect);
				rect.X = (int)(hatchWidth / 2.0f);
				rect.Y = 0;
				setPixels(context, rect);
			}

			// 50 and 40 start out the same with a 50 percent
			if (hatchStyle == HatchStyle.Percent50 ||
			    hatchStyle == HatchStyle.Percent40 ) 
			{
				int x = 0;
				int y = 0;
				
				for (y = 0; y<hatchHeight; y+=2)
				{
					for (x = 1; x < hatchWidth; x+=2)
					{
						rect.X = x;
						rect.Y = y;
						setPixels(context, rect);
					}
				}
				for (y = 1; y<hatchHeight; y+=2)
				{
					for (x = 0; x < hatchWidth; x+=2)
					{
						rect.X = x;
						rect.Y = y;
						setPixels(context, rect);
					}
				}

				// Percent40 is a 50 with two more dots set of back color
				// within a set area.  This creates an interesting effect
				// of a double plus sign in opposite corners.
				if (hatchStyle == HatchStyle.Percent40)
				{
					rect.X = 1;
					rect.Y = 1;
					setPixels(context, rect);
					rect.X = 5;
					rect.Y = 5;
					setPixels(context, rect);
				}

			}

			// Percent30 and Percent60 are really messed up so we will just set some dots
			// to present the pattern.  Percent60 is a 30 with colors reversed, go figure.
			if (hatchStyle == HatchStyle.Percent30 ||
			    hatchStyle == HatchStyle.Percent60)
			{
				rect.X = 0;
				rect.Y = 0;
				setPixels(context, rect);
				rect.X = 2;
				rect.Y = 0;
				setPixels(context, rect);
				rect.X = 0;
				rect.Y = 2;
				setPixels(context, rect);
				rect.X = 2;
				rect.Y = 2;
				setPixels(context, rect);

				rect.X = 1;
				rect.Y = 3;
				setPixels(context, rect);

				rect.X = 3;
				rect.Y = 1;
				setPixels(context, rect);
			}

		}

		private void HatchUpwardDiagonal (CGContext context)
		{
			var hatchSize = getHatchWidth (hatchStyle);
			var lineWidth = getLineWidth (hatchStyle);


			if (hatchStyle != HatchStyle.ForwardDiagonal &&
			    hatchStyle != HatchStyle.BackwardDiagonal)
			{
				initializeContext(context, hatchSize, false);
			}
			else { 
				initializeContext(context, hatchSize, true);
			}


			/* draw background */
			drawBackground (context, backColor, hatchSize, hatchSize);


			/* draw lines in the foreground color */
			context.SetStrokeColor(foreColor);
			context.SetFillColor(foreColor);

			context.SetLineWidth(1);
			context.SetLineCap(CGLineCap.Square);

			context.MoveTo(0,0);
			context.AddLineToPoint(hatchSize,hatchSize);
			/* draw a diagonal line for as many times as linewidth*/
			for (int l = 0; l < lineWidth; l++)
			{
				/* draw a diagonal line */
				context.MoveTo(l,0);
				context.AddLineToPoint(hatchSize, hatchSize-l);

				context.StrokePath();
			}

			/**
			 * because we are within a rectangular pattern we have to complete the tip of the preceeding line 
			 * pattern
			 */
			for (int k = 1; k < lineWidth; k++)
			{
				/* draw a diagonal line */
				context.MoveTo(0,hatchSize-k);
				context.AddLineToPoint(k-1, hatchSize-1);
				
				context.StrokePath();
			}

		}

		void HatchDiagonalCross(CGContext context) 
		{
			var hatchWidth = getHatchWidth (hatchStyle);
			var hatchHeight = getHatchHeight (hatchStyle);
			var lineWidth = getLineWidth (hatchStyle);

			initializeContext(context, hatchHeight, true);

			/* draw background */
			drawBackground (context, backColor, hatchWidth, hatchHeight);
			
			/* draw lines in the foreground color */
			context.SetStrokeColor(foreColor);
			context.SetLineWidth(lineWidth);
			context.SetLineCap(CGLineCap.Square);
			
			//float halfMe = hatchHeight / 2.0f;
			
			context.MoveTo(0, 0);
			context.AddLineToPoint(hatchWidth, hatchHeight);
			context.StrokePath();

			context.MoveTo(hatchWidth, 0);
			context.AddLineToPoint(0, hatchHeight);
			context.StrokePath();
		}

		/**
		 * This is fill of hackish stuff.
		 * Thought this was going to be easier but that just did not work out.
		 **/
		private void HatchSphere (CGContext context)
		{
			var hatchSize = getHatchWidth (hatchStyle);
			//var lineWidth = getLineWidth (hatchStyle);

			initializeContext(context, hatchSize, false);

			/* draw background in fore ground color*/
			drawBackground (context, backColor, hatchSize, hatchSize);
			
			context.SetStrokeColor(foreColor);
			context.SetFillColor(foreColor);
			
			context.SetLineWidth(1);
			context.SetLineCap(CGLineCap.Square);


			// Initialize work rectangle
			CGRect rect = new CGRect(0,0,1,1);

			float quad = hatchSize / 2.0f;

			// Left lower quad
			rect.Width = quad;
			rect.Height = quad;
			rect.X = 0;
			rect.Y = 0;

			context.StrokeRect(rect);

			// right upper quad
			rect.Width = quad;
			rect.Height = quad;
			rect.X = quad;
			rect.Y = quad;

			context.StrokeRect(rect);

			// left upper quad
			rect.Width = quad;
			rect.Height = quad;
			rect.X = 0;
			rect.Y = quad + 1;

			context.FillRect(rect);

			// right lower quod
			rect.Width = quad;
			rect.Height = quad;
			rect.X = quad + 1;
			rect.Y = 0;
			
			context.FillRect(rect);

			// Now we fill in some corner bits with background
			// This is a bad hack but now sure what else to do
			context.SetFillColor(backColor);

			rect.Height = 1;
			rect.Width = 1;

			rect.X = 0;
			rect.Y = 0;
			setPixels(context, rect);

			rect.X = 0;
			rect.Y = quad;
			setPixels(context, rect);

			rect.X = 0;
			rect.Y = quad;
			setPixels(context, rect);

			rect.X = quad;
			rect.Y = 0;
			setPixels(context, rect);

			rect.X = quad;
			rect.Y = quad;
			setPixels(context, rect);

			rect.X = quad;
			rect.Y = hatchSize;
			setPixels(context, rect);

			rect.X = hatchSize;
			rect.Y = 0;
			setPixels(context, rect);
			
			rect.X = hatchSize;
			rect.Y = quad;
			setPixels(context, rect);
			
			rect.X = hatchSize;
			rect.Y = hatchSize;
			setPixels(context, rect);

			// Now for the fake shiny thingys hack
			// Probably could use a line here but it is already
			// so hacky I will just use this.
			rect.X = 5;
			rect.Y = 3;
			setPixels(context, rect);

			rect.X = 6;
			rect.Y = 3;
			setPixels(context, rect);

			rect.X = 1;
			rect.Y = 7;
			setPixels(context, rect);
			
			rect.X = 2;
			rect.Y = 7;
			setPixels(context, rect);
		}

		void HatchDashedDiagonal(CGContext context) 
		{
			var hatchWidth = getHatchWidth (hatchStyle);
			var hatchHeight = getHatchHeight (hatchStyle);
			var lineWidth = getLineWidth (hatchStyle);

			initializeContext(context, hatchHeight, false);

			/* draw background */
			drawBackground (context, backColor, hatchWidth, hatchHeight);
			
			/* draw lines in the foreground color */
			context.SetStrokeColor(foreColor);
			context.SetLineWidth(lineWidth);
			context.SetLineCap(CGLineCap.Square);

			float halfMe = hatchHeight / 2.0f;

			context.MoveTo(0, halfMe);
			context.AddLineToPoint(hatchWidth, hatchHeight);
			context.StrokePath();
		}

		void HatchDashedHorizontal(CGContext context) 
		{
			var hatchWidth = getHatchWidth (hatchStyle);
			var hatchHeight = getHatchHeight (hatchStyle);
			var lineWidth = getLineWidth (hatchStyle);
			
			initializeContext(context, hatchHeight, false);

			/* draw background */
			drawBackground (context, backColor, hatchWidth, hatchHeight);
			
			/* draw lines in the foreground color */
			context.SetStrokeColor(foreColor);
			context.SetLineWidth(lineWidth);
			context.SetLineCap(CGLineCap.Square);
			
			float halfMe = hatchHeight / 2.0f - 1;
			hatchWidth -=1;
			hatchHeight -= 1;
			
			context.MoveTo(halfMe+1, halfMe);
			context.AddLineToPoint(hatchWidth, halfMe);
			context.StrokePath();

			context.MoveTo(0,hatchHeight);
			context.AddLineToPoint(halfMe, hatchHeight);
			context.StrokePath();

		}

		void HatchConfetti (CGContext context) 
		{
			var hatchWidth = getHatchWidth (hatchStyle);
			var hatchHeight = getHatchHeight (hatchStyle);
			//var lineWidth = getLineWidth (hatchStyle);
			
			initializeContext(context, hatchHeight, false);

			/* draw background */
			drawBackground (context, backColor, hatchWidth, hatchHeight);
			
			/* draw confetti Rectangles in the foreground color */
			context.SetFillColor(foreColor);

			CGRect rect = new CGRect(0,0,2,2);

			// Do not see a mathematical equation so will just set
			// the same ones as in windows pattern.
			if (hatchStyle == HatchStyle.LargeConfetti)
			{
				setPixels(context, rect);

				rect.X = 2;
				rect.Y = 2;
				setPixels(context, rect);

				rect.X = 5;
				rect.Y = 1;
				setPixels(context, rect);

				rect.X = 6;
				rect.Y = 4;
				setPixels(context, rect);

				rect.X = 4;
				rect.Y = 6;
				setPixels(context, rect);

				rect.X = 1;
				rect.Y = 5;
				setPixels(context, rect);
			}
			else
			{
				rect.Width = 1;
				rect.Height = 1;

				setPixels(context, rect);
				
				rect.X = 3;
				rect.Y = 1;
				setPixels(context, rect);
				
				rect.X = 6;
				rect.Y = 2;
				setPixels(context, rect);
				
				rect.X = 2;
				rect.Y = 3;
				setPixels(context, rect);
				
				rect.X = 7;
				rect.Y = 4;
				setPixels(context, rect);
				
				rect.X = 4;
				rect.Y = 5;
				setPixels(context, rect);

				rect.X = 1;
				rect.Y = 6;
				setPixels(context, rect);

				rect.X = 5;
				rect.Y = 7;
				setPixels(context, rect);

			}
		}

		void HatchZigZag(CGContext context) 
		{
			var hatchWidth = getHatchWidth (hatchStyle);
			var hatchHeight = getHatchHeight (hatchStyle);
			var lineWidth = getLineWidth (hatchStyle);
			
			initializeContext(context, hatchHeight, false);

			/* draw background */
			drawBackground (context, backColor, hatchWidth, hatchHeight);
			
			/* draw lines in the foreground color */
			context.SetStrokeColor(foreColor);
			context.SetLineWidth(lineWidth);
			context.SetLineCap(CGLineCap.Square);
			
			float halfMe = hatchWidth / 2.0f;
			hatchHeight -= 1;
			
			context.MoveTo(0, 0);
			context.AddLineToPoint(halfMe-1, hatchHeight);
			context.StrokePath();
			
			context.MoveTo(halfMe+1,hatchHeight);
			context.AddLineToPoint(hatchWidth, 0);
			context.StrokePath();
			
		}

		void HatchWave(CGContext context) 
		{
			var hatchWidth = getHatchWidth (hatchStyle);
			var hatchHeight = getHatchHeight (hatchStyle);
			//var lineWidth = getLineWidth (hatchStyle);
			
			initializeContext(context, hatchHeight, false);

			/* draw background */
			drawBackground (context, backColor, hatchWidth, hatchHeight);
			
			/* draw lines in the foreground color */
			context.SetFillColor(foreColor);

			CGRect rect = new CGRect(0,0,1,1);
			
			// We could maybe draw some arcs here but there are so few pixels
			// that it just is not worth it.
			rect.X = 1;
			setPixels(context, rect);
			rect.X = 2;
			setPixels(context, rect);

			rect.Y = 1;

			rect.X = 0;
			setPixels(context, rect);
			rect.X = 3;
			setPixels(context, rect);
			rect.X = 6;
			setPixels(context, rect);

			rect.Y = 2;
			
			rect.X = 4;
			setPixels(context, rect);
			rect.X = 5;
			setPixels(context, rect);

			
		}

		void HatchHorizontalBrick (CGContext context) 
		{
			var hatchWidth = getHatchWidth (hatchStyle);
			var hatchHeight = getHatchHeight (hatchStyle);
			var lineWidth = getLineWidth (hatchStyle);

			initializeContext(context, hatchHeight, false);

			/* draw background */
			drawBackground (context, backColor, hatchWidth, hatchHeight);
			
			/* draw lines in the foreground color */
			context.SetFillColor(foreColor);
			
			/* draw lines in the foreground color */
			context.SetStrokeColor(foreColor);
			context.SetLineWidth(lineWidth);
			context.SetLineCap(CGLineCap.Square);

			CGRect rect = new CGRect(0,0,1,1);

			rect.Y = 3;
			rect.Width = hatchWidth;
			rect.Height = hatchHeight - 4;
			context.StrokeRect(rect);

			context.MoveTo(hatchWidth / 2.0f, 0);
			context.AddLineToPoint(hatchWidth / 2.0f,3);
			context.StrokePath();
		}

		void HatchDiagonalBrick (CGContext context) 
		{
			var hatchWidth = getHatchWidth (hatchStyle);
			var hatchHeight = getHatchHeight (hatchStyle);
			var lineWidth = getLineWidth (hatchStyle);
			
			initializeContext(context, hatchHeight, false);

			/* draw background */
			drawBackground (context, backColor, hatchWidth, hatchHeight);
			
			/* draw lines in the foreground color */
			context.SetStrokeColor(foreColor);
			context.SetLineWidth(lineWidth);
			context.SetLineCap(CGLineCap.Square);

			hatchHeight -= 1;
			hatchWidth -= 1;

			context.MoveTo(0, 0);
			context.AddLineToPoint(hatchWidth,hatchHeight);
			context.StrokePath();

			context.MoveTo(0, hatchHeight);
			context.AddLineToPoint(hatchWidth / 2.0f,hatchHeight / 2.0f);
			context.StrokePath();
		}

		void HatchWeave (CGContext context) 
		{
			var hatchWidth = getHatchWidth (hatchStyle);
			var hatchHeight = getHatchHeight (hatchStyle);
			var lineWidth = getLineWidth (hatchStyle);
			
			initializeContext(context, hatchHeight, false);

			/* draw background */
			drawBackground (context, backColor, hatchWidth, hatchHeight);
			
			/* draw lines in the foreground color */
			context.SetFillColor(foreColor);
			
			/* draw lines in the foreground color */
			context.SetStrokeColor(foreColor);
			context.SetLineWidth(lineWidth);
			context.SetLineCap(CGLineCap.Square);
			
			float halfWidth = hatchWidth / 2.0f;
			float halfHeight = hatchHeight / 2.0f;
			
			//RectangleF rect = new RectangleF(0,0,1,1);

			// Add upward diagonals
			context.MoveTo(0,0);
			context.AddLineToPoint(halfWidth, halfHeight);
			context.StrokePath();
			
			context.MoveTo(0, halfHeight);
			context.AddLineToPoint(halfWidth -1, hatchHeight - 1);
			context.StrokePath();

			context.MoveTo(halfWidth, 0);
			context.AddLineToPoint(6, 2);
			context.StrokePath();		

//			context.MoveTo(0, 4);
//			context.AddLineToPoint(2, 2);
//			context.StrokePath();

			context.MoveTo(2,6);
			context.AddLineToPoint(7, 1);
			context.StrokePath();		

		}

		void HatchTrellis (CGContext context) 
		{
			var hatchWidth = getHatchWidth (hatchStyle);
			var hatchHeight = getHatchHeight (hatchStyle);
			//var lineWidth = getLineWidth (hatchStyle);
			
			initializeContext(context, hatchHeight, false);

			/* draw background */
			drawBackground (context, foreColor, hatchWidth, hatchHeight);
			
			context.SetFillColor(backColor);

			CGRect rect = new CGRect(0,0,2,1);
			setPixels(context, rect);

			rect.X = hatchWidth / 2.0f;
			rect.Y = hatchHeight / 2.0f;
			setPixels(context, rect);
		}

		void HatchCheckered (CGContext context) 
		{
			var hatchWidth = getHatchWidth (hatchStyle);
			var hatchHeight = getHatchHeight (hatchStyle);
			//var lineWidth = getLineWidth (hatchStyle);
			
			initializeContext(context, hatchHeight, false);

			/* draw background */
			drawBackground (context, backColor, hatchWidth, hatchHeight);
			
			context.SetFillColor(foreColor);
			
			CGRect rect = new CGRect(0,0,hatchWidth / 2.0f,hatchHeight / 2.0f);
			setPixels(context, rect);
			
			rect.X = hatchWidth / 2.0f;
			rect.Y = hatchHeight / 2.0f;
			setPixels(context, rect);
		}

		void HatchOutlinedDiamond (CGContext context) 
		{
			var hatchWidth = getHatchWidth (hatchStyle);
			var hatchHeight = getHatchHeight (hatchStyle);
			var lineWidth = getLineWidth (hatchStyle);
			
			initializeContext(context, hatchHeight, false);

			/* draw background */
			drawBackground (context, backColor, hatchWidth, hatchHeight);
			
			/* draw lines in the foreground color */
			context.SetFillColor(foreColor);
			context.SetStrokeColor(foreColor);
			context.SetLineWidth(lineWidth);
			context.SetLineCap(CGLineCap.Square);

			// this is really just two diagonal lines from each corner too
			// their opposite corners.
			context.MoveTo(0,0);
			context.AddLineToPoint(hatchWidth, hatchHeight);
			context.StrokePath();
			context.MoveTo(1,hatchHeight);
			context.AddLineToPoint(hatchWidth, 1);
			context.StrokePath();
		}

		void HatchSolidDiamond (CGContext context) 
		{
			var hatchWidth = getHatchWidth (hatchStyle);
			var hatchHeight = getHatchHeight (hatchStyle);
			var lineWidth = getLineWidth (hatchStyle);
			
			initializeContext(context, hatchHeight, false);

			/* draw background */
			drawBackground (context, backColor, hatchWidth, hatchHeight);
			
			/* draw lines in the foreground color */
			context.SetFillColor(foreColor);
			context.SetStrokeColor(foreColor);
			context.SetLineWidth(lineWidth);
			context.SetLineCap(CGLineCap.Square);

			float halfMe = hatchWidth / 2.0f;

			// We will paint two triangles from corners meeting in the middle
			// make sure to offset by half pixels so that the point is actually a point.
			context.MoveTo(-HALF_PIXEL_X,HALF_PIXEL_Y);
			context.AddLineToPoint(2+HALF_PIXEL_X, halfMe - HALF_PIXEL_Y);
			context.AddLineToPoint(-HALF_PIXEL_X, hatchHeight- (1.0f + HALF_PIXEL_Y));
			context.ClosePath();
			context.FillPath();

			// now we do the right one
			context.MoveTo(hatchWidth,HALF_PIXEL_Y);
			context.AddLineToPoint(halfMe+HALF_PIXEL_X, halfMe - HALF_PIXEL_Y);
			context.AddLineToPoint(hatchWidth, hatchHeight - (1.0f + HALF_PIXEL_Y));
			context.ClosePath();
			context.FillPath();

		}

		void HatchDottedDiamond (CGContext context) 
		{
			var hatchWidth = getHatchWidth (hatchStyle);
			var hatchHeight = getHatchHeight (hatchStyle);
			var lineWidth = getLineWidth (hatchStyle);
			
			initializeContext(context, hatchHeight, false);

			/* draw background */
			drawBackground (context, backColor, hatchWidth, hatchHeight);
			
			/* draw lines in the foreground color */
			context.SetFillColor(foreColor);
			context.SetStrokeColor(foreColor);
			context.SetLineWidth(lineWidth);
			context.SetLineCap(CGLineCap.Square);

			float halfMe = hatchWidth / 2.0f;
			float quarter = halfMe / 2.0f;

			// this is really just 6 dots that when tiled will 
			// create the effect we are looking for
			CGRect rect = new CGRect(0,0,1,1);
			setPixels(context, rect);

			rect.Y = halfMe;
			setPixels(context, rect);

			rect.X = halfMe;
			setPixels(context, rect);

			rect.Y = 0;
			setPixels(context, rect);

			rect.X = quarter;
			rect.Y = quarter;
			setPixels(context, rect);

			rect.X = halfMe + quarter;
			rect.Y = halfMe + quarter;
			setPixels(context, rect);
		}


		void HatchShingle (CGContext context) 
		{
			var hatchWidth = getHatchWidth (hatchStyle);
			var hatchHeight = getHatchHeight (hatchStyle);
			var lineWidth = getLineWidth (hatchStyle);
			
			initializeContext(context, hatchHeight, false);

			/* draw background */
			drawBackground (context, backColor, hatchWidth, hatchHeight);
			
			/* draw lines in the foreground color */
			context.SetFillColor(foreColor);
			context.SetStrokeColor(foreColor);
			context.SetLineWidth(lineWidth);
			context.SetLineCap(CGLineCap.Square);
			
			float halfMe = hatchWidth / 2.0f;

			// We are basically going to draw a lamda sign

			// Draw base
			context.MoveTo(0,0);
			context.AddLineToPoint(halfMe,halfMe-HALF_PIXEL_Y);
			context.AddLineToPoint(hatchWidth, HALF_PIXEL_Y);
			context.StrokePath();

			// draw the first part of tail
			context.MoveTo(halfMe + HALF_PIXEL_X, halfMe);
			context.AddLineToPoint(halfMe + HALF_PIXEL_X, halfMe + 1);
			context.StrokePath();

			// now the last curl on the tail
			CGRect rect = new CGRect(1,hatchHeight-1,1,1);
			setPixels(context, rect);
			
			rect.X += 1;
			setPixels(context, rect);

			rect.X += 1;
			rect.Y -= 1;
			setPixels(context, rect);

		}

		void HatchDivot (CGContext context) 
		{
			var hatchWidth = getHatchWidth (hatchStyle);
			var hatchHeight = getHatchHeight (hatchStyle);
			var lineWidth = getLineWidth (hatchStyle);
			
			initializeContext(context, hatchHeight, false);

			/* draw background */
			drawBackground (context, backColor, hatchWidth, hatchHeight);
			
			/* draw lines in the foreground color */
			context.SetFillColor(foreColor);
			context.SetStrokeColor(foreColor);
			context.SetLineWidth(lineWidth);
			context.SetLineCap(CGLineCap.Square);
			
			float halfMe = hatchWidth / 2.0f;
			
			// draw a little wirly thingy
			CGRect rect = new CGRect(0,0,1,1);
			setPixels(context, rect);
			
			rect.X += 1;
			rect.Y += 1;
			setPixels(context, rect);
			
			rect.X -= 1;
			rect.Y += 1;
			setPixels(context, rect);

			// now top one
			rect.X = halfMe;// + HALF_PIXEL_X;
			rect.Y = halfMe;// + HALF_PIXEL_Y;
			setPixels(context, rect);

			rect.X -= 1;
			rect.Y += 1;
			setPixels(context, rect);

			rect.X += 1;
			rect.Y += 1;
			setPixels(context, rect);

		}

		void HatchPlaid (CGContext context) 
		{
			var hatchWidth = getHatchWidth (hatchStyle);
			var hatchHeight = getHatchHeight (hatchStyle);
			var lineWidth = getLineWidth (hatchStyle);
			
			initializeContext(context, hatchHeight, false);

			/* draw background */
			drawBackground (context, backColor, hatchWidth, hatchHeight);
			
			/* draw lines in the foreground color */
			context.SetFillColor(foreColor);
			context.SetStrokeColor(foreColor);
			context.SetLineWidth(lineWidth);
			context.SetLineCap(CGLineCap.Square);
			
			float halfMe = hatchWidth / 2.0f;
			CGRect rect = new CGRect(0,0,1,1);

			// fraw the alternating pattern for half of area
			int x = 0;
			int y = 0;
			
			for (y = 0; y<halfMe; y+=2)
			{
				for (x = 1; x < hatchWidth; x+=2)
				{
					rect.X = x;
					rect.Y = y;
					setPixels(context, rect);
				}
			}
			for (y = 1; y<halfMe; y+=2)
			{
				for (x = 0; x < hatchWidth; x+=2)
				{
					rect.X = x;
					rect.Y = y;
					setPixels(context, rect);
				}
			}

			// draw a square
			rect.X = 0;
			rect.Y = halfMe;
			rect.Width = halfMe;
			rect.Height = halfMe;
			setPixels(context, rect);
		}

		void setPixels (CGContext context, float x, float y, float size = 1.0f) 
		{
			setPixels(context, new CGRect((int)x, (int)y, size,size));

		}

		void setPixels (CGContext context, CGRect rect) 
		{
			context.FillRect(rect);

		}

		// Purple poka dots test
		private void DrawPolkaDotPattern (CGContext context)
		{
			context.SetFillColor(Color.Purple);
			context.FillEllipseInRect (new CGRect (4, 4, 8, 8));
		}

		// https://developer.apple.com/library/mac/#documentation/graphicsimaging/conceptual/drawingwithquartz2d/dq_patterns/dq_patterns.html#//apple_ref/doc/uid/TP30001066-CH206-TPXREF101
		internal override void Setup (Graphics graphics, bool fill)
		{

			// if this is the same as the last that was set then return
			if (graphics.LastBrush == this)
				return;

			// obtain our width and height so we can set the pattern rectangle
			float hatch_width = getHatchWidth (hatchStyle);
			float hatch_height = getHatchHeight (hatchStyle);

			//choose the pattern to be filled based on the currentPattern selected
			var patternSpace = CGColorSpace.CreatePattern(null);
			graphics.context.SetFillColorSpace(patternSpace);
			graphics.context.SetStrokeColorSpace(patternSpace);
			patternSpace.Dispose();

			// Pattern default work variables
			var patternRect = new CGRect(HALF_PIXEL_X,HALF_PIXEL_Y,hatch_width+HALF_PIXEL_X,hatch_height+HALF_PIXEL_Y);
			var patternTransform = CGAffineTransform.MakeIdentity();

			// Since all the patterns were developed with MonoMac on Mac OS the coordinate system is
			// defaulted to the lower left corner being 0,0 which means for MonoTouch and any view 
			// that is flipped we need to flip it again.  Yep should have thought about it to begin with
			// will look into changing it later if need be.
#if MONOMAC
			if (graphics.isFlipped)
				patternTransform = new CGAffineTransform(1, 0, 0, -1, 0, hatch_height);
#endif
#if MONOTOUCH
			if (!graphics.isFlipped)
				patternTransform = new CGAffineTransform(1, 0, 0, -1, 0, hatch_height);
#endif

			// DrawPattern callback which will be set depending on hatch style
			CGPattern.DrawPattern drawPattern;

			switch (hatchStyle) 
			{
			case HatchStyle.Horizontal:
			case HatchStyle.LightHorizontal:
			case HatchStyle.NarrowHorizontal:
			case HatchStyle.DarkHorizontal:
				drawPattern = HatchHorizontal;
				break;
			case HatchStyle.Vertical:
			case HatchStyle.LightVertical:
			case HatchStyle.NarrowVertical:
			case HatchStyle.DarkVertical:
				patternTransform = CGAffineTransform.MakeRotation(90 * (float)Math.PI / 180);
				drawPattern = HatchHorizontal;
				break;
			case HatchStyle.ForwardDiagonal:
			case HatchStyle.LightDownwardDiagonal:
			case HatchStyle.DarkDownwardDiagonal:
			case HatchStyle.WideDownwardDiagonal:
				// We will flip the x-axis here
				patternTransform = CGAffineTransform.MakeScale(-1,1);
				drawPattern = HatchUpwardDiagonal;
				break;
			case HatchStyle.BackwardDiagonal:
			case HatchStyle.LightUpwardDiagonal:
			case HatchStyle.DarkUpwardDiagonal:
			case HatchStyle.WideUpwardDiagonal:
				drawPattern = HatchUpwardDiagonal;
				break;
			case HatchStyle.LargeGrid:
			case HatchStyle.SmallGrid:
			case HatchStyle.DottedGrid:
				drawPattern = HatchGrid;
				break;
			case HatchStyle.DiagonalCross:
				drawPattern = HatchDiagonalCross;
				break;
			case HatchStyle.Percent05:
			case HatchStyle.Percent10:
			case HatchStyle.Percent20:
			case HatchStyle.Percent25:
			case HatchStyle.Percent30:
			case HatchStyle.Percent40:
			case HatchStyle.Percent50:
			case HatchStyle.Percent60:
			case HatchStyle.Percent70:
			case HatchStyle.Percent75:
			case HatchStyle.Percent80:
			case HatchStyle.Percent90:
				drawPattern = HatchPercentage;
				break;
			case HatchStyle.Sphere:
				drawPattern = HatchSphere;
				break;
			case HatchStyle.DashedDownwardDiagonal:
				patternTransform = CGAffineTransform.MakeScale(-1,1);
				drawPattern = HatchDashedDiagonal;
				break;
			case HatchStyle.DashedUpwardDiagonal:
				drawPattern = HatchDashedDiagonal;
				break;
			case HatchStyle.DashedHorizontal:
				drawPattern = HatchDashedHorizontal;
				break;
			case HatchStyle.DashedVertical:
				patternTransform = CGAffineTransform.MakeRotation(-90 * (float)Math.PI / 180);
				drawPattern = HatchDashedHorizontal;
				break;
			case HatchStyle.LargeConfetti:
			case HatchStyle.SmallConfetti:
				drawPattern = HatchConfetti;
				break;
			case HatchStyle.ZigZag:
				drawPattern = HatchZigZag;
				break;
			case HatchStyle.Wave:
				drawPattern = HatchWave;
				break;
			case HatchStyle.HorizontalBrick:
				drawPattern = HatchHorizontalBrick;
				break;
			case HatchStyle.DiagonalBrick:
				drawPattern = HatchDiagonalBrick;
				break;
//			case HatchStyle.Weave:
//				drawPattern = HatchWeave;
//				break;
			case HatchStyle.Trellis:
				drawPattern = HatchTrellis;
				break;
			case HatchStyle.LargeCheckerBoard:
			case HatchStyle.SmallCheckerBoard:
				drawPattern = HatchCheckered;
				break;
			case HatchStyle.OutlinedDiamond:
				drawPattern = HatchOutlinedDiamond;
				break;
			case HatchStyle.SolidDiamond:
				drawPattern = HatchSolidDiamond;
				break;
			case HatchStyle.DottedDiamond:
				drawPattern = HatchDottedDiamond;
				break;
			case HatchStyle.Divot:
				drawPattern = HatchDivot;
				break;
			case HatchStyle.Shingle:
				drawPattern = HatchShingle;
				break;
			case HatchStyle.Plaid:
				drawPattern = HatchPlaid;
				break;
			default:
				drawPattern = DrawPolkaDotPattern;
				break;
			}

			//set the pattern as the Current Context’s fill pattern
			var pattern = new CGPattern(patternRect, 
			                        patternTransform,
			                        hatch_width,hatch_height,
			                            CGPatternTiling.NoDistortion,
			                        true, drawPattern);
			//we dont need to set any color, as the pattern cell itself has chosen its own color
			graphics.context.SetFillPattern(pattern, new nfloat[] { 1 });
			graphics.context.SetStrokePattern(pattern, new nfloat[] { 1 });


			graphics.LastBrush = this;
			// I am setting this to be used for Text coloring in DrawString
			graphics.lastBrushColor = foreColor;
		}

		public override bool Equals(object obj)
		{
			return (obj is HatchBrush hb)
				&& backColor.Equals(hb.backColor)
				&& foreColor.Equals(hb.foreColor)
				&& hatchStyle.Equals(hb.hatchStyle);
		}
	}
}
