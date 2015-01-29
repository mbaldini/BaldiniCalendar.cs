using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Baldini.Controls.Calendar
{
    /// <summary>
    /// Hosts a month-level calendar where user can select day-based dates
    /// </summary>
    [DefaultEvent("SelectionChanged")]
    public class MonthView
        : ContainerControl
    {
        #region Subclasses

        /// <summary>
        /// Represents the different kinds of selection in MonthView
        /// </summary>
        public enum MonthViewSelection
        {
            /// <summary>
            /// User can select whatever date available to mouse reach
            /// </summary>
            Manual,

            /// <summary>
            /// Selection is limited to just one day
            /// </summary>
            Day,

            /// <summary>
            /// Selecion is limited toWorkWeekStart andWorkWeekEnd weekly ranges
            /// </summary>
            WorkWeek,

            /// <summary>
            /// Selection is limited to a full week
            /// </summary>
            Week,

            /// <summary>
            /// Selection is limited to a full month
            /// </summary>
            Month
        }

        #endregion

        #region Variables
        private int _forwardMonthIndex;
        private MonthViewDay _lastHitted;
        private bool _mouseDown;
        private Size _daySize;
        private DateTime _selectionStart;
        private DateTime _selectionEnd;
        private string _monthTitleFormat;
        private DayOfWeek _weekStart;
        private DayOfWeek _workWeekStart;
        private DayOfWeek _workWeekEnd;
        private MonthViewSelection _selectionMode;
        private string _dayNamesFormat;
        private bool _dayNamesVisible;
        private int _dayNamesLength;
        private DateTime _viewStart;
        private Size _monthSize;
        private MonthViewMonth[] _months;
        private Padding _itemPadding;
        private Color _monthTitleColor;
        private Color _monthTitleColorInactive;
        private Color _monthTitleTextColor;
        private Color _monthTitleTextColorInactive;
        private Color _dayBackgroundColor;
        private Color _daySelectedBackgroundColor;
        private Color _dayTextColor;
        private Color _daySelectedTextColor;
        private Color _arrowsColor;
        private Color _arrowsSelectedColor;
        private Color _dayGrayedText;
        private Color _todayBorderColor;
        private int _maxSelectionCount;
        private Rectangle _forwardButtonBounds;
        private bool _forwardButtonSelected;
        private Rectangle _backwardButtonBounds;
        private bool _backwardButtonSelected;

        private Rectangle _fastForwardButtonBounds;
        private bool _fastForwardButtonSelected;
        private Rectangle _fastBackwardButtonBounds;
        private bool _fastBackwardButtonSelected;
        #endregion

        #region Events

        /// <summary>
        /// Occurs when selection has changed.
        /// </summary>
        public event EventHandler SelectionChanged;

        #endregion

        #region Ctors

        public MonthView()
        {
            SetStyle(ControlStyles.Opaque, true);
            DoubleBuffered = true;

            _dayNamesFormat = "ddd";
            _monthTitleFormat = "MMMM yyyy";
            _selectionMode = MonthViewSelection.Manual;
            _workWeekStart = DayOfWeek.Monday;
            _workWeekEnd = DayOfWeek.Friday;
            _weekStart = DayOfWeek.Sunday;
            _dayNamesVisible = true;
            _dayNamesLength = 2;
            _viewStart = DateTime.Now;
            _itemPadding = new Padding(2);
            _monthTitleColor = Color.FromArgb(184, 202, 249); //SystemColors.ActiveCaption;
            _monthTitleColorInactive = Color.FromArgb(184, 202, 249); //SystemColors.InactiveCaption;
            _monthTitleTextColor = Color.FromArgb(71, 71, 71); //SystemColors.ActiveCaptionText;
            _monthTitleTextColorInactive = Color.FromArgb(71, 71, 71); //SystemColors.InactiveCaptionText;
            _dayBackgroundColor = Color.Empty;
            _daySelectedBackgroundColor = Color.FromArgb(255, 220, 135); //SystemColors.Highlight;
            _dayTextColor = SystemColors.WindowText;
            _daySelectedTextColor = Color.FromArgb(71, 71, 71); //SystemColors.HighlightText;
            _arrowsColor = SystemColors.Window;
            _arrowsSelectedColor = Color.Gold;
            _dayGrayedText = SystemColors.GrayText;
            _todayBorderColor = Color.FromArgb(253, 198, 161); //Color.Maroon;

            UpdateMonthSize();
            UpdateMonths();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the size of days rectangles
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Size DaySize
        {
            get { return _daySize; }
        }

        /// <summary>
        /// Gets or sets the format of day names
        /// </summary>
        [DefaultValue("ddd")]
        public string DayNamesFormat
        {
            get { return _dayNamesFormat; }
            set { _dayNamesFormat = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if day names should be visible
        /// </summary>
        [DefaultValue(true)]
        public bool DayNamesVisible
        {
            get { return _dayNamesVisible; }
            set { _dayNamesVisible = value; }
        }

        /// <summary>
        /// Gets or sets how many characters of day names should be displayed
        /// </summary>
        [DefaultValue(2)]
        public int DayNamesLength
        {
            get { return _dayNamesLength; }
            set { _dayNamesLength = value; UpdateMonths(); }
        }

        /// <summary>
        /// Gets or sets what the first day of weeks should be
        /// </summary>
        [DefaultValue(DayOfWeek.Sunday)]
        public DayOfWeek FirstDayOfWeek
        {
            get { return _weekStart; }
            set { _weekStart = value; }
        }

        /// <summary>
        /// Gets a value indicating if the backward button is selected
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool BackwardButtonSelected
        {
            get { return _backwardButtonSelected; }
        }

        /// <summary>
        /// Gets the bounds of the backward button
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle BackwardButtonBounds
        {
            get { return _backwardButtonBounds; }
        }

        /// <summary>
        /// Gets a value indicating if the forward button is selected
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ForwardButtonSelected
        {
            get { return _forwardButtonSelected; }
        }

        /// <summary>
        /// Gets the bounds of the forward button
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle ForwardButtonBounds
        {
            get { return _forwardButtonBounds; }
        }


        /// <summary>
        /// Gets a value indicating if the fast backward button is selected
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool FastBackwardButtonSelected
        {
            get { return _fastBackwardButtonSelected; }
        }

        /// <summary>
        /// Gets the bounds of the fast backward button
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle FastBackwardButtonBounds
        {
            get { return _fastBackwardButtonBounds; }
        }

        /// <summary>
        /// Gets a value indicating if the fast forward button is selected
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool FastForwardButtonSelected
        {
            get { return _fastForwardButtonSelected; }
        }

        /// <summary>
        /// Gets the bounds of the fast forward button
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle FastForwardButtonBounds
        {
            get { return _fastForwardButtonBounds; }
        }

        /// <summary>
        /// Gets or sets the Font of the Control
        /// </summary>
        public override Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;

                UpdateMonthSize();
                UpdateMonths();
            }
        }

        /// <summary>
        /// Gets or sets the internal padding of items (Days, day names, month names)
        /// </summary>
        public Padding ItemPadding
        {
            get { return _itemPadding; }
            set { _itemPadding = value; }
        }

        /// <summary>
        /// Gets or sets the maximum selection count of days
        /// </summary>
        [DefaultValue(0)]
        public int MaxSelectionCount
        {
            get { return _maxSelectionCount; }
            set { _maxSelectionCount = value; }
        }

        /// <summary>
        /// Gets the Months currently displayed on the calendar
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MonthViewMonth[] Months
        {
            get { return _months; }
        }

        /// <summary>
        /// Gets the size of an entire month inside theMonthView
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Size MonthSize
        {
            get { return _monthSize; }
        }

        /// <summary>
        /// Gets or sets the format of month titles
        /// </summary>
        [DefaultValue("MMMM yyyy")]
        public string MonthTitleFormat
        {
            get { return _monthTitleFormat; }
            set { _monthTitleFormat = value; UpdateMonths(); }
        }

        /// <summary>
        /// Gets or sets the start of selection
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
        public DateTime SelectionStart
        {
            get { return _selectionStart; }
            set
            {
                if (MaxSelectionCount > 0)
                {
                    if (Math.Abs(value.Subtract(SelectionEnd).TotalDays) >= MaxSelectionCount)
                    {
                        _selectionEnd = value.AddDays(-1 * MaxSelectionCount).Add(new TimeSpan(23, 59, 59));
                    }
                }

                _selectionStart = value.Date;
                Invalidate();
                OnSelectionChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the end of selection
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime SelectionEnd
        {
            get { return _selectionEnd; }
            set
            {
                if (MaxSelectionCount > 0)
                {
                    if (Math.Abs(value.Subtract(SelectionStart).TotalDays) >= MaxSelectionCount)
                    {
                        value = _selectionStart.AddDays(MaxSelectionCount);
                    }
                }

                _selectionEnd = value.Date.Add(new TimeSpan(23,59,59));
                Invalidate();
                OnSelectionChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the selection mode ofMonthView
        /// </summary>
        [DefaultValue(MonthViewSelection.Manual)]
        public MonthViewSelection SelectionMode
        {
            get { return _selectionMode; }
            set { _selectionMode = value; }
        }

        /// <summary>
        /// Gets or sets the date of the first displayed month
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
        public DateTime ViewStart
        {
            get { return _viewStart; }
            set { _viewStart = value; UpdateMonths(); Invalidate(); }
        }

        /// <summary>
        /// Gets the last day of the last month showed on the view.
        /// </summary>
        public DateTime ViewEnd
        {
            get
            {
                DateTime month = Months[Months.Length - 1].Date;
                return month.Date.AddDays(DateTime.DaysInMonth(month.Year, month.Month));
            }
        }

        /// <summary>
        /// Gets or sets the day that starts a work-week
        /// </summary>
        [DefaultValue(DayOfWeek.Monday)]
        public DayOfWeek WorkWeekStart
        {
            get { return _workWeekStart; }
            set { _workWeekStart = value; }
        }

        /// <summary>
        /// Gets or sets the day that ends a work-week
        /// </summary>
        [DefaultValue(DayOfWeek.Friday)]
        public DayOfWeek WorkWeekEnd
        {
            get { return _workWeekEnd; }
            set { _workWeekEnd = value; }
        }

        #endregion

        #region Color Properties

        [Category("Appearance")]
        public Color ArrowsSelectedColor
        {
            get { return _arrowsSelectedColor; }
            set { _arrowsSelectedColor = value; }
        }
        [Category("Appearance")]
        public Color ArrowsColor
        {
            get { return _arrowsColor; }
            set { _arrowsColor = value; }
        }

        [Category("Appearance")]
        public Color DaySelectedTextColor
        {
            get { return _daySelectedTextColor; }
            set { _daySelectedTextColor = value; }
        }
        [Category("Appearance")]
        public Color DaySelectedColor
        {
            get { return _dayTextColor; }
            set { _dayTextColor = value; }
        }
        [Category("Appearance")]
        public Color DaySelectedBackgroundColor
        {
            get { return _daySelectedBackgroundColor; }
            set { _daySelectedBackgroundColor = value; }
        }
        [Category("Appearance")]
        public Color DayBackgroundColor
        {
            get { return _dayBackgroundColor; }
            set { _dayBackgroundColor = value; }
        }
        [Category("Appearance")]
        public Color DayGrayedText
        {
            get { return _dayGrayedText; }
            set { _dayGrayedText = value; }
        }

        [Category("Appearance")]
        public Color MonthTitleColor
        {
            get { return _monthTitleColor; }
            set { _monthTitleColor = value; }
        }
        [Category("Appearance")]
        public Color MonthTitleTextColorInactive
        {
            get { return _monthTitleTextColorInactive; }
            set { _monthTitleTextColorInactive = value; }
        }
        [Category("Appearance")]
        public Color MonthTitleTextColor
        {
            get { return _monthTitleTextColor; }
            set { _monthTitleTextColor = value; }
        }
        [Category("Appearance")]
        public Color MonthTitleColorInactive
        {
            get { return _monthTitleColorInactive; }
            set { _monthTitleColorInactive = value; }
        }

        /// <summary>
        /// Gets or sets the color of the today day border color
        /// </summary>
        [Category("Appearance")]
        public Color TodayBorderColor
        {
            get { return _todayBorderColor; }
            set { _todayBorderColor = value; }
        }


        #endregion

        #region Public Methods

        /// <summary>
        /// Checks if a day is hitted on the specified point
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public MonthViewDay HitTest(Point p)
        {
            for (int i = 0; i < Months.Length; i++)
            {
                if (Months[i].Bounds.Contains(p))
                {
                    for (int j = 0; j < Months[i].Days.Length; j++)
                    {
                        if (/*Months[i].Days[j].Visible && */Months[i].Days[j].Bounds.Contains(p))
                        {
                            return Months[i].Days[j];
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Moves the view one month forward
        /// </summary>
        public void GoForward()
        {
            ViewStart = ViewStart.AddMonths(1);
        }

        /// <summary>
        /// Moves the view one month backward
        /// </summary>
        public void GoBackward()
        {
            ViewStart = ViewStart.AddMonths(-1);
        }

        /// <summary>
        /// Sets the range of days that is selected on the calendar
        /// </summary>
        /// <param name="dtStart">Start Date of the selection range</param>
        /// <param name="dtEnd">End Date of the selection range</param>
        public void SetViewRange(DateTime dtStart, DateTime dtEnd)
        {
            if (this.SelectionEnd != dtEnd.Date.Add(new TimeSpan(23, 59, 59)) || this._selectionStart.Date != dtStart.Date)
            {
                this._selectionEnd = dtEnd;
                this.SelectionStart = dtStart;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets the value of the property
        /// </summary>
        /// <param name="bounds">Bounds of the button</param>
        private void SetForwardButtonBounds(Rectangle bounds)
        {
            _forwardButtonBounds = bounds;
        }

        /// <summary>
        /// Sets the value of the property
        /// </summary>
        /// <param name="bounds">Bounds of the button</param>
        private void SetBackwardButtonBounds(Rectangle bounds)
        {
            _backwardButtonBounds = bounds;
        }

        /// <summary>
        /// Sets the value of the property
        /// </summary>
        /// <param name="bounds">Value indicating if button is selected</param>
        private void SetForwardButtonSelected(bool selected)
        {
            _forwardButtonSelected = selected;
            Invalidate(ForwardButtonBounds);
        }

        /// <summary>
        /// Sets the value of the property
        /// </summary>
        /// <param name="bounds">Value indicating if button is selected</param>
        private void SetBackwardButtonSelected(bool selected)
        {
            _backwardButtonSelected = selected;
            Invalidate(BackwardButtonBounds);
        }

        /// <summary>
        /// Sets the value of the property
        /// </summary>
        /// <param name="bounds">Bounds of the button</param>
        private void SetFastForwardButtonBounds(Rectangle bounds)
        {
            _fastForwardButtonBounds = bounds;
        }

        /// <summary>
        /// Sets the value of the property
        /// </summary>
        /// <param name="bounds">Bounds of the button</param>
        private void SetFastBackwardButtonBounds(Rectangle bounds)
        {
            _fastBackwardButtonBounds = bounds;
        }

        /// <summary>
        /// Sets the value of the property
        /// </summary>
        /// <param name="bounds">Value indicating if button is selected</param>
        private void SetFastForwardButtonSelected(bool selected)
        {
            _fastForwardButtonSelected = selected;
            Invalidate(FastForwardButtonBounds);
        }

        /// <summary>
        /// Sets the value of the property
        /// </summary>
        /// <param name="bounds">Value indicating if button is selected</param>
        private void SetFastBackwardButtonSelected(bool selected)
        {
            _fastBackwardButtonSelected = selected;
            Invalidate(FastBackwardButtonBounds);
        }

        /// <summary>
        /// Selects the week where the hit is contained
        /// </summary>
        /// <param name="hit"></param>
        private void SelectWeek(DateTime hit)
        {
            int preDays = (new int[] { 0, 1, 2, 3, 4, 5, 6 })[(int)hit.DayOfWeek] - (int)FirstDayOfWeek;

            _selectionStart = hit.AddDays(-preDays).Date;
            SelectionEnd = SelectionStart.AddDays(6);
        }

        /// <summary>
        /// Selecs the work-week where the hit is contanied
        /// </summary>
        /// <param name="hit"></param>
        private void SelectWorkWeek(DateTime hit)
        {
            int preDays = (new int[] { 0, 1, 2, 3, 4, 5, 6 })[(int)hit.DayOfWeek] - (int)WorkWeekStart;

            _selectionStart = hit.AddDays(-preDays).Date;
            SelectionEnd = SelectionStart.AddDays(Math.Abs(WorkWeekStart - WorkWeekEnd));
        }

        /// <summary>
        /// Selecs the month where the hit is contanied
        /// </summary>
        /// <param name="hit"></param>
        private void SelectMonth(DateTime hit)
        {
            _selectionStart = new DateTime(hit.Year, hit.Month, 1).Date;
            SelectionEnd = new DateTime(hit.Year, hit.Month, DateTime.DaysInMonth(hit.Year, hit.Month));
        }

        /// <summary>
        /// Draws a box of text
        /// </summary>
        /// <param name="e"></param>
        private void DrawBox(MonthViewBoxEventArgs e)
        {
            if (e.Bounds.Height <= 0 && e.Bounds.Width <= 0)
                return;

            if (!e.BackgroundColor.IsEmpty)
            {
                if (e.BackgroundColor == this._daySelectedBackgroundColor)
                {
                    int r, g, b;
                    r = (int)(e.BackgroundColor.R * 1.1);
                    g = (int)(e.BackgroundColor.G * 1.1);
                    b = (int)(e.BackgroundColor.B * 1.1);
                    Color c2 = Color.FromArgb(r > 255 ? 255 : r, g > 255 ? 255 : g, b > 255 ? 255 : b);
                    Rectangle rTop = new Rectangle(new Point(e.Bounds.X, e.Bounds.Y + 1), new Size(e.Bounds.Width, (e.Bounds.Height / 2) + 1));
                    Rectangle rBot = new Rectangle(new Point(e.Bounds.X, e.Bounds.Y + (e.Bounds.Height / 2)), rTop.Size);

                    using (System.Drawing.Drawing2D.LinearGradientBrush gb = new System.Drawing.Drawing2D.LinearGradientBrush(rTop, c2, e.BackgroundColor, 90f))
                    {
                        e.Graphics.FillRectangle(gb, rTop);
                    }
                    using (System.Drawing.Drawing2D.LinearGradientBrush gb = new System.Drawing.Drawing2D.LinearGradientBrush(rBot, e.BackgroundColor, c2, 90f))
                    {
                        e.Graphics.FillRectangle(gb, rBot);
                    }
                }
                else if (e.BackgroundColor == this._monthTitleColor)
                {
                    int r, g, b;
                    r = (int)(e.BackgroundColor.R * 1.07);
                    g = (int)(e.BackgroundColor.G * 1.07);
                    b = (int)(e.BackgroundColor.B * 1.07);
                    Color c2 = Color.FromArgb(r > 255 ? 255 : r, g > 255 ? 255 : g, b > 255 ? 255 : b);
                    Rectangle rTop = new Rectangle(new Point(e.Bounds.X, e.Bounds.Y + 1), new Size(e.Bounds.Width, (e.Bounds.Height / 2) + 1));
                    Rectangle rBot = new Rectangle(new Point(e.Bounds.X, e.Bounds.Y + (e.Bounds.Height / 2)), rTop.Size);

                    using (System.Drawing.Drawing2D.LinearGradientBrush gb = new System.Drawing.Drawing2D.LinearGradientBrush(rTop, c2, e.BackgroundColor, 90f))
                    {
                        e.Graphics.FillRectangle(gb, rTop);
                    }
                    using (System.Drawing.Drawing2D.LinearGradientBrush gb = new System.Drawing.Drawing2D.LinearGradientBrush(rBot, e.BackgroundColor, c2, 90f))
                    {
                        e.Graphics.FillRectangle(gb, rBot);
                    }
                }
                else
                {
                    using (SolidBrush b = new SolidBrush(e.BackgroundColor))
                    {
                        e.Graphics.FillRectangle(b, e.Bounds);
                    }
                }
            }

            if (!e.TextColor.IsEmpty && !string.IsNullOrEmpty(e.Text))
            {
                TextRenderer.DrawText(e.Graphics, e.Text, e.Font != null ? e.Font : Font, e.Bounds, e.TextColor, e.TextFlags);
            }

            if (!e.BorderColor.IsEmpty)
            {
                using (Pen p = new Pen(e.BorderColor))
                {
                    Rectangle r = e.Bounds;
                    r.Width--; r.Height--;
                    e.Graphics.DrawRectangle(p, r);
                }
            }
        }

        private void UpdateMonthSize()
        {
            //One row of day names plus 31 possible numbers
            string[] strs = new string[7 + 31];
            int maxWidth = 0;
            int maxHeight = 0;

            for (int i = 0; i < 7; i++)
            {
                strs[i] = ViewStart.AddDays(i).ToString(DayNamesFormat).Substring(0, DayNamesLength);
            }

            for (int i = 7; i < strs.Length; i++)
            {
                strs[i] = (i - 6).ToString();
            }

            Font f = new Font(Font, FontStyle.Bold);

            for (int i = 0; i < strs.Length; i++)
            {
                Size s = TextRenderer.MeasureText(strs[i], f);
                maxWidth = Math.Max(s.Width, maxWidth);
                maxHeight = Math.Max(s.Height, maxHeight);
            }

            maxWidth += ItemPadding.Horizontal;
            maxHeight += ItemPadding.Vertical;

            _daySize = new Size(maxWidth, maxHeight);
            _monthSize = new Size(maxWidth * 7, maxHeight * 7 + maxHeight * (DayNamesVisible ? 1 : 0));
        }

        private void UpdateMonths()
        {
            int gapping = 2;
            int calendarsX = Convert.ToInt32(Math.Max(Math.Floor((double)ClientSize.Width / (double)(MonthSize.Width + gapping)), 1.0));
            int calendarsY = Convert.ToInt32(Math.Max(Math.Floor((double)ClientSize.Height / (double)(MonthSize.Height + gapping)), 1.0));
            int calendars = calendarsX * calendarsY;
            int monthsWidth = (calendarsX * MonthSize.Width) + (calendarsX - 1) * gapping;
            int monthsHeight = (calendarsY * MonthSize.Height) + (calendarsY - 1) * gapping;
            int startX = (ClientSize.Width - monthsWidth) / 2;
            int startY = (ClientSize.Height - monthsHeight) / 2;
            int curX = startX;
            int curY = startY;
            _forwardMonthIndex = calendarsX - 1;

            _months = new MonthViewMonth[calendars];

            for (int i = 0; i < Months.Length; i++)
            {
                Months[i] = new MonthViewMonth(this, ViewStart.AddMonths(i));
                Months[i].SetLocation(new Point(curX, curY));

                curX += gapping + MonthSize.Width;

                if ((i + 1) % calendarsX == 0)
                {
                    curX = startX;
                    curY += gapping + MonthSize.Height;
                }
            }

            MonthViewMonth first = Months[0];
            MonthViewMonth last = Months[_forwardMonthIndex];

            SetBackwardButtonBounds(new Rectangle((first.Bounds.Left + ItemPadding.Left) + (DaySize.Height - ItemPadding.Horizontal) + 7, first.Bounds.Top + ItemPadding.Top, DaySize.Height - ItemPadding.Horizontal, DaySize.Height - ItemPadding.Vertical));
            SetFastBackwardButtonBounds(new Rectangle(BackwardButtonBounds.Left - (int)(BackwardButtonBounds.Width * 1.25), BackwardButtonBounds.Y, (int)(BackwardButtonBounds.Width * 1.25), BackwardButtonBounds.Height));

            SetForwardButtonBounds(new Rectangle((first.Bounds.Right - ItemPadding.Right - BackwardButtonBounds.Width * 2) - 7, first.Bounds.Top + ItemPadding.Top, BackwardButtonBounds.Width, BackwardButtonBounds.Height ));
            SetFastForwardButtonBounds(new Rectangle(ForwardButtonBounds.Left + (int)(ForwardButtonBounds.Width), ForwardButtonBounds.Y, (int)(ForwardButtonBounds.Width * 1.25), ForwardButtonBounds.Height));
        }

        #endregion

        #region Overrides

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Focus();

            _mouseDown = true;

            MonthViewDay day = HitTest(e.Location);

            if (day != null)
            {
                switch (SelectionMode)
                {
                    case MonthViewSelection.Manual:
                    case MonthViewSelection.Day:
                        SelectionEnd = _selectionStart = day.Date.Date;
                        break;
                    case MonthViewSelection.WorkWeek:
                        SelectWorkWeek(day.Date.Date);
                        break;
                    case MonthViewSelection.Week:
                        SelectWeek(day.Date.Date);
                        break;
                    case MonthViewSelection.Month:
                        SelectMonth(day.Date.Date);
                        break;
                }
            }

            if (ForwardButtonSelected)
            {
                GoForward();
            }
            else if (BackwardButtonSelected)
            {
                GoBackward();
            }
            else if (FastForwardButtonSelected)
            {
                int ct = Months.Length;
                for (int i = 0; i < ct; i++)
                {
                    GoForward();
                }
            }
            else if (FastBackwardButtonSelected)
            {
                int ct = Months.Length;
                for (int i = 0; i < ct; i++)
                {
                    GoBackward();
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_mouseDown)
            {
                MonthViewDay day = HitTest(e.Location);

                if (day != null && day != _lastHitted)
                {
                    switch (SelectionMode)
                    {
                        case MonthViewSelection.Manual:
                            if (day.Date > SelectionStart)
                            {
                                SelectionEnd = day.Date.Date;
                            }
                            else
                            {
                                SelectionStart = day.Date.Date;
                            }
                            break;
                        case MonthViewSelection.Day:
                            SelectionEnd = _selectionStart = day.Date.Date;
                            break;
                        case MonthViewSelection.WorkWeek:
                            SelectWorkWeek(day.Date.Date);
                            break;
                        case MonthViewSelection.Week:
                            SelectWeek(day.Date.Date);
                            break;
                        case MonthViewSelection.Month:
                            SelectMonth(day.Date.Date);
                            break;
                    }

                    _lastHitted = day;
                }
            }

            if (ForwardButtonBounds.Contains(e.Location))
            {
                SetForwardButtonSelected(true);
            }
            else if (ForwardButtonSelected)
            {
                SetForwardButtonSelected(false);
            }

            if (BackwardButtonBounds.Contains(e.Location))
            {
                SetBackwardButtonSelected(true);
            }
            else if (BackwardButtonSelected)
            {
                SetBackwardButtonSelected(false);
            }

            if (FastForwardButtonBounds.Contains(e.Location))
            {
                SetFastForwardButtonSelected(true);
            }
            else if (FastForwardButtonSelected)
            {
                SetFastForwardButtonSelected(false);
            }

            if (FastBackwardButtonBounds.Contains(e.Location))
            {
                SetFastBackwardButtonSelected(true);
            }
            else if (FastBackwardButtonSelected)
            {
                SetFastBackwardButtonSelected(false);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _mouseDown = false;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (e.Delta < 0)
            {
                GoForward();
            }
            else
            {
                GoBackward();
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.Clear(SystemColors.Window);

            for (int i = 0; i < Months.Length; i++)
            {
                if (Months[i].Bounds.IntersectsWith(e.ClipRectangle))
                {
                    #region MonthTitle

                    string title = Months[i].Date.ToString(MonthTitleFormat);
                    MonthViewBoxEventArgs evtTitle = new MonthViewBoxEventArgs(e.Graphics, Months[i].MonthNameBounds,
                        title,
                        Focused ? MonthTitleTextColor : MonthTitleTextColorInactive,
                        Focused ? MonthTitleColor : MonthTitleColorInactive);

                    DrawBox(evtTitle);

                    #endregion

                    #region DayNames

                    for (int j = 0; j < Months[i].DayNamesBounds.Length; j++)
                    {
                        MonthViewBoxEventArgs evtDay = new MonthViewBoxEventArgs(e.Graphics, Months[i].DayNamesBounds[j], Months[i].DayHeaders[j],
                            StringAlignment.Far, ForeColor, DayBackgroundColor);

                        DrawBox(evtDay);
                    }

                    if (Months[i].DayNamesBounds != null && Months[i].DayNamesBounds.Length != 0)
                    {
                        using (Pen p = new Pen(MonthTitleColor))
                        {
                            int y = Months[i].DayNamesBounds[0].Bottom;
                            e.Graphics.DrawLine(p, new Point(Months[i].Bounds.X, y), new Point(Months[i].Bounds.Right, y));
                        }
                    }
                    #endregion

                    #region Days
                    foreach (MonthViewDay day in Months[i].Days)
                    {
                        if (!day.Visible) continue;

                        MonthViewBoxEventArgs evtDay = new MonthViewBoxEventArgs(e.Graphics, day.Bounds, day.Date.Day.ToString(),
                            StringAlignment.Far,
                            day.Grayed ? DayGrayedText : (day.Selected ? DaySelectedTextColor : ForeColor),
                            day.Selected ? DaySelectedBackgroundColor : DayBackgroundColor);

                        if (day.Date.Equals(DateTime.Now.Date))
                        {
                            evtDay.BorderColor = TodayBorderColor;
                        }

                        DrawBox(evtDay);
                    }
                    #endregion

                    #region Arrows

                    if (i == 0)
                    {
                        Rectangle r = BackwardButtonBounds;
                        using (Brush b = new SolidBrush(BackwardButtonSelected ? ArrowsSelectedColor : ArrowsColor))
                        {
                            e.Graphics.FillPolygon(b, new Point[] {
                                new Point(r.Right, r.Top + r.Height / 5),
                                new Point(r.Right, (r.Bottom - r.Height / 5)- 1),
                                new Point(r.Left + r.Width / 2, r.Top + r.Height / 2),
                            });
                        }
                    }

                    if (i == _forwardMonthIndex)
                    {
                        Rectangle r = ForwardButtonBounds;
                        using (Brush b = new SolidBrush(ForwardButtonSelected ? ArrowsSelectedColor : ArrowsColor))
                        {
                            e.Graphics.FillPolygon(b, new Point[] {
                                new Point(r.X, r.Top + r.Height / 5),
                                new Point(r.X, (r.Bottom - r.Height / 5)- 1),
                                new Point(r.Left + r.Width / 2, r.Top + r.Height / 2),
                            });
                        }
                    }

                    if (i == 0)
                    {
                        Rectangle r = new Rectangle(BackwardButtonBounds.X - (int)(BackwardButtonBounds.Width * 1.5), BackwardButtonBounds.Y, BackwardButtonBounds.Width, BackwardButtonBounds.Height);
                        using (Brush b = new SolidBrush(FastBackwardButtonSelected ? ArrowsSelectedColor : ArrowsColor))
                        {
                            e.Graphics.FillPolygon(b, new Point[] {
                                new Point(r.Right, r.Top + r.Height / 5),
                                new Point(r.Right, (r.Bottom - r.Height / 5) - 1),
                                new Point(r.Left + r.Width / 2, r.Top + r.Height / 2),
                            });
                        }

                        r = new Rectangle(BackwardButtonBounds.X - BackwardButtonBounds.Width / 2, BackwardButtonBounds.Y, BackwardButtonBounds.Width, BackwardButtonBounds.Height);
                        using (Brush b = new SolidBrush(FastBackwardButtonSelected ? ArrowsSelectedColor : ArrowsColor))
                        {
                            e.Graphics.FillPolygon(b, new Point[] {
                                new Point(r.Right - r.Width / 2, r.Top + r.Height / 5),
                                new Point(r.Right - r.Width / 2, (r.Bottom - r.Height / 5)- 1),
                                new Point(r.Left, r.Top + r.Height / 2),
                            });
                        }
                    }

                    if (i == _forwardMonthIndex)
                    {

                        Rectangle r = new Rectangle(ForwardButtonBounds.X + (int)(ForwardButtonBounds.Width * 1.5), ForwardButtonBounds.Y, ForwardButtonBounds.Width, ForwardButtonBounds.Height);
                        using (Brush b = new SolidBrush(FastForwardButtonSelected ? ArrowsSelectedColor : ArrowsColor))
                        {
                            e.Graphics.FillPolygon(b, new Point[] {
                                new Point(r.X, r.Top + r.Height / 5),
                                new Point(r.X, (r.Bottom - r.Height / 5)- 1),
                                new Point(r.Left + r.Width / 2, r.Top + r.Height / 2),
                            });
                        }
                        r = new Rectangle(ForwardButtonBounds.X + ForwardButtonBounds.Width / 2, ForwardButtonBounds.Y, ForwardButtonBounds.Width, ForwardButtonBounds.Height);
                        using (Brush b = new SolidBrush(FastForwardButtonSelected ? ArrowsSelectedColor : ArrowsColor))
                        {
                            e.Graphics.FillPolygon(b, new Point[] {
                                new Point(r.X + r.Width / 2, r.Top + r.Height / 5),
                                new Point(r.X + r.Width / 2, (r.Bottom - r.Height / 5)- 1),
                                new Point(r.Left + r.Width, r.Top + r.Height / 2),
                            });
                        }
                    }

                    #endregion
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            UpdateMonths();
            Invalidate();
        }

        protected void OnSelectionChanged(EventArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, e);
            }
        }

        #endregion
    }
}

