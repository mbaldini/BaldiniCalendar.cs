using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Baldini.Controls.Calendar
{
    public interface ICalendarItem
    {
        int AppointmentID { get; set; }
        int TagID { get; set; }
        DateTime BeginTime { get; set; }
        DateTime EndTime { get; set; }
        bool IsAllDay { get; set; }
        string Display { get; }
        string EditString { get; set; }
        string Notes { get; set; }
        int ResourceID { get; set; }
        object Tag { get; }
        System.Drawing.Color BackColor { get; }
        System.Drawing.Color ForeColor { get; }
        System.Drawing.Image BackImage { get; }

        //void SetBeginEndTime(DateTime dtBegin, DateTime dtEnd);
    }
}
