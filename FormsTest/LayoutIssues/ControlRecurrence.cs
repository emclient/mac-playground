using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
//using MailClient.Common.UI;
/*using MailClient.Schedule;
using MailClient.Schedule.Event;
using MailClient.Schedule.Recurrence;
using MailClient.Schedule.TimeZone;*/

namespace MailClient.UI.Controls
{
	public partial class ControlRecurrence : UserControl
	{
		//private EventComposer previewItem;
		//private RecurrenceRuleComposer previewRule;
		private bool changed;
		private bool readOnly;
		private bool loading;
		private DateTime start;
		private DateTime end;
		private bool rangeDaySet = false;
		private bool settingRangeDay = false;
		private bool settingRadioButton;

		public ControlRecurrence()
		{
			InitializeComponent();

			this.changed = false;

			check_Recurrence_WeeklyMonday.Text = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)DayOfWeek.Monday];
			check_Recurrence_WeeklyTuesday.Text = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)DayOfWeek.Tuesday];
			check_Recurrence_WeeklyWednesday.Text = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)DayOfWeek.Wednesday];
			check_Recurrence_WeeklyThursday.Text = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)DayOfWeek.Thursday];
			check_Recurrence_WeeklyFriday.Text = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)DayOfWeek.Friday];
			check_Recurrence_WeeklySaturday.Text = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)DayOfWeek.Saturday];
			check_Recurrence_WeeklySunday.Text = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)DayOfWeek.Sunday];
			combo_Recurrence_MonthPositionDay.Items.Clear();
			combo_Recurrence_YearlyPositionDay.Items.Clear();
			combo_Recurrence_YearlyPositionMonth.Items.Clear();
			combo_Recurrence_YearlyDayMonth.Items.Clear();
			for (int day = 0; day < 7; day++)
			{
				combo_Recurrence_MonthPositionDay.Items.Add(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[day]);
				combo_Recurrence_YearlyPositionDay.Items.Add(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[day]);
			}
			for (int month = 0; month < 12; month++)
			{
				combo_Recurrence_YearlyPositionMonth.Items.Add(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[month]);
				combo_Recurrence_YearlyDayMonth.Items.Add(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[month]);
			}

			radio_Recurrence_FrequencyNone.Checked = true;
			radio_Recurrence_RangeNoEnd.Checked = true;
			combo_Recurrence_MonthPosition.SelectedIndex = 0;
			combo_Recurrence_MonthPositionDay.SelectedIndex = 0;
			combo_Recurrence_YearlyDayMonth.SelectedIndex = 0;
			combo_Recurrence_YearlyPosition.SelectedIndex = 0;
			combo_Recurrence_YearlyPositionDay.SelectedIndex = 0;
			combo_Recurrence_YearlyPositionMonth.SelectedIndex = 0;

			//if (!UIUtils.DesignMode)
			{
				/*this.previewItem = new EventComposer(
					null, null, DateTime.MinValue,
					DateTime.MinValue, DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc), TimeZoneConvert.Floating,
					DateTime.MaxValue, DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc), TimeZoneConvert.Floating, ScheduleStatus.NoStatus,
					String.Empty, String.Empty, String.Empty, null, null,
					null, String.Empty, null, new ScheduleClass(StandardScheduleClass.Public), ScheduleTransparency.Opaque,
					new TimeSpan(-1), default(DateTime),
					DateTime.MinValue, DateTime.MinValue,
					TimeZoneConvert.Floating, null);
				this.previewRule = previewItem.Recurrence as RecurrenceRuleComposer;*/

				setFont();
			}
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			setFont();
		}

		protected override void OnLoad(EventArgs e)
		{
			Size biggest = Size.Empty;
			foreach (Control ctrl in controlPanelSwitcher1.Controls)
			{
				Size sz = ctrl.GetPreferredSize(new Size(0, 0));

				if (sz.Width + ctrl.Margin.Horizontal > biggest.Width)
					biggest.Width = sz.Width + ctrl.Margin.Horizontal;
				if (sz.Height + ctrl.Margin.Vertical > biggest.Height)
					biggest.Height = sz.Height + ctrl.Margin.Vertical;

				// strange but getting the size for parent control is probably not enough
                foreach (Control innerCtrl in ctrl.Controls)
                {
                    sz = innerCtrl.GetPreferredSize(new Size(0, 0));

                    if (sz.Width + innerCtrl.Margin.Horizontal > biggest.Width)
                        biggest.Width = sz.Width + innerCtrl.Margin.Horizontal;
                    if (sz.Height + innerCtrl.Margin.Vertical > biggest.Height)
                        biggest.Height = sz.Height + innerCtrl.Margin.Vertical;
                }
            }

			tableLayoutPanel_Main.ColumnStyles[2].SizeType = SizeType.Absolute;
			tableLayoutPanel_Main.ColumnStyles[2].Width = biggest.Width;
			//tableLayoutPanel_Main.RowStyles[1].Height = Math.Max(biggest.Height, tableLayoutPanel_RecurrenceRange.Height) + 3;

			base.OnLoad(e);

			controlPanelSwitcher1.SetPanelsVisibility = true;
		}

		private void setFont()
		{
			label_Frequency.Font = label_Pattern.Font = label_RecurrenceRange.Font = label_Exmple.Font =
			label_Recurrence_NoneThisEvent.Font = MailClient.Common.UI.FontCache.TryCreateSemiboldFont(this.Font);
		}

		[DefaultValue(false)]
		public bool ReadOnly
		{
			get
			{
				return this.readOnly;
			}
			set
			{
				this.readOnly = value;
				this.radio_Recurrence_FrequencyDaily.Enabled = !value;
				this.radio_Recurrence_FrequencyYearly.Enabled = !value;
				this.radio_Recurrence_FrequencyMonthly.Enabled = !value;
				this.radio_Recurrence_FrequencyNone.Enabled = !value;
				this.radio_Recurrence_FrequencyWeekly.Enabled = !value;
				this.radio_Recurrence_YearlyOnYearDay.Enabled = !value;
				this.radio_Recurrence_YearlyOnMonthDayPosition.Enabled = !value;
				this.radio_Recurrence_MonthlyOnMonthDay.Enabled = !value;
				this.radio_Recurrence_MonthOnPosition.Enabled = !value;
				this.radio_Recurrence_RangeEndAfter.Enabled = !value;
				this.radio_Recurrence_RangeEndBy.Enabled = !value;
				this.radio_Recurrence_RangeNoEnd.Enabled = !value;
				this.text_Recurrence_DailyIntervalDays.Enabled = !value;
				this.text_Recurrence_YearlyDayNumber.Enabled = !value;
				this.text_Recurrence_YearlyInterval.Enabled = !value;
				this.text_Recurrence_MonthDay.Enabled = !value;
				this.text_Recurrence_MonthlyIntervalMonths.Enabled = !value;
				this.text_Recurrence_RangeOccurrences.Enabled = !value;
				this.text_Recurrence_WeeklyInterval.Enabled = !value;
				this.check_Recurrence_WeeklyFriday.Enabled = !value;
				this.check_Recurrence_WeeklyMonday.Enabled = !value;
				this.check_Recurrence_WeeklySaturday.Enabled = !value;
				this.check_Recurrence_WeeklySunday.Enabled = !value;
				this.check_Recurrence_WeeklyThursday.Enabled = !value;
				this.check_Recurrence_WeeklyTuesday.Enabled = !value;
				this.check_Recurrence_WeeklyWednesday.Enabled = !value;
				this.date_Recurrence_RangeDay.Enabled = !value;
				SwitchRecurrenceControls();
			}
		}

		public bool Changed
		{
			get { return this.changed; }
			set { this.changed = value; }
		}

		/*public IRecurrenceRule PreviewRule
		{
			get { return this.previewRule; }
		}*/

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DateTime Start
		{
			get { return this.start; }
			set
			{
				this.start = value;
				//this.previewItem.Start = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
				RulePropertyUpdated();

				if (!rangeDaySet)
				{
					settingRangeDay = true;
					date_Recurrence_RangeDay.Value = value.Date;
					rangeDaySet = true;
					settingRangeDay = false;
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DateTime End
		{
			get { return this.end; }
			set
			{
				this.end = value;
				UpdateRulePreview();
			}
		}

		/*public void LoadData(IRecurrenceRule rule, TimeZoneInfo startTimeZone)
		{
			this.previewItem = new EventComposer(
				null, rule, DateTime.MinValue,
				DateTime.MinValue, DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc), TimeZoneConvert.Floating,
				DateTime.MaxValue, DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc), TimeZoneConvert.Floating,
				ScheduleStatus.NoStatus,
				String.Empty, String.Empty, String.Empty, null, null,
				null, String.Empty, null, new ScheduleClass(StandardScheduleClass.Public), ScheduleTransparency.Opaque,
				new TimeSpan(-1), default(DateTime),
				DateTime.MinValue, DateTime.MinValue,
				TimeZoneConvert.Floating, null);
			this.previewRule = previewItem.Recurrence as RecurrenceRuleComposer;

			this.loading = true;
			LoadRecurrenceRule(rule, startTimeZone);
			UpdateRulePreview();
			this.loading = false;
		}*/

		private void rbNone_CheckedChanged(object sender, EventArgs e)
		{
			label_Exmple.Visible = !radio_Recurrence_FrequencyNone.Checked;

			if (radio_Recurrence_FrequencyNone.Checked)
			{
				controlPanelSwitcher1.ActivatePanel("panel_Recurrence_None");
				RulePropertyUpdated();
			}
		}

		private void rbDaily_CheckedChanged(object sender, EventArgs e)
		{
			if (radio_Recurrence_FrequencyDaily.Checked)
			{
				controlPanelSwitcher1.ActivatePanel("panel_Recurrence_Daily");
				//UpdateDailyRecurrence(false);
				RulePropertyUpdated();
			}
		}

		private void rbWeekly_CheckedChanged(object sender, EventArgs e)
		{
			if (radio_Recurrence_FrequencyWeekly.Checked)
			{
				controlPanelSwitcher1.ActivatePanel("panel_Recurrence_Weekly");
				//UpdateWeeklyRecurrence(false);
				RulePropertyUpdated();
			}
		}

		private void rbMonthly_CheckedChanged(object sender, EventArgs e)
		{
			if (radio_Recurrence_FrequencyMonthly.Checked)
			{
				controlPanelSwitcher1.ActivatePanel("panel_Recurrence_Monthly");
				//UpdateMonthlyRecurrence(false);
				RulePropertyUpdated();
			}
		}

		private void rbYearly_CheckedChanged(object sender, EventArgs e)
		{
			if (radio_Recurrence_FrequencyYearly.Checked)
			{
				controlPanelSwitcher1.ActivatePanel("panel_Recurrence_Yearly");
				//UpdateYearlyRecurrence(false);
				RulePropertyUpdated();
			}
		}

		private void text_Recurrence_RangeOccurrences_TextChanged(object sender, EventArgs e)
		{
			if (!loading && !radio_Recurrence_RangeEndAfter.Checked)
				radio_Recurrence_RangeEndAfter.Checked = true;
			else
				RulePropertyUpdated();
		}

		private void date_Recurrence_RangeDay_ValueChanged(object sender, EventArgs e)
		{
			if (settingRangeDay)
				return;

			if (!loading && !radio_Recurrence_RangeEndBy.Checked)
				radio_Recurrence_RangeEndBy.Checked = true;
			else
				RulePropertyUpdated();

		}

		private void ruleControl_Changed(object sender, EventArgs e)
		{
			if (settingRadioButton)
				return;

			if (sender is RadioButton)
			{
				settingRadioButton = true;

				// handle radiobuttons which do not have common parent control
				if (sender == radio_Recurrence_MonthOnPosition)
				{
					radio_Recurrence_MonthlyOnMonthDay.Checked = !radio_Recurrence_MonthOnPosition.Checked;
                }
				else if (sender == radio_Recurrence_MonthlyOnMonthDay)
				{
					radio_Recurrence_MonthOnPosition.Checked = !radio_Recurrence_MonthlyOnMonthDay.Checked;
				}
				if (sender == radio_Recurrence_YearlyOnMonthDayPosition)
				{
					radio_Recurrence_YearlyOnYearDay.Checked = !radio_Recurrence_YearlyOnMonthDayPosition.Checked;
				}
				else if (sender == radio_Recurrence_YearlyOnYearDay)
				{
					radio_Recurrence_YearlyOnMonthDayPosition.Checked = !radio_Recurrence_YearlyOnYearDay.Checked;
				}

				settingRadioButton = false;
			}

			RulePropertyUpdated();

		}

		#region Recurrence helper methods

		/*private void LoadRecurrenceRule(IRecurrenceRule rule, TimeZoneInfo startTimeZone)
		{
			switch (rule.Frequency)
			{
				case RecurrenceFrequency.NoRecurrence:
					radio_Recurrence_FrequencyNone.Checked = true;
					break;
				case RecurrenceFrequency.Daily:
					radio_Recurrence_FrequencyDaily.Checked = true;
					text_Recurrence_DailyIntervalDays.Text = Convert.ToString(rule.Interval, CultureInfo.InvariantCulture);
					break;
				case RecurrenceFrequency.Weekly:
					radio_Recurrence_FrequencyWeekly.Checked = true;
					UpdateWeeklyRecurrence(true);
					break;
				case RecurrenceFrequency.Monthly:
					radio_Recurrence_FrequencyMonthly.Checked = true;
					UpdateMonthlyRecurrence(true);
					break;
				case RecurrenceFrequency.Yearly:
					radio_Recurrence_FrequencyYearly.Checked = true;
					UpdateYearlyRecurrence(true);
					break;
			}

			if (rule.Count != 0)
			{
				radio_Recurrence_RangeEndAfter.Checked = true;
				text_Recurrence_RangeOccurrences.Text = Convert.ToString(rule.Count, CultureInfo.InvariantCulture);
			}
			else if (rule.Until != DateTime.MinValue)
			{
				radio_Recurrence_RangeEndBy.Checked = true;
				DateTime until = startTimeZone.ConvertTimeFromUtc(rule.Until);

				if (until > date_Recurrence_RangeDay.MaxDate)
					until = date_Recurrence_RangeDay.MaxDate;
				else if (until < date_Recurrence_RangeDay.MinDate)
					until = date_Recurrence_RangeDay.MinDate;
				date_Recurrence_RangeDay.Value = until;
			}
		}

		public void SaveRecurrenceRule(IRecurrenceRule rule, TimeZoneInfo startTimeZone)
		{
			//if (loading)
			//	return;

			ClearRecurrenceRule(rule);

			//We store recurrence start always to be able to prefill some recurrence fields from previewRule
			DateTime start, end;
			start = DateTime.SpecifyKind(this.Start, DateTimeKind.Unspecified);
			end = DateTime.SpecifyKind(this.End, DateTimeKind.Unspecified);
			rule.RecurrenceStart = DateTime.SpecifyKind(TimeZoneInfo.Local.ConvertTimeToTimeZone(start, startTimeZone), DateTimeKind.Unspecified);
			rule.RecurrenceDuration = start < end ? (end - start) : default(TimeSpan);
			if (radio_Recurrence_FrequencyNone.Checked)
			{
				rule.Frequency = RecurrenceFrequency.NoRecurrence;
			}
			else
			{
				if (radio_Recurrence_FrequencyDaily.Checked)
				{
					rule.Frequency = RecurrenceFrequency.Daily;
					rule.Interval = Convert.ToInt32(text_Recurrence_DailyIntervalDays.Text, CultureInfo.InvariantCulture);
				}
				else if (radio_Recurrence_FrequencyWeekly.Checked)
				{
					rule.Frequency = RecurrenceFrequency.Weekly;
					rule.Interval = Convert.ToInt32(text_Recurrence_WeeklyInterval.Text, CultureInfo.InvariantCulture);
					List<WeekDayNum> list = new List<WeekDayNum>();
					if (check_Recurrence_WeeklyMonday.Checked) list.Add(new WeekDayNum(0, DayOfWeek.Monday));
					if (check_Recurrence_WeeklyTuesday.Checked) list.Add(new WeekDayNum(0, DayOfWeek.Tuesday));
					if (check_Recurrence_WeeklyWednesday.Checked) list.Add(new WeekDayNum(0, DayOfWeek.Wednesday));
					if (check_Recurrence_WeeklyThursday.Checked) list.Add(new WeekDayNum(0, DayOfWeek.Thursday));
					if (check_Recurrence_WeeklyFriday.Checked) list.Add(new WeekDayNum(0, DayOfWeek.Friday));
					if (check_Recurrence_WeeklySaturday.Checked) list.Add(new WeekDayNum(0, DayOfWeek.Saturday));
					if (check_Recurrence_WeeklySunday.Checked) list.Add(new WeekDayNum(0, DayOfWeek.Sunday));
					rule.ByDay = list.ToArray();
				}
				else if (radio_Recurrence_FrequencyMonthly.Checked)
				{
					rule.Frequency = RecurrenceFrequency.Monthly;
					rule.Interval = Convert.ToInt32(text_Recurrence_MonthlyIntervalMonths.Text, CultureInfo.InvariantCulture);
					if (radio_Recurrence_MonthlyOnMonthDay.Checked)
					{
						rule.ByMonthDay = new int[] { Convert.ToInt32(text_Recurrence_MonthDay.Text, CultureInfo.InvariantCulture) };
					}
					else if (radio_Recurrence_MonthOnPosition.Checked)
					{
						int pos, dayNum;
						if (combo_Recurrence_MonthPosition.SelectedIndex == 4) pos = -1;
						else pos = combo_Recurrence_MonthPosition.SelectedIndex + 1;
						dayNum = combo_Recurrence_MonthPositionDay.SelectedIndex;
						rule.ByDay = new WeekDayNum[] { new WeekDayNum(pos, (DayOfWeek)dayNum) };
					}

				}
				else if (radio_Recurrence_FrequencyYearly.Checked)
				{
					rule.Frequency = RecurrenceFrequency.Yearly;
					rule.Interval = Convert.ToInt32(text_Recurrence_YearlyInterval.Text, CultureInfo.InvariantCulture);
					if (radio_Recurrence_YearlyOnYearDay.Checked)
					{
						rule.ByMonth = new int[] { combo_Recurrence_YearlyDayMonth.SelectedIndex + 1 };
						rule.ByMonthDay = new int[] { Convert.ToInt32(text_Recurrence_YearlyDayNumber.Text, CultureInfo.InvariantCulture) };
					}
					else if (radio_Recurrence_YearlyOnMonthDayPosition.Checked)
					{
						int pos, dayNum;
						if (combo_Recurrence_YearlyPosition.SelectedIndex == 4) pos = -1;
						else pos = combo_Recurrence_YearlyPosition.SelectedIndex + 1;
						dayNum = combo_Recurrence_YearlyPositionDay.SelectedIndex;
						rule.ByMonth = new int[] { combo_Recurrence_YearlyPositionMonth.SelectedIndex + 1 };
						rule.ByDay = new WeekDayNum[] { new WeekDayNum(pos, (DayOfWeek)dayNum) };
					}
				}

				if (radio_Recurrence_RangeEndAfter.Checked)
				{
					rule.Count = Convert.ToInt32(text_Recurrence_RangeOccurrences.Text, CultureInfo.InvariantCulture);
				}
				else if (radio_Recurrence_RangeEndBy.Checked)
				{
					DateTime until = date_Recurrence_RangeDay.Value.Date;
					if (startTimeZone != TimeZoneConvert.Floating)
						rule.Until = TimeZoneConvert.ConvertTimeToUtc(until + start.TimeOfDay, startTimeZone);
					else
						rule.Until = DateTime.SpecifyKind(until + start.TimeOfDay, DateTimeKind.Unspecified);
				}
			}
		}

		public bool CheckRecurrenceFields(bool showErrors)
		{
			int temp;
			if (radio_Recurrence_FrequencyDaily.Checked)
			{
				if (text_Recurrence_DailyIntervalDays.Text.Trim() == "")
				{
					if (showErrors) MailClient.Common.UI.MessageBox.Show(this, Resources.UI.Forms.PleaseEnterTheDaysInterval);
					return false;
				}
				else if (!Int32.TryParse(text_Recurrence_DailyIntervalDays.Text, out temp) || temp < 1)
				{
					if (showErrors) MailClient.Common.UI.MessageBox.Show(this, Resources.UI.Forms.InvalidNumberInDaysInterval);
					return false;
				}
			}
			else if (radio_Recurrence_FrequencyWeekly.Checked)
			{
				if (text_Recurrence_WeeklyInterval.Text.Trim() == "")
				{
					if (showErrors) MailClient.Common.UI.MessageBox.Show(this, Resources.UI.Forms.PleaseEnterTheWeeksInterval);
					return false;
				}
				else if (!Int32.TryParse(text_Recurrence_WeeklyInterval.Text, out temp) || temp < 1)
				{
					if (showErrors) MailClient.Common.UI.MessageBox.Show(this, Resources.UI.Forms.InvalidNumberInWeeksInterval);
					return false;
				}
			}
			else if (radio_Recurrence_FrequencyMonthly.Checked)
			{
				if (text_Recurrence_MonthlyIntervalMonths.Text.Trim() == "")
				{
					if (showErrors) MailClient.Common.UI.MessageBox.Show(this, Resources.UI.Forms.PleaseEnterTheMonthsInterval);
					return false;
				}
				else if (!Int32.TryParse(text_Recurrence_MonthlyIntervalMonths.Text, out temp) || temp < 1)
				{
					if (showErrors) MailClient.Common.UI.MessageBox.Show(this, Resources.UI.Forms.InvalidNumberInMonthInterval);
					return false;
				}

				if (radio_Recurrence_MonthlyOnMonthDay.Checked)
				{
					if (text_Recurrence_MonthDay.Text.Trim() == "")
					{
						if (showErrors) MailClient.Common.UI.MessageBox.Show(this, Resources.UI.Forms.PleaseEnterTheMonthDay);
						return false;
					}
					else if (!Int32.TryParse(text_Recurrence_MonthDay.Text, out temp) || temp < 1 || temp > 31)
					{
						if (showErrors) MailClient.Common.UI.MessageBox.Show(this, Resources.UI.Forms.InvalidDayOfMonthNumber);
						return false;
					}
				}
			}
			else if (radio_Recurrence_FrequencyYearly.Checked)
			{
				if (text_Recurrence_YearlyInterval.Text.Trim() == "")
				{
					if (showErrors) MailClient.Common.UI.MessageBox.Show(this, Resources.UI.Forms.PleaseEnterTheYearsInterval);
					return false;
				}
				else if (!Int32.TryParse(text_Recurrence_YearlyInterval.Text, out temp) || temp < 1)
				{
					if (showErrors) MailClient.Common.UI.MessageBox.Show(this, Resources.UI.Forms.InvalidNumberInYearInterval);
					return false;
				}
				if (radio_Recurrence_YearlyOnYearDay.Checked)
				{
					if (text_Recurrence_YearlyDayNumber.Text.Trim() == "")
					{
						if (showErrors) MailClient.Common.UI.MessageBox.Show(this, Resources.UI.Forms.PleaseEnterTheMonthDay2);
						return false;
					}
					else if (!Int32.TryParse(text_Recurrence_YearlyDayNumber.Text, out temp) || temp < 1 || temp > 31)
					{
						if (showErrors) MailClient.Common.UI.MessageBox.Show(this, Resources.UI.Forms.InvalidDayOfMonthNumber2);
						return false;
					}
				}
			}

			if (radio_Recurrence_RangeEndAfter.Checked)
			{
				if (text_Recurrence_RangeOccurrences.Text.Trim() == "")
				{
					if (showErrors) MailClient.Common.UI.MessageBox.Show(this, Resources.UI.Forms.PleaseEnterTheNumberOf);
					return false;
				}
				else if (!Int32.TryParse(text_Recurrence_RangeOccurrences.Text, out temp) || temp < 0)
				{
					if (showErrors) MailClient.Common.UI.MessageBox.Show(this, Resources.UI.Forms.InvalidNumberOfOccurrences);
					return false;
				}
			}
			return true;
		}*/

		private void RulePropertyUpdated()
		{
			SwitchRecurrenceControls();
			/*if (!this.loading)
			{
				this.changed = true;
				if (previewRule != null && CheckRecurrenceFields(false))
				{
					SaveRecurrenceRule(previewRule, TimeZoneConvert.Floating);
					UpdateRulePreview();
				}
				else
					label_Recurrence_Preview.Text = "";
			}*/
		}

		private void UpdateRulePreview()
		{
			/*if (previewRule != null && previewRule.Frequency != RecurrenceFrequency.NoRecurrence)
			{
				//Preview rule
				StringBuilder builder = new StringBuilder();
				// The occurrences are generated from today if RecurrenceStart is not set (because of tasks not having RecurrenceStart filled at the moment).
				previewRule.CalculateRuleBoundaries();

				int max = 10;
				int end = 3;

				List<DateTime> list = previewRule.GenerateOccurrences(
					new TimeInterval[] { new TimeInterval(DateTime.MinValue, DateTime.MaxValue) }, max + 1);

				if (list.Count > max && (previewRule.Count != 0 || previewRule.Until != default(DateTime)))
					max = max - end;
				else
					end = 0;

				// first part
				for (int i = 0; i < list.Count && i < max; i++)
					builder.AppendFormat("{0}, ", DateTimeUtils.ToShortDateString(list[i]));
				if (list.Count == 11)
					builder.Append("..");
				else if (builder.Length > 2)
					builder.Remove(builder.Length - 2, 2);

				// end
				if (end > 0)
				{
					TimeSpan span = new TimeSpan(1, 0, 0);
					do
					{
						span += span;
						DateTime start = previewItem.End - span;
						list = previewRule.GenerateOccurrences(
							new TimeInterval[] { new TimeInterval(start, DateTime.MaxValue) }, -1);
					}
					while (list.Count < end);

					for (int i = list.Count - end; i < list.Count; i++)
						builder.AppendFormat(", {0}", DateTimeUtils.ToShortDateString(list[i]));
				}

				label_Recurrence_Preview.Text = builder.ToString();
			}
			else
				label_Recurrence_Preview.Text = "";*/
		}

		private void SwitchRecurrenceControls()
		{
			radio_Recurrence_RangeNoEnd.Enabled = !radio_Recurrence_FrequencyNone.Checked && !this.readOnly;
			radio_Recurrence_RangeEndAfter.Enabled = !radio_Recurrence_FrequencyNone.Checked && !this.readOnly;
			text_Recurrence_RangeOccurrences.Enabled = !radio_Recurrence_FrequencyNone.Checked && !this.readOnly;
			radio_Recurrence_RangeEndBy.Enabled = !radio_Recurrence_FrequencyNone.Checked && !this.readOnly;
			date_Recurrence_RangeDay.Enabled = !radio_Recurrence_FrequencyNone.Checked && !this.readOnly;
		}

		/*
		private void ClearRecurrenceRule(IRecurrenceRule rule)
		{
			//rule.frequency = RecurrenceFrequency.Daily;
			rule.Until = DateTime.MinValue;
			rule.Count = 0;
			rule.Interval = 1;
			rule.BySecond = new int[] { };
			rule.ByMinute = new int[] { };
			rule.ByHour = new int[] { };
			rule.ByDay = new WeekDayNum[] { };
			rule.ByMonthDay = new int[] { };
			rule.ByYearDay = new int[] { };
			rule.ByWeekNo = new int[] { };
			rule.ByMonth = new int[] { };
			rule.BySetpos = new int[] { };
			rule.ExceptionRecurrenceIds = new DateTime[] { };
			rule.WeekStart = DayOfWeek.Monday;
		}

		private void UpdateDailyRecurrence(bool showIncompatibilityError)
		{
			text_Recurrence_DailyIntervalDays.Text = Convert.ToString(previewRule.Interval, CultureInfo.InvariantCulture);
		}

		private void UpdateWeeklyRecurrence(bool showIncompatibilityError)
		{
			text_Recurrence_WeeklyInterval.Text = Convert.ToString(previewRule.Interval, CultureInfo.InvariantCulture);

			check_Recurrence_WeeklySunday.Checked = false;
			check_Recurrence_WeeklyMonday.Checked = false;
			check_Recurrence_WeeklyTuesday.Checked = false;
			check_Recurrence_WeeklyWednesday.Checked = false;
			check_Recurrence_WeeklyThursday.Checked = false;
			check_Recurrence_WeeklyFriday.Checked = false;
			check_Recurrence_WeeklySaturday.Checked = false;

			if (previewRule.Frequency == RecurrenceFrequency.Weekly && previewRule.ByDay.Length != 0 &&
				previewRule.ByHour.Length == 0 && previewRule.ByMinute.Length == 0 && previewRule.BySetpos.Length == 0)
			{
				foreach (WeekDayNum weekDay in previewRule.ByDay)
				{
					switch (weekDay.Day)
					{
						case DayOfWeek.Sunday: check_Recurrence_WeeklySunday.Checked = true; break;
						case DayOfWeek.Monday: check_Recurrence_WeeklyMonday.Checked = true; break;
						case DayOfWeek.Tuesday: check_Recurrence_WeeklyTuesday.Checked = true; break;
						case DayOfWeek.Wednesday: check_Recurrence_WeeklyWednesday.Checked = true; break;
						case DayOfWeek.Thursday: check_Recurrence_WeeklyThursday.Checked = true; break;
						case DayOfWeek.Friday: check_Recurrence_WeeklyFriday.Checked = true; break;
						case DayOfWeek.Saturday: check_Recurrence_WeeklySaturday.Checked = true; break;
					}
				}
			}
			//Check this is a plain recurrence a fill data from RecurrenceStart
			else if (previewRule.ByHour.Length == 0 && previewRule.ByMinute.Length == 0 && previewRule.ByMonthDay.Length == 0 &&
				previewRule.ByDay.Length == 0 && previewRule.ByMonth.Length == 0 && previewRule.BySecond.Length == 0 &&
				previewRule.BySetpos.Length == 0 && previewRule.ByWeekNo.Length == 0 && previewRule.ByYearDay.Length == 0)
			{
				switch (Start.DayOfWeek)
				{
					case DayOfWeek.Sunday: check_Recurrence_WeeklySunday.Checked = true; break;
					case DayOfWeek.Monday: check_Recurrence_WeeklyMonday.Checked = true; break;
					case DayOfWeek.Tuesday: check_Recurrence_WeeklyTuesday.Checked = true; break;
					case DayOfWeek.Wednesday: check_Recurrence_WeeklyWednesday.Checked = true; break;
					case DayOfWeek.Thursday: check_Recurrence_WeeklyThursday.Checked = true; break;
					case DayOfWeek.Friday: check_Recurrence_WeeklyFriday.Checked = true; break;
					case DayOfWeek.Saturday: check_Recurrence_WeeklySaturday.Checked = true; break;
				}
			}
			else if (showIncompatibilityError)
				MailClient.Common.UI.MessageBox.Show(Resources.UI.Forms.RecurrencePatternInThe);

		}

		private void UpdateMonthlyRecurrence(bool showIncompatibilityError)
		{
			text_Recurrence_MonthlyIntervalMonths.Text = Convert.ToString(previewRule.Interval, CultureInfo.InvariantCulture);

			//Either ByMonthDay or ByDay will be present
			if (previewRule.Frequency == RecurrenceFrequency.Monthly && previewRule.ByMonthDay.Length == 1)
			{
				radio_Recurrence_MonthlyOnMonthDay.Checked = true;
				text_Recurrence_MonthDay.Text = Convert.ToString(
					previewRule.ByMonthDay[0], CultureInfo.InvariantCulture);
			}
			else if (previewRule.Frequency == RecurrenceFrequency.Monthly && previewRule.ByDay.Length == 1)
			{
				radio_Recurrence_MonthOnPosition.Checked = true;
				combo_Recurrence_MonthPosition.SelectedIndex = previewRule.ByDay[0].OrderWeek == -1 ? 4 : previewRule.ByDay[0].OrderWeek - 1;
				combo_Recurrence_MonthPositionDay.SelectedIndex = (int)previewRule.ByDay[0].Day;
			}
			//Check this is a plain recurrence a fill data from RecurrenceStart
			else if (previewRule.ByHour.Length == 0 && previewRule.ByMinute.Length == 0 && previewRule.ByMonthDay.Length == 0 &&
				previewRule.ByDay.Length == 0 && previewRule.ByMonth.Length == 0 && previewRule.BySecond.Length == 0 &&
				previewRule.BySetpos.Length == 0 && previewRule.ByWeekNo.Length == 0 && previewRule.ByYearDay.Length == 0)
			{
				radio_Recurrence_MonthlyOnMonthDay.Checked = true;
				text_Recurrence_MonthDay.Text = Convert.ToString(Start.Day, CultureInfo.InvariantCulture);
			}
			else if (showIncompatibilityError)
				MailClient.Common.UI.MessageBox.Show(Resources.UI.Forms.RecurrencePatternInThe);
		}

		private void UpdateYearlyRecurrence(bool showIncompatibilityError)
		{
			text_Recurrence_YearlyInterval.Text = Convert.ToString(previewRule.Interval, CultureInfo.InvariantCulture);
			//Either ByMonthDay, ByMonth for radio_Recurrence_YearlyOnYearDay or 
			// ByMonth, ByDay for radio_Recurrence_YearlyOnMonthDayPosition will be present

			if (previewRule.Frequency == RecurrenceFrequency.Yearly && previewRule.ByDay.Length == 1 && previewRule.ByMonth.Length == 1)
			{
				radio_Recurrence_YearlyOnMonthDayPosition.Checked = true;
				combo_Recurrence_YearlyPosition.SelectedIndex = previewRule.ByDay[0].OrderWeek == -1 ? 4 : previewRule.ByDay[0].OrderWeek - 1;
				combo_Recurrence_YearlyPositionDay.SelectedIndex = (int)previewRule.ByDay[0].Day;
				combo_Recurrence_YearlyPositionMonth.SelectedIndex = previewRule.ByMonth[0] - 1;
			}
			else if (previewRule.Frequency == RecurrenceFrequency.Yearly && previewRule.ByMonthDay.Length <= 1 && previewRule.ByMonth.Length == 1)
			{
				radio_Recurrence_YearlyOnYearDay.Checked = true;
				if (previewRule.ByMonthDay.Length == 0)
					text_Recurrence_YearlyDayNumber.Text = Convert.ToString(previewRule.RecurrenceStart.Day, CultureInfo.InvariantCulture);
				else
					text_Recurrence_YearlyDayNumber.Text = Convert.ToString(previewRule.ByMonthDay[0], CultureInfo.InvariantCulture);
				combo_Recurrence_YearlyDayMonth.SelectedIndex = previewRule.ByMonth[0] - 1;
			}
			//Check this is a plain recurrence a fill data from RecurrenceStart
			else if (previewRule.ByHour.Length == 0 && previewRule.ByMinute.Length == 0 && previewRule.ByMonth.Length == 0 &&
				previewRule.BySecond.Length == 0 && previewRule.BySetpos.Length == 0 && previewRule.ByWeekNo.Length == 0 &&
				previewRule.ByYearDay.Length == 0)
			{
				radio_Recurrence_YearlyOnYearDay.Checked = true;
				text_Recurrence_YearlyDayNumber.Text = Convert.ToString(Start.Day, CultureInfo.InvariantCulture);
				combo_Recurrence_YearlyDayMonth.SelectedIndex = Start.Month - 1;
				//For now don't bother filling out YearlyPosition field except month
				combo_Recurrence_YearlyPositionMonth.SelectedIndex = Start.Month - 1;
			}
			else if (showIncompatibilityError)
				MailClient.Common.UI.MessageBox.Show(Resources.UI.Forms.RecurrencePatternInThe);
		}*/

		#endregion
	}
}
