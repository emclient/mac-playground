using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace FormsTest
{
	partial class RegionsForm : Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;
		Panel mainPanel;

		public RegionsForm()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			components = new Container();

			this.mainPanel = new Panel();
            this.SuspendLayout();

			// mainPanel
			mainPanel.SuspendLayout();
			mainPanel.AutoSize = true;
			mainPanel.BorderStyle = BorderStyle.FixedSingle;
			mainPanel.Dock = DockStyle.Fill;
			mainPanel.Name = "mainPanel";
			mainPanel.Padding = new Padding(1);

			mainPanel.Paint += new PaintEventHandler(PaintHandler);

			Controls.Add(mainPanel);
			FormBorderStyle = FormBorderStyle.Sizable;
			SizeGripStyle = SizeGripStyle.Show;
			Name = "RegionsForm";
			Padding = new Padding(6);
			Text = "Regions";

			mainPanel.ResumeLayout(true);
			ResumeLayout(false);
			PerformLayout();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
				components.Dispose();
			base.Dispose(disposing);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		void PaintHandler(object sender, PaintEventArgs e)
		{
			var g = e.Graphics;
			g.FillRectangle(Brushes.LightGray, e.ClipRectangle);

			var rect1 = new Rectangle (30, 30, 60, 80);
			var rect2 = new Rectangle (60, 40, 60, 80);
			using var rgn1 = new Region (rect1);
			using var rgn2 = new Region (rect2);

			// Exclusion
			g.DrawString("Exclusion", Font, Brushes.Black, new PointF(30, 10));
			g.DrawRectangle(Pens.Green, rect1);
            g.DrawRectangle(Pens.Black, rect2);
			using var exclusion = rgn1.Clone();
            exclusion.Exclude(rgn2);			
            g.FillRegion(Brushes.Blue, exclusion);

			e.Graphics.TranslateTransform(120, 0);

			// Complement
			g.DrawString("Complement", Font, Brushes.Black, new PointF(30, 10));
			g.DrawRectangle (Pens.Green, rect1);
			g.DrawRectangle (Pens.Black, rect2);
			using var complement = rgn1.Clone();
			complement.Complement(rect2);
			g.FillRegion(Brushes.Blue, complement);

			e.Graphics.TranslateTransform(-120, 120);

			// Xor
			g.DrawString("Xor", Font, Brushes.Black, new PointF(30, 10));
			g.DrawRectangle(Pens.Green, rect1);
            g.DrawRectangle(Pens.Black, rect2);
			using var xor = rgn1.Clone();
            xor.Xor(rgn2);
            g.FillRegion(Brushes.Blue, xor);

			e.Graphics.TranslateTransform(120, 0);

			// Union
			g.DrawString("Union", Font, Brushes.Black, new PointF(30, 10));
			g.DrawRectangle(Pens.Green, rect1);
            g.DrawRectangle(Pens.Black, rect2);
			using var union = rgn1.Clone();
            union.Union(rgn2);
            g.FillRegion(Brushes.Blue, union);
		}
	}
}
