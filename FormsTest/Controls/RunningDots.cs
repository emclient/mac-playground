using System;
using System.Drawing;
using System.Drawing.Mac;
using System.Windows.Forms;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Foundation;

namespace FormsTest
{
	public class RunningDots : UserControl, IMacNativeControl
	{
		#region fields etc

		float shapeCount = 5;
		float duration = 3.75f;
		float easeFactor = 0.1f;
		NSColor shapeColor = Color.LightBlue.ToNSColor(); //NSColor.LightGray;

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

			var width = view.Bounds.Width;
			var distance = width / 5.0f / (float)shapeCount;
			// replicator layer
			replicatorLayer = new CAReplicatorLayer();
			replicatorLayer.Frame = new CGRect(0, 0, view.Bounds.Width*2, view.Bounds.Height);
			replicatorLayer.PreservesDepth = false;
			replicatorLayer.InstanceCount = (nint)shapeCount;
			replicatorLayer.InstanceTransform = CATransform3D.MakeTranslation(distance, 0, 0);
			replicatorLayer.InstanceDelay = -0.06f;
			if (view.Layer == null)
				view.Layer = new CALayer();

			view.Layer.AddSublayer(replicatorLayer);
			view.WantsLayer = true;

			// instance layer
			var dotSize = view.Bounds.Height;
			var instanceLayer = new CALayer();
			instanceLayer.Frame = new CGRect(-(shapeCount * distance / 2.0f) - dotSize, 0, dotSize, dotSize);
			instanceLayer.BackgroundColor = shapeColor.CGColor;
			instanceLayer.Bounds = new CGRect(0, 0, dotSize, dotSize);
			instanceLayer.CornerRadius = dotSize / 2.0f;
			replicatorLayer.AddSublayer(instanceLayer);

			var runAnimation = CABasicAnimation.FromKeyPath("transform.translation.x");
			runAnimation.From = NSNumber.FromDouble(-2f * width);
			runAnimation.To = NSNumber.FromDouble(3f * width);
			runAnimation.Duration = duration;
			runAnimation.RepeatCount = float.PositiveInfinity;
			runAnimation.TimingFunction = CAMediaTimingFunction.FromControlPoints(0, 1.0f - easeFactor, 1, easeFactor);

			instanceLayer.AddAnimation(runAnimation, "RunAnimation");
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
