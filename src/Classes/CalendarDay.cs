using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Baldini.Controls.Calendar
{

    /// <summary>
    /// Enumerates the possible modes of the days visualization on theCalendar
    /// </summary>
    public enum CalendarDaysMode
    {
        /// <summary>
        /// A short version of the day is visible without time scale.
        /// </summary>
        Short,

        /// <summary>
        /// The day is fully visible with time scale.
        /// </summary>
        Expanded,

        /// <summary>
        /// Same as expanded, just shows the Resource name rather than the date.
        /// </summary>
        Resource
    }
    /// <summary>
    /// Represents a day present on theCalendar control's view.
    /// </summary>
    public class CalendarDay
        : CalendarSelectableElement
    {
        #region Static

        private Size overflowSize = new Size(16, 16);
        private Padding overflowPadding = new Padding(5);

        #endregion

        #region Events

        #endregion

        #region Variables
        private List<CalendarItem> _containedItems;
        private Calendar _calendar;
        private DateTime _date;
        private CalendarResource _resource;
        private CalendarDayTop _dayTop;
        private int _index;
        private int _itemIndex;
        private bool _overflowStart;
        private bool _overflowEnd;
        private bool _overflowStartSelected;
        private bool _overlowEndSelected;
        private CalendarTimeScaleUnit[] _timeUnits;
        //private CalendarDayAvailability[] _availability;
        private CalendarItemComparer containedItemSort;
        #endregion

        #region Ctor

        /// <summary>
        /// Creates a new Day
        /// </summary>
        /// <param name="calendar">Calendar this day belongs to</param>
        /// <param name="date">Date of the day</param>
        /// <param name="index">Index of the day on the current calendar's view</param>
        internal CalendarDay(Calendar calendar, DateTime date, int index)
            : base(calendar)
        {
            _containedItems = new List<CalendarItem>();
            _calendar = calendar;
            _dayTop = new CalendarDayTop(this);
            _date = date;
            _index = index;
            containedItemSort = new CalendarItemComparer();

            UpdateUnits();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a list of items contained on the day
        /// </summary>
        internal List<CalendarItem> ContainedItems
        {
            get { return _containedItems; }
        }

        /// <summary>
        /// Gets the DayTop of the day, the place where multi-day and all-day items are placed
        /// </summary>
        public CalendarDayTop DayTop
        {
            get { return _dayTop; }
        }

        /// <summary>
        /// Gets the bounds of the body of the day (where time-based CalendarItems are placed)
        /// </summary>
        public Rectangle BodyBounds
        {
            get
            {
                return Rectangle.FromLTRB(Bounds.Left, DayTop.Bounds.Bottom, Bounds.Right, Bounds.Bottom);
            }
        }

        /// <summary>
        /// Gets the date this day represents
        /// </summary>
        public override DateTime Date
        {
            get { return _date; }
        }

        /// <summary>
        /// Gets the bounds of the header of the day
        /// </summary>
        public Rectangle HeaderBounds
        {
            get
            {
                return new Rectangle(Bounds.Left, Bounds.Top, Bounds.Width, Calendar.Renderer.DayHeaderHeight);
            }
        }

        /// <summary>
        /// Gets the index of this day on the calendar
        /// </summary>
        public int Index
        {
            get { return _index; }
        }

        /// <summary>
        /// Gets the index of the top item on the day.
        /// </summary>
        public int ItemIndex
        {
            get { return _itemIndex; }
        }

        /// <summary>
        /// Gets a value indicating if the day is specified on the view (See remarks).
        /// </summary>
        /// <remarks>
        /// A day may not be specified on the view, but still present to make up a square calendar.
        /// This days should be drawn in a way that indicates it's necessary but unrequested presence.
        /// </remarks>
        public bool SpecifiedOnView
        {
            get
            {
                return Date.CompareTo(Calendar.ViewStart) >= 0 && Date.CompareTo(Calendar.ViewEnd) <= 0;
            }
        }

        /// <summary>
        /// Gets the time units contained on the day
        /// </summary>
        public CalendarTimeScaleUnit[] TimeUnits
        {
            get { return _timeUnits; }
        }

        /// <summary>
        /// /// <summary>
        /// Gets a value indicating if the day contains items not shown through the start of the day
        /// </summary>
        /// </summary>
        public bool OverflowStart
        {
            get { return _overflowStart; }
        }

        /// <summary>
        /// Gets the bounds of theOverflowStart indicator
        /// </summary>
        public virtual Rectangle OverflowStartBounds
        {
            get { return new Rectangle(new Point(Bounds.Right - overflowPadding.Right - overflowSize.Width, Bounds.Top + overflowPadding.Top), overflowSize); }
        }

        /// <summary>
        /// Gets a value indicating if theOverflowStart indicator is currently selected
        /// </summary>
        /// <remarks>
        /// This value set to <c>true</c> when user hovers the mouse on theOverflowStartBounds area
        /// </remarks>
        public bool OverflowStartSelected
        {
            get { return _overflowStartSelected; }
        }


        /// <summary>
        /// Gets a value indicating if the day contains items not shown through the end of the day
        /// </summary>
        public bool OverflowEnd
        {
            get { return _overflowEnd; }
        }

        /// <summary>
        /// Gets the bounds of theOverflowEnd indicator
        /// </summary>
        public virtual Rectangle OverflowEndBounds
        {
            get { return new Rectangle(new Point(Bounds.Right - overflowPadding.Right - overflowSize.Width, Bounds.Bottom - overflowPadding.Bottom - overflowSize.Height), overflowSize); }
        }

        /// <summary>
        /// Gets a value indicating if theOverflowEnd indicator is currently selected
        /// </summary>
        /// <remarks>
        /// This value set to <c>true</c> when user hovers the mouse on theOverflowStartBounds area
        /// </remarks>
        public bool OverflowEndSelected
        {
            get { return _overlowEndSelected; }
        }

        /// <summary>
        /// Gets a value containing the availability items for the specified day.
        /// </summary>
        public CalendarDayAvailability[] Availability
        {
            get
            {
                List<CalendarDayAvailability> ret = new List<CalendarDayAvailability>();
                if (this.Calendar != null && this.Calendar.Availability != null)
                {
                    foreach (var a in this.Calendar.Availability)
                        if (a.StartTime.Date == this.Date.Date)
                            ret.Add(a);
                }
                return ret.ToArray();
            }
        }

        /// <summary>
        /// Gets or sets the value of the resource for the specified day.
        /// </summary>
        public CalendarResource Resource
        {
            get { return _resource; }
            set { _resource = value; }
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return Date.ToShortDateString();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Clears the contained items and the PassingItems for each CalendarTimeScaleUnit
        /// </summary>
        internal void ClearContainedItems()
        {
            this.ContainedItems.Clear();
            foreach (var unit in this.TimeUnits)
                unit.PassingItems.Clear();
        }

        /// <summary>
        /// Adds an item to theContainedItems list if not in yet
        /// </summary>
        /// <param name="item"></param>
        internal void AddContainedItem(CalendarItem item)
        {
            if (!ContainedItems.Contains(item))
            {
                ContainedItems.Add(item);
                ContainedItems.Sort(containedItemSort);
            }
        }

        /// <summary>
        /// Sets the index of the top item on the day.
        /// </summary>
        /// <param name="index">Index of the first item to show</param>
        internal void SetItemIndex(int index)
        {
            if (index >= ContainedItems.Count)
                return;
            _itemIndex = index;
            if (_itemIndex > 0)
                SetOverflowStart(true);
            else
                SetOverflowStart(false);
            this.Calendar.Renderer.PerformItemsLayout();
            this.Calendar.Invalidate();
        }

        /// <summary>
        /// Sets the value of heOverflowEnd property
        /// </summary>
        /// <param name="overflow">Value of the property</param>
        internal void SetOverflowEnd(bool overflow)
        {
            _overflowEnd = overflow;
        }

        ///// <summary>
        ///// Sets the value of theOverflowEndSelected property
        ///// </summary>
        ///// <param name="selected">Value to pass to the property</param>
        //internal void SetOverflowEndSelected(bool selected)
        //{
        //    _overlowEndSelected= selected;
        //}

        /// <summary>
        /// Sets the value of heOverflowStart property
        /// </summary>
        /// <param name="overflow">Value of the property</param>
        internal void SetOverflowStart(bool overflow)
        {
            _overflowStart = overflow;
        }

        ///// <summary>
        ///// Sets the value of theOverflowStartSelected property
        ///// </summary>
        ///// <param name="selected">Value to pass to the property</param>
        //internal void SetOverflowStartSelected(bool selected)
        //{
        //    _overflowStartSelected = selected;
        //}

        /// <summary>
        /// Updates the value ofTimeUnits property
        /// </summary>
        internal void UpdateUnits()
        {
            int factor = 0;

            switch (Calendar.TimeScale)
            {
                case 0:
                case CalendarTimeScale.SixtyMinutes:    factor = 1;     break;
                case CalendarTimeScale.ThirtyMinutes:   factor = 2;     break;
                case CalendarTimeScale.FifteenMinutes:  factor = 4;     break;
                case CalendarTimeScale.TenMinutes:      factor = 6;     break;
                case CalendarTimeScale.SixMinutes:      factor = 10;    break;
                case CalendarTimeScale.FiveMinutes:     factor = 12;    break;
                default: throw new NotImplementedException("TimeScale not supported");
            }

            _timeUnits = new CalendarTimeScaleUnit[24 * factor];

            int hourSum = 0;
            int minSum = 0;

            for (int i = 0; i < _timeUnits.Length; i++)
            {
                _timeUnits[i] = new CalendarTimeScaleUnit(this, i, hourSum, minSum);

                minSum += 60 / factor;

                if (minSum >= 60)
                {
                    minSum = 0;
                    hourSum++;
                }
            }

            UpdateHighlights();
        }

        /// <summary>
        /// Updates the highlights of the units
        /// </summary>
        internal void UpdateHighlights()
        {
            if (TimeUnits == null)
                return;

            for (int i = 0; i < TimeUnits.Length; i++)
            {
                TimeUnits[i].SetHighlighted(TimeUnits[i].CheckHighlighted());
            }
        }

        #endregion

    }

    /// <summary>
    /// Represents the top area of a day, where multiday and all day items are stored
    /// </summary>
    public class CalendarDayTop
        : CalendarSelectableElement
    {

        #region Variables
        private CalendarDay _day;
        private List<CalendarItem> _passingItems;
        #endregion

        #region Ctor

        /// <summary>
        /// Creates a new DayTop for the specified day
        /// </summary>
        /// <param name="day"></param>
        public CalendarDayTop(CalendarDay day)
            : base(day.Calendar)
        {
            _day = day;
            _passingItems = new List<CalendarItem>();
        }

        #endregion

        #region Properties

        public override DateTime Date
        {
            get
            {
                return new DateTime(Day.Date.Year, Day.Date.Month, Day.Date.Day);
            }
        }

        /// <summary>
        /// Gets the Day of this DayTop
        /// </summary>
        public CalendarDay Day
        {
            get { return _day; }
        }


        /// <summary>
        /// Gets the list of items passing on this daytop
        /// </summary>
        public List<CalendarItem> PassingItems
        {
            get { return _passingItems; }
        }


        #endregion

        #region Private Methods

        internal void AddPassingItem(CalendarItem item)
        {
            if (!PassingItems.Contains(item))
            {
                PassingItems.Add(item);
            }
        }

        #endregion
    }

    /// <summary>
    /// Event data with a Calendar
    /// </summary>
    public class CalendarEventArgs
        : EventArgs
    {
        #region Ctor
        /// <summary>
        /// Creates a new event handler for the specified Calendar
        /// </summary>
        /// <param name="calendar">Calendar relating to the event</param>
        public CalendarEventArgs(Calendar calendar)
        {
            _calendar = calendar;
        }
        #endregion

        #region Properties
        private Calendar _calendar;

        /// <summary>
        /// Gets the calendar related to this event.
        /// </summary>
        public Calendar Calender
        {
            get { return _calendar; }
        }
        #endregion
    }

    /// <summary>
    /// Event data with a CalendarDay element
    /// </summary>
    public class CalendarDayEventArgs
        : EventArgs
    {
        #region Ctor

        /// <summary>
        /// Creates a new event with the specified day
        /// </summary>
        /// <param name="day">Day of the event</param>
        public CalendarDayEventArgs(CalendarDay day)
        {
            _calendarDay = day;
        }

        #endregion

        #region Properties
        private CalendarDay _calendarDay;

        /// <summary>
        /// Gets the day related to the event
        /// </summary>
        public CalendarDay CalendarDay
        {
            get { return _calendarDay; }
        }

        #endregion
    }

    [Serializable]
    public class CalendarDayAvailability
    {
        #region ctor
        public CalendarDayAvailability()
        {

        }

        public CalendarDayAvailability(DateTime startTime, TimeSpan duration)
        {
            StartTime = startTime;
            Duration = duration;
        }
        #endregion

        #region Properties

        public CalendarResource Resource { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }

        #endregion
    }

    public class CalendarHolidayCollection
    {

        private List<CalendarHoliday> holidays = new List<CalendarHoliday>();

        /// <summary> Adds a Holiday for the specific date with a background image. </summary>
        /// <param name="dateto,e">Date of the holiday</param>
        /// <param name="description">Name of the Holiday</param>
        public void Add(DateTime datetime, string description)
        {
            holidays.Add(new CalendarHoliday(datetime, description));
        }

        /// <summary> Adds a Holiday for the specific date with a background image. </summary>
        /// <param name="datetime">Date of the holiday</param>
        /// <param name="description">Name of the Holiday</param>
        /// <param name="background">Background Image</param>
        public void Add(DateTime datetime, string description, Image background)
        {
            holidays.Add(new CalendarHoliday(datetime, description, background));
        }

        /// <summary> Determines whether or not the list contains an entry for that date </summary>
        /// <param name="datetime">Date to check for</param>
        public bool ContainsKey(DateTime datetime)
        {
            bool ret = false;
            foreach (var item in holidays)
            {
                if (item.Date == datetime)
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }


        public Image GetImageForDay(DateTime date)
        {
            Image ret = null;
            if (ContainsKey(date))
            {
                foreach (var holiday in this[date])
                {
                    if (holiday.Background != null)
                    {
                        ret = holiday.Background;
                        break;
                    }
                }
            }
            return ret;
        }

        public string GetDescriptionForDay(DateTime date)
        {
            string ret = string.Empty;
            if (ContainsKey(date))
            {
                foreach (var holiday in this[date])
                {
                    if (!String.IsNullOrEmpty(holiday.Description))
                    {
                        if (!string.IsNullOrEmpty(ret))
                            ret += ", ";
                        ret += holiday.Description;
                    }
                }
            }
            return ret;
        }


        public CalendarHoliday[] this[DateTime date]
        {
            get
            {
                List<CalendarHoliday> ret = new List<CalendarHoliday>();
                foreach (var item in holidays)
                {
                    if (item.Date.Date == date.Date)
                    {
                        ret.Add(item);
                        break;
                    }
                }
                return ret.ToArray();
            }
        }

    }

    public class CalendarHoliday
    {
        private static Dictionary<string, Image> imageCache = new Dictionary<string, Image>();
        private static object cacheLock = new object();

        /// <summary> Description of the Holiday (Holiday Name) </summary>
        public string Description { get; set; }
        /// <summary> Date of the Holiday </summary>
        public DateTime Date { get; set; }
        /// <summary> Optional Background Image to display for the holiday </summary>
        public Image Background
        {
            get
            {
                Image ret = null;
                if (!string.IsNullOrEmpty(Hash))
                {
                    lock (cacheLock)
                        ret = imageCache[Hash];
                    return imageCache[Hash];
                }
                else
                    return null;
            }
        }

        private string Hash { get; set; }

        /// <summary> Creates a new CalendarHoliday with a background image </summary>
        public CalendarHoliday()
        {

        }

        /// <summary> Creates a new CalendarHoliday </summary>
        /// <param name="datetime">Date of the holiday</param>
        /// <param name="description">Name of the holiday</param>
        public CalendarHoliday(DateTime datetime, string description)
        {
            Date = datetime;
            Description = description;

            /* Comment out this code later */
            //Bitmap b = new Bitmap(100, 100);
            //Graphics g = Graphics.FromImage(b);
            //using (Brush s = new SolidBrush(Color.Red))
            //{
            //    g.FillRectangle(s, 0, 0, b.Width, b.Height);
            //}
            //Background = b;
            /* Comment out above code later */
        }

        /// <summary> Creates a new CalendarHoliday with a background image </summary>
        /// <param name="datetime">Date of the holiday</param>
        /// <param name="description">Name of the holiday</param>
        /// <param name="background">Background Image for the Month View</param>
        public CalendarHoliday(DateTime datetime, string description, Image background)
        {
            Date = datetime;
            Description = description;
            Hash = Cache(background);
        }

        private static string Cache(Image source)
        {
            Image ret = null;
            string _hash = string.Empty;

            ImageConverter c = new ImageConverter();
            using (var m = new System.Security.Cryptography.MD5CryptoServiceProvider())
            {
                byte[] data = c.ConvertTo(source, typeof(byte[])) as byte[];
                foreach (byte b in m.ComputeHash(data))
                    _hash += b.ToString("X2");
            }
            // Check the cache for the hash. if it exists, return the hash.
            bool found = false;
            lock (cacheLock)
                if (imageCache.ContainsKey(_hash))
                    found = true;

            if (found)
                return _hash;

            // hash not found in the cache. Add it to it.

            // Lets start with a generic size...
            // largest block on a 1920x1080 resolution screen with 2 weeks showing
            // is around 160 x 200px. So shrink below that.
            float delta = 1;
            if (source.Height > 200)
                delta = source.Height / 200;
            if (source.Width > 160)
                delta = Math.Max(delta, source.Width / 160);

            // Now adjust the dimensions and create a thumbnail.
            if (delta != 1)
                ret = source.GetThumbnailImage(Convert.ToInt32(source.Width / delta), Convert.ToInt32(source.Height / delta), null, IntPtr.Zero);
            else
                ret = source.GetThumbnailImage(source.Width, source.Height, null, IntPtr.Zero);

            lock (cacheLock)
                imageCache.Add(_hash, ret);

            return _hash;
        }

        public override string ToString()
        {
            return Description;
        }
    }

}
