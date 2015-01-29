using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;

namespace Baldini.Controls.Calendar
{
    public class MultiResourceCalendar : System.Windows.Forms.Panel
    {

        #region Variables
        private bool _allowNew;
        private bool _allowItemEdit;
        private bool _allowItemEditExternal;
        private bool _allowItemResize;
        private int _dayTopHeight = 59;
        private DayOfWeek _firstDayOfWeek;
        private CalendarHighlightRange[] _highlightRanges;
        private CalendarHolidayCollection _holidays;
        private string _itemsDateFormat;
        private string _itemsTimeFormat;
        private int _maximumFullDays = 8;
        private int _maximumViewDays = 35;
        private DateTime _selEnd;
        private DateTime _selStart;
        private CalendarTimeScale _timeScale;
        private int _timeUnitsOffset;
        private DateTime _viewEnd;
        private DateTime _viewStart;
        private AvailabilityGroup _availability;
        private CalendarDayAvailability[] _avail;
        private CalendarResource[] _resources;
        private ICalendarItem[] _dataSource;
        private ResourceGroup DataItems;
        private ResourceGroup ReminderDataItems;
        private List<Calendar> ControlList = new List<Calendar>();
        private object ControlListLock = new object();
        private int _displayCount = 1;
        private int _resourceHeight = 20;
        private Font _resourceFont = DefaultFont;
        private int _calendarIndex = 0;
        private CalendarItemPopupEventArgs _lastPopup;
        private CalendarResource _selectedResource;
        private bool suspendScroll = false;
        private object synclockObject = new object();
        private bool criticalChange = true;



        private ICalendarItem[] remindersource;
        private bool showImageOnReminders = true;
        private bool showImageOnAppointments = false;
        private bool showRemindersAsAllDay = true;

        //private string _resourceMember = "";
        //private CalendarItemBinding itemBinding = new CalendarItemBinding();

        private Calendar.CalendarLoadEventHandler calLoadHandler;
        private Calendar.OpenItemExternalEventHandler calOpenExternalHandler;
        private Calendar.MouseWheelEventHandler calWheelHandler;
        private Calendar.CalendarItemEventHandler calItemDatesChanged;
        private Calendar.CalendarItemCancelEventHandler calItemDeleting;
        private Calendar.CalendarItemEventHandler calItemDeleted;
        private Calendar.CalendarItemCancelEventHandler calItemCreated;
        private Calendar.CalendarItemCancelEventHandler calItemCreating;
        private Calendar.CalendarItemEventHandler calItemSelected;
        private Calendar.CalendarItemCancelEventHandler calItemTextEdited;
        private Calendar.CalendarItemCancelEventHandler calItemTextEditing;
        private Calendar.CalendarItemEventHandler calItemHover;
        private Calendar.CalendarItemEventHandler calItemMouseHoverEnd;
        private Calendar.CalendarItemEventHandler calItemClick;
        private Calendar.CalendarItemEventHandler calItemImageClick;
        private Calendar.CalendarItemEventHandler calItemDoubleClick;
        private MouseEventHandler calMouseClick;
        private MouseEventHandler calMouseDoubleClick;
        private Calendar.MouseWheelEventHandler calScrolled;
        private Calendar.CalendarEventHandler calEmptyTimeClick;

        public delegate void DisplayItemEventHandler(object sender, CalendarItemPopupEventArgs e);
        #endregion

        #region Events

        /// <summary>
        /// Occurs when an empty cell is clicked
        /// </summary>
        [Description("Occurs when an empty cell is clicked")]
        public event Calendar.CalendarEventHandler EmptyTimeClick;

        /// <summary>
        /// Occurs when an item is about to be created.
        /// </summary>
        /// <remarks>
        /// Event can be cancelled
        /// </remarks>
        [Description("Occurs when an item is about to be created.")]
        public event Calendar.CalendarItemCancelEventHandler ItemCreating;

        /// <summary>
        /// Occurs when an item has been created.
        /// </summary>
        [Description("Occurs when an item has been created.")]
        public event Calendar.CalendarItemCancelEventHandler ItemCreated;

        /// <summary>
        /// Occurs before an item is deleted
        /// </summary>
        [Description("Occurs before an item is deleted")]
        public event Calendar.CalendarItemCancelEventHandler ItemDeleting;

        /// <summary>
        /// Occurs when an item has been deleted
        /// </summary>
        [Description("Occurs when an item has been deleted")]
        public event Calendar.CalendarItemEventHandler ItemDeleted;

        /// <summary>
        /// Occurs when an item text is about to be edited
        /// </summary>
        [Description("Occurs when an item text is about to be edited")]
        public event Calendar.CalendarItemCancelEventHandler ItemTextEditing;

        /// <summary>
        /// Occurs when an item text is edited
        /// </summary>
        [Description("Occurs when an item text is edited")]
        public event Calendar.CalendarItemCancelEventHandler ItemTextEdited;

        /// <summary>
        /// Occurs when an item time range has changed
        /// </summary>
        [Description("Occurs when an item time range has changed")]
        public event Calendar.CalendarItemEventHandler ItemDatesChanged;

        /// <summary>
        /// Occurs when an item is clicked
        /// </summary>
        [Description("Occurs when an item is clicked")]
        public event Calendar.CalendarItemEventHandler ItemClick;

        /// <summary>
        /// Occurs when an item is clicked
        /// </summary>
        [Description("Occurs when the image on an item is clicked")]
        public event Calendar.CalendarItemEventHandler ItemImageClick;

        /// <summary>
        /// Occurs when an item is double-clicked
        /// </summary>
        [Description("Occurs when an item is double-clicked")]
        public event Calendar.CalendarItemEventHandler ItemDoubleClick;

        /// <summary>
        /// Occurs when an item is selected
        /// </summary>
        [Description("Occurs when an item is selected")]
        public event Calendar.CalendarItemEventHandler ItemSelected;

        /// <summary>
        ///
        /// </summary>
        [Description("")]
        public event DisplayItemEventHandler DisplayItemPopup;

        /// <summary>
        /// Occurs when the mouse is moved off an item after hovering
        /// </summary>
        [Description("Occurs when the mouse is moved off an item after hovering")]
        public event DisplayItemEventHandler ItemMouseHoverEnd;

        /// <summary>
        /// Occurs when the mouse wheel is turned
        /// </summary>
        [Description("Occurs when the mouse wheel is turned")]
        public event Calendar.MouseWheelEventHandler CalendarScrolled;

        /// <summary>
        /// Occurs when the user opens the item in an external editor
        /// </summary>
        [Description("Occurs when the user opens the item in an external editor")]
        public event Calendar.OpenItemExternalEventHandler OpenItemExternal;

        /// <summary>
        /// Occurs when the View Range changes
        /// </summary>
        [Description("Occurs when the View Range changes")]
        public event Calendar.CalendarEventHandler ViewRangeChanged;

        /// <summary>
        ///
        /// </summary>
        [Description("")]
        public event MouseEventHandler DoubleClick;

        #endregion

        #region Constants
        private static DateTime cStartValue = new DateTime(2012, 1, 1);
        private static DateTime cEndValue = new DateTime(2012, 1, 1).Add(new TimeSpan(23, 59, 59));
        #endregion

        #region Properties

        [Browsable(true), DefaultValue(null)]
        public CalendarResource[] Resources
        {
            get { return _resources; }
            set
            {
                if (!AreSame(value, _resources))
                {
                    _resources = value;
                    this._calendarIndex = 0;
                    criticalChange = true;
                    SetDisplayControls();
                }
            }
        }

        public ICalendarItem[] DataSource
        {
            get { return _dataSource; }
            set
            {
                _dataSource = value;
                if (value != null)
                {
                    ExamineDataItems();
                    ExamineAvailability();
                }
                else
                    DataItems = null;
                ReloadItems();
            }
        }

        public CalendarDaysMode DaysMode
        {
            get
            {
                CalendarDaysMode ret = CalendarDaysMode.Expanded;
                lock(ControlListLock)
                    if (this.ControlList != null && this.ControlList.Count > 0)
                        ret = this.ControlList[0].DaysMode;
                return ret;
            }
        }

        /// <summary>
        /// Gets or Sets the Image that is shown when dragging an item out of the calendar
        /// </summary>
        [DefaultValue(null)]
        public Image DragImage { get; set; }

        /// <summary>
        /// Gets or sets the number of displayed calendars.
        /// </summary>
        [DefaultValue(1)]
        [Description("Gets or sets the number of displayed calendars.")]
        public int DisplayCount
        {
            get
            {
                return _displayCount;
            }
            set
            {
                if (value == 0)
                    throw new ArgumentOutOfRangeException("value", "DisplayCount must be greater than 0");
                if (_displayCount != value)
                {
                    _displayCount = value;
                    criticalChange = true;

                    if (Resources != null && _calendarIndex + _displayCount >= Resources.Length)
                        _calendarIndex = Math.Min(0, Resources.Length - _displayCount);

                    SetDisplayControls();
                    //Do this a second time...
                    SetControlPositions();
                }

            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the control let's the user create new items.
        /// </summary>
        [DefaultValue(true)]
        [Description("Allows the user to create new items on the view")]
        public bool AllowNew
        {
            get { return _allowNew; }
            set { _allowNew = value; SetControlProperties(); }
        }

        /// <summary>
        /// Gets or sets a value indicating if the user can edit the item using the mouse or keyboard
        /// </summary>
        [DefaultValue(true)]
        [Description("Allows or denies the user the addition of items text or date ranges.")]
        public bool AllowItemEdit
        {
            get { return _allowItemEdit; }
            set { _allowItemEdit = value; SetControlProperties(); }
        }

        /// <summary>
        /// Gets or sets a value indicating if the user can edit the item using the mouse or keyboard
        /// </summary>
        [DefaultValue(true)]
        [Description("Allows or denies the user the edition of items text or date ranges.")]
        public bool AllowItemEditExternal
        {
            get { return _allowItemEditExternal; }
            set { _allowItemEditExternal = value; SetControlProperties(); }
        }

        /// <summary>
        /// Gets or sets a value indicating if calendar allows user to resize the calendar.
        /// </summary>
        [DefaultValue(true)]
        [Description("Allows or denies the user to resize items on the calendar")]
        public bool AllowItemResize
        {
            get { return _allowItemResize; }
            set { _allowItemResize = value; SetControlProperties(); }
        }

        /// <summary>
        /// Gets or Sets the height of the day top
        /// </summary>
        [DefaultValue(59)]
        public int DayTopHeight
        {
            get { return _dayTopHeight; }
            set { _dayTopHeight = value; SetControlProperties(); }
        }

        /// <summary>
        /// Gets or sets the first day of weeks
        /// </summary>
        [Description("Starting day of weeks")]
        [DefaultValue(DayOfWeek.Sunday)]
        public DayOfWeek FirstDayOfWeek
        {
            get { return _firstDayOfWeek; }
            set { _firstDayOfWeek = value; SetControlProperties(); }
        }


        /// <summary>
        /// Gets or sets the time ranges that should be highlighted as work-time.
        /// This ranges are week based.
        /// </summary>
        public CalendarHighlightRange[] HighlightRanges
        {
            get { return _highlightRanges; }
            set { _highlightRanges = value; }
        }

        public CalendarHolidayCollection Holidays
        {
            get
            {
                if (_holidays == null)
                    _holidays = new CalendarHolidayCollection();
                return _holidays;
            }
            set
            {
                _holidays = value;
                lock(ControlListLock)
                    foreach (var cal in ControlList)
                        cal.Holidays = _holidays;
            }
        }

        /// <summary>
        /// Gets the collection of items currently on the view.
        /// </summary>
        /// <remarks>
        /// This collection changes every time the view is changed
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IEnumerable<CalendarItem> Items
        {
            get
            {
                List<CalendarItem> ret = new List<CalendarItem>();
                lock(ControlListLock)
                    foreach (var item in ControlList)
                        ret.AddRange(item.Items);
                return ret;
            }
        }

        /// <summary>
        /// Gets or sets the format in which time is shown in the items, when applicable
        /// </summary>
        [DefaultValue("dd/MMM")]
        public string ItemsDateFormat
        {
            get { return _itemsDateFormat; }
            set { _itemsDateFormat = value; SetControlProperties(); }
        }

        /// <summary>
        /// Gets or sets the format in which time is shown in the items, when applicable
        /// </summary>
        [DefaultValue("hh:mm tt")]
        public string ItemsTimeFormat
        {
            get { return _itemsTimeFormat; }
            set { _itemsTimeFormat = value; SetControlProperties(); }
        }

        /// <summary>
        /// Gets or sets the maximum full days shown on the view.
        /// After this amount of days, they will be shown as short days.
        /// </summary>
        [DefaultValue(8)]
        public int MaximumFullDays
        {
            get { return _maximumFullDays; }
            set { _maximumFullDays = value; SetControlProperties(); }
        }

        /// <summary>
        /// Gets or sets the maximum amount of days supported by the view.
        /// Value must be multiple of 7
        /// </summary>
        [DefaultValue(35)]
        public int MaximumViewDays
        {
            get { return _maximumViewDays; }
            set
            {
                if (value % 7 != 0)
                {
                    throw new Exception("MaximumViewDays must be multiple of 7");
                }
                _maximumViewDays = value;
            }
        }

        /// <summary>
        /// Gets the selected Calendar
        /// </summary>
        public Calendar SelectedCalendar
        {
            get
            {
                Calendar ret = null;
                lock(ControlListLock)
                    ret = ControlList.FirstOrDefault((x) => x.Resource == _selectedResource);
                return ret;
            }
        }

        /// <summary>
        /// Gets the last selected resource item
        /// </summary>
        public CalendarResource SelectedResource
        {
            get
            {
                if (_selectedResource == null && _resources != null && _resources.Length > 0)
                    return _resources[0];
                else
                    return _selectedResource;
            }
        }

        /// <summary>
        /// Gets or sets the end date-time of the view's selection.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime SelectionEnd
        {
            get { return _selEnd; }
            set { _selEnd = value; SetControlProperties(); }
        }

        /// <summary>
        /// Gets or sets the start date-time of the view's selection.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime SelectionStart
        {
            get { return _selStart; }
            set { _selStart = value; SetControlProperties(); }
        }

        /// <summary>
        /// Gets or sets theCalendarTimeScale for visualization.
        /// </summary>
        [DefaultValue(CalendarTimeScale.ThirtyMinutes)]
        public CalendarTimeScale TimeScale
        {
            get { return _timeScale; }
            set { _timeScale = value; SetControlProperties(); }
        }

        /// <summary>
        /// Gets or sets the offset of scrolled units
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TimeUnitsOffset
        {
            get
            {
                int ret = 0;
                lock(ControlListLock)
                    if (ControlList == null || ControlList.Count == 0)
                        ret = _timeUnitsOffset;
                    else
                        ret = ControlList[0].TimeUnitsOffset;

                return ret;
            }
            set
            {
                _timeUnitsOffset = value;
                lock (ControlListLock)
                    foreach (var ctl in ControlList)
                        ctl.UpdateScrollValue(value);
            }
        }

        /// <summary>
        /// Gets or sets the end date-time of the current view.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime ViewEnd
        {
            get
            {
                if (_viewEnd < new DateTime(1900, 1, 1))
                    _viewEnd = cEndValue;
                return _viewEnd;
            }
            set
            {
                _viewEnd = value.Date.Add(new TimeSpan(23, 59, 59));
                SetControlProperties();
                SetControlPositions();
                if (ViewRangeChanged != null)
                    ViewRangeChanged(this, new CalendarEventArgs(this.ControlList[0]));
            }
        }

        /// <summary>
        /// Gets or sets the start date-time of the current view.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime ViewStart
        {
            get
            {
                if (_viewStart < new DateTime(1900, 1, 1))
                    _viewStart = cStartValue;
                return _viewStart;
            }
            set
            {
                _viewStart = value.Date;
                SetControlProperties();
                SetControlPositions();
                if (ViewRangeChanged != null)
                    ViewRangeChanged(this, new CalendarEventArgs(this.ControlList[0]));
            }
        }

        /// <summary>
        /// Gets the Availability objects that are visible on the calendar.
        /// Only applicable to Expanded days.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CalendarDayAvailability[] Availability
        {
            get { if (_availability == null) _availability = new AvailabilityGroup(); return _availability.ToArray();}// .ToList(); }
            set
            {
                if (value != null)
                {
                    _availability = AvailabilityGroup.FromArray(value);//.FromList(value);
                    _avail = value;
                }
                else
                {
                    _availability = new AvailabilityGroup();
                    _avail = null;
                }
                ExamineAvailability();
            }
        }

        /// <summary>
        /// Gets or sets the height of the resource header.
        /// </summary>
        [DefaultValue(20)]
        [Description("Gets or sets the height of the resource header.")]
        public int ResourceHeight
        {
            get
            {
                return _resourceHeight;
            }
            set
            {
                if (_resourceHeight != value)
                {
                    _resourceHeight = value;
                    SetControlPositions();
                }
            }
        }

        /// <summary>
        /// Gets or sets the font used to display the text of the resource.
        /// </summary>
        [Description("Gets or sets the font used to display the text of the resource.")]
        public Font ResourceFont
        {
            get
            {
                return _resourceFont;
            }
            set
            {
                if (_resourceFont != value)
                {
                    _resourceFont = value;
                    this.Refresh();
                }
            }
        }

        /// <summary>
        /// Gets or sets the current index of the displayed calendars (index is for the far-left calendar)
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CalendarIndex
        {
            get
            {
                return _calendarIndex;
            }
            set
            {
                if (_calendarIndex != value)
                {
                    if (value > -1 && value < CalendarCount)
                    {
                        if (value > _calendarIndex && value + DisplayCount <= CalendarCount)
                            _calendarIndex = value;
                        else if (value < _calendarIndex)
                            _calendarIndex = value;
                    }

                    criticalChange = true;
                    SetDisplayControls();
                }
            }
        }

        /// <summary>
        /// Gets the total number of calendars as set by the resources
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CalendarCount
        {

            get
            {
                if (Resources == null)
                    return 0;
                else
                    return Resources.Count();
            }
        }

        //public string ResourceMember
        //{
        //    get
        //    {
        //        return _resourceMember;
        //    }
        //    set
        //    {
        //        if (_resourceMember != value)
        //        {
        //            _resourceMember = value;
        //            ExamineDataItems();
        //        }
        //    }
        //}

        //public CalendarItemBinding ItemBindings
        //{
        //    get
        //    {
        //        return itemBinding;
        //    }
        //    set
        //    {
        //        if (itemBinding != value)
        //        {
        //            itemBinding = value;
        //            SetControlProperties();
        //        }
        //    }
        //}

        /// <summary>
        /// The last popup message that was displayed.
        /// </summary>
        private CalendarItemPopupEventArgs LastPopup
        {
            get { return _lastPopup; }
            set { _lastPopup = value; }
        }

        /// <summary>
        /// Gets or sets whether the image is shown on appointments
        /// </summary>
        [DefaultValue(false)]
        public bool ShowImageOnAppointments
        {
            get
            {
                return showImageOnAppointments;
            }
            set
            {
                showImageOnAppointments = value;
                SetControlProperties();
                SetControlPositions();
            }
        }

        /// <summary>
        /// Gets or sets whether the image is shown on reminders
        /// </summary>
        [DefaultValue(true)]
        public bool ShowImageOnReminders
        {
            get
            {
                return showImageOnReminders;
            }
            set
            {
                showImageOnReminders = value;
                SetControlProperties();
                SetControlPositions();
            }
        }

        /// <summary>
        /// Gets or sets whether reminders are forced to show as all day
        /// </summary>
        [DefaultValue(true)]
        public bool ShowRemindersAsAllDay
        {
            get
            {
                return showRemindersAsAllDay;
            }
            set
            {
                showRemindersAsAllDay = value;
                SetControlProperties();
                SetControlPositions();
            }
        }

        /// <summary>
        /// Data Source for the items.
        /// </summary>
        [DefaultValue((string)null), AttributeProvider(typeof(IListSource)), RefreshProperties(RefreshProperties.Repaint)]
        public ICalendarItem[] ReminderDataSource
        {
            get
            {
                return remindersource;
            }
            set
            {
                remindersource = value;
                if (value != null)
                    ExamineReminderDataItems();
                else
                    ReminderDataItems = null;
            }
        }


        #endregion

        #region ctor

        public MultiResourceCalendar()
        {
            this.BackColor = Color.FromArgb(227, 239, 255);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.Selectable, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            SetDisplayControls();
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Sets both the main and reminder datasources without triggering the handlers twice.
        /// </summary>
        /// <param name="_dataSource">main data source collection</param>
        /// <param name="_reminderSource">reminder data source collection</param>
        public void SetDataSources(ICalendarItem[] mainDataSource, ICalendarItem[] reminderDataSource)
        {
            remindersource = reminderDataSource;
            DataSource = mainDataSource;
        }

        /// <summary>
        /// Sets both the main and reminder datasources without triggering the handlers twice.
        /// </summary>
        /// <param name="_dataSource">main data source collection</param>
        /// <param name="_reminderSource">reminder data source collection</param>
        public void SetDataSources(IEnumerable<ICalendarItem> mainDataSource, IEnumerable<ICalendarItem> reminderDataSource)
        {
            SetDataSources(mainDataSource.ToArray(), reminderDataSource.ToArray());
        }

        /// <summary>
        /// Gets or Sets the date range that the calendars will display
        /// </summary>
        /// <param name="begin">the beginning date of the range</param>
        /// <param name="end">the ending date of the range</param>
        public void SetViewRange(DateTime begin, DateTime end)
        {
            this._viewStart = begin.Date;
            this.ViewEnd = end.Date.Add(new TimeSpan(23, 59, 59));
            //SetControlProperties();
            //SetControlPositions();
        }

        /// <summary>
        /// Refreshes all the calendars and causes their visible areas to redraw.
        /// </summary>
        public override void Refresh()
        {
            lock(ControlListLock)
                foreach (var item in ControlList)
                    item.Refresh();

            base.Refresh();
            SetControlPositions();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            SetControlPositions();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            lock(ControlListLock)
                foreach (var item in ControlList)
                {
                    item.Invalidate();
                    item.Renderer.PerformLayout();
                }
            DrawHeaderNames(e.Graphics);
        }

        /// <summary>
        /// Forces a complete reset of the displayed calendars. (Processor Intensive)
        /// </summary>
        /// <remarks>
        /// PROCESSOR INTENSIVE (SLOW)
        /// </remarks>
        public void ForceReset()
        {
            SetDisplayControls();
        }

        public int GetVisibleTimeUnits()
        {
            int ret = 0;
            lock(ControlListLock)
                if (ControlList != null && ControlList.Count > 0)
                    ret = ControlList[0].GetVisibleTimeUnits();
            return ret;
        }

        public void ReloadItems()
        {
            lock(ControlListLock)
                if(ControlList != null)
                    foreach (var cal in ControlList)
                        cal.ReloadItems();
        }

        public void RefreshData()
        {
            lock(ControlListLock)
                if (ControlList != null)
                    foreach (var cal in ControlList)
                        cal.RefreshData();
        }

        #endregion

        #region Private Members

        private void DrawHeaderNames()
        {
            Graphics e = Graphics.FromHwnd(this.Handle);
            DrawHeaderNames(e);
        }

        private void DrawHeaderNames(Graphics e)
        {
            e.FillRectangle(new SolidBrush(this.BackColor), new Rectangle(0, 0, this.Width, ResourceHeight));
            int timeOffset = 60;
            //int itemWidth = (this.Width / DisplayCount) + (timeOffset - (20 * DisplayCount));
            bool dayMode = true;
            try
            {
                lock (ControlListLock)
                    dayMode = ControlList[0].DaysMode == CalendarDaysMode.Expanded;
            }
            catch (Exception)
            {
                dayMode = (ViewEnd.Date.Subtract(ViewStart.Date).TotalDays <= MaximumFullDays - 1);
            }

            if (dayMode)
                timeOffset = 60;
            else
                timeOffset = 0;

            lock (ControlListLock)
            {
                int index = 0;
                foreach (var item in this.ControlList)
                {
                    SolidBrush backBrush;
                    SolidBrush fontBrush;
                    try
                    {
                        backBrush = new SolidBrush(item.Resource.ResourceBackColor);
                    }
                    catch (Exception)
                    {
                        backBrush = new SolidBrush(((CalendarProfessionalRenderer)item.Renderer).ColorTable.DayBorder);
                    }
                    try
                    {
                        fontBrush = new SolidBrush(item.Resource.ResourceFontColor);
                    }
                    catch (Exception)
                    {
                        fontBrush = new SolidBrush(((CalendarProfessionalRenderer)item.Renderer).ColorTable.WeekHeaderText);
                    }

                    SolidBrush bordBrush = new SolidBrush(((CalendarProfessionalRenderer)item.Renderer).ColorTable.WeekHeaderBorder);
                    Rectangle rect = new Rectangle(new Point(index == 0 ? item.Left + timeOffset : item.Left, 0), new Size(index == 0 ? item.Width - (timeOffset + 20) : item.Width - 20, ResourceHeight - 1));
                    //e.FillRectangle(backBrush, rect);
                    CalendarProfessionalRenderer.GradientRect(e, rect, Color.White, backBrush.Color);
                    e.DrawRectangle(new Pen(bordBrush), rect);
                    try
                    {
                        if (item.Resource.DataItem != null)
                        {
                            string name = item.Resource.ToString();
                            var x = e.MeasureString(name, ResourceFont);
                            PointF pt = new PointF(0, (rect.Height - x.Height) / 2);

                            if (x.Width < rect.Width)
                                pt.X = (rect.Width - x.Width) / 2;
                            pt.X += item.Left + timeOffset;

                            var sf = new StringFormat();
                            sf.Trimming = StringTrimming.EllipsisCharacter;
                            sf.Alignment = StringAlignment.Center;
                            sf.LineAlignment = StringAlignment.Center;
                            sf.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.Hide;

                            e.DrawString(name, ResourceFont, fontBrush, rect, sf);
                        }
                    }
                    catch (Exception) { }
                    index++;
                }
            }
        }

        void Calendar_LoadItems(object sender, CalendarLoadEventArgs e)
        {
            try
            {
				// It is possible that the datasource has not been set yet.
                //if (DataItems != null)
                //{
                //    e.Calendar.DataSource = DataItems[e.Calendar.Resource.ResourceID].Where((x) => x.BeginTime >= e.DateStart && x.BeginTime <= e.DateEnd).ToArray();
                //    e.Calendar.ReminderDataSource = ReminderDataItems[e.Calendar.Resource.ResourceID].Where((x) => x.BeginTime >= e.DateStart && x.BeginTime <= e.DateEnd).ToArray();
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //For some reason, you cannot say Resource ==/!= null
            //if (e.Calendar.Resource != null)
            //{
            //    e.Calendar.DataSource = DataItems[e.Calendar.Resource];
            //}
        }

        void Calendar_MouseWheeled(object sender, EventArgs e)
        {
            if (suspendScroll) return;
            Calendar c = sender as Calendar;

            int SUSPEND_PAINT = 11;
            suspendScroll = true;
            NativeMethods.SendMessage(this.Handle, SUSPEND_PAINT, (IntPtr)Convert.ToInt32(false), (IntPtr)0);
            lock (ControlListLock)
            {
                foreach (var item in ControlList)
                {
                    //SendMessage(item.Handle, SUSPEND_PAINT, false, 0);

                    if (item.DaysMode == CalendarDaysMode.Expanded)
                    {
                        item.TimeUnitsOffset = c.TimeUnitsOffset;
                    }
                    else
                    {
                        item.SetViewRange(c.ViewStart, c.ViewEnd);
                    }
                }
            }

            //foreach (var item in ControlList)
            //{ SendMessage(item.Handle, SUSPEND_PAINT, true, 0); item.Invalidate(); }
            NativeMethods.SendMessage(this.Handle, SUSPEND_PAINT, (IntPtr)Convert.ToInt32(true), (IntPtr)0);
            this.Invalidate();

            suspendScroll = false;
        }

        void Calendar_CalendarScrolled(object sender, EventArgs e)
        {
            if (CalendarScrolled != null)
                CalendarScrolled(sender as Calendar, EventArgs.Empty);
        }

        void Calendar_OpenItemExternal(object sender, CalendarItemEventArgs item)
        {
            if (OpenItemExternal != null)
                OpenItemExternal(sender, item);
        }

        void Calendar_ItemTextEditing(object sender, CalendarItemCancelEventArgs e)
        {
            if (ItemTextEditing != null)
                ItemTextEditing(sender, e);
        }

        void Calendar_ItemTextEdited(object sender, CalendarItemCancelEventArgs e)
        {
            if (ItemTextEdited != null)
                ItemTextEdited(sender, e);
        }

        void Calendar_ItemSelected(object sender, CalendarItemEventArgs e)
        {
            if (ItemSelected != null)
                ItemSelected(sender, e);
        }

        void Calendar_ItemCreating(object sender, CalendarItemCancelEventArgs e)
        {
            if (ItemCreating != null)
                ItemCreating(sender, e);
        }

        void Calendar_ItemCreated(object sender, CalendarItemCancelEventArgs e)
        {
            if (ItemCreated != null)
                ItemCreated(sender, e);
        }

        void Calendar_ItemDeleted(object sender, CalendarItemEventArgs e)
        {
            if (ItemDeleted != null)
                ItemDeleted(sender, e);
        }

        void Calendar_ItemDeleting(object sender, CalendarItemCancelEventArgs e)
        {
            if (ItemDeleting != null)
                ItemDeleting(sender, e);
        }

        void Calendar_ItemDatesChanged(object sender, CalendarItemEventArgs e)
        {
            if (ItemDatesChanged != null)
                ItemDatesChanged(sender, e);
        }

        void Calendar_ItemMouseHover(object sender, CalendarItemEventArgs e)
        {
            //var item = DataSource.FirstOrDefault((x) => x.AppointmentID == e.Item.AppointmentID);
            //if (item == null)
            //    return;

            //CalendarItemPopupEventArgs args = new CalendarItemPopupEventArgs();
            //args.Title = item.Display;
            //args.Text = item.Notes;
            //args.Show = true;
            //args.Item = e.Item;
            //args.Calendar = e.Item.Calendar;

            //LastPopup = args;
            //if (DisplayItemPopup != null)
            //    DisplayItemPopup(e.Item.Calendar, args);
        }

        void Calendar_ItemMouseHoverEnd(object sender, CalendarItemEventArgs e)
        {
            if (DisplayItemPopup != null && LastPopup != null)
            {
                LastPopup.Show = false;
                DisplayItemPopup(LastPopup.Calendar, LastPopup);
                e.Item.Calendar.HoverItem = null;
                _lastPopup = null;
            }
            if (ItemMouseHoverEnd != null)
                ItemMouseHoverEnd(sender, new CalendarItemPopupEventArgs(false, e.Item.Calendar, e.Item, "", ""));
        }

        void Calendar_ItemClick(object sender, CalendarItemEventArgs e)
        {
            if (ItemClick != null)
                ItemClick(sender, e);

            var sndr = sender as Calendar;
            if (sndr != null)
                _selectedResource = sndr.Resource;

            if (e.Item.IsDragging)
                return;

            var item = DataSource.FirstOrDefault((x) => x.AppointmentID == e.Item.AppointmentID);
            if (item == null)
                return;

            CalendarItemPopupEventArgs args = new CalendarItemPopupEventArgs();
            args.Title = string.Format("{0} - {1} : {2}", item.BeginTime.ToShortTimeString(), item.EndTime.ToShortTimeString(), item.Display);
            args.Text = item.Notes;
            args.Show = true;
            args.Item = e.Item;
            args.Calendar = e.Item.Calendar;

            LastPopup = args;
            if (DisplayItemPopup != null)
                DisplayItemPopup(e.Item.Calendar, args);
        }

        void Calendar_ItemImageClick(object sender, CalendarItemEventArgs e)
        {
            if (ItemImageClick != null)
                ItemImageClick(sender, e);
        }

        void Calendar_ItemDoubleClick(object sender, CalendarItemEventArgs e)
        {
            if (ItemDoubleClick != null)
                ItemDoubleClick(sender, e);
        }

        void Calendar_MouseClick(object sender, MouseEventArgs e)
        {
            var sndr = sender as Calendar;
            if (sndr != null)
            {
                _selectedResource = sndr.Resource;
                if (sndr.GetSelectedItems().Count() == 0)
                    if (EmptyTimeClick != null) EmptyTimeClick(sender, new CalendarEventArgs(sndr));
            }
            this.OnMouseClick(e);
        }

        void Calendar_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var sndr = sender as Calendar;
            if (sndr != null)
            {
                _selectedResource = sndr.Resource;
                if (sndr.GetSelectedItems().Count() == 0)
                    if (DoubleClick != null) DoubleClick(sender, e);
            }
        }

        void Calendar_EmptyTimeClick(object sender, CalendarEventArgs e)
        {
            var sndr = sender as Calendar;
            if (sndr != null)
                _selectedResource = sndr.Resource;

            if (EmptyTimeClick != null)
                EmptyTimeClick(sender, e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (ControlList.Count > 0)
                ControlList[0].ScrollView(e.Delta);
        }

        private void SetDisplayControls()
        {
            lock (synclockObject)
            {
                if (criticalChange)
                {
                    criticalChange = false;
                    lock (ControlListLock)
                    {
                        foreach (var item in ControlList)
                        {
                            // Unload each event handler
                            RemoveHandlers(item);

                            //Dispose of the item
                            item.Dispose();
                        }

                        // Clear the list
                        ControlList.Clear();

                        // Reset the display count if the resources changed
                        if (Resources != null && Resources.Length < DisplayCount && Resources.Length > 0)
                            _displayCount = Resources.Length;

                        CalendarResource[] tempRes = new CalendarResource[0];
                        if (Resources != null)
                            tempRes = Resources.Reverse().ToArray();

                        // Counting backward, add the calendars from right to left
                        for (int i = DisplayCount - 1; i > -1; i--)
                        {
                            // Create new items, set the properties,
                            // add the handlers, and add them to the list
                            var calendar = new Calendar();
                            ControlList.Add(calendar);
                            this.Controls.Add(calendar);
                            calendar.SetViewRange(this.ViewStart, this.ViewEnd);
                            calendar.Holidays = this.Holidays;

                            // NOW add the handlers.
                            //AddHandlers(calendar); // Actually.... wait. Do it in the next loop

                            // And set the focus to index == 0
                            if (i == 0)
                                if (this.ContainsFocus)
                                    calendar.Focus();
                        }

                        int id = 0;
                        foreach (var calendar in ControlList)
                        {
                            // Add the resources to each item
                            if (Resources != null && Resources.Length > 0)
                                if (Resources.Length == 1)
                                    calendar.Resource = Resources[0];
                                else
                                    calendar.Resource = Resources[_calendarIndex + (id)];
                            SetControlProperties(calendar, id == 0);
                            AddHandlers(calendar);
                            id++;
                        }
                    }
                }

                // Update the positions and the common properties.
                //SetControlProperties();
                SetControlPositions();
            }
        }

        private void SetControlPositions()
        {
            int timeOffset = 60;
            //int itemWidth = (this.Width / DisplayCount) + (timeOffset - (20 * DisplayCount));
            int itemWidth;
            bool dayMode = true;
            lock (ControlListLock)
            {
                try
                {
                    dayMode = ControlList[0].DaysMode == CalendarDaysMode.Expanded;
                }
                catch (Exception)
                {
                    dayMode = (ViewEnd.Date.Subtract(ViewStart.Date).TotalDays <= MaximumFullDays - 1);
                }

                if (dayMode)
                    itemWidth = ((this.Width - timeOffset) / DisplayCount);// + timeOffset;
                else
                    itemWidth = this.Width / DisplayCount;

                int id = 0;
                Console.WriteLine(string.Format("{0}", this.Width * DisplayCount));
                foreach (var calendar in ControlList)
                {
                    if (!Controls.Contains(calendar))
                        Controls.Add(calendar);
                    calendar.Height = this.Height - ResourceHeight;
                    calendar.Top = ResourceHeight;

                    //item.ViewEnd = item.ViewStart.AddDays(id + 1);
                    if (dayMode) calendar.Width = id == 0 ? itemWidth + timeOffset : itemWidth;
                    else calendar.Width = itemWidth;

                    if (id == 0) calendar.Left = 0;
                    else
                        if (dayMode)
                            calendar.Left = (itemWidth * id) + timeOffset;// - (timeOffset * id);
                        else
                            calendar.Left = (itemWidth * id) + id;

                    id++;
                    calendar.Renderer.PerformLayout(true);
                    calendar.Invalidate();
                }
            }
            this.Invalidate();
            DrawHeaderNames();
        }

        private void SetControlProperties()
        {
            int i = 0;
            lock(ControlListLock)
                foreach (var ctl in ControlList)
                    SetControlProperties(ctl, i++ == 0);
        }

        private void SetControlProperties(Calendar ctl, bool isFirst)
        {
            ctl.DayTopHeightFixed = true;
            if (ctl.AllowNew != this.AllowNew)
                ctl.AllowNew = this.AllowNew;
            if (ctl.AllowItemEdit != this.AllowItemEdit)
                ctl.AllowItemEdit = this.AllowItemEdit;
            if (ctl.AllowItemEditExternal != this.AllowItemEditExternal)
                ctl.AllowItemEditExternal = this.AllowItemEditExternal;
            if (ctl.AllowItemResize != this.AllowItemResize)
                ctl.AllowItemResize = this.AllowItemResize;
            if (ctl.MaximumDayTopHeight != this.DayTopHeight)
                ctl.MaximumDayTopHeight = this.DayTopHeight;
            if (ctl.FirstDayOfWeek != this.FirstDayOfWeek)
                ctl.FirstDayOfWeek = this.FirstDayOfWeek;
            if (ctl.ItemsDateFormat != this.ItemsDateFormat)
                ctl.ItemsDateFormat = this.ItemsDateFormat;
            if (ctl.ItemsTimeFormat != this.ItemsTimeFormat)
                ctl.ItemsTimeFormat = this.ItemsTimeFormat;
            if (ctl.MaximumFullDays != this.MaximumFullDays)
                ctl.MaximumFullDays = this.MaximumFullDays;
            if (ctl.MaximumViewDays != this.MaximumViewDays)
                ctl.MaximumViewDays = this.MaximumViewDays;
            if (ctl.SelectionStart != this.SelectionStart)
                ctl.SelectionStart = this.SelectionStart;
            if (ctl.SelectionEnd != this.SelectionEnd)
                ctl.SelectionEnd = this.SelectionEnd;
            if (ctl.TimeScale != this.TimeScale)
                ctl.TimeScale = this.TimeScale;
            if (ctl.TimeUnitsOffset != this.TimeUnitsOffset)
                ctl.TimeUnitsOffset = this.TimeUnitsOffset;
            if (ctl.ShowImageOnAppointments != this.ShowImageOnAppointments)
                ctl.ShowImageOnAppointments = this.ShowImageOnAppointments;
            if (ctl.ShowImageOnReminders != this.ShowImageOnReminders)
                ctl.ShowImageOnReminders = this.ShowImageOnReminders;
            if (ctl.ShowRemindersAsAllDay != this.ShowRemindersAsAllDay)
                ctl.ShowRemindersAsAllDay = this.ShowRemindersAsAllDay;
            if (ctl.DragImage != this.DragImage)
                ctl.DragImage = this.DragImage;
            //if (ctl.ViewStart != this.ViewStart)
            //    ctl.ViewStart = this.ViewStart;
            //if (ctl.ViewEnd != this.ViewEnd)
            //    ctl.ViewEnd = this.ViewEnd;
            if (isFirst)
                ctl.HideTimeScale = false;
            else
                ctl.HideTimeScale = true;
            ctl.SetViewRange(this._viewStart, this._viewEnd);
            if (ctl.Resource == null)
                ctl.Resource = new CalendarResource("");
            //if (_availability != null)
            //    if (ctl.Resource != null)
            //        if (_availability.ContainsKey(ctl.Resource))
            //            ctl.Availability = _availability[ctl.Resource].ToArray();
            if (_avail != null && _avail.Length > 0)
                ctl.Availability = this._avail.Where((x) => x.Resource == ctl.Resource).ToArray();

            ctl.Holidays = Holidays;
            ctl.AllowNew = false;
        }

        private void AddHandlers(Calendar c)
        {
            //Set the event handlers if they havent been already;
            if (calLoadHandler == null)
                calLoadHandler = new Calendar.CalendarLoadEventHandler(Calendar_LoadItems);
            if (calWheelHandler == null)
                calWheelHandler = new Calendar.MouseWheelEventHandler(Calendar_MouseWheeled);
            if (calOpenExternalHandler == null)
                calOpenExternalHandler += new Calendar.OpenItemExternalEventHandler(Calendar_OpenItemExternal);
            if (calItemDatesChanged == null)
                calItemDatesChanged += new Calendar.CalendarItemEventHandler(Calendar_ItemDatesChanged);
            if (calItemDeleting == null)
                calItemDeleting += new Calendar.CalendarItemCancelEventHandler(Calendar_ItemDeleting);
            if (calItemDeleted == null)
                calItemDeleted += new Calendar.CalendarItemEventHandler(Calendar_ItemDeleted);
            if (calItemCreated == null)
                calItemCreated += new Calendar.CalendarItemCancelEventHandler(Calendar_ItemCreated);
            if (calItemCreating == null)
                calItemCreating += new Calendar.CalendarItemCancelEventHandler(Calendar_ItemCreating);
            if (calItemSelected == null)
                calItemSelected += new Calendar.CalendarItemEventHandler(Calendar_ItemSelected);
            if (calItemTextEdited == null)
                calItemTextEdited += new Calendar.CalendarItemCancelEventHandler(Calendar_ItemTextEdited);
            if (calItemTextEditing == null)
                calItemTextEditing += new Calendar.CalendarItemCancelEventHandler(Calendar_ItemTextEditing);
            if (calItemHover == null)
                calItemHover += new Calendar.CalendarItemEventHandler(Calendar_ItemMouseHover);
            if (calItemMouseHoverEnd == null)
                calItemMouseHoverEnd += new Calendar.CalendarItemEventHandler(Calendar_ItemMouseHoverEnd);
            if (calItemClick == null)
                calItemClick += new Calendar.CalendarItemEventHandler(Calendar_ItemClick);
            if (calItemImageClick == null)
                calItemImageClick += new Calendar.CalendarItemEventHandler(Calendar_ItemImageClick);
            if (calItemDoubleClick == null)
                calItemDoubleClick += new Calendar.CalendarItemEventHandler(Calendar_ItemDoubleClick);
            if (calMouseClick == null)
                calMouseClick += new MouseEventHandler(Calendar_MouseClick);
            if (calMouseDoubleClick == null)
                calMouseDoubleClick = new MouseEventHandler(Calendar_MouseDoubleClick);
            if (calScrolled == null)
                calScrolled += new Calendar.MouseWheelEventHandler(Calendar_CalendarScrolled);
            if (calEmptyTimeClick == null)
                calEmptyTimeClick += new Calendar.CalendarEventHandler(Calendar_EmptyTimeClick);

            c.OpenItemExternal += this.calOpenExternalHandler;
            c.LoadItems += this.calLoadHandler;
            c.CalendarScrolled += this.calWheelHandler;
            c.ItemDatesChanged += calItemDatesChanged;
            c.ItemDeleting += this.calItemDeleting;
            c.ItemDeleted += this.calItemDeleted;
            c.ItemCreated += this.calItemCreated;
            c.ItemCreating += this.calItemCreating;
            c.ItemSelected += this.calItemSelected;
            c.ItemTextEdited += this.calItemTextEdited;
            c.ItemTextEditing += this.calItemTextEditing;
            c.ItemMouseHover += this.calItemHover;
            c.ItemMouseHoverEnd += this.calItemMouseHoverEnd;
            c.ItemClick += this.calItemClick;
            c.ItemImageClick += this.calItemImageClick;
            c.ItemDoubleClick += this.calItemDoubleClick;
            c.MouseClick += this.calMouseClick;
            c.MouseDoubleClick += this.calMouseDoubleClick;
            c.CalendarScrolled += this.calScrolled;
            c.EmptyTimeClick += this.calEmptyTimeClick;
        }

        private void RemoveHandlers(Calendar c)
        {
            c.OpenItemExternal -= this.calOpenExternalHandler;
            c.LoadItems -= this.calLoadHandler;
            c.CalendarScrolled -= this.calWheelHandler;
            c.ItemDatesChanged -= calItemDatesChanged;
            c.ItemDeleting -= this.calItemDeleting;
            c.ItemDeleted -= this.calItemDeleted;
            c.ItemCreated -= this.calItemCreated;
            c.ItemCreating -= this.calItemCreating;
            c.ItemSelected -= this.calItemSelected;
            c.ItemTextEdited -= this.calItemTextEdited;
            c.ItemTextEditing -= this.calItemTextEditing;
            c.ItemMouseHover -= this.calItemHover;
            c.ItemMouseHoverEnd -= calItemMouseHoverEnd;
            c.ItemClick -= calItemClick;
            c.ItemImageClick -= calItemImageClick;
            c.ItemDoubleClick -= calItemDoubleClick;
            c.MouseClick -= calMouseClick;
            c.MouseDoubleClick -= calMouseDoubleClick;
            c.CalendarScrolled -= calScrolled;
            c.EmptyTimeClick -= calEmptyTimeClick;
        }

        private void ExamineAvailability()
        {
            lock(ControlListLock)
                foreach (var ctl in ControlList)
                    if (_availability != null)
                        if (ctl.Resource != null)
                            if (_availability.ContainsKey(ctl.Resource))
                                ctl.Availability = _availability[ctl.Resource].ToArray();
        }

        private void ExamineDataItems()
        {
            DateTime dt1 = DateTime.Now;
            DataItems = new ResourceGroup();

            foreach (var rec in Resources)
                if (!DataItems.ContainsKey(rec.ResourceID))
                    DataItems.Add(rec.ResourceID, new List<ICalendarItem>());

            foreach (var rec in DataItems)
                rec.Value.AddRange(DataSource.Where((x) => x.ResourceID == rec.Key && (x.BeginTime.Date >= this.ViewStart || x.BeginTime.Date <= this.ViewEnd)));

            lock (ControlListLock)
                foreach (Calendar ctl in ControlList)
                    if (DataItems != null)
					    ctl.DataSource = DataItems[ctl.Resource.ResourceID].Where((x) => x.BeginTime >= ctl.ViewStart && x.BeginTime <= ctl.ViewEnd).ToArray();
        }

        private void ExamineReminderDataItems()
        {
            DateTime dt1 = DateTime.Now;
            ReminderDataItems = new ResourceGroup();
            foreach (var rec in Resources)
                if (!ReminderDataItems.ContainsKey(rec.ResourceID))
                    ReminderDataItems.Add(rec.ResourceID, new List<ICalendarItem>());

            foreach (var rec in ReminderDataItems)
                rec.Value.AddRange(ReminderDataSource.Where((x) => x.ResourceID == rec.Key && (x.BeginTime.Date >= this.ViewStart || x.BeginTime.Date <= this.ViewEnd)).ToArray());

            lock (ControlListLock)
                foreach (var ctl in ControlList)
                    if (DataItems != null)
                        ctl.ReminderDataSource = ReminderDataItems[ctl.Resource.ResourceID].Where((x) => x.BeginTime >= ctl.ViewStart && x.BeginTime <= ctl.ViewEnd).ToArray();

        }

        private static bool AreSame(CalendarResource[] r1, CalendarResource[] r2)
        {
            if (r1 == null ^ r2 == null)
                return false;
            else if (r1 == null && r2 == null)
                return true;
            else if (r1.Length != r2.Length)
                return false;
            else
            {
                for (int i = 0; i < r1.Length; i++)
                {
                    if (r1[i].ResourceID != r2[i].ResourceID ||
                        r1[i].DisplayMember != r2[i].DisplayMember)
                        return false;
                }
                return true;
            }
        }

        #endregion

        #region Subclasses

        internal class AvailabilityGroup : Dictionary<CalendarResource, List<CalendarDayAvailability>>
        {

            internal static AvailabilityGroup FromArray(CalendarDayAvailability[] lst)
            {
                AvailabilityGroup grp = new AvailabilityGroup();
                if(lst != null)
                    foreach (var item in lst)
                    {
                        CalendarResource r = item.Resource;
                        if (r == null)
                            r = new CalendarResource("");

                        if (!grp.ContainsKey(r))
                            grp.Add(r, new List<CalendarDayAvailability>());

                        grp[r].Add(item);
                    }
                return grp;
            }

            //internal static AvailabilityGroup FromList(List<CalendarDayAvailability> lst)
            //{
            //    AvailabilityGroup grp = new AvailabilityGroup();
            //    foreach (var item in lst)
            //    {
            //        CalendarResource r = item.Resource;
            //        if (r == null)
            //            r = new CalendarResource("");

            //        if (!grp.ContainsKey(r))
            //            grp.Add(r, new List<CalendarDayAvailability>());

            //        grp[r].Add(item);
            //    }
            //    return grp;
            //}

            internal CalendarDayAvailability[] ToArray()
            {
                var ret = new List<CalendarDayAvailability>();
                foreach (var item in this.Values)
                {
                    ret.AddRange(item);
                }
                return ret.ToArray();
            }

            //internal List<CalendarDayAvailability> ToList()
            //{
            //    var ret = new List<CalendarDayAvailability>();
            //    foreach (var item in this.Values)
            //    {
            //        ret.AddRange(item);
            //    }
            //    return ret;
            //}
        }

        internal class ResourceGroup : Dictionary<int, List<ICalendarItem>>
        {
        }

        #endregion

    }
}
