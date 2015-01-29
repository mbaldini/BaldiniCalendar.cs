using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Baldini.Controls.Calendar
{
    public class CalendarTextBox
        : TextBox
    {
        #region Variables
        private Calendar _calendar;
        #endregion

        #region Ctor

        /// <summary>
        /// Creates a newCalendarTextBox for the specifiedCalendar
        /// </summary>
        /// <param name="calendar">Calendar where this control lives</param>
        public CalendarTextBox(Calendar calendar)
        {
            _calendar = calendar;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the calendar where this control lives
        /// </summary>
        public Calendar Calendar
        {
            get { return _calendar; }
        }


        #endregion

        #region Methods



        #endregion
    }
}
