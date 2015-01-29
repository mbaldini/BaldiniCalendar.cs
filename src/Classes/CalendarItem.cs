using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace Baldini.Controls.Calendar
{
    /// <summary>
    /// Represents an item of the calendar with a date and timespan
    /// </summary>
    /// <remarks>
    /// <para>CalendarItem provides a graphical representation of tasks within a date range.</para>
    /// </remarks>
    public class CalendarItem
        : CalendarSelectableElement
    {
        #region Static

        private static int CompareBounds(Rectangle r1, Rectangle r2)
        {
            return r1.Top.CompareTo(r2.Top);
        }

        #endregion

        #region Events
        #endregion

        #region Variables
        private int _appointmentID;
        private CalendarResource _resource;
        private Rectangle[] _additionalBounds;
        private Color _backgroundColor;
        private Color _backgroundColorLighter;
        private Color _borderColor;
        private DateTime _startDate;
        private DateTime _endDate;
        private Color _oreColor;
        private bool _locked;
        private TimeSpan _duration;
        private Image _image;
        private CalendarItemImageAlign _imageAlign;
        private bool _isDragging;
        private bool _isEditing;
        private bool _isResizingStartDate;
        private bool _isResizingEndDate;
        private bool _isOnView;
        private int _minuteStartTop;
        private int _minuteEndTop;
        private HatchStyle _pattern;
        private Color _patternColor;
        private List<CalendarTimeScaleUnit> _unitsPassing;
        private List<CalendarDayTop> _topsPassing;
        private object _tag;
        private string _text;
        private bool _isAllDay;
        private ICalendarItem _dataItem;
        #endregion

        #region Ctor

        /// <summary>
        /// Creates a new Item that belongs to the specified calendar
        /// </summary>
        /// <param name="calendar">Calendar to reference item</param>
        public CalendarItem(Calendar calendar)
            : base(calendar)
        {
            _unitsPassing = new List<CalendarTimeScaleUnit>();
            _topsPassing = new List<CalendarDayTop>();
            _backgroundColor = Color.Empty;
            _borderColor = Color.Empty;
            _oreColor = Color.Empty;
            _backgroundColorLighter = Color.Empty;
            _imageAlign = CalendarItemImageAlign.West;
        }

        /// <summary>
        /// Creates a new item with the specified date range and text
        /// </summary>
        /// <param name="calendar">Calendar to reference item</param>
        /// <param name="startDate">Start date of the item</param>
        /// <param name="endDate">End date of the item</param>
        /// <param name="text">Text of the item</param>
        public CalendarItem(Calendar calendar, DateTime startDate, DateTime endDate, string text)
            : this(calendar)
        {
            StartDate = startDate;
            EndDate = endDate;
            Text = text;
        }

        /// <summary>
        /// Creates a new item with the specified date, duration and text
        /// </summary>
        /// <param name="calendar">Calendar to reference item</param>
        /// <param name="startDate">Start date of the item</param>
        /// <param name="duration">Duration of the item</param>
        /// <param name="text">Text of the item</param>
        public CalendarItem(Calendar calendar, DateTime startDate, TimeSpan duration, string text)
            : this(calendar, startDate, startDate.Add(duration), text)
        { }

        /// <summary>
        /// Creates a new item with the specified date, duration and text
        /// </summary>
        /// <param name="calendar">Calendar to reference item</param>
        /// <param name="startDate">Start date of the item</param>
        /// <param name="duration">Duration of the item</param>
        /// <param name="text">Text of the item</param>
        /// <param name="resource">Resource associated to the item.</param>
        public CalendarItem(Calendar calendar, DateTime startDate, TimeSpan duration, string text, CalendarResource resource)
            : this(calendar, startDate, startDate.Add(duration), text)
        {
            Resource = resource;
        }

        #endregion

        #region Properties

        private string uniqueId = string.Empty;
        internal string UniqueId
        {
            get
            {
                if (string.IsNullOrEmpty(uniqueId))
                {
                    byte[] hashBytes = new byte[32];
                    int index = 0;

                    // index 0 to 3 - AppointmentID
                    foreach (var b in BitConverter.GetBytes(this.AppointmentID))
                    { hashBytes[index] = b; index++; }

                    // index 4 to 7 - TagID
                    foreach (var b in BitConverter.GetBytes(this.TagID))
                    { hashBytes[index] = b; index++; }

                    // index 8 to 11 - ResourceID
                    foreach (var b in BitConverter.GetBytes(this.Resource == null ? 0 : this.Resource.ResourceID))
                    { hashBytes[index] = b; index++; }

                    // index = 12 to 15 - BackColor
                    foreach (var b in BitConverter.GetBytes(this.BackgroundColor == null ? Color.White.ToArgb() : this.BackgroundColor.ToArgb()))
                    { hashBytes[index] = b; index++; }

                    // index 16 to 23 - Date
                    foreach (var b in BitConverter.GetBytes(this.Date.ToOADate()))
                    { hashBytes[index] = b; index++; }

                    // index 24 to 31 - Duration
                    foreach (var b in BitConverter.GetBytes(this.Duration.TotalSeconds))
                    { hashBytes[index] = b; index++; }

                    using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
                    {
                        hashBytes = md5.ComputeHash(hashBytes);
                        foreach (var b in hashBytes)
                            uniqueId += b.ToString("x2");
                    }
                }
                return uniqueId;
            }
        }

        /// <summary>
        /// Gets or sets the identifier for the databound item.
        /// </summary>
        public int AppointmentID
        {
            get { return _appointmentID; }
            set { _appointmentID = value; }
        }

        ///// <summary>
        ///// Gets or sets the actual underlying data item
        ///// </summary>
        //public ICalendarItem DataItem { get; set; }

        /// <summary>
        /// gets or sets an additional ID for the item.
        /// </summary>
        public int TagID { get; set; }

        /// <summary>
        /// Gets or sets an array of rectangles containing bounds additional toBounds property.
        /// </summary>
        /// <remarks>
        /// Items may contain additional bounds because of several graphical occourences, mostly whenCalendar in
        ///CalendarDaysMode.Short mode, due to the duration of the item; e.g. when an all day item lasts several weeks,
        /// one rectangle for week must be drawn to indicate the presence of the item.
        /// </remarks>
        public virtual Rectangle[] AditionalBounds
        {
            get { return _additionalBounds; }
            set { _additionalBounds = value; }
        }

        /// <summary>
        /// Gets or sets the resource (eg. staff or location) for the object.
        /// </summary>
        public CalendarResource Resource
        {
            get { return _resource; }
            set { _resource = value; }
        }

        /// <summary>
        /// Gets or sets the background color for the object. If Color.Empty, renderer default's will be used.
        /// </summary>
        public Color BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }

        /// <summary>
        /// Gets or sets the lighter background color of the item
        /// </summary>
        public Color BackgroundColorLighter
        {
            get { return _backgroundColorLighter; }
            set { _backgroundColorLighter = value; }
        }

        /// <summary>
        /// Gets or sets the bordercolor of the item. If Color.Empty, renderer default's will be used.
        /// </summary>
        public Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; }
        }

        /// <summary>
        /// Gets or sets the DataItem for this item.
        /// </summary>
        public ICalendarItem DataItem
        {
            get { return _dataItem; }
            set { _dataItem = value; }
        }

        /// <summary>
        /// Gets the StartDate of the item. Implemented
        /// </summary>
        public override DateTime Date
        {
            get
            {
                return StartDate;
            }
        }

        /// <summary>
        /// Gets the day on theCalendar where this item ends
        /// </summary>
        /// <remarks>
        /// This day is not necesarily the day corresponding to the day onEndDate,
        /// since this date can be out of the range of the current view.
        /// <para>If Item is not on view date range this property will return null.</para>
        /// </remarks>
        public CalendarDay DayEnd
        {
            get
            {
                if (!IsOnViewDateRange)
                {
                    return null;
                }
                else if (IsOpenEnd)
                {
                    return Calendar.Days[Calendar.Days.Length - 1];
                }
                else
                {
                    return Calendar.FindDay(EndDate);
                }
            }
        }

        /// <summary>
        /// Gets the day on theCalendar where this item starts
        /// </summary>
        /// <remarks>
        /// This day is not necesarily the day corresponding to the day onStartDate,
        /// since start date can be out of the range of the current view.
        /// <para>If Item is not on view date range this property will return null.</para>
        /// </remarks>
        public CalendarDay DayStart
        {
            get
            {
                if (!IsOnViewDateRange)
                {
                    return null;
                }
                else if (IsOpenStart)
                {
                    return Calendar.Days[0];
                }
                else
                {
                    return Calendar.FindDay(StartDate);
                }
            }
        }

        /// <summary>
        /// Gets the duration of the item
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                if (_duration.TotalMinutes == 0)
                {
                    _duration = EndDate.Subtract(StartDate);
                }
                return _duration;
            }
        }

        /// <summary>
        /// Gets or sets the end time of the item
        /// </summary>
        public DateTime EndDate

        {
            get { return _endDate; }
            set
            {
                _endDate = value;
                _duration = new TimeSpan(0, 0, 0);
                ClearPassings();
            }
        }

        /// <summary>
        /// Gets the text of the end date
        /// </summary>
        public virtual string EndDateText
        {
            get
            {
                string date = string.Empty;
                string time = string.Empty;

                if (IsOpenEnd)
                {
                    date = EndDate.ToString(Calendar.ItemsDateFormat);
                }

                if (ShowEndTime && !EndDate.TimeOfDay.Equals(new TimeSpan(23, 59, 59)))
                {
                    time = EndDate.ToString(Calendar.ItemsTimeFormat);
                }

                return string.Format("{0} {1}", date, time).Trim();
            }
        }

        /// <summary>
        /// Gets or sets the forecolor of the item. If Color.Empty, renderer default's will be used.
        /// </summary>
        public Color ForeColor
        {
            get { return _oreColor; }
            set { _oreColor = value;
        }
        }

        /// <summary>
        /// Gets or sets an image for the item
        /// </summary>
        public Image Image
        {
            get { return _image; }
            set { _image = value; }
        }

        /// <summary>
        /// Gets or sets the alignment of the image relative to the text
        /// </summary>
        public CalendarItemImageAlign ImageAlign
        {
            get { return _imageAlign; }
            set { _imageAlign = value; }
        }

        /// <summary>
        /// Gets or sets the bounds of the image
        /// </summary>
        public Rectangle ImageBounds { get; set; }

        /// <summary>
        /// Gets a value indicating if the item is being dragged
        /// </summary>
        public bool IsDragging
        {
            get { return _isDragging; }
        }

        /// <summary>
        /// Gets a value indicating if the item is currently being edited by the user
        /// </summary>
        public bool IsEditing
        {
            get { return _isEditing; }
        }

        /// <summary>
        /// Gets or Sets a value that determines if this is an all day appointment (forcing it to show on the DayTop area of the calendar)
        /// </summary>
        public bool IsAllDay
        {
            get { return _isAllDay; }
            set { _isAllDay = value; }
        }

        /// <summary>
        /// Gets a value indicating if the item goes on the DayTop area of theCalendarDay
        /// </summary>
        public bool IsOnDayTop
        {
            get
            {
                return StartDate.Day != EndDate.AddSeconds(1).Day || IsAllDay;
            }
        }

        /// <summary>
        /// Gets a value indicating if the item is currently on view.
        /// </summary>
        /// <remarks>
        /// The item may not be on view because of scrolling
        /// </remarks>
        public bool IsOnView
        {
            get { return _isOnView; }
        }

        /// <summary>
        /// Gets a value indicating if the item is on the range specified byCalendar.ViewStart andCalendar.ViewEnd
        /// </summary>
        public bool IsOnViewDateRange
        {
            get
            {
                //Checks for an intersection of item's dates against calendar dates
                DateTime fd = Calendar.Days[0].Date;
                DateTime ld = Calendar.Days[Calendar.Days.Length - 1].Date.Add(new TimeSpan(23,59,59));
                DateTime sd = StartDate;
                DateTime ed = EndDate;
                return sd < ld && fd < ed;
            }
        }

        /// <summary>
        /// Gets a value indicating if the item'sStartDate is before theCalendar.ViewStart date.
        /// </summary>
        public bool IsOpenStart
        {
            get
            {
                return StartDate.CompareTo(Calendar.Days[0].Date) < 0;
            }
        }

        /// <summary>
        /// Gets a value indicating if the item'sEndDate is aftter theCalendar.ViewEnd date.
        /// </summary>
        public bool IsOpenEnd
        {
            get
            {
                return EndDate.CompareTo(Calendar.Days[Calendar.Days.Length - 1].Date.Add(new TimeSpan(23, 59, 59))) > 0;
            }
        }

        /// <summary>
        /// Gets a value indicating if item is being resized by theStartDate
        /// </summary>
        public bool IsResizingStartDate
        {
            get { return _isResizingStartDate; }
        }

        /// <summary>
        /// Gets a value indicating if item is being resized by theEndDate
        /// </summary>
        public bool IsResizingEndDate
        {
            get { return _isResizingEndDate; }
        }

        /// <summary>
        /// Gets a value indicating if this item is locked.
        /// </summary>
        /// <remarks>
        /// When an item is locked, the user can't drag it or change it's text
        /// </remarks>
        public bool Locked
        {
            get { return _locked; }
            set { _locked = value; }
        }

        /// <summary>
        /// Gets the top correspoinding to the ending minute
        /// </summary>
        public int MinuteEndTop
        {
            get { return _minuteEndTop; }
        }


        /// <summary>
        /// Gets the top corresponding to the starting minute
        /// </summary>
        public int MinuteStartTop
        {
            get { return _minuteStartTop; }
        }


        /// <summary>
        /// Gets or sets the units that this item passes by
        /// </summary>
        internal List<CalendarTimeScaleUnit> UnitsPassing
        {
            get { return _unitsPassing; }
            set { _unitsPassing = value; }
        }

        /// <summary>
        /// Gets or sets the pattern style to use in the background of item.
        /// </summary>
        public HatchStyle Pattern
        {
            get { return _pattern; }
            set { _pattern = value; }
        }


        /// <summary>
        /// Gets or sets the pattern's color
        /// </summary>
        public Color PatternColor
        {
            get { return _patternColor; }
            set { _patternColor = value; }
        }


        /// <summary>
        /// Gets the list of DayTops that this item passes thru
        /// </summary>
        internal List<CalendarDayTop> TopsPassing
        {
            get { return _topsPassing; }
        }

        /// <summary>
        /// Gets a value indicating if the item should show the time of theStartDate
        /// </summary>
        public bool ShowStartTime
        {
            get
            {
                //return IsOpenStart || ((this.IsOnDayTop || Calendar.DaysMode == CalendarDaysMode.Short) && !StartDate.TimeOfDay.Equals(new TimeSpan(0, 0, 0)));
                return IsOpenStart || (this.IsOnDayTop || Calendar.DaysMode == CalendarDaysMode.Short);
            }
        }

        /// <summary>
        /// Gets a value indicating if the item should show the time of theEndDate
        /// </summary>
        public virtual bool ShowEndTime
        {
            get
            {
                return (IsOpenEnd ||
                    ((this.IsOnDayTop || Calendar.DaysMode == CalendarDaysMode.Short) && !EndDate.TimeOfDay.Equals(new TimeSpan(23, 59, 59)))) &&
                    !(Calendar.DaysMode == CalendarDaysMode.Short && StartDate.Date == EndDate.Date);
            }
        }

        /// <summary>
        /// Gets the text of the start date
        /// </summary>
        public virtual string StartDateText
        {
            get
            {
                string date = string.Empty;
                string time = string.Empty;

                if (IsOpenStart)
                {
                    date = StartDate.ToString(Calendar.ItemsDateFormat);
                }

                if (ShowStartTime && !StartDate.TimeOfDay.Equals(new TimeSpan(0, 0, 0)))
                {
                    time = StartDate.ToString(Calendar.ItemsTimeFormat);
                }

                return string.Format("{0} {1}", date, time).Trim();
            }
        }

        /// <summary>
        /// Gets or sets the start time of the item
        /// </summary>
        public virtual DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                _duration = new TimeSpan(0, 0, 0);
                ClearPassings();
            }
        }

        /// <summary>
        /// Gets or sets a tag object for the item
        /// </summary>
        public object Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        /// <summary>
        /// Gets or sets the text of the item
        /// </summary>
        public virtual string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Applies color to background, border, and forecolor, from the specified color.
        /// </summary>
        public void ApplyColor(Color color)
        {
            BackgroundColor = color;
            BackgroundColorLighter = Color.FromArgb(
                color.R + (255 - color.R) / 2 + (255 - color.R) / 3,
                color.G + (255 - color.G) / 2 + (255 - color.G) / 3,
                color.B + (255 - color.B) / 2 + (255 - color.B) / 3);

            BorderColor = Color.FromArgb(
                Convert.ToInt32(Convert.ToSingle(color.R) * .8f),
                Convert.ToInt32(Convert.ToSingle(color.G) * .8f),
                Convert.ToInt32(Convert.ToSingle(color.B) * .8f));

            //int avg = (color.R + color.G + color.B )/3;

            //if (avg > 255 / 2)
            //{
                ForeColor = Color.Black;
            //}
            //else
            //{
            //    ForeColor = Color.White;
            //}
        }

        /// <summary>
        /// Gets all the bounds related to the item.
        /// </summary>
        /// <remarks>
        ///  Items that are broken on two or more weeks may have more than one rectangle bounds.
        /// </remarks>
        /// <returns></returns>
        public IEnumerable<Rectangle> GetAllBounds()
        {
            List<Rectangle> r = new List<Rectangle>(AditionalBounds == null ? new Rectangle[] { } : AditionalBounds);
            r.Add(Bounds);

            r.Sort(CompareBounds);

            return r;
        }

        /// <summary>
        /// Removes all specific coloring for the item.
        /// </summary>
        public void RemoveColors()
        {
            BackgroundColor = Color.Empty;
            ForeColor = Color.Empty;
            BorderColor = Color.Empty;
        }

        /// <summary>
        /// Gets a value indicating if the specified point is in a resize zone ofStartDate
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool ResizeStartDateZone(Point p)
        {
            int margin = 4;

            List<Rectangle> rects = new List<Rectangle>(GetAllBounds());
            Rectangle first = rects[0];
            Rectangle last = rects[rects.Count - 1];

            if (IsOnDayTop || Calendar.DaysMode == CalendarDaysMode.Short)
            {
                return Rectangle.FromLTRB(first.Left, first.Top, first.Left + margin, first.Bottom).Contains(p);
            }
            else
            {
                return Rectangle.FromLTRB(first.Left, first.Top, first.Right, first.Top + margin).Contains(p);
            }
        }

        /// <summary>
        /// Gets a value indicating if the specified point is in a resize zone ofEndDate
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool ResizeEndDateZone(Point p)
        {
            int margin = 4;

            List<Rectangle> rects = new List<Rectangle>(GetAllBounds());
            Rectangle first = rects[0];
            Rectangle last = rects[rects.Count - 1];

            if (IsOnDayTop || Calendar.DaysMode == CalendarDaysMode.Short)
            {
                return Rectangle.FromLTRB(last.Right - margin, last.Top, last.Right, last.Bottom).Contains(p);
            }
            else
            {
                return Rectangle.FromLTRB(last.Left, last.Bottom - margin, last.Right, last.Bottom).Contains(p);
            }
        }

        /// <summary>
        /// Sets the bounds of the item
        /// </summary>
        /// <param name="r"></param>
        public new void SetBounds(Rectangle r)
        {
            base.SetBounds(r);
        }

        /// <summary>
        /// Indicates if the time of the item intersects with the provided time
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool IntersectsWith(TimeSpan timeStart, TimeSpan timeEnd, CalendarTimeScale scale)
        {
            int startMin = Convert.ToInt32(StartDate.TimeOfDay.TotalMinutes);
            int endMin = Convert.ToInt32(EndDate.TimeOfDay.TotalMinutes);
            int startMin2 = Convert.ToInt32(timeStart.TotalMinutes);
            int endMin2 = Convert.ToInt32(timeEnd.TotalMinutes);

            int scaleInt = Convert.ToInt32(scale);

            while (startMin % scaleInt != 0)
            {
                startMin--;
            }
            while (endMin % scaleInt != 0)
            {
                endMin++;
            }
            while (startMin2 % scaleInt != 0)
            {
                startMin2--;
            }
            while (endMin2 % scaleInt != 0)
            {
                endMin2++;
            }

            Rectangle r1 = Rectangle.FromLTRB(0, Convert.ToInt32(startMin), 5, Convert.ToInt32(endMin));
            Rectangle r2 = Rectangle.FromLTRB(0, Convert.ToInt32(startMin2), 5, Convert.ToInt32(endMin2 - 1));
            //Rectangle r1 = Rectangle.FromLTRB(0, Convert.ToInt32(StartDate.TimeOfDay.TotalMinutes), 5, Convert.ToInt32(EndDate.TimeOfDay.TotalMinutes));
            //Rectangle r2 = Rectangle.FromLTRB(0, Convert.ToInt32(timeStart.TotalMinutes), 5, Convert.ToInt32(timeEnd.TotalMinutes - 1));
            return r1.IntersectsWith(r2);
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", StartDate.ToShortTimeString(), EndDate.ToShortTimeString());
        }

        public override bool Equals(object obj)
        {
            if (obj is CalendarItem)
                return ((CalendarItem)obj).UniqueId == this.UniqueId;
            else
                return base.Equals(obj);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds bounds for the item
        /// </summary>
        /// <param name="r"></param>
        internal void AddBounds(Rectangle r)
        {
            if (r.IsEmpty) throw new ArgumentException("r can't be empty");

            if (Bounds.IsEmpty)
            {
                SetBounds(r);
            }
            else
            {
                List<Rectangle> rs = new List<Rectangle>(AditionalBounds == null ? new Rectangle[] { } : AditionalBounds);
                rs.Add(r);
                AditionalBounds = rs.ToArray();
            }
        }

        /// <summary>
        /// Adds the specified unit as a passing unit
        /// </summary>
        /// <param name="calendarTimeScaleUnit"></param>
        internal void AddUnitPassing(CalendarTimeScaleUnit calendarTimeScaleUnit)
        {
            if (!UnitsPassing.Contains(calendarTimeScaleUnit))
            {
                UnitsPassing.Add(calendarTimeScaleUnit);
            }
        }

        /// <summary>
        /// Adds the specifiedCalendarDayTop as a passing one
        /// </summary>
        /// <param name="top"></param>
        internal void AddTopPassing(CalendarDayTop top)
        {
            if (!TopsPassing.Contains(top))
            {
                TopsPassing.Add(top);
            }
        }

        /// <summary>
        /// Clears the item's existance off passing units and tops
        /// </summary>
        internal void ClearPassings()
        {
            foreach (CalendarTimeScaleUnit unit in UnitsPassing)
            {
                unit.ClearItemExistance(this);
            }

            UnitsPassing.Clear();
            TopsPassing.Clear();
        }

        /// <summary>
        /// Clears all bounds of the item
        /// </summary>
        internal void ClearBounds()
        {
            SetBounds(Rectangle.Empty);
            AditionalBounds = new Rectangle[] { };
            SetMinuteStartTop(0);
            SetMinuteEndTop(0);
        }

        /// <summary>
        /// It pushes the left and the right to the center of the item
        /// to visually indicate start and end time
        /// </summary>
        internal void FirstAndLastRectangleGapping()
        {
            if(!IsOpenStart)
                SetBounds(Rectangle.FromLTRB(Bounds.Left + Calendar.Renderer.ItemsPadding,
                    Bounds.Top, Bounds.Right, Bounds.Bottom));

            if (!IsOpenEnd)
            {
                if (AditionalBounds != null && AditionalBounds.Length > 0)
                {
                    Rectangle r = AditionalBounds[AditionalBounds.Length - 1];
                    AditionalBounds[AditionalBounds.Length - 1] = Rectangle.FromLTRB(
                        r.Left, r.Top, r.Right - Calendar.Renderer.ItemsPadding, r.Bottom);
                }
                else
                {
                    Rectangle r = Bounds;
                    SetBounds(Rectangle.FromLTRB(
                        r.Left, r.Top, r.Right - Calendar.Renderer.ItemsPadding, r.Bottom));
                }
            }
        }

        /// <summary>
        /// Sets the value of theDragging property
        /// </summary>
        /// <param name="dragging">Value indicating if the item is currently being dragged</param>
        internal void SetIsDragging(bool dragging)
        {
            _isDragging = dragging;
        }

        /// <summary>
        /// Sets the value of theIsEditing property
        /// </summary>
        /// <param name="editing">Value indicating if user is currently being editing</param>
        internal void SetIsEditing(bool editing)
        {
            _isEditing = editing;
        }

        /// <summary>
        /// Sets the value of theIsOnView property
        /// </summary>
        /// <param name="onView">Indicates if the item is currently on view</param>
        internal void SetIsOnView(bool onView)
        {
            _isOnView = onView;
        }

        /// <summary>
        /// Sets the value of theIsResizingStartDate property
        /// </summary>
        /// <param name="resizing"></param>
        internal void SetIsResizingStartDate(bool resizing)
        {
            _isResizingStartDate = resizing;
        }

        /// <summary>
        /// Sets the value of theIsResizingEndDate property
        /// </summary>
        /// <param name="resizing"></param>
        internal void SetIsResizingEndDate(bool resizing)
        {
            _isResizingEndDate = resizing;
        }

        /// <summary>
        /// Sets the value of theMinuteStartTop property
        /// </summary>
        /// <param name="top"></param>
        internal void SetMinuteStartTop(int top)
        {
            _minuteStartTop = top;
        }

        /// <summary>
        /// Sets the value of theMinuteEndTop property
        /// </summary>
        /// <param name="top"></param>
        internal void SetMinuteEndTop(int top)
        {
            _minuteEndTop = top;
        }

        #endregion

    }

    /// <summary>
    /// Possible alignment forCalendarItem images
    /// </summary>
    [Flags]
    public enum CalendarItemImageAlign
    {
        /// <summary>
        /// Image is drawn at north of text
        /// </summary>
        North = 0x1,

        /// <summary>
        /// Image is drawn at south of text
        /// </summary>
        South = 0x2,

        /// <summary>
        /// Image is drawn at east of text
        /// </summary>
        East = 0x4,

        /// <summary>
        /// Image is drawn at west of text
        /// </summary>
        West = 0x8,
    }

    public class CalendarItemEventArgs
    : EventArgs
    {
        #region Ctor

        /// <summary>
        /// Creates a newCalendarItemEventArgs
        /// </summary>
        /// <param name="item">Related Item</param>
        public CalendarItemEventArgs(CalendarItem item)
        {
            _item = item;
        }

        #endregion

        #region Properties

        private CalendarItem _item;

        /// <summary>
        /// Gets theCalendarItem related to the event
        /// </summary>
        public CalendarItem Item
        {
            get { return _item; }
        }


        #endregion
    }

    public class CalendarItemCollection
    : List<CalendarItem>
    {

        #region Variables
        private Calendar _calendar;
        #endregion

        #region Ctor

        /// <summary>
        /// Creates a new collection that will belong to the specified calendar.
        /// </summary>
        /// <param name="c">Calendar this collection will belong to.</param>
        internal CalendarItemCollection(Calendar c)
        {
            _calendar = c;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the calendar this collection belongs to
        /// </summary>
        public Calendar Calendar
        {
            get { return _calendar; }
        }


        #endregion

        #region Public Methods

        /// <summary>
        /// Adds an item to the end of the list
        /// </summary>
        /// <param name="item">The object to be added to the end of the collection. The value can be null for reference types.</param>
        public new void Add(CalendarItem item)
        {
            base.Add(item); CollectionChanged();
        }

        /// <summary>
        /// Adds the items of the specified collection to the end of the list.
        /// </summary>
        /// <param name="items">The items whose elements should be added to the end of the collection. The collection itself cannont be null, but it can contain elements that are null.</param>
        public new void AddRange(IEnumerable<CalendarItem> items)
        {
            base.AddRange(items); CollectionChanged();
        }

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        public new void Clear()
        {
            base.Clear(); CollectionChanged();
        }

        /// <summary>
        /// Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert. The value can be null for reference types.</param>
        public new void Insert(int index, CalendarItem item)
        {
            base.Insert(index, item); CollectionChanged();
        }

        /// <summary>
        /// Inserts the items of a collection into this collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <param name="items"></param>
        public new void InsertRange(int index, IEnumerable<CalendarItem> items)
        {
            base.InsertRange(index, items); CollectionChanged();
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the collection.
        /// </summary>
        /// <param name="item">The item to remove from the collection. The value can be null for reference types.</param>
        /// <returns><c>true</c> if item is successfully removed; otherwise, <c>false</c>. This method also returns false if item was not found in the collection.</returns>
        public new bool Remove(CalendarItem item)
        {
            bool result = base.Remove(item);

            CollectionChanged();

            return result;
        }

        /// <summary>
        /// Removes the item at the specified index of the collection
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <returns></returns>
        public new void RemoveAt(int index)
        {
            base.RemoveAt(index); CollectionChanged();
        }

        /// <summary>
        /// Removes the all the items that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The Predicate delegate that defines the conditions of the items to remove.</param>
        /// <returns>The number of items removed from the collection.</returns>
        public new int RemoveAll(Predicate<CalendarItem> match)
        {
            int result = base.RemoveAll(match);

            CollectionChanged();

            return result;
        }

        /// <summary>
        /// Removes a range of elements from the collection
        /// </summary>
        /// <param name="index">The zero-based starting index of the range of items to remove.</param>
        /// <param name="count">The number of items to remove</param>
        public new void RemoveRange(int index, int count)
        {
            base.RemoveRange(index, count); CollectionChanged();
        }

        /// <summary>
        /// Clears the items from the list, then repopulated them. Only fires CollectionChanged once
        /// </summary>
        /// <param name="items"></param>
        public void ClearAndReplace(IEnumerable<CalendarItem> items)
        {
            base.Clear();
            base.AddRange(items);
            CollectionChanged();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles what to do when this collection changes
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private void CollectionChanged()
        {
            //Calendar.Renderer.PerformItemsLayout();
            //Calendar.Invalidate();
        }

        #endregion
    }

    public class CalendarItemCancelEventArgs
    : CancelEventArgs
    {
        #region Ctor

        /// <summary>
        /// Creates a newCalendarItemEventArgs
        /// </summary>
        /// <param name="item">Related Item</param>
        public CalendarItemCancelEventArgs(CalendarItem item)
        {
            _item = item;
        }

        #endregion

        #region Properties

        private CalendarItem _item;

        /// <summary>
        /// Gets theCalendarItem related to the event
        /// </summary>
        public CalendarItem Item
        {
            get { return _item; }
        }


        #endregion
    }

    class CalendarItemComparer
        : Comparer<CalendarItem>
    {
        public override int Compare(CalendarItem x, CalendarItem y)
        {
            if (x == null && y == null)
                return 0;
            else if (x == null && y != null)
                return -1;
            else if (x != null && y == null)
                return 1;
            else if (x.StartDate == y.StartDate)
                if (x.EndDate == y.EndDate)
                    return x.AppointmentID.CompareTo(y.AppointmentID) * -1;
                else
                    return x.EndDate.CompareTo(y.EndDate) * -1;
            else return x.StartDate.CompareTo(y.StartDate) * -1;
        }
    }

    //Not using this. Performance was entirely too slow.
    //public class CalendarItemBinding
    //{

    //    #region Ctor

    //    #endregion

    //    #region Properties

    //    public string ResourceMember { get; set; }
    //    public string StartMember { get; set; }
    //    public string EndMember { get; set; }
    //    public string DisplayMember { get; set; }
    //    public string ImageMember { get; set; }
    //    public string ForeColorMember { get; set; }
    //    public string BackColorMember { get; set; }

    //    #endregion

    //}

}
