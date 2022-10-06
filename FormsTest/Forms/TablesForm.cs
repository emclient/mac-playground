using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace FormsTest
{
	partial class TablesForm : Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;
		Panel mainPanel;
		FlowLayoutPanel rightPanel, leftPanel;
		TableLayoutPanel table1;
		public TablesForm()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			components = new Container();

			this.mainPanel = new Panel();
			this.leftPanel = new FlowLayoutPanel();
			this.rightPanel = new FlowLayoutPanel();
			this.table1 = new TableLayoutPanel();
            this.SuspendLayout();

			// mainPanel
			mainPanel.SuspendLayout();
			//mainPanel.FlowDirection = FlowDirection.LeftToRight;
			mainPanel.AutoSize = true;
			mainPanel.BorderStyle = BorderStyle.FixedSingle;
			mainPanel.Dock = DockStyle.Fill;
			mainPanel.Name = "panel1";
			mainPanel.Padding = new Padding(1);
			mainPanel.Controls.Add(rightPanel);
			mainPanel.Controls.Add(leftPanel);

			// rightPanel
			leftPanel.SuspendLayout();
			leftPanel.AutoSize = true;
			leftPanel.BorderStyle = BorderStyle.FixedSingle;
			leftPanel.Dock = DockStyle.Left;
			leftPanel.Name = "leftPanel";
			leftPanel.FlowDirection = FlowDirection.TopDown;
			leftPanel.Padding = new Padding(1);

			// rightPanel
			rightPanel.SuspendLayout();
			rightPanel.AutoSize = true;
			rightPanel.BorderStyle = BorderStyle.FixedSingle;
			rightPanel.Dock = DockStyle.Right;
			rightPanel.Name = "rightPanel";
			rightPanel.FlowDirection = FlowDirection.TopDown;
			rightPanel.Padding = new Padding(1);

			table1.SuspendLayout();
			table1.AutoSize = true;
			table1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			table1.BorderStyle = BorderStyle.FixedSingle;
			table1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
			table1.Name = "table1";
			table1.Padding = new Padding(1);

			table1.RowStyles.Add(new RowStyle(SizeType.AutoSize, 0));
			table1.RowStyles.Add(new RowStyle(SizeType.AutoSize, 0));
			table1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
			table1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

			// table1.Controls.Add(new Panel { BackColor = Color.Yellow, AutoSize = true, Dock = DockStyle.Fill }, 0, 0);
			table1.Controls.Add(new Label { Text = "Row 0", AutoSize = true }, 0, 0);
			table1.Controls.Add(new Label { Text = "Row 1", AutoSize = true }, 0, 1);

			var square = new Panel { BackColor = Color.Beige, Size = new Size(80, 80) };
			table1.Controls.Add(square, 1, 0);
			table1.SetRowSpan(square, 2);

			leftPanel.Controls.Add(table1);

			var button1 = new Button();
			button1.AutoSize = true;
			//button1.Click += button1_Click;
			button1.Text = "Button 1";
			rightPanel.Controls.Add(button1);

			var button2 = new Button();
			button2.AutoSize = true;
			//button2.Click += button2_Click;
			button2.Text = "Button 2";
			rightPanel.Controls.Add(button2);

			var button3 = new Button();
			button3.AutoSize = true;
			//button1.Click += button3_Click;
			button3.Text = "Button 3";
			rightPanel.Controls.Add(button3);

			// TablesForm
			MinimumSize = new Size(100, 100);
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			ClientSize = new Size(480, 320);
			Controls.Add(mainPanel);
			FormBorderStyle = FormBorderStyle.Sizable;
			SizeGripStyle = SizeGripStyle.Show;
			Name = "TablesForm";
			Padding = new Padding(6);
			Text = "TablesForm";

			table1.ResumeLayout(true);
			leftPanel.ResumeLayout(true);
			rightPanel.ResumeLayout(true);
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
	}
}
