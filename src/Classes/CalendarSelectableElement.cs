using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Baldini.Controls.Calendar
{
    /// <summary>
    /// Implements a basicICalendarSelectableElement
    /// </summary>
    public abstract class CalendarSelectableElement
        : ICalendarSelectableElement
    {
        #region Variables
        private Calendar _calendar;
        private Rectangle _bounds;
        private DateTime _date;
        private bool _selected;
        #endregion

        #region Ctor

        /// <summary>
        /// Creates a new Element
        /// </summary>
        /// <param name="calendar"></param>
        public CalendarSelectableElement(Calendar calendar)
        {
            if (calendar == null) throw new ArgumentNullException("calendar", "calendar cannot be null.");

            _calendar = calendar;
        }

        #endregion

        #region ICalendarSelectableElement Members


        public virtual DateTime Date
        {
            get { return _date; }
        }


        /// <summary>
        /// Gets the Calendar this element belongs to
        /// </summary>
        public virtual Calendar Calendar
        {
            get { return _calendar; }
        }

        /// <summary>
        /// Gets the Bounds of the element on theCalendar window
        /// </summary>
        public virtual Rectangle Bounds
        {
            get { return _bounds; }
        }

        /// <summary>
        /// Gets a value indicating if the element is currently selected
        /// </summary>
        public virtual bool Selected
        {
            get
            {
                return _selected;
            }
        }

        /// <summary>
        /// Compares this element with other using date as comparer
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public virtual int CompareTo(ICalendarSelectableElement element)
        {
            return this.Date.CompareTo(element.Date);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Sets the calendar object. Mainly intended for use with Drag & Drop operations
        /// </summary>
        /// <param name="calendar"></param>
        internal virtual void SetCalendar(Calendar calendar)
        {
            this._calendar = calendar;
        }

        /// <summary>
        /// Sets the value of theBounds property
        /// </summary>
        /// <param name="bounds">Bounds of the element</param>
        internal virtual void SetBounds(Rectangle bounds)
        {
            _bounds = bounds;
        }

        /// <summary>
        /// Sets the value of theSelected property
        /// </summary>
        /// <param name="selected">Value indicating if the element is currently selected</param>
        internal virtual void SetSelected(bool selected)
        {
            _selected = selected;

            //Calendar.Invalidate(Bounds);
        }

        #endregion
    }
}
