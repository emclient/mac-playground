using System;
using System.Windows.Forms;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Foundation;

namespace FormsTest
{
	public class ProgressIndicator : UserControl, IMacNativeControl
	{
		#region fields etc

		int finCount = 12;
		double revolutionDuration = 0.5;
		double finAspectRatio = 0.3;		// finWidth / finHeight
		double finSizeMultiplier = 0.26; 	// finHeight / viewSize
		NSColor color = NSColor.Black;

		PIView view;
		CAReplicatorLayer replicatorLayer;

		#endregion

		#region public api

		#endregion

		#region internals

		void SetupLayers()
		{
			if (view == null || view.Bounds.Size.IsEmpty)
				return;

			RemoveLayers();

			// replicator layer
			replicatorLayer = new CAReplicatorLayer();
			replicatorLayer.Frame = view.Bounds;
			replicatorLayer.InstanceCount = finCount;
			replicatorLayer.PreservesDepth = false;
			replicatorLayer.InstanceColor = color.CGColor;
			var angle = (nfloat)(Math.PI * 2.0 / (double)finCount);
			replicatorLayer.InstanceTransform = CATransform3D.MakeRotation(angle, 0, 0, 1);

			if (view.Layer == null)
				view.Layer = new CALayer();

			view.Layer.AddSublayer(replicatorLayer);
			view.WantsLayer = true;

			// instance layer
			var layerHeight = Math.Min(view.Bounds.Width, view.Bounds.Height) * finSizeMultiplier;
			var layerWidth = layerHeight * finAspectRatio;
			var midX = view.Bounds.GetMidX() - layerWidth / 2.0;
			var instanceLayer = new CALayer();
			instanceLayer.Frame = new CGRect(midX, 0, layerWidth, layerHeight);
			instanceLayer.BackgroundColor = color.CGColor;
			instanceLayer.CornerRadius = (nfloat)(layerWidth / 2.0);
			instanceLayer.Bounds = new CGRect(CGPoint.Empty, new CGSize(layerWidth, layerHeight));

			replicatorLayer.AddSublayer(instanceLayer);

			// fade animation
			var fadeAnimation = CABasicAnimation.FromKeyPath("opacity");
			fadeAnimation.From = NSNumber.FromDouble(1.0);
			fadeAnimation.To = NSNumber.FromDouble(0);
			fadeAnimation.RepeatCount = float.MaxValue;

			//set layer fade animation
			instanceLayer.Opacity = 0;
			fadeAnimation.Duration = revolutionDuration;
			instanceLayer.AddAnimation(fadeAnimation, "FadeAnimation");

			replicatorLayer.InstanceDelay = revolutionDuration / (double)finCount;
		}

		void RemoveLayers()
		{
			if (replicatorLayer != null)
			{
				replicatorLayer.RemoveAllAnimations();
				replicatorLayer.RemoveFromSuperLayer();
				replicatorLayer = null;
			}
		}

		public virtual NSView CreateView()
		{
			view = new PIView();
			view.SizeChanged += View_SizeChanged;
			return view;
		}

		void View_SizeChanged(object sender, EventArgs e)
		{
			SetupLayers();
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);

			if (Enabled)
				SetupLayers();
			else
				RemoveLayers();
		}

		class PIView : NSView
		{
			public event EventHandler<EventArgs> SizeChanged;

			public override void SetFrameSize(CGSize newSize)
			{
				var size = Frame.Size;

				base.SetFrameSize(newSize);

				if (size != newSize)
					SizeChanged(this, EventArgs.Empty);
			}

			public override bool IsFlipped
			{
				get { return true; }
			}
		}

		#endregion
	}
}
