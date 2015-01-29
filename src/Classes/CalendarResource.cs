using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Baldini.Controls.Calendar
{
    public class CalendarResource
    {

        #region variables
        private object _dataitem;
        private string _displayMember;
        private string _valueMember;
        private int _resourceid = 0;
        #endregion

        #region properties

        /// <summary>
        /// Gets or sets the actual underlying data item for the resource.
        /// </summary>
        public object DataItem
        {
            get { return _dataitem; }
            set { _dataitem = value; }
        }

        /// <summary>
        /// Gets or sets the display member for the resource item
        /// </summary>
        public string DisplayMember
        {
            get { return _displayMember; }
            set { _displayMember = value; }
        }

        public string ValueMember
        {
            get { return _valueMember; }
            set { _valueMember = value; }
        }

        public int ResourceID
        {
            get
            {
                if (_resourceid > 0)
                    return _resourceid;
                else if (!string.IsNullOrEmpty(ValueMember))
                {
                    _resourceid = (int)DataItem.GetType().GetProperty(_valueMember).GetValue(DataItem, null);
                    return _resourceid;
                }
                else
                    return 0;
            }
        }

        public System.Drawing.Color ResourceBackColor { get; set; }
        public System.Drawing.Color ResourceFontColor { get; set; }

        #endregion

        #region ctor

        public CalendarResource(object data)
        {
            DataItem = data;
            ResourceBackColor = System.Drawing.Color.CornflowerBlue;
            ResourceFontColor = System.Drawing.Color.Black;
        }

        public CalendarResource(object data, string display, string value)
        {
            DataItem = data;
            DisplayMember = display;
            ValueMember = value;
            ResourceBackColor = System.Drawing.Color.CornflowerBlue;
            ResourceFontColor = System.Drawing.Color.Black;
        }

        #endregion

        #region public methods
        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.DisplayMember))
                return base.ToString();
            else
                return DataItem.GetType().GetProperty(DisplayMember).GetValue(DataItem, null).ToString();
        }

        public static bool operator ==(CalendarResource r1, CalendarResource r2)
        {
            bool ret = false;
            try
            {
                if (((object)r1) == null)
                {
                    if (((object)r2) == null)
                        ret = true;
                    else
                        ret = false;
                }
                else if (((object)r2) == null)
                    ret = false;
                else
                    ret = r1.ToString() == r2.ToString();
            }
            catch (Exception) { }
            return ret;
        }

        public static bool operator != (CalendarResource r1, CalendarResource r2)
        {
            bool ret = false;
            try
            {
                if (((object)r1) == null)
                {
                    if (((object)r2) == null)
                        ret = false;
                    else
                        ret = true;
                }
                else if (((object)r2) == null)
                    ret = true;
                else
                    ret = r1.ToString() != r2.ToString();
            }
            catch (Exception) { }
            return ret;
        }

        #endregion

        #region private methods

        #endregion

    }
}
