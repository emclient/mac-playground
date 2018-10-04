using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using ObjCRuntime;

namespace FormsTest
{
	public class ProgressIndicator : UserControl, IMacNativeControl
	{
		#region fields etc

		NSView view;
		ProgressIndicatorLayer animationLayer;

		#endregion

		public ProgressIndicator()
		{
		}

		public virtual NSView CreateView()
		{
			view = new NSView();

			animationLayer = new ProgressIndicatorLayer();

			view.Layer = animationLayer;
			view.WantsLayer = true;

			return view;
		}

		public void StartSpinning()
		{
			animationLayer?.StartProgressAnimation();
		}

		public void StopSpinning()
		{
			animationLayer?.StopProgressAnimation();
		}

		public void ToggleSpinning()
		{
			animationLayer.ToggleProgressAnimation();
		}

		public bool IsSpinning { get { return animationLayer?.IsRunning ?? false; } }
	}

	class ProgressIndicatorLayer : CALayer
	{
		#region constants, fields, etc

		const string advancePositionSelName = "advancePosition";

		bool isRunning;
		NSTimer animationTimer;
		int position;

		CGColor foreColor;
		float fadeDownOpacity;

		int numFins;
		List<CALayer> finLayers;

		#endregion

		#region public methods and props

		public ProgressIndicatorLayer()
		{
			position = 0;
			numFins = 12;
			fadeDownOpacity = 0.0f;
			isRunning = false;

			foreColor = NSColor.Black.CGColor;

			base.Bounds = new CGRect(0.0f, 0.0f, 10.0f, 10.0f);
			CreateFinLayers();
		}

		public bool IsRunning
		{
			get { return isRunning; }
		}

		public NSColor Color
		{
			get { return NSColor.FromCGColor(foreColor); }
			set
			{
				foreColor = value.CGColor;

				// Update do all of the fins to this new color, at once, immediately
				CATransaction.Begin();
				CATransaction.SetValueForKey(NSNumber.FromBoolean(true), CATransaction.DisableActionsKey);
			    foreach (var fin in finLayers)
        			fin.BackgroundColor = foreColor;
				CATransaction.Commit();
			}
		}

		public void ToggleProgressAnimation()
		{
			if (IsRunning)
				StopProgressAnimation();
			else
				StartProgressAnimation();
		}

		public void StartProgressAnimation()
		{
			Hidden = false;
			isRunning = true;
			position = numFins - 1;
			SetNeedsDisplay();
			SetupAnimTimer();
		}

		public void StopProgressAnimation()
		{
			isRunning = false;
			DisposeAnimTimer();
			SetNeedsDisplay();
		}

		public override CGRect Bounds
		{ 
			get => base.Bounds;
			set
			{
				base.Bounds = value;

				// Resize the fins
				var finBounds = FinBoundsForCurrentBounds();
				var finAnchorPoint = FinAnchorPointForCurrentBounds();
				var finPosition = new CGPoint(Bounds.Width / 2f, Bounds.Height / 2f);
    			var finCornerRadius = finBounds.Width / 2f;

				// do the resizing all at once, immediately
				CATransaction.Begin();
		    	CATransaction.SetValueForKey(NSNumber.FromBoolean(true), CATransaction.DisableActionsKey);
		    	foreach (var fin in finLayers)
		    	{
		        	fin.Bounds = finBounds;
		        	fin.AnchorPoint = finAnchorPoint;
		        	fin.Position = finPosition;
		        	fin.CornerRadius = finCornerRadius;
			    }
				CATransaction.Commit();
			}			
		}

		#endregion

		#region internals

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				foreColor = null;
				RemoveFinLayers();
			}
		}

		void CreateFinLayers()
		{
			RemoveFinLayers();

			// Create new fin layers
			finLayers = new List<CALayer>(numFins);
			var finBounds = FinBoundsForCurrentBounds();
			var finAnchorPoint = FinAnchorPointForCurrentBounds();
			var finPosition = new CGPoint(Bounds.Width / 2f, Bounds.Height / 2f);
			var finCornerRadius = finBounds.Width / 2f;

			for (int i = 0; i < numFins; i++)
			{
				var newFin = new CALayer
				{
					Bounds = finBounds,
					AnchorPoint = finAnchorPoint,
					Position = finPosition,
					Transform = CATransform3D.MakeRotation(i * (-6.282185f / (float)numFins), 0.0f, 0.0f, 1.0f),
					CornerRadius = finCornerRadius,
					BackgroundColor = foreColor,
				};

				// Set the fin's initial opacity
				CATransaction.Begin();
				CATransaction.SetValueForKey(NSNumber.FromBoolean(true), CATransaction.DisableActionsKey);
				newFin.Opacity = fadeDownOpacity;
				CATransaction.Commit();

				// set the fin's fade-out time (for when it's animating)
				var anim = new CABasicAnimation();
				anim.Duration = 0.6f;
				var actions = NSDictionary.FromObjectAndKey(anim, (NSString)"opacity");
				newFin.Actions = actions;

				AddSublayer(newFin);
				finLayers.Add(newFin);
			}
		}

		CGRect FinBoundsForCurrentBounds()
		{
			var size = Bounds.Size;
			var minSide = size.Width > size.Height ? size.Height : size.Width;
			var width = minSide * 0.08f;
			var height = minSide * 0.25f;
			return new CGRect(0, 0, width, height);
		}

		CGPoint FinAnchorPointForCurrentBounds()
		{
			var size = Bounds.Size;
			var minSide = size.Width > size.Height ? size.Height : size.Width;
			var height = minSide * 0.25f;
			return new CGPoint(0.5, -.2f + -0.9f * (minSide - height) / minSide);
		}

		void RemoveFinLayers()
		{
			if (finLayers != null)
				foreach (var layer in finLayers)
					layer.RemoveFromSuperLayer();
		}

		void SetupAnimTimer()
		{
			DisposeAnimTimer();

			// Why animate if not visible?  viewDidMoveToWindow will re-call this method when needed.
			animationTimer = NSTimer.CreateTimer(0.04, this, new Selector(advancePositionSelName), null, true);

			animationTimer.FireDate = new NSDate();
			NSRunLoop.Current.AddTimer(animationTimer, NSRunLoopMode.Common);
			NSRunLoop.Current.AddTimer(animationTimer, NSRunLoopMode.Default);
			NSRunLoop.Current.AddTimer(animationTimer, NSRunLoopMode.EventTracking);
		}

		void DisposeAnimTimer()
		{
			if (animationTimer != null)
			{
				animationTimer.Invalidate();
				animationTimer = null;
			}
		}

		[Export(advancePositionSelName)]
		void AdvancePosition()
		{
			position = (1 + position) % numFins;
			var fin = finLayers[position];

			// Set the next fin to full opacity, but do it immediately, without any animation
			CATransaction.Begin();
			CATransaction.SetValueForKey(NSNumber.FromBoolean(true), CATransaction.DisableActionsKey);
    		fin.Opacity = 1.0f;
			CATransaction.Commit();

    		// Tell that fin to animate its opacity to transparent.
    		fin.Opacity = fadeDownOpacity;

			SetNeedsDisplay();
		}

		#endregion
	}
}
