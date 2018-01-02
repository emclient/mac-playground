using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormsTest
{
    public partial class LayoutForm : Form
    {
        public LayoutForm()
        {
            InitializeComponent();

            Load += LayoutForm_Load;
        }

        void LayoutForm_Load(object sender, EventArgs e)
        {
            setComboBoxItems();
        }

        private void setComboBoxItems()
        {
            int rememberSelectedIndex = combo_General_AfterStart.SelectedIndex;
            combo_General_AfterStart.Items.Clear();
            combo_General_AfterStart.Items.Add("Inbox");
            combo_General_AfterStart.Items.Add("GlobalInbox");
            combo_General_AfterStart.Items.Add("GlobalUnread");
            combo_General_AfterStart.Items.Add("Calendar");
            combo_General_AfterStart.Items.Add("Tasks");
            combo_General_AfterStart.Items.Add("Contacts");
            combo_General_AfterStart.Items.Add("Widgets");
            combo_General_AfterStart.Items.Add("SpecificFolder");
            combo_General_AfterStart.Items.Add("LastSelectedFolder");
            if (rememberSelectedIndex < combo_General_AfterStart.Items.Count)
            {
                combo_General_AfterStart.SelectedIndex = rememberSelectedIndex;
            }

            //rememberSelectedIndex = combo_Compose_MailFormat.SelectedIndex;
            //combo_Compose_MailFormat.Items.Clear();
            //combo_Compose_MailFormat.Items.Add(Resources.UI.Forms.MailFormatHtml);
            //combo_Compose_MailFormat.Items.Add(Resources.UI.Forms.MailFormatPlain);
            //if (rememberSelectedIndex < combo_Compose_MailFormat.Items.Count)
            //{
            //    combo_Compose_MailFormat.SelectedIndex = rememberSelectedIndex;
            //}

            //rememberSelectedIndex = combo_Compose_MailFormatForReply.SelectedIndex;
            //combo_Compose_MailFormatForReply.Items.Clear();
            //combo_Compose_MailFormatForReply.Items.Add(Resources.UI.Forms.MailFormatAutodetect);
            //combo_Compose_MailFormatForReply.Items.Add(Resources.UI.Forms.MailFormatHtml);
            //combo_Compose_MailFormatForReply.Items.Add(Resources.UI.Forms.MailFormatPlain);
            //if (rememberSelectedIndex < combo_Compose_MailFormatForReply.Items.Count)
            //{
            //    combo_Compose_MailFormatForReply.SelectedIndex = rememberSelectedIndex;
            //}

            //rememberSelectedIndex = combo_RepliesForwards_RepliesChangeStyle.SelectedIndex;
            //combo_RepliesForwards_RepliesChangeStyle.Items.Clear();
            //combo_RepliesForwards_RepliesChangeStyle.Items.Add(Resources.UI.Forms.Re);
            //combo_RepliesForwards_RepliesChangeStyle.Items.Add(Resources.UI.Forms.ReReRe);
            //combo_RepliesForwards_RepliesChangeStyle.Items.Add(Resources.UI.Forms.Re3);
            //if (rememberSelectedIndex < combo_RepliesForwards_RepliesChangeStyle.Items.Count)
            //{
            //    combo_RepliesForwards_RepliesChangeStyle.SelectedIndex = rememberSelectedIndex;
            //}

            //rememberSelectedIndex = combo_RepliesForwards_ForwardsChangeStyle.SelectedIndex;
            //combo_RepliesForwards_ForwardsChangeStyle.Items.Clear();
            //combo_RepliesForwards_ForwardsChangeStyle.Items.Add(Resources.UI.Forms.Fw);
            //combo_RepliesForwards_ForwardsChangeStyle.Items.Add(Resources.UI.Forms.FwFwFw);
            //combo_RepliesForwards_ForwardsChangeStyle.Items.Add(Resources.UI.Forms.Fw3);
            //if (rememberSelectedIndex < combo_RepliesForwards_ForwardsChangeStyle.Items.Count)
            //{
            //    combo_RepliesForwards_ForwardsChangeStyle.SelectedIndex = rememberSelectedIndex;
            //}

            //rememberSelectedIndex = combo_Calendar_Granularity.SelectedIndex;
            //combo_Calendar_Granularity.Items.Clear();
            //foreach (int granularity in calendarGranularityValues)
            //    combo_Calendar_Granularity.Items.Add(string.Format(Resources.UI.Forms.GranularityMinutes, granularity));
            //if (rememberSelectedIndex < combo_Calendar_Granularity.Items.Count)
            //{
            //    combo_Calendar_Granularity.SelectedIndex = rememberSelectedIndex;
            //}

            //rememberSelectedIndex = combo_Calendar_ShowRange.SelectedIndex;
            //combo_Calendar_ShowRange.Items.Clear();
            //combo_Calendar_ShowRange.Items.Add(Resources.UI.Forms.WorkDayHours);
            //combo_Calendar_ShowRange.Items.Add(Resources.UI.Forms.Range1Hour);
            //combo_Calendar_ShowRange.Items.Add(string.Format(Resources.UI.Forms.RangeHours, 2));
            //combo_Calendar_ShowRange.Items.Add(string.Format(Resources.UI.Forms.RangeHours, 4));
            //combo_Calendar_ShowRange.Items.Add(string.Format(Resources.UI.Forms.RangeHours, 8));
            //combo_Calendar_ShowRange.Items.Add(string.Format(Resources.UI.Forms.RangeHours, 12));
            //combo_Calendar_ShowRange.Items.Add(string.Format(Resources.UI.Forms.RangeHours, 24));
            //if (rememberSelectedIndex < combo_Calendar_ShowRange.Items.Count)
            //{
            //    combo_Calendar_ShowRange.SelectedIndex = rememberSelectedIndex;
            //}

            //rememberSelectedIndex = combo_Calendar_SourceOfColor.SelectedIndex;
            //combo_Calendar_SourceOfColor.Items.Clear();
            //combo_Calendar_SourceOfColor.Items.Add(Resources.UI.Forms.CategoryThenCalendar);
            //combo_Calendar_SourceOfColor.Items.Add(Resources.UI.Forms.CalendarThenCategory);
            //combo_Calendar_SourceOfColor.Items.Add(Resources.UI.Forms.CalendarOnly);
            //if (rememberSelectedIndex < combo_Calendar_SourceOfColor.Items.Count)
            //{
            //    combo_Calendar_SourceOfColor.SelectedIndex = rememberSelectedIndex;
            //}


            //rememberSelectedIndex = combo_Calendar_FirstWeekOfYear.SelectedIndex;
            //combo_Calendar_FirstWeekOfYear.Items.Clear();
            //combo_Calendar_FirstWeekOfYear.Items.Add(Resources.UI.Forms.DetectFromSystem);
            //combo_Calendar_FirstWeekOfYear.Items.Add(Resources.UI.Forms.StartsOnJan1);
            //combo_Calendar_FirstWeekOfYear.Items.Add(Resources.UI.Forms.First4DayWeek);
            //combo_Calendar_FirstWeekOfYear.Items.Add(Resources.UI.Forms.FirstFullWeek);
            //if (rememberSelectedIndex < combo_Calendar_FirstWeekOfYear.Items.Count)
            //{
            //    combo_Calendar_FirstWeekOfYear.SelectedIndex = rememberSelectedIndex;
            //}

            //#region backup
            //rememberSelectedIndex = combo_Backup_Frequency.SelectedIndex;
            //combo_Backup_Frequency.Items.Clear();
            //combo_Backup_Frequency.Items.Add(Resources.UI.Forms.BackupFrequency1Day);
            //combo_Backup_Frequency.Items.Add(Resources.UI.Forms.BackupFrequency3Days);
            //combo_Backup_Frequency.Items.Add(Resources.UI.Forms.BackupFrequency1Week);
            //combo_Backup_Frequency.Items.Add(Resources.UI.Forms.BackupFrequency2Weeks);
            //combo_Backup_Frequency.Items.Add(Resources.UI.Forms.BackupFrequency1Month);
            //combo_Backup_Frequency.Items.Add(Resources.UI.Forms.BackupFrequency2Months);
            //combo_Backup_Frequency.Items.Add(Resources.UI.Forms.BackupFrequency3Months);
            //combo_Backup_Frequency.Items.Add(Resources.UI.Forms.BackupFrequency6Months);

            //if (rememberSelectedIndex < combo_Backup_Frequency.Items.Count)
            //    combo_Backup_Frequency.SelectedIndex = rememberSelectedIndex;


            //rememberSelectedIndex = combo_Backup_Preserve.SelectedIndex;
            //combo_Backup_Preserve.Items.Clear();
            //combo_Backup_Preserve.Items.Add(Resources.UI.Forms.OneBackup);
            //for (int i = 2; i < 6; i++)
            //{
            //    combo_Backup_Preserve.Items.Add(string.Format(Resources.UI.Forms.TwoOrMoreBackups, i));
            //}

            //if (rememberSelectedIndex < combo_Backup_Preserve.Items.Count)
            //    combo_Backup_Preserve.SelectedIndex = rememberSelectedIndex;
            //#endregion

        }
    }
}
