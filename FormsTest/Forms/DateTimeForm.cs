using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace FormsTest
{
	public class DateTimeForm : Form
	{
		IContainer components;
		FlowLayoutPanel panel1;
		DateTimePicker picker1;
		MonthCalendar monthCal1;

		TextBox textBox1;

		public DateTimeForm()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			components = new Container();

			panel1 = new FlowLayoutPanel();
			picker1 = new DateTimePicker();
			monthCal1 = new MonthCalendar();
			textBox1 = new TextBox();

			// panel1
			panel1.SuspendLayout();
			panel1.FlowDirection = FlowDirection.TopDown;
			panel1.AutoSize = true;
			panel1.BorderStyle = BorderStyle.FixedSingle;
			panel1.Dock = DockStyle.Fill;
			panel1.Location = new Point(6, 6);
			panel1.Name = "panel1";
			panel1.Padding = new Padding(1);
			panel1.Size = new Size(600, 249);
			panel1.TabIndex = 0;

			// picker1
			picker1.AutoSize = true;
			picker1.Size = new Size(180, 27);
			picker1.ShowUpDown = false;
			picker1.Format = DateTimePickerFormat.Short;
			picker1.DropDownAlign = LeftRightAlignment.Left;

			//monthCal1
			monthCal1.AutoSize = true;
			monthCal1.Size = new Size(100, 27);
			monthCal1.Padding = new Padding(2);
			monthCal1.CalendarDimensions = new Size(1, 1);
			monthCal1.MaxSelectionCount = 1;
			monthCal1.Margin = Padding.Empty;
			monthCal1.MaxSelectionCount = 10;
			//monthCal1.Font = calendarFont;
			//monthCal1.BackColor = monthCalendar.BackColor = calendarBackColor;
			//monthCal1.TitleBackColor = calendarTitleBackColor;
			//monthCal1.TitleForeColor = calendarTitleForeColor;
			//monthCal1.TrailingForeColor = calendarTrailingForeColor;

			// textBox1
			this.textBox1.AutoSize = false;
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(250, 300);
			this.textBox1.TabIndex = 5;
			this.textBox1.Font = System.Drawing.SystemFonts.MenuFont;
			this.textBox1.Multiline = true;
			this.textBox1.AcceptsTab = true;
			this.textBox1.AcceptsReturn = true;
			this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;


			panel1.Controls.Add(picker1);
			panel1.Controls.Add(monthCal1);
			panel1.Controls.Add(textBox1);

			// DateTimeForm
			MinimumSize = new Size (100,100);
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			ClientSize = new Size(480, 320);
			Controls.Add(panel1);
			FormBorderStyle = FormBorderStyle.Sizable;
			SizeGripStyle = SizeGripStyle.Show;
			Name = "DateTimeForm";
			Padding = new Padding(6);
			Text = "DateTimeForm";

			panel1.ResumeLayout(true);
			ResumeLayout(false);
			PerformLayout();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			var cal = new HebrewCalendar();
 			var cul = CultureInfo.CreateSpecificCulture("he-IL");
			cul.DateTimeFormat.Calendar = cal;

			var sb = new StringBuilder();

			var today = DateTime.Now;
			var toyear = cal.GetYear(today);
			var newyear = cal.ToDateTime(toyear, 1, 1, 0, 0, 0, 0, cal.GetEra(today));

			var ddd = cal.ToDateTime(toyear, 1, 1, 0, 0, 0, 0, cal.GetEra(today));

			for (var j = 0; j < 10; ++j)
			{
				var y = toyear + j;
				var dd = cal.AddYears(ddd, j);
				sb.AppendLine($"({y}) {dd.ToString("y", cul)}");
				sb.AppendLine($"---------------");

				var months = cal.GetMonthsInYear(y);
				for (int i = 0; i < months; ++i)
				{
					var d = cal.AddMonths(dd, i);
					var MMMM = d.ToString("MMMM", cul);
					sb.AppendLine($"({1 + i}) {MMMM}");
				}

				sb.AppendLine();
			}

			textBox1.Text = sb.ToString();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
				components.Dispose();
			base.Dispose(disposing);
		}
	}
}
