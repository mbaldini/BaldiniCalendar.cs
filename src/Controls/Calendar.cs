using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace Baldini.Controls.Calendar
{
    /// <summary>
    /// Hosts a calendar view where user can manage calendar items.
    /// </summary>
    [DefaultEvent("LoadItems")]
    public class Calendar
        : ScrollableControl
    {
        #region Enums

        /// <summary>
        /// Possible states of the calendar
        /// </summary>
        public enum CalendarState
        {
            /// <summary>
            /// Nothing happening
            /// </summary>
            Idle,

            /// <summary>
            /// User is currently dragging on view to select a time range
            /// </summary>
            DraggingTimeSelection,

            /// <summary>
            /// User is currently dragging an item among the view
            /// </summary>
            DraggingItem,

            /// <summary>
            /// User is editing an item's Text
            /// </summary>
            EditingItemText,

            /// <summary>
            /// User is currently resizing an item
            /// </summary>
            ResizingItem
        }

        #endregion

        #region Static

        /// <summary>
        /// Returns a value indicating if two date ranges intersect
        /// </summary>
        /// <param name="startA"></param>
        /// <param name="endA"></param>
        /// <param name="startB"></param>
        /// <param name="endB"></param>
        /// <returns></returns>
        public static bool DateIntersects(DateTime startA, DateTime endA, DateTime startB, DateTime endB)
        {
            return startB < endA && startA < endB;
        }

        #endregion

        #region Events

        /// <summary>
        /// Delegate that supportsLoadItems event
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event Data</param>
        public delegate void CalendarLoadEventHandler(object sender, CalendarLoadEventArgs e);

        /// <summary>
        /// Delegate that supports item-related events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void CalendarItemEventHandler(object sender, CalendarItemEventArgs e);

        /// <summary>
        /// Delegate that supports cancelable item-related events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void CalendarItemCancelEventHandler(object sender, CalendarItemCancelEventArgs e);

        /// <summary>
        /// Delegate that supportsCalendarDay-related events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void CalendarDayEventHandler(object sender, CalendarDayEventArgs e);

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void CalendarEventHandler(object sender, CalendarEventArgs e);

        /// <summary>
        /// Occurs when items are load into view
        /// </summary>
        [Description("Occurs when items are loaded into view")]
        public event CalendarLoadEventHandler LoadItems;

        /// <summary>
        /// Occurs when a day header is clicked
        /// </summary>
        [Description("Occurs when a day header is clicked")]
        public event CalendarDayEventHandler DayHeaderClick;

        /// <summary>
        /// Occurs when an empty cell is clicked
        /// </summary>
        [Description("Occurs when an empty cell is clicked")]
        public event CalendarEventHandler EmptyTimeClick;

        /// <summary>
        /// Occurs when an item is about to be created.
        /// </summary>
        /// <remarks>
        /// Event can be cancelled
        /// </remarks>
        [Description("Occurs when an item is about to be created.")]
        public event CalendarItemCancelEventHandler ItemCreating;

        /// <summary>
        /// Occurs when an item has been created.
        /// </summary>
        [Description("Occurs when an item has been created.")]
        public event CalendarItemCancelEventHandler ItemCreated;

        /// <summary>
        /// Occurs before an item is deleted
        /// </summary>
        [Description("Occurs before an item is deleted")]
        public event CalendarItemCancelEventHandler ItemDeleting;

        /// <summary>
        /// Occurs when an item has been deleted
        /// </summary>
        [Description("Occurs when an item has been deleted")]
        public event CalendarItemEventHandler ItemDeleted;

        /// <summary>
        /// Occurs when an item has been dragged & dropped
        /// </summary>
        [Description("Occurs when an item has been dragged & dropped out of this control")]
        public event CalendarItemEventHandler ItemDragged;

        /// <summary>
        /// Occurs when an item has been dragged & dropped
        /// </summary>
        [Description("Occurs when an item has been dragged & dropped INTO this control")]
        public event CalendarItemEventHandler ItemDropped;

        /// <summary>
        /// Occurs when an item text is about to be edited
        /// </summary>
        [Description("Occurs when an item text is about to be edited")]
        public event CalendarItemCancelEventHandler ItemTextEditing;

        /// <summary>
        /// Occurs when an item text is edited
        /// </summary>
        [Description("Occurs when an item text is edited")]
        public event CalendarItemCancelEventHandler ItemTextEdited;

        /// <summary>
        /// Occurs when an item time range has changed
        /// </summary>
        [Description("Occurs when an item time range has changed")]
        public event CalendarItemEventHandler ItemDatesChanged;

        /// <summary>
        /// Occurs when an item is clicked
        /// </summary>
        [Description("Occurs when an item is clicked")]
        public event CalendarItemEventHandler ItemClick;

        /// <summary>
        /// Occurs when an item is clicked
        /// </summary>
        [Description("Occurs when the image on an item is clicked")]
        public event CalendarItemEventHandler ItemImageClick;

        /// <summary>
        /// Occurs when an item is double-clicked
        /// </summary>
        [Description("Occurs when an item is double-clicked")]
        public event CalendarItemEventHandler ItemDoubleClick;

        /// <summary>
        /// Occurs when an item is selected
        /// </summary>
        [Description("Occurs when an item is selected")]
        public event CalendarItemEventHandler ItemSelected;

        /// <summary>
        /// Occurs after the items are positioned
        /// </summary>
        /// <remarks>
        /// Items bounds can be altered using theCalendarItem.SetBounds method.
        /// </remarks>
        [Description("Occurs after the items are positioned")]
        public event EventHandler ItemsPositioned;

        /// <summary>
        /// Occurs when the mouse is moved over an item
        /// </summary>
        [Description("Occurs when the mouse is moved over an item")]
        public event CalendarItemEventHandler ItemMouseHover;

        /// <summary>
        /// Occurs when the mouse is moved off an item after hovering
        /// </summary>
        [Description("Occurs when the mouse is moved off an item after hovering")]
        public event CalendarItemEventHandler ItemMouseHoverEnd;

        /// <summary>
        /// Occurs when the mouse wheel is turned
        /// </summary>
        [Description("Occurs when the mouse wheel is turned")]
        public event MouseWheelEventHandler CalendarScrolled;

        /// <summary>
        /// Occurs when the user opens the item in an external editor
        /// </summary>
        [Description("Occurs when the user opens the item in an external editor")]
        public event OpenItemExternalEventHandler OpenItemExternal;

        /// <summary>
        /// Occurs when the VIew Range changes
        /// </summary>
        [Description("Occurs when the View Range changes")]
        public event CalendarEventHandler ViewRangeChanged;

        #endregion

        #region Variables
        private string DRAGDROP_FORMAT = "Baldini.Controls.Calendar.CalendarItem";
        internal CalendarItem HoverItem;
        public delegate void MouseWheelEventHandler(object sender, EventArgs e);
        public delegate void OpenItemExternalEventHandler(object sender, CalendarItemEventArgs e);
        private CalendarTextBox _textBox;
        //private Dictionary<DateTime, Image> _dayBackgrounds;
        private bool _allowNew;
        private bool _allowItemEdit;
        private bool _allowItemEditExternal;
        private bool _allowItemResize;
        private bool _creatingItem;
        private CalendarDay[] _days;
        private CalendarResource _resource;
        private CalendarDaysMode _daysMode;
        private bool _dayTopFixed;
        private CalendarItem _editModeItem;
        private bool _finalizingEdition;
        private DayOfWeek _firstDayOfWeek;
        private bool hideTimeScale;
        private CalendarHighlightRange[] _highlightRanges;
		private bool isCreated = false;
        private CalendarItemCollection _items;
        private string _itemsDateFormat;
        private string _itemsTimeFormat;
        private int _maximumDayTopHeight;
        private int _maximumFullDays;
        private int _maximumViewDays;
        private CalendarRenderer _renderer;
        private DateTime _selEnd;
        private DateTime _selStart;
        private CalendarState _state;
        private CalendarTimeScale _timeScale;
        private int _timeUnitsOffset;
        private DateTime _viewEnd;
        private DateTime _viewStart;
        private CalendarWeek[] _weeks;
        private List<CalendarSelectableElement> _selectedElements;
        private ICalendarSelectableElement _selectedElementEnd;
        private ICalendarSelectableElement _selectedElementStart;
        private Rectangle _selectedElementSquare;
        private CalendarItem itemOnState;
        private bool itemOnStateChanged;
        private CalendarDayAvailability[] _availability;
        //private CalendarItemBinding _itemBinding = new CalendarItemBinding();
        private ICalendarItem[] datasource;
        private ICalendarItem[] remindersource;
        private bool showImageOnReminders = true;
        private bool showImageOnAppointments = false;
        private bool showRemindersAsAllDay = true;
        private MonthView monthView = null;

        private EventHandler MonthViewUpdatedEventHandler;
        private ListChangedEventHandler DatasetChanged;
        //private EventHandler BindingCurrentChanged;

        private List<CalendarItem> DroppedItems;
        private Point mouseDownPoint;
        //private DateTime lastDragScrollTime = DateTime.Now;
        #endregion

        #region Ctor

        /// <summary>
        /// Creates a newCalendar control
        /// </summary>
        public Calendar()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();

            DoubleBuffered = true;

            _selectedElements = new List<CalendarSelectableElement>();
            _items = new CalendarItemCollection(this);
            _renderer = new CalendarProfessionalRenderer(this);
            _maximumFullDays = 8;
            _maximumViewDays = 35;

            Holidays = new CalendarHolidayCollection();

            HighlightRanges = new CalendarHighlightRange[] { };
            // Sample datas
            //    new CalendarHighlightRange( DayOfWeek.Monday, new TimeSpan(8,0,0), new TimeSpan(17,0,0)),
            //    new CalendarHighlightRange( DayOfWeek.Tuesday, new TimeSpan(8,0,0), new TimeSpan(17,0,0)),
            //    new CalendarHighlightRange( DayOfWeek.Wednesday, new TimeSpan(8,0,0), new TimeSpan(17,0,0)),
            //    new CalendarHighlightRange( DayOfWeek.Thursday, new TimeSpan(8,0,0), new TimeSpan(17,0,0)),
            //    new CalendarHighlightRange( DayOfWeek.Friday, new TimeSpan(8,0,0), new TimeSpan(17,0,0)),
            //};

            _timeScale = CalendarTimeScale.ThirtyMinutes;
            SetViewRange(DateTime.Now, DateTime.Now.AddDays(2));


            _itemsDateFormat = "dd/MMM";
            _itemsTimeFormat = "hh:mm tt";
            _allowItemEdit = true;
            _allowNew = true;
            _allowItemResize = true;
            DroppedItems = new List<CalendarItem>();
			isCreated = true;
            PerformLayout();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating if the control let's the user create new items.
        /// </summary>
        [DefaultValue(true)]
        [Description("Allows the user to create new items on the view")]
        public bool AllowNew
        {
            get { return _allowNew; }
            set { _allowNew = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if the user can edit the item in an external editor
        /// </summary>
        [DefaultValue(true)]
        [Description("Allows or denies the user the edition of items text or date ranges.")]
        public bool AllowItemEdit
        {
            get { return _allowItemEdit; }
            set { _allowItemEdit = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if the user can edit the item using the mouse or keyboard
        /// </summary>
        [DefaultValue(true)]
        [Description("Allows or denies the user the edition of items text or date ranges.")]
        public bool AllowItemEditExternal
        {
            get { return _allowItemEditExternal; }
            set { _allowItemEditExternal = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if calendar allows user to resize the calendar.
        /// </summary>
        [DefaultValue(true)]
        [Description("Allows or denies the user to resize items on the calendar")]
        public bool AllowItemResize
        {
            get { return _allowItemResize; }
            set { _allowItemResize = value; }
        }

        /// <summary>
        /// Gets the days visible on the current view
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CalendarDay[] Days
        {
            get { return _days; }
        }

        /// <summary>
        /// Gets the resources visible on the current view
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CalendarResource Resource
        {
            get { return _resource; }
            set { _resource = value; }
        }

        /// <summary>
        /// Gets or sets whether the day top is fixed
        /// </summary>
        [DefaultValue(false)]
        public bool DayTopHeightFixed
        {
            get { return _dayTopFixed; }
            set { _dayTopFixed = value; }
        }

        /// <summary>
        /// Gets the mode in which days are drawn.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CalendarDaysMode DaysMode
        {
            get { return _daysMode; }
        }

        /// <summary>
        /// Gets the union of day body rectangles
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle DaysBodyRectangle
        {
            get
            {
                Rectangle first = Days[0].BodyBounds;
                Rectangle last = Days[Days.Length - 1].BodyBounds;

                return Rectangle.Union(first, last);
            }
        }

        /// <summary>
        /// Gets or Sets the Image that is shown when dragging an item out of the calendar
        /// </summary>
        [DefaultValue(null)]
        public Image DragImage { get; set; }

        /// <summary>
        /// Gets if the calendar is currently in edit mode of some item
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool EditMode
        {
            get { return TextBox != null; }
        }

        /// <summary>
        /// Gets the item being edited (if any)
        /// </summary>
        public CalendarItem EditModeItem
        {
            get
            {
                return _editModeItem;
            }
        }

        /// <summary>
        /// Gets or sets the first day of weeks
        /// </summary>
        [Description("Starting day of weeks")]
        [DefaultValue(DayOfWeek.Sunday)]
        public DayOfWeek FirstDayOfWeek
        {
            set { _firstDayOfWeek = value; }
            get { return _firstDayOfWeek; }
        }

        /// <summary>
        /// Option to hide the Time Scale on Expanded View modes
        /// </summary>
        [Description("Option to hide the Time Scale on Expanded View modes")]
        [DefaultValue(false)]
        public bool HideTimeScale
        {
            get { return hideTimeScale; }
            set
            {
                if (hideTimeScale != value)
                {
                    hideTimeScale = value;
                    RefreshData();
                }
            }
        }

        /// <summary>
        /// Gets or sets the time ranges that should be highlighted as work-time.
        /// This ranges are week based.
        /// </summary>
        public CalendarHighlightRange[] HighlightRanges
        {
            get { return _highlightRanges; }
            set { _highlightRanges = value; UpdateHighlights(); }
        }

        /// <summary>
        /// Gets or sets the holidays that will be shown on the calendar.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CalendarHolidayCollection Holidays { get; set; }

        /// <summary>
        /// Gets the collection of items currently on the view.
        /// </summary>
        /// <remarks>
        /// This collection changes every time the view is changed
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CalendarItemCollection Items
        {
            get
            {
                return _items;
            }
        }

        /// <summary>
        /// Gets or sets the format in which time is shown in the items, when applicable
        /// </summary>
        [DefaultValue("dd/MMM")]
        public string ItemsDateFormat
        {
            get { return _itemsDateFormat; }
            set { _itemsDateFormat = value; }
        }

        /// <summary>
        /// Gets or sets the format in which time is shown in the items, when applicable
        /// </summary>
        [DefaultValue("hh:mm tt")]
        public string ItemsTimeFormat
        {
            get { return _itemsTimeFormat; }
            set { _itemsTimeFormat = value; }
        }

        /// <summary>
        /// Gets or sets the maximum height of the Day Top
        /// </summary>
        [DefaultValue(29)]
        public int MaximumDayTopHeight
        {
            get { return _maximumDayTopHeight; }
            set { _maximumDayTopHeight = value; }
        }

        /// <summary>
        /// Gets or sets the maximum full days shown on the view.
        /// After this amount of days, they will be shown as short days.
        /// </summary>
        [DefaultValue(8)]
        public int MaximumFullDays
        {
            get { return _maximumFullDays; }
            set { _maximumFullDays = value; }
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
        /// Gets or sets the MonthView item that controls the View Range of this control
        /// </summary>
        [DefaultValue(null)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public MonthView MonthView 
        {
            get { return monthView; }
            set 
            {
                if (monthView != value)
                {
                    // Setup the event handler if it is not initialized
                    if (MonthViewUpdatedEventHandler == null)
                        MonthViewUpdatedEventHandler = new EventHandler(UpdateDateRangeFromMonthView);
                    
                    // if the source variable for this property has a value, be sure to remove the event handler
                    if (monthView != null)
                        monthView.SelectionChanged -= MonthViewUpdatedEventHandler;

                    // update the source variable to the set value
                    monthView = value;

                    // if the source variable has a value, be sure to add the event handler to the proper event on the monthView
                    if (monthView != null)
                        monthView.SelectionChanged += MonthViewUpdatedEventHandler;
                }
            }
        }

        /// <summary>
        /// Gets or sets the CalendarRenderer of the Calendar
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public CalendarRenderer Renderer
        {
            get { return _renderer; }
            set
            {
                _renderer = value;

                if (value != null && Created)
                {
                    value.OnInitialize(new CalendarRendererEventArgs(null, null, Rectangle.Empty));
                }
            }
        }

        /// <summary>
        /// Gets the last selected element
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ICalendarSelectableElement SelectedElementEnd
        {
            get { return _selectedElementEnd; }
            set
            {
                _selectedElementEnd = value;

                UpdateSelectionElements();
            }
        }

        /// <summary>
        /// Gets the first selected element
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ICalendarSelectableElement SelectedElementStart
        {
            get { return _selectedElementStart; }
            set
            {
                _selectedElementStart = value;

                UpdateSelectionElements();
            }
        }

        /// <summary>
        /// Gets or sets the end date-time of the view's selection.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime SelectionEnd
        {
            get { return _selEnd; }
            set { _selEnd = value; }
        }

        /// <summary>
        /// Gets or sets the start date-time of the view's selection.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime SelectionStart
        {
            get { return _selStart; }
            set { _selStart = value; }
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
            }
        }

        /// <summary>
        /// Gets or sets the state of the calendar
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CalendarState State
        {
            get { return _state; }
        }

        /// <summary>
        /// Gets the TextBox of the edit mode
        /// </summary>
        internal CalendarTextBox TextBox
        {
            get { return _textBox; }
            set { _textBox = value; }
        }

        /// <summary>
        /// Gets or sets theCalendarTimeScale for visualization.
        /// </summary>
        [DefaultValue(CalendarTimeScale.ThirtyMinutes)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CalendarTimeScale TimeScale
        {
            get
            {
                if ((int)_timeScale == 0)
                    _timeScale = CalendarTimeScale.SixtyMinutes;
                return _timeScale;
            }
            set
            {
                _timeScale = value;

                if (Days != null)
                {
                    for (int i = 0; i < Days.Length; i++)
                    {
                        Days[i].UpdateUnits();
                    }
                }

                Renderer.PerformLayout();
                Refresh();
                ScrollTimeUnits(10);
            }
        }

        /// <summary>
        /// Gets or sets the offset of scrolled units
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TimeUnitsOffset
        {
            get { return _timeUnitsOffset; }
            set
            {
                if (_timeUnitsOffset != value)
                {
                    UpdateScrollValue(value);
                    if (CalendarScrolled != null)
                        CalendarScrolled(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the end date-time of the current view.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime ViewEnd
        {
            get { return _viewEnd; }
            set
            {
                if((int)value.Date.Subtract(_viewStart).TotalDays > MaximumViewDays)
                    value = _viewStart.AddDays(MaximumViewDays).Add(new TimeSpan(23, 59, 59));

                if (_viewEnd != value.Date.Add(new TimeSpan(23, 59, 59)))
                {
                    _viewEnd = value.Date.Add(new TimeSpan(23, 59, 59));
                    RefreshData();

                    if (CalendarScrolled != null)
                        CalendarScrolled(this, EventArgs.Empty);

                    if (ViewRangeChanged != null)
                        ViewRangeChanged(this, new CalendarEventArgs(this));
                }

            }
        }

        /// <summary>
        /// Gets or sets the start date-time of the current view.
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
        public DateTime ViewStart
        {
            get { return _viewStart; }
            set
            {
                if ((int)_viewEnd.Date.Subtract(value).TotalDays > MaximumViewDays)
                    _viewEnd = value.AddDays(MaximumViewDays).Add(new TimeSpan(23, 59, 59));

                _viewStart = value.Date;
                RefreshData();

                if (CalendarScrolled != null)
                    CalendarScrolled(this, EventArgs.Empty);

                if (ViewRangeChanged != null)
                    ViewRangeChanged(this, new CalendarEventArgs(this));
            }
        }

        /// <summary>
        /// Gets the weeks currently visible on the calendar, ifDaysMode isCalendarDaysMode.Short
        /// </summary>
        public CalendarWeek[] Weeks
        {
            get { return _weeks; }
        }

        /// <summary>
        /// Gets the Availability objects that are visible on the calendar.
        /// Only applicable to Expanded days.
        /// </summary>
        public CalendarDayAvailability[] Availability
        {
            get { return _availability; }
            set { if (value != null) { _availability = new CalendarDayAvailability[value.Length]; Array.Copy(value, _availability, value.Length); } else _availability = null; }
        }

        /// <summary>
        /// Data Source for the items.
        /// </summary>
        [DefaultValue((string)null), AttributeProvider(typeof(IListSource)), RefreshProperties(RefreshProperties.Repaint)]
        public ICalendarItem[] DataSource
        {
            get
            {
                return datasource;
            }
            set
            {
                if (DatasetChanged == null)
                    DatasetChanged = new ListChangedEventHandler(Calendar_ListChanged);
                datasource = value;
                RefreshData();
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
                if (DatasetChanged == null)
                    DatasetChanged = new ListChangedEventHandler(Calendar_ListChanged);
                remindersource = value;
                RefreshData();
            }
        }

        /// <summary>
        /// EventHandler triggered by the internal datasource list changing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Calendar_ListChanged(object sender, ListChangedEventArgs e)
        {
            RefreshData();
        }

        ///// <summary>
        ///// EventHandler triggered by the remindersource list being updated
        ///// </summary>
        //void remindersource_CalendarItemCollectionChanged()
        //{
        //    RefreshData();
        //}

        ///// <summary>
        ///// EventHandler triggered by the datasource list being updated
        ///// </summary>
        //void datasource_CalendarItemCollectionChanged()
        //{
        //    RefreshData();
        //}

        #endregion

        #region Public Methods

        /// <summary>
        /// Activates the edit mode on the first selected item
        /// </summary>
        /// <param name="item"></param>
        public void ActivateEditMode()
        {
            foreach (CalendarItem item in Items)
            {
                if (item.Selected)
                {
                    ActivateEditMode(item);
                    return;
                }
            }
        }

        /// <summary>
        /// Activates the edit mode on the specified item
        /// </summary>
        /// <param name="item"></param>
        public void ActivateEditMode(CalendarItem item)
        {
            CalendarItemCancelEventArgs evt = new CalendarItemCancelEventArgs(item);

            if (!_creatingItem)
            {
                OnItemEditing(evt);
            }

            if (evt.Cancel)
            {
                return;
            }

            _editModeItem = item;
            TextBox = new CalendarTextBox(this);
            TextBox.KeyDown += new KeyEventHandler(TextBox_KeyDown);
            TextBox.LostFocus += new EventHandler(TextBox_LostFocus);
            Rectangle r = item.Bounds;
            r.Inflate(-2, -2);
            TextBox.Bounds = r;
            TextBox.BorderStyle = BorderStyle.None;

            TextBox.Text = GetItemByID(item.AppointmentID).EditString;
            TextBox.Multiline = true;
            Controls.Add(TextBox);
            TextBox.Visible = true;
            TextBox.Focus();
            TextBox.SelectionStart = TextBox.Text.Length;

            SetState(CalendarState.EditingItemText);
        }

        /// <summary>
        /// Creates a new item on the current selection.
        /// If there's no selection, this will be ignored.
        /// </summary>
        /// <param name="itemText">Text of the item</param>
        /// <param name="editMode">If <c>true</c> activates the edit mode so user can edit the text of the item.</param>
        public void CreateItemOnSelection(string itemText, bool editMode)
        {
            if (SelectedElementEnd == null || SelectedElementStart == null) return;

            CalendarTimeScaleUnit unitEnd = SelectedElementEnd as CalendarTimeScaleUnit;
            CalendarDayTop dayTop = SelectedElementEnd as CalendarDayTop;
            CalendarDay day = SelectedElementEnd as CalendarDay;
            TimeSpan duration = unitEnd != null ? unitEnd.Duration : new TimeSpan(23, 59, 59);
            CalendarItem item = new CalendarItem(this);

            DateTime dstart = SelectedElementStart.Date;
            DateTime dend = SelectedElementEnd.Date;

            if (dend.CompareTo(dstart) < 0)
            {
                DateTime dtmp = dend;
                dend = dstart;
                dstart = dtmp;
            }

            item.StartDate = dstart;
            item.EndDate = dend.Add(duration);
            item.Text = itemText;

            CalendarItemCancelEventArgs evtA = new CalendarItemCancelEventArgs(item);

            OnItemCreating(evtA);

            if (!evtA.Cancel)
            {
                Items.Add(item);

                if (editMode)
                {
                    _creatingItem = true;
                    ActivateEditMode(item);
                }
            }


        }

        /// <summary>
        /// Ensures the scrolling shows the specified time unit. It doesn't affect View date ranges.
        /// </summary>
        /// <param name="unit">Unit to ensure visibility</param>
        public void EnsureVisible(CalendarTimeScaleUnit unit)
        {
            if (Days == null || Days.Length == 0) return;

            Rectangle view = Days[0].BodyBounds;

            if (unit.Bounds.Bottom > view.Bottom)
            {
                TimeUnitsOffset = -Convert.ToInt32(Math.Ceiling(unit.Date.TimeOfDay.TotalMinutes / (double)TimeScale))
                     + Renderer.GetVisibleTimeUnits();
            }
            else if (unit.Bounds.Top < view.Top)
            {
                TimeUnitsOffset = -Convert.ToInt32(Math.Ceiling(unit.Date.TimeOfDay.TotalMinutes / (double)TimeScale));
            }
        }

        /// <summary>
        /// Finalizes editing theEditModeItem.
        /// </summary>
        /// <param name="cancel">Value indicating if edition of item should be canceled.</param>
        public void FinalizeEditMode(bool cancel)
        {
            if (!EditMode || EditModeItem == null || _finalizingEdition) return;

            _finalizingEdition = true;

            string cancelText = GetItemByID(_editModeItem.AppointmentID).EditString;
            CalendarItem itemBuffer = _editModeItem;
            _editModeItem = null;
            CalendarItemCancelEventArgs evt = new CalendarItemCancelEventArgs(itemBuffer);

            if(!cancel)
                itemBuffer.Text = TextBox.Text.Trim();

            if (TextBox != null)
            {
                TextBox.Visible = false;
                Controls.Remove(TextBox);
                TextBox.Dispose();
            }

            if(_editModeItem != null)
                Invalidate(itemBuffer);

            _textBox = null;

            if (_creatingItem)
            {
                OnItemCreated(evt);
            }
            else
            {
                OnItemEdited(evt);
            }

            if (evt.Cancel)
            {
                itemBuffer.Text = cancelText;
            }


            _creatingItem = false;
            _finalizingEdition = false;

            if (State == CalendarState.EditingItemText)
            {
                SetState(CalendarState.Idle);
            }
        }

        /// <summary>
        /// Finds theCalendarDay for the specified date, if in the view.
        /// </summary>
        /// <param name="d">Date to find day</param>
        /// <returns><see cref="CalendarDay object that matches the date, <c>null</c> if day was not found.</returns>
        public CalendarDay FindDay(DateTime d)
        {
            if (Days == null) return null;

            for (int i = 0; i < Days.Length; i++)
            {
                if (Days[i].Date.Date.Equals(d.Date.Date))
                {
                    return Days[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the items that are currently selected
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CalendarItem> GetSelectedItems()
        {
            List<CalendarItem> items = new List<CalendarItem>();

            foreach (CalendarItem item in Items)
            {
                if (item.Selected)
                {
                    items.Add(item);
                }
            }

            return items;
        }

        /// <summary>
        /// Gets the time unit that starts with the specified date
        /// </summary>
        /// <param name="dateTime">Date and time of the unit you want to extract</param>
        /// <returns>Matching time unit. <c>null</c> If out of range.</returns>
        public CalendarTimeScaleUnit GetTimeUnit(DateTime d)
        {
            if (Days != null)
            {
                foreach (CalendarDay day in Days)
                {
                    if (day.Date.Equals(d.Date))
                    {
                        double duration = Convert.ToDouble((int)TimeScale);
                        int index =
                            Convert.ToInt32(
                                Math.Floor(
                                    d.TimeOfDay.TotalMinutes / duration
                                )
                            );

                        return day.TimeUnits[index];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Searches for the first hittedICalendarSelectableElement
        /// </summary>
        /// <param name="p">Point to check for hit test</param>
        /// <returns></returns>
        public ICalendarSelectableElement HitTest(Point p)
        {
            return HitTest(p, false);
        }

        /// <summary>
        /// Searches for the first hittedICalendarSelectableElement
        /// </summary>
        /// <param name="p">Point to check for hit test</param>
        /// <returns></returns>
        public ICalendarSelectableElement HitTest(Point p, bool ignoreItems)
        {
            if(!ignoreItems)
                foreach (CalendarItem item in Items)
                {
                    foreach (Rectangle r in item.GetAllBounds())
                    {
                        if (r.Contains(p))
                        {
                            return item;
                        }
                    }
                }

            for (int i = 0; i < Days.Length; i++)
            {
                if (Days[i].Bounds.Contains(p))
                {
                    if (DaysMode == CalendarDaysMode.Expanded)
                    {
                        if (Days[i].DayTop.Bounds.Contains(p))
                        {
                            return Days[i].DayTop;
                        }
                        else
                        {
                            for (int j = 0; j < Days[i].TimeUnits.Length; j++)
                            {
                                if (Days[i].TimeUnits[j].Visible &&
                                    Days[i].TimeUnits[j].Bounds.Contains(p))
                                {
                                    return Days[i].TimeUnits[j];
                                }
                            }
                        }

                        return Days[i];
                    }
                    else if (DaysMode == CalendarDaysMode.Short)
                    {
                        return Days[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the item hit at the specified location. Null if no item hit.
        /// </summary>
        /// <param name="p">Location to serach for items</param>
        /// <returns>Hit item at the location. Null if no item hit.</returns>
        public CalendarItem ItemAt(Point p)
        {
            return HitTest(p) as CalendarItem;
        }

        /// <summary>
        /// Invalidates the bounds of the specified day
        /// </summary>
        /// <param name="day"></param>
        public void Invalidate(CalendarDay day)
        {
            Invalidate(day.Bounds);
        }

        /// <summary>
        /// Ivalidates the bounds of the specified unit
        /// </summary>
        /// <param name="unit"></param>
        public void Invalidate(CalendarTimeScaleUnit unit)
        {
            Invalidate(unit.Bounds);
        }

        /// <summary>
        /// Invalidates the area of the specified item
        /// </summary>
        /// <param name="item"></param>
        public void Invalidate(CalendarItem item)
        {
            Rectangle r = item.Bounds;

            foreach (Rectangle bounds in item.GetAllBounds())
            {
                r = Rectangle.Union(r, bounds);
            }

            r.Inflate(Renderer.ItemShadowPadding + Renderer.ItemInvalidateMargin, Renderer.ItemShadowPadding + Renderer.ItemInvalidateMargin);
            Invalidate(r);
        }

        /// <summary>
        /// Establishes the selection range with only one graphical update.
        /// </summary>
        /// <param name="selectionStart">Fisrt selected element</param>
        /// <param name="selectionEnd">Last selection element</param>
        public void SetSelectionRange(ICalendarSelectableElement selectionStart, ICalendarSelectableElement selectionEnd)
        {
            _selectedElementStart = selectionStart;
            SelectedElementEnd = selectionEnd;
        }

        /// <summary>
        /// Sets the value ofViewStart andViewEnd properties
        /// triggering only one repaint process
        /// </summary>
        /// <param name="dateStart">Start date of view</param>
        /// <param name="dateEnd">End date of view</param>
        public void SetViewRange(DateTime dateStart, DateTime dateEnd)
        {
            _viewStart = dateStart.Date;
            ViewEnd = dateEnd;
            //ReloadItems();
        }

        /// <summary>
        /// Updates TimeUnitsOffset and instructs the renderer to
        /// run PerformLayout, then invalidates the control to repaint.
        /// </summary>
        /// <param name="offset"></param>
        public void UpdateScrollValue(int offset)
        {
            _timeUnitsOffset = offset;
            Renderer.PerformLayout();
            Invalidate();
        }

        /// <summary>
        /// Returns a value indicating if the view range intersects the specified date range.
        /// </summary>
        /// <param name="dateStart"></param>
        /// <param name="dateEnd"></param>
        public bool ViewIntersects(DateTime dateStart, DateTime dateEnd)
        {
            return DateIntersects(ViewStart, ViewEnd, dateStart, dateEnd);
        }

        /// <summary>
        /// Returns a value indicating if the view range intersect the date range of the specified item
        /// </summary>
        /// <param name="item"></param>
        public bool ViewIntersects(CalendarItem item)
        {
            return ViewIntersects(item.StartDate, item.EndDate);
        }

        /// <summary>
        /// Scrolls the calendar by the specified number of items
        /// </summary>
        /// <param name="delta">Number of items to scroll</param>
        public void ScrollView(int delta)
        {
            if (DaysMode == CalendarDaysMode.Expanded)
            {
                ScrollTimeUnits(delta);
            }
            else if (DaysMode == CalendarDaysMode.Short)
            {
                ScrollCalendar(delta);
            }
        }

        /// <summary>
        /// Sets both the main and reminder datasources without triggering the handlers twice.
        /// </summary>
        /// <param name="_dataSource">main data source collection</param>
        /// <param name="_reminderSource">reminder data source collection</param>
        public void SetDataSources(ICalendarItem[] _dataSource, ICalendarItem[] _reminderSource)
        {
            if (DatasetChanged == null)
                DatasetChanged = new ListChangedEventHandler(Calendar_ListChanged);
            this.datasource = _dataSource;
            this.remindersource = _reminderSource;
            RefreshData();
        }

        /// <summary>
        /// Sets both the main and reminder datasources without triggering the handlers twice.
        /// </summary>
        /// <param name="_dataSource">main data source collection</param>
        /// <param name="_reminderSource">reminder data source collection</param>
        public void SetDataSources(IEnumerable<ICalendarItem> _dataSource, IEnumerable<ICalendarItem> _reminderSource)
        {
            SetDataSources((ICalendarItem[])_dataSource, (ICalendarItem[])_reminderSource);
        }

        /// <summary>
        /// Returns the number of Time Units that are visible on the calendar (when in Full Day mode)
        /// </summary>
        /// <returns></returns>
        public int GetVisibleTimeUnits()
        {
            return Renderer.GetVisibleTimeUnits();
        }

        /// <summary>
        /// Gets the calendar item whose data item has the specified ID
        /// </summary>
        /// <param name="id">ID of the data item to find</param>
        /// <returns></returns>
        public CalendarItem GetCalendarItemByID(int id)
        {
            CalendarItem ret = null;
            foreach (var item in Items)
            {
                if (item.AppointmentID == id)
                {
                    ret = item;
                    break;
                }
            }
            return ret;
        }

        /// <summary>
        /// Causes the calendar to re-evaluate the data to find any changes and update the view
        /// </summary>
        public void RefreshData()
        {
            UpdateDaysAndWeeks();
            if(isCreated) EvaluateDataSource();
            Renderer.PerformLayout();
            Invalidate();
            //ReloadItems();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns whether or not the key pressed is considered an InputKey
        /// Added Up, Down, Left, and Right in addition to the default keys
        /// </summary>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool IsInputKey(Keys keyData)
        {
            if (
                keyData == Keys.Down ||
                keyData == Keys.Up ||
                keyData == Keys.Right ||
                keyData == Keys.Left)
            {
                return true;
            }
            else
            {

                return base.IsInputKey(keyData);
            }
        }

        /// <summary>
        /// Removes all the items currently on the calendar
        /// </summary>
        private void ClearItems()
        {
            Items.Clear();
            if (this.DayTopHeightFixed)
                Renderer.DayTopHeight = this.MaximumDayTopHeight;
            else
                Renderer.DayTopHeight = Renderer.DayTopMinHeight;
        }

        /// <summary>
        /// Unselects the selected items
        /// </summary>
        private void ClearSelectedItems()
        {
            Rectangle r = Rectangle.Empty;

            foreach (CalendarItem item in Items)
            {
                if (item.Selected)
                {
                    if (r.IsEmpty)
                    {
                        r = item.Bounds;
                    }
                    else
                    {
                        r = Rectangle.Union(r, item.Bounds);
                    }
                }

                item.SetSelected(false);
            }

            Invalidate(r);
        }

        /// <summary>
        /// Deletes the currently selected item
        /// </summary>
        private void DeleteSelectedItems()
        {
            Stack<CalendarItem> toDelete = new Stack<CalendarItem>();

            foreach (CalendarItem item in Items)
            {
                if (item.Selected)
                {
                    CalendarItemCancelEventArgs evt = new CalendarItemCancelEventArgs(item);

                    OnItemDeleting(evt);

                    if (!evt.Cancel)
                    {
                        toDelete.Push(item);
                    }
                }
            }

            if (toDelete.Count  > 0)
            {
                while (toDelete.Count > 0)
                {
                    CalendarItem item = toDelete.Pop();

                    Items.Remove(item);

                    OnItemDeleted(new CalendarItemEventArgs(item));
                }

                Renderer.PerformItemsLayout();
            }
        }

        /// <summary>
        /// Clears current items and reloads for specified view
        /// </summary>
        public void ReloadItems()
        {
            OnLoadItems(new CalendarLoadEventArgs(this, ViewStart, ViewEnd));
        }

        /// <summary>
        /// Grows the rectangle to repaint currently selected elements
        /// </summary>
        /// <param name="rect"></param>
        private void GrowSquare(Rectangle rect)
        {
            if (_selectedElementSquare.IsEmpty)
            {
                _selectedElementSquare = rect;
            }
            else
            {
                _selectedElementSquare = Rectangle.Union(_selectedElementSquare, rect);
            }
        }

        /// <summary>
        /// Clears selection of currently selected components (As quick as possible)
        /// </summary>
        private void ClearSelectedComponents()
        {
            foreach (CalendarSelectableElement element in _selectedElements)
            {
                element.SetSelected(false);
            }

            _selectedElements.Clear();

            Invalidate(_selectedElementSquare);
            _selectedElementSquare = Rectangle.Empty;

        }

        /// <summary>
        /// Scrolls the calendar using the specified delta
        /// </summary>
        /// <param name="delta"></param>
        private void ScrollCalendar(int delta)
        {
            if (delta < 0)
            {
                SetViewRange(ViewStart.AddDays(7), ViewEnd.AddDays(7));
            }
            else
            {
                SetViewRange(ViewStart.AddDays(-7), ViewEnd.AddDays(-7));
            }
        }

        /// <summary>
        /// Raises theItemsPositioned event
        /// </summary>
        internal void RaiseItemsPositioned()
        {
            OnItemsPositioned(EventArgs.Empty);
        }

        /// <summary>
        /// Scrolls the time units using the specified delta
        /// </summary>
        /// <param name="delta"></param>
        private void ScrollTimeUnits(int delta)
        {
            int possible = TimeUnitsOffset;
            int visible = Renderer.GetVisibleTimeUnits();

            if (delta < 0)
            {
                possible--;
            }
            else
            {
                possible++;
            }

            if (possible > 0)
            {
                possible = 0;
            }
            else if (
                Days != null
                && Days.Length > 0
                && Days[0].TimeUnits != null
                && possible * -1 >= Days[0].TimeUnits.Length)
            {
                possible = Days[0].TimeUnits.Length - 1;
                possible *= -1;
            }
            else if (Days != null
               && Days.Length > 0
               && Days[0].TimeUnits != null)
            {
                int max = Days[0].TimeUnits.Length - visible;
                max *= -1;
                if (possible < max) possible = max;
            }

            if (possible != TimeUnitsOffset)
            {
                TimeUnitsOffset = possible;
            }
        }

        /// <summary>
        /// Sets the value of theDaysMode property.
        /// </summary>
        /// <param name="mode">Mode in which days will be rendered</param>
        private void SetDaysMode(CalendarDaysMode mode)
        {
            _daysMode = mode;
        }

        /// <summary>
        /// Sets the value of theState property
        /// </summary>
        /// <param name="state">Current state of the calendar</param>
        private void SetState(CalendarState state)
        {
            _state = state;
        }

        /// <summary>
        /// Handles the LostFocus event of the TextBox that edit items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_LostFocus(object sender, EventArgs e)
        {
            FinalizeEditMode(false);
        }

        /// <summary>
        /// Handles the Keydown event of the TextBox that edit items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                FinalizeEditMode(true);
            }
            else if (e.KeyCode == Keys.Enter)
            {
                FinalizeEditMode(false);
            }
        }

        /// <summary>
        /// Updates the
        /// </summary>
        internal void UpdateDaysAndWeeks()
        {
            TimeSpan span = (new DateTime(ViewEnd.Year, ViewEnd.Month, ViewEnd.Day, 23, 59, 59)).Subtract(ViewStart.Date);
            int preDays = 0;
            span = span.Add(new TimeSpan(0,0,0,1,0));

            if (span.Days < 1 || span.Days > MaximumViewDays )
            {
                //return;
                if (span.Days < 1)
                    ViewEnd = _viewStart.AddDays(1).Date.Add(new TimeSpan(23, 59, 59));
                else
                    ViewEnd = _viewStart.AddDays(MaximumViewDays - 1).Date.Add(new TimeSpan(23, 59, 59));
                return;
                //throw new Exception("Days between ViewStart and ViewEnd should be between 1 and MaximumViewDays");
            }

            if (span.Days > MaximumFullDays)
            {
                SetDaysMode(CalendarDaysMode.Short);
                preDays = (new int[] { 0, 1, 2, 3, 4, 5, 6 })[(int)ViewStart.DayOfWeek] - (int)FirstDayOfWeek;
                span = span.Add(new TimeSpan(preDays, 0, 0, 0));

                while (span.Days % 7 != 0)
                    span = span.Add(new TimeSpan(1, 0, 0, 0));
            }
            else
            {
                SetDaysMode(CalendarDaysMode.Expanded);
            }

            _days = new CalendarDay[span.Days];

            for (int i = 0; i < Days.Length; i++)
            {
                Days[i] = new CalendarDay(this, ViewStart.AddDays(-preDays + i), i);
            }


            //Weeks
            if (DaysMode == CalendarDaysMode.Short)
            {
                List<CalendarWeek> weeks = new List<CalendarWeek>();

                for (int i = 0; i < Days.Length; i++)
                {
                    if (Days[i].Date.DayOfWeek == FirstDayOfWeek)
                    {
                        weeks.Add(new CalendarWeek(this, Days[i].Date));
                    }
                }

                _weeks = weeks.ToArray();
            }
            else
            {
                _weeks = new CalendarWeek[] { };
            }

            UpdateHighlights();

        }

        /// <summary>
        /// Updates the View Range of this control to use the SelectionBegin and SelectionEnd properties
        /// of the provided MonthView control.
        /// </summary>
        /// <param name="sender">the MonthView Control that raised the SelectionChanged event</param>
        /// <param name="e">Event Args</param>
        private void UpdateDateRangeFromMonthView(object sender, EventArgs e)
        {
            this.SetViewRange((sender as MonthView).SelectionStart, (sender as MonthView).SelectionEnd);
        }

        /// <summary>
        /// Updates the value of theCalendarTimeScaleUnit.Highlighted property on the time units of days.
        /// </summary>
        internal void UpdateHighlights()
        {
            if (Days == null) return;

            for (int i = 0; i < Days.Length; i++)
            {
                Days[i].UpdateHighlights();
            }
        }

        /// <summary>
        /// Informs elements who's selected and who's not, and repaints_selectedElementSquare
        /// </summary>
        private void UpdateSelectionElements()
        {
            CalendarTimeScaleUnit unitStart = _selectedElementStart as CalendarTimeScaleUnit;
            CalendarDayTop topStart = _selectedElementStart as CalendarDayTop;
            CalendarDay dayStart = _selectedElementStart as CalendarDay;
            CalendarTimeScaleUnit unitEnd = _selectedElementEnd as CalendarTimeScaleUnit;
            CalendarDayTop topEnd = _selectedElementEnd as CalendarDayTop;
            CalendarDay dayEnd = _selectedElementEnd as CalendarDay;

            ClearSelectedComponents();

            if (_selectedElementEnd == null || _selectedElementStart == null) return;

            if (_selectedElementEnd.CompareTo(_selectedElementStart) < 0)
            {
                //swap
                unitStart = _selectedElementEnd as CalendarTimeScaleUnit;
                topStart = _selectedElementEnd as CalendarDayTop;
                dayStart = _selectedElementEnd as CalendarDay;
                unitEnd = _selectedElementStart as CalendarTimeScaleUnit;
                topEnd = _selectedElementStart as CalendarDayTop;
                dayEnd = _selectedElementStart as CalendarDay;
            }

            if (unitStart != null && unitEnd != null)
            {
                bool reached = false;
                for (int i = unitStart.Day.Index; !reached; i++)
                {
                    for (int j = (i == unitStart.Day.Index ? unitStart.Index : 0); i < Days.Length && j < Days[i].TimeUnits.Length; j++)
                    {
                        CalendarTimeScaleUnit unit = Days[i].TimeUnits[j];
                        unit.SetSelected(true);
                        GrowSquare(unit.Bounds);
                        _selectedElements.Add(unit);

                        if (unit.Equals(unitEnd))
                        {
                            reached = true;
                            break;
                        }
                    }
                }
            }
            else if (topStart != null && topEnd != null)
            {
                for (int i = topStart.Day.Index; i <= topEnd.Day.Index ; i++)
                {
                    CalendarDayTop top = Days[i].DayTop;

                    top.SetSelected(true);
                    GrowSquare(top.Bounds);
                    _selectedElements.Add(top);
                }
            }
            else if (dayStart != null && dayEnd != null)
            {
                for (int i = dayStart.Index; i <= dayEnd.Index; i++)
                {
                    CalendarDay day = Days[i];

                    day.SetSelected(true);
                    GrowSquare(day.Bounds);
                    _selectedElements.Add(day);
                }
            }

            Invalidate(_selectedElementSquare);
        }

        /// <summary>
        /// Evaluates the data source and sets the internal list of searchable items.
        /// </summary>
        /// <remarks>This fires when the datasource is set, or the binding context has changed.</remarks>
        private void EvaluateDataSource()
        {
            try
            {

                ClearItems();
                List<CalendarItem> tmp = new List<CalendarItem>();
				if (datasource != null)
					foreach (var item in datasource)
					{
						if (DateIntersects(item.BeginTime, item.EndTime, ViewStart, ViewEnd))
						{
							var ci = new CalendarItem(this, item.BeginTime, item.EndTime, item.Display);

							ci.AppointmentID = item.AppointmentID;
							ci.TagID = item.TagID;
							ci.IsAllDay = item.IsAllDay;
							ci.Tag = item.Tag;
							ci.DataItem = item;
							ci.ImageAlign = CalendarItemImageAlign.North | CalendarItemImageAlign.West;

							if (showImageOnAppointments && item.BackImage != null)
								ci.Image = item.BackImage;

							if (item.ForeColor != null)
								ci.ForeColor = item.ForeColor;
							else
								ci.ForeColor = this.ForeColor;

							if (item.BackColor != null)
								ci.BackgroundColor = item.BackColor;

							if (Resource != null)
								ci.Resource = Resource;

							tmp.Add(ci);
						}
					}

                //if (DroppedItems != null)
                //    foreach (var item in DroppedItems)
                //    { item.SetCalendar(this); this.Items.Add(item); }


                if (remindersource != null)
                    foreach (var item in remindersource)
                    {
                        if (DateIntersects(item.BeginTime, item.EndTime, ViewStart, ViewEnd))
                        {
                            var ci = new CalendarItem(this, item.BeginTime, item.EndTime, item.Display);

                            ci.AppointmentID = item.AppointmentID;
                            ci.TagID = item.TagID;
                            ci.IsAllDay = showRemindersAsAllDay ? true : item.IsAllDay;
                            ci.Tag = item.Tag;
                            ci.DataItem = item;
                            ci.ImageAlign = CalendarItemImageAlign.North | CalendarItemImageAlign.West;

                            if (showImageOnReminders && item.BackImage != null)
                                ci.Image = item.BackImage;

                            if (item.ForeColor != null)
                                ci.ForeColor = item.ForeColor;
                            else
                                ci.ForeColor = this.ForeColor;

                            if (item.BackColor != null)
                                ci.BackgroundColor = item.BackColor;

                            if (Resource != null)
                                ci.Resource = Resource;

                            tmp.Add(ci);
                        }
                    }
                tmp.Sort(new CalendarItemComparer());
                Items.ClearAndReplace(tmp);
                tmp.Clear();
            }
            catch
            {
                datasource = null;
            }
            if (datasource == null)
            {

            }
        }

        ///// <summary>
        ///// Uses reflection to get the value of the specified property
        ///// </summary>
        ///// <param name="o">Object to evaluate</param>
        ///// <param name="property">Property to retrieve from the object</param>
        ///// <returns></returns>
        //private object GetValue(object o, string property)
        //{
        //    return o.GetType().GetProperty(property).GetValue(o, null);
        //}

        /// <summary>
        /// Returns the CalendarItem whose data item has the specified ID
        /// </summary>
        /// <param name="id">id of the data item to find</param>
        /// <returns></returns>
        private ICalendarItem GetItemByID(int id)
        {
            ICalendarItem ret = null;
            foreach (var item in datasource)
            {
                if (item.AppointmentID == id)
                {
                    ret = item;
                    break;
                }
            }
            return ret;
        }

        #endregion

        #region Overridden Events and Raisers

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            Renderer.OnInitialize(new CalendarRendererEventArgs(new CalendarRendererEventArgs(this, null, Rectangle.Empty)));
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Select();
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);
            if(AllowNew)
                CreateItemOnSelection(string.Empty, true);
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
//#if DEBUG
//            drgevent.Effect = DragDropEffects.Move;
//            base.OnDragEnter(drgevent);
//#endif
        }

        protected override void OnDragOver(DragEventArgs drgevent)
        {
            base.OnDragOver(drgevent);
        }

        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
#if DEBUG
            //var dobj = drgevent.Data;
            //foreach (string format in dobj.GetFormats())
            //    if(format == DRAGDROP_FORMAT)
            //    {
            //        mouseDownPoint = Cursor.Position;
            //        CalendarItemEventArgs itm = (CalendarItemEventArgs)drgevent.Data.GetData(DRAGDROP_FORMAT);
            //        CalendarItem hittedItem = itm.Item;

            //        hittedItem.SetSelected(true);
            //        Invalidate(hittedItem);
            //        OnItemSelected(new CalendarItemEventArgs(hittedItem));

            //        itemOnState = hittedItem;
            //        itemOnStateChanged = false;

            //        if (AllowItemEdit)
            //            SetState(CalendarState.DraggingItem);

            //        OnItemDropped(itm);

            //        SetSelectionRange(null, null);
            //        break;
            //    }
#endif
        }

        protected virtual void OnDayHeaderClick(CalendarDayEventArgs e)
        {
            if (DayHeaderClick != null)
            {
                DayHeaderClick(this, e);
            }
        }

        protected virtual void OnItemClick(CalendarItemEventArgs e)
        {
            if (ItemClick != null)
            {
                ItemClick(this, e);
            }
        }

        protected virtual void OnItemImageClick(CalendarItemEventArgs e)
        {
            if (ItemImageClick != null)
            {
                ItemImageClick(this, e);
            }
        }

        protected virtual void OnEmptyTimeClick(Calendar e)
        {
            if (EmptyTimeClick != null)
            {
                EmptyTimeClick(this, new CalendarEventArgs(e));
            }
        }

        protected virtual void OnItemCreating(CalendarItemCancelEventArgs e)
        {
            if (ItemCreating != null)
            {
                ItemCreating(this, e);
            }
        }

        protected virtual void OnItemCreated(CalendarItemCancelEventArgs e)
        {
            if (ItemCreated != null)
            {
                ItemCreated(this, e);
            }
        }

        protected virtual void OnItemDeleting(CalendarItemCancelEventArgs e)
        {
            if (ItemDeleting != null)
            {
                ItemDeleting(this, e);
            }
        }

        protected virtual void OnItemDeleted(CalendarItemEventArgs e)
        {
            if (ItemDeleted != null)
            {
                ItemDeleted(this, e);
            }
        }

        protected virtual void OnItemDoubleClick(CalendarItemEventArgs e)
        {
            if (ItemDoubleClick != null)
            {
                ItemDoubleClick(this, e);
            }
            if (_allowItemEditExternal && OpenItemExternal != null)
                OpenItemExternal(this, new CalendarItemEventArgs(e.Item));
        }

        protected virtual void OnItemDragged(CalendarItemEventArgs e)
        {
            if (ItemDragged != null)
                ItemDragged(this, e);
        }

        protected virtual void OnItemDropped(CalendarItemEventArgs e)
        {
            this.DroppedItems.Add(e.Item);
            if (ItemDropped != null)
                ItemDropped(this, e);
        }

        protected virtual void OnItemEditing(CalendarItemCancelEventArgs e)
        {
            if (ItemMouseHoverEnd != null)
                ItemMouseHoverEnd(this, new CalendarItemEventArgs(e.Item));

            if (ItemTextEditing != null)
                ItemTextEditing(this, e);
        }

        protected virtual void OnItemEdited(CalendarItemCancelEventArgs e)
        {
            if (ItemTextEdited != null)
                ItemTextEdited(this, e);
        }

        protected virtual void OnItemSelected(CalendarItemEventArgs e)
        {
            if (ItemSelected != null)
                ItemSelected(this, e);
            if (ItemMouseHover == null)
                return;

            if (e.Item != null)
            {
                HoverItem = e.Item;
                CalendarItemEventArgs arg = new CalendarItemEventArgs(HoverItem);
                ItemMouseHover(this, arg);
            }
        }

        protected virtual void OnItemsPositioned(EventArgs e)
        {
            if (ItemsPositioned != null)
                ItemsPositioned(this, e);
        }

        protected virtual void OnItemDatesChanged(CalendarItemEventArgs e)
        {
            if (ItemDatesChanged != null)
                ItemDatesChanged(this, e);
        }

        protected override void  OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);
            //if (ItemMouseHover == null)
            //    return;

            //Point curPt = this.PointToClient(Cursor.Position);
            //if (HitTest(curPt) is CalendarItem)
            //{
            //    isHovering = true;
            //    CalendarItemEventArgs arg = new CalendarItemEventArgs((CalendarItem)HitTest(curPt));
            //    ItemMouseHover(this, arg);
            //}
        }

        protected virtual void OnItemMouseHover(CalendarItemEventArgs e)
        {
            if (ItemMouseHover != null)
                ItemMouseHover(this, e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            bool shiftPressed = (ModifierKeys & Keys.Shift) == Keys.Shift;
            int jump = (int)TimeScale;
            ICalendarSelectableElement sStart = null;
            ICalendarSelectableElement sEnd = null;

            if (e.KeyCode == Keys.F2)
            {
                ActivateEditMode();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                try
                {
                    DeleteSelectedItems();
                }
                catch (Exception)
                { }
            }
            else if (e.KeyCode == Keys.Insert)
            {
                if (AllowNew)
                    CreateItemOnSelection(string.Empty, true);
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (e.Shift)
                    sStart = SelectedElementStart;

                sEnd = GetTimeUnit(SelectedElementEnd.Date.Add(new TimeSpan(0,(int)TimeScale,0)));
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (e.Shift)
                    sStart = SelectedElementStart;

                sEnd = GetTimeUnit(SelectedElementEnd.Date.Add(new TimeSpan(0, -(int)TimeScale, 0)));
            }
            else if (e.KeyCode == Keys.Right)
            {
                sEnd = GetTimeUnit(SelectedElementEnd.Date.Add(new TimeSpan(24, 0, 0)));
            }
            else if (e.KeyCode == Keys.Left)
            {
                sEnd = GetTimeUnit(SelectedElementEnd.Date.Add(new TimeSpan(-24, 0, 0)));
            }
            else if (e.KeyCode == Keys.PageDown)
            {

            }
            else if (e.KeyCode == Keys.PageUp)
            {

            }


            if (sStart != null)
            {
                SetSelectionRange(sStart, sEnd);
            }
            else if (sEnd != null)
            {
                SetSelectionRange(sEnd, sEnd);

                var s = sEnd as CalendarTimeScaleUnit;
                if(s != null)
                    EnsureVisible(s);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            bool ctrlPressed = (ModifierKeys & Keys.Control) == Keys.Control;

            if(!ctrlPressed && AllowNew)
                CreateItemOnSelection(e.KeyChar.ToString(), true);
        }

        protected virtual void OnLoadItems(CalendarLoadEventArgs e)
        {
            if (LoadItems != null)
            {
                LoadItems(this, e);
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (this.DaysMode == CalendarDaysMode.Short)
            {
                CalendarDay currentDay = HitTest(e.Location, true) as CalendarDay;
                if (currentDay != null && currentDay.OverflowEnd)
                {
                    if (currentDay.OverflowEndBounds.Contains(e.Location))
                        return;
                }
                if (currentDay != null && currentDay.OverflowStart)
                {
                    if (currentDay.OverflowStartBounds.Contains(e.Location))
                        return;
                }
            }

            base.OnMouseDoubleClick(e);

            CalendarItem item = ItemAt(e.Location);

            if (item != null)
            {
                OnItemDoubleClick(new CalendarItemEventArgs(item));
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            //Set the point at which the mouse button was pressed
            mouseDownPoint = Cursor.Position;

            ICalendarSelectableElement hitted = HitTest(e.Location);
            CalendarItem hittedItem = hitted as CalendarItem;
            bool shiftPressed = (ModifierKeys & Keys.Shift) == Keys.Shift;

            if (!Focused)
            {
                Focus();
            }

            switch (State)
            {
                case CalendarState.Idle:
                    CalendarDay currentDay = HitTest(e.Location, true) as CalendarDay;

                    if (currentDay != null && currentDay.OverflowEnd)
                    {
                        if (currentDay.OverflowEndBounds.Contains(e.Location))
                        { currentDay.SetItemIndex(currentDay.ItemIndex + 1); return; }
                    }
                    if (currentDay != null && currentDay.OverflowStart)
                    {
                        if (currentDay.OverflowStartBounds.Contains(e.Location))
                        { currentDay.SetItemIndex(currentDay.ItemIndex - 1); return; }
                    }

                    if (hittedItem != null)
                    {
                        if (!shiftPressed)
                            ClearSelectedItems();

                        hittedItem.SetSelected(true);
                        Invalidate(hittedItem);
                        OnItemSelected(new CalendarItemEventArgs(hittedItem));

                        itemOnState = hittedItem;
                        itemOnStateChanged = false;

                        if (AllowItemEdit)
                        {

                            if (itemOnState.ResizeStartDateZone(e.Location) && AllowItemResize)
                            {
                                SetState(CalendarState.ResizingItem);
                                itemOnState.SetIsResizingStartDate(true);
                            }
                            else if (itemOnState.ResizeEndDateZone(e.Location) && AllowItemResize)
                            {
                                SetState(CalendarState.ResizingItem);
                                itemOnState.SetIsResizingEndDate(true);
                            }
                            else
                            {
                                SetState(CalendarState.DraggingItem);
                            }
                        }

                        SetSelectionRange(null, null);
                    }
                    else
                    {
                        ClearSelectedItems();

                        if (shiftPressed)
                        {
                            if (hitted != null && SelectedElementEnd == null && !SelectedElementEnd.Equals(hitted))
                                SelectedElementEnd = hitted;
                        }
                        else
                        {
                            if (SelectedElementStart == null || (hitted != null && !SelectedElementStart.Equals(hitted)))
                            {
                                SetSelectionRange(hitted, hitted);
                            }
                        }

                        SetState(CalendarState.DraggingTimeSelection);
                    }
                    break;
                case CalendarState.DraggingTimeSelection:
                    break;
                case CalendarState.DraggingItem:
                    break;
                case CalendarState.ResizingItem:
                    break;
                case CalendarState.EditingItemText:
                    break;

            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            ICalendarSelectableElement hitted = HitTest(e.Location, State != CalendarState.Idle);
            CalendarItem hittedItem = hitted as CalendarItem;
            CalendarDayTop hittedTop = hitted as CalendarDayTop;
            bool shiftPressed = (ModifierKeys & Keys.Shift) == Keys.Shift;

            if (hittedItem == null && HoverItem != null)
            {
                if (ItemMouseHoverEnd != null)
                    ItemMouseHoverEnd(this, new CalendarItemEventArgs(HoverItem));
            }

            if (hitted != null)
            {
                OnMouseMove_ProcessHittedItem(e, hitted, hittedItem, hittedTop);
            }
        }

        private void OnMouseMove_ProcessHittedItem(MouseEventArgs e, ICalendarSelectableElement hitted, CalendarItem hittedItem, CalendarDayTop hittedTop)
        {
            switch (State)
            {
                case CalendarState.Idle:
                    OnMouseMove_ProcessIdle(e, hittedItem);
                    break;
                case CalendarState.DraggingTimeSelection:
                    if (SelectedElementStart != null && !SelectedElementEnd.Equals(hitted))
                        SelectedElementEnd = hitted;
                    break;
                case CalendarState.DraggingItem:
                    OnMouseMove_ProcessDraggingItem(hitted);
                    break;
                case CalendarState.ResizingItem:

                    OnMouseMove_ProcessResizingItem(hitted, hittedTop);
                    break;
                case CalendarState.EditingItemText:
                    break;
            }
        }

        private void OnMouseMove_ProcessResizingItem(ICalendarSelectableElement hitted, CalendarDayTop hittedTop)
        {
            if (itemOnState.IsResizingEndDate && hitted.Date.CompareTo(itemOnState.StartDate) >= 0)
            {
                itemOnState.EndDate = hitted.Date.Add(hittedTop != null || DaysMode == CalendarDaysMode.Short ? new TimeSpan(23, 59, 59) : Days[0].TimeUnits[0].Duration);
            }
            else if (itemOnState.IsResizingStartDate && hitted.Date.CompareTo(itemOnState.EndDate) <= 0)
            {
                itemOnState.StartDate = hitted.Date;
            }
            Renderer.PerformItemsLayout();
            Invalidate();
            itemOnStateChanged = true;
        }

        private void OnMouseMove_ProcessDraggingItem(ICalendarSelectableElement hitted)
        {
            var pt = Cursor.Position;

            //Find the vertical change in cursor location since the mousedown
            var dx = Math.Abs(mouseDownPoint.Y - pt.Y);

            //Usually, unintentional mouse moves are 1-2 pixels.
            //So only move if the dx is > 3 to filter out unintentional movements.
            if (dx > 3)
            {
                TimeSpan duration = itemOnState.Duration;
                itemOnState.SetIsDragging(true);
                if (DaysMode != CalendarDaysMode.Expanded)
                {
                    itemOnState.StartDate = hitted.Date.Date.AddHours(itemOnState.StartDate.Hour).AddMinutes(itemOnState.StartDate.Minute);
                    itemOnState.EndDate = itemOnState.StartDate.Add(duration);
                }
                else
                {
                    itemOnState.StartDate = hitted.Date;
                    itemOnState.EndDate = itemOnState.StartDate.Add(duration);
                }
                Renderer.PerformItemsLayout();
                Invalidate();
                itemOnStateChanged = true;
            }
        }

        private void OnMouseMove_ProcessIdle(MouseEventArgs e, CalendarItem hittedItem)
        {
            Cursor should = Cursors.Default;

            if (hittedItem != null)
            {
                if ((hittedItem.ResizeEndDateZone(e.Location) || hittedItem.ResizeStartDateZone(e.Location)) && AllowItemResize)
                {
                    should = hittedItem.IsOnDayTop || DaysMode == CalendarDaysMode.Short ? Cursors.SizeWE : Cursors.SizeNS;
                }

                //OnItemMouseHover(new CalendarItemEventArgs(hittedItem));

            }
            if (!Cursor.Equals(should)) Cursor = should;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            mouseDownPoint = Point.Empty;

            ICalendarSelectableElement hitted = HitTest(e.Location, State == CalendarState.DraggingTimeSelection);
            CalendarItem hittedItem = hitted as CalendarItem;
            CalendarDay hittedDay = hitted as CalendarDay;
            bool shiftPressed = (ModifierKeys & Keys.Shift) == Keys.Shift;

            if (this.DaysMode == CalendarDaysMode.Short)
            {
                CalendarDay currentDay = HitTest(e.Location, true) as CalendarDay;
                if (currentDay != null && currentDay.OverflowEnd)
                {
                    if (currentDay.OverflowEndBounds.Contains(e.Location))
                        return;
                }
                if (currentDay != null && currentDay.OverflowStart)
                {
                    if (currentDay.OverflowStartBounds.Contains(e.Location))
                        return;
                }
            }

            switch (State)
            {
                case CalendarState.Idle:

                    break;
                case CalendarState.DraggingTimeSelection:
                    if (SelectedElementStart == null || (hitted != null && !SelectedElementEnd.Equals(hitted)))
                    {
                        SelectedElementEnd = hitted;
                    }
                    if (hittedDay != null)
                    {
                        if (hittedDay.HeaderBounds.Contains(e.Location))
                        {
                            OnDayHeaderClick(new CalendarDayEventArgs(hittedDay));
                        }
                    }
                    break;
                case CalendarState.DraggingItem:
                    if (itemOnStateChanged)
                        OnItemDatesChanged(new CalendarItemEventArgs(itemOnState));
                    //{
                    //    if (!Items.Contains(itemOnState))
                    //    {

                    //    }
                    //    else
                    //        OnItemDatesChanged(new CalendarItemEventArgs(itemOnState));
                    //}
                    break;
                case CalendarState.ResizingItem:
                    if (itemOnStateChanged)
                        OnItemDatesChanged(new CalendarItemEventArgs(itemOnState));
                    break;
                case CalendarState.EditingItemText:
                    break;
            }

            if (itemOnState != null)
            {
                itemOnState.SetIsDragging(false);
                itemOnState.SetIsResizingEndDate(false);
                itemOnState.SetIsResizingStartDate(false);
                Invalidate(itemOnState);
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                    if (itemOnState.Image != null && itemOnState.ImageBounds.Contains(e.Location))
                        OnItemImageClick(new CalendarItemEventArgs(itemOnState));
                    else
                        OnItemClick(new CalendarItemEventArgs(itemOnState));
                itemOnState = null;
            }
            else if((hitted as CalendarTimeScaleUnit) != null)
            {
                OnEmptyTimeClick(this);
            }
            SetState(CalendarState.Idle);

        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (DaysMode == CalendarDaysMode.Expanded)
            {
                ScrollTimeUnits(e.Delta);
            }
            else if (DaysMode == CalendarDaysMode.Short)
            {
                ScrollCalendar(e.Delta);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            CalendarRendererEventArgs evt = new CalendarRendererEventArgs(this, e.Graphics, e.ClipRectangle);

            ///Calendar background
            Renderer.OnDrawBackground(evt);

            /// Headers / Timescale
            switch (DaysMode)
            {
                case CalendarDaysMode.Short:
                    Renderer.OnDrawDayNameHeaders(evt);
                    Renderer.OnDrawWeekHeaders(evt);
                    break;
                case CalendarDaysMode.Expanded:
                    Renderer.OnDrawTimeScale(evt);
                    break;
                default:
                    throw new NotImplementedException("Current DaysMode not implemented");
            }

            ///Days on view
            Renderer.OnDrawDays(evt);

            ///Draw Time of Day
            Renderer.OnDrawTimeOfDay(evt);

            ///Items
            Renderer.OnDrawItems(evt);

            ///Overflow marks
            Renderer.OnDrawOverflows(evt);

        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_daysMode == CalendarDaysMode.Expanded)
                TimeUnitsOffset = TimeUnitsOffset;
            else
                RefreshData(); //SetViewRange(this.ViewStart, this.ViewEnd);
            Renderer.PerformLayout();
        }

        #endregion
    }

    /// <summary>
    /// Holds data of a Calendar Loading Items of certain date range
    /// </summary>
    public class CalendarLoadEventArgs
        : EventArgs
    {
        #region Variables
        private Calendar _calendar;
        private DateTime _dateStart;
        private DateTime _dateEnd;

        #endregion

        #region Ctor

        public CalendarLoadEventArgs(Calendar calendar, DateTime dateStart, DateTime dateEnd)
        {
            _calendar = calendar;
            _dateEnd = dateEnd;
            _dateStart = dateStart;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the calendar that originated the event
        /// </summary>
        public Calendar Calendar
        {
            get { return _calendar; }
        }

        /// <summary>
        /// Gets the start date of the load
        /// </summary>
        public DateTime DateStart
        {
            get { return _dateStart; }
            set { _dateStart = value; }
        }

        /// <summary>
        /// Gets the end date of the load
        /// </summary>
        public DateTime DateEnd
        {
            get { return _dateEnd; }
        }


        #endregion
    }


    public class CalendarItemPopupEventArgs
        : EventArgs
    {
        #region Variables
        private bool _show;
        private Calendar _sender;
        private CalendarItem _item;
        private string _text;
        private string _title;
        #endregion

        #region Ctor

        public CalendarItemPopupEventArgs() : base()
        {

        }

        public CalendarItemPopupEventArgs(bool show, Calendar calendar, CalendarItem itm, string title, string text)
        {
            _show = show;
            _sender = calendar;
            _item = itm;
            _title = title;
            _text = text;
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets whether the balloon should be shown or hidden
        /// </summary>
        public bool Show
        {
            get { return _show; }
            set { _show = value; }
        }

        /// <summary>
        /// Gets or sets the Calendar that will be displaying the popup
        /// </summary>
        public Calendar Calendar
        {
            get { return _sender; }
            set { _sender = value; }
        }

        /// <summary>
        /// Gets or sets the Item that owns the popup
        /// </summary>
        public CalendarItem Item
        {
            get { return _item; }
            set { _item = value; }
        }

        /// <summary>
        /// Gets or sets the title text of the popup balloon
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        /// <summary>
        /// Gets or sets the Text that needs to be displayed
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }
        #endregion
    }
}
