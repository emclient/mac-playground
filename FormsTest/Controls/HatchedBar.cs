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
	public class HatchedBar : UserControl, IMacNativeControl
	{
		#region fields etc

		float hatchSize = 16;
		float shearFactor = -0.5f;
		NSColor trackColor = Color.DarkGray.ToNSColor(); //NSColor.DarkGray;
		NSColor hatchColor = Color.LightBlue.ToNSColor(); //NSColor.LightGray;

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

			var shear = new CGAffineTransform(1.0f, 0, shearFactor, 1.0f, 0.0f, 0.0f);

			// replicator layer
			replicatorLayer = new CAReplicatorLayer();
			replicatorLayer.BackgroundColor = trackColor.CGColor;
			replicatorLayer.Frame = view.Bounds;
			replicatorLayer.InstanceCount = (nint)(1 + view.Bounds.Width / hatchSize);
			replicatorLayer.PreservesDepth = false;
			replicatorLayer.InstanceTransform = CATransform3D.MakeTranslation((nfloat)(hatchSize * 2.0), 0, 0);

			if (view.Layer == null)
				view.Layer = new CALayer();

			view.Layer.AddSublayer(replicatorLayer);
			view.WantsLayer = true;

			// instance layer
			var layerHeight = view.Bounds.Height;
			var layerWidth = hatchSize;
			var instanceLayer = new CALayer();
			instanceLayer.Frame = new CGRect(0, 0, layerWidth, layerHeight);
			instanceLayer.BackgroundColor = hatchColor.CGColor;
			instanceLayer.Bounds = new CGRect(0, 0, layerWidth, layerHeight);
			instanceLayer.Transform = CATransform3D.MakeFromAffine(shear);
			replicatorLayer.AddSublayer(instanceLayer);

			var runAnimation = CABasicAnimation.FromKeyPath("transform.translation.x");
			runAnimation.From = NSNumber.FromDouble(-hatchSize);
			runAnimation.To = NSNumber.FromDouble(hatchSize);
			runAnimation.Duration = 0.5;
			runAnimation.RepeatCount = float.PositiveInfinity;
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
