using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormsTest
{
	public class LabelEx : Label
	{
		public enum MacTextAlignment { Default, Left, Right }


		[DefaultValue(typeof(Color), "GrayText")]
		public Color DisabledForeColor { get; set; } = SystemColors.GrayText;

		[DefaultValue(false)]
		public bool NoPadding { get; set; } = false;
		public bool NoClipping { get; set; } = false;

		[DefaultValue(MacTextAlignment.Default)]
		public MacTextAlignment MacAlignment { get; set; }

		protected override void OnPaint(PaintEventArgs e)
		{
			if (UseCompatibleTextRendering)
			{
				base.OnPaint(e);
			}
			else
			{
				if (Enabled && !NoPadding && !NoClipping)
					base.OnPaint(e);
				else
				{
					Rectangle rectangle = base.ClientRectangle.Inset(base.Padding);
					var method = typeof(Label).GetMethod("CreateTextFormatFlags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new Type[0], new ParameterModifier[0]); //.Where(m => m.GetParameters().Length == 0).FirstOrDefault();
					if (method != null)
					{
						var flags = (TextFormatFlags)method.Invoke(this, Array.Empty<object>()).EnsureNotNull();

						if (NoPadding)
							flags |= TextFormatFlags.NoPadding;
						if (NoClipping)
							flags |= TextFormatFlags.NoClipping;
						if (!UseMnemonic)
							flags |= TextFormatFlags.NoPrefix;


						TextRenderer.DrawText(e.Graphics, Text, Font, rectangle, Enabled ? ForeColor : DisabledForeColor, flags);
					}
					else
						base.OnPaint(e);
				}
			}
			
		}
	}
}
