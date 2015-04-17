namespace Baldini.Controls.Calendar.Demo
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.calendar1 = new Baldini.Controls.Calendar.Calendar();
            this.monthView1 = new Baldini.Controls.Calendar.MonthView();
            this.SuspendLayout();
            // 
            // calendar1
            // 
            this.calendar1.AllowItemEditExternal = false;
            this.calendar1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.calendar1.Availability = null;
            this.calendar1.Font = new System.Drawing.Font("Segoe UI", 11.25F);
            this.calendar1.HighlightRanges = new Baldini.Controls.Calendar.CalendarHighlightRange[0];
            this.calendar1.Location = new System.Drawing.Point(232, 12);
            this.calendar1.MaximumDayTopHeight = 0;
            this.calendar1.MonthView = this.monthView1;
            this.calendar1.Name = "calendar1";
            this.calendar1.Size = new System.Drawing.Size(1106, 817);
            this.calendar1.TabIndex = 1;
            this.calendar1.Text = "calendar1";
            // 
            // monthView1
            // 
            this.monthView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.monthView1.ArrowsColor = System.Drawing.SystemColors.Window;
            this.monthView1.ArrowsSelectedColor = System.Drawing.Color.Gold;
            this.monthView1.DayBackgroundColor = System.Drawing.Color.Empty;
            this.monthView1.DayGrayedText = System.Drawing.SystemColors.GrayText;
            this.monthView1.DaySelectedBackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(220)))), ((int)(((byte)(135)))));
            this.monthView1.DaySelectedColor = System.Drawing.SystemColors.WindowText;
            this.monthView1.DaySelectedTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(71)))), ((int)(((byte)(71)))));
            this.monthView1.ItemPadding = new System.Windows.Forms.Padding(2);
            this.monthView1.Location = new System.Drawing.Point(12, 12);
            this.monthView1.MonthTitleColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(202)))), ((int)(((byte)(249)))));
            this.monthView1.MonthTitleColorInactive = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(202)))), ((int)(((byte)(249)))));
            this.monthView1.MonthTitleTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(71)))), ((int)(((byte)(71)))));
            this.monthView1.MonthTitleTextColorInactive = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(71)))), ((int)(((byte)(71)))));
            this.monthView1.Name = "monthView1";
            this.monthView1.Size = new System.Drawing.Size(214, 817);
            this.monthView1.TabIndex = 0;
            this.monthView1.Text = "monthView1";
            this.monthView1.TodayBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(198)))), ((int)(((byte)(161)))));
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1350, 841);
            this.Controls.Add(this.calendar1);
            this.Controls.Add(this.monthView1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private MonthView monthView1;
        private Calendar calendar1;
    }
}

