using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Baldini.Controls.Calendar
{
    /// <summary> Provides color information of calendar graphical elements
    /// </summary>
    public class CalendarColorTable
    {
        #region Static

        /// <summary> Returns the result of combining the specified colors
        /// </summary>
        /// <param name="c1">First color to combine</param>
        /// <param name="c2">Second olor to combine</param>
        /// <returns>Average of both colors</returns>
        public static Color Combine(Color c1, Color c2)
        {
            return Color.FromArgb(
                (c1.R + c2.R) / 2,
                (c1.G + c2.G) / 2,
                (c1.B + c2.B) / 2
                );
        }

        /// <summary> Converts the hex formatted color to aColor
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static Color FromHex(string hex)
        {
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);

            if (hex.Length != 6) throw new Exception("Color not valid");

            return Color.FromArgb(
                int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber));
        }

        /// <summary> Take an input color and applies a transform to it
        /// </summary>
        /// <param name="c">base color to modify</param>
        /// <param name="alpha">the explicit alpha channel value</param>
        /// <param name="r">the R channel value to ADD to the existing color</param>
        /// <param name="g">the G channel value to ADD to the existing color</param>
        /// <param name="b">the B channel value to ADD to the existing color</param>
        /// <returns></returns>
        public static Color FromBaseColor(Color c, int alpha = 255, int r = 0, int g = 0, int b = 0)
        {
            if (r > 0 | g > 0 | b > 0)
            {
                int R = r > 0 ? (c.R + r > 255 ? 255 : c.R + r) : (c.R - r < 0 ? 0 : c.R - r);
                int G = g > 0 ? (c.G + g > 255 ? 255 : c.G + g) : (c.G - g < 0 ? 0 : c.G - g);
                int B = b > 0 ? (c.B + b > 255 ? 255 : c.B + b) : (c.B - b < 0 ? 0 : c.B - b);
                return Color.FromArgb(alpha, R, G, B);
            }
            else
                return Color.FromArgb(alpha, c.R, c.G, c.B);
        }

        #endregion

        #region Colors

        /// <summary> Background color of calendar
        /// </summary>
        public Color Background = SystemColors.Control;

        /// <summary> Background color of days in even months
        /// </summary>
        public Color DayBackgroundEven = SystemColors.Control;

        /// <summary> Background color of days in odd months
        /// </summary>
        public Color DayBackgroundOdd = SystemColors.ControlLight;

        /// <summary> Background color of selected days
        /// </summary>
        public Color DayBackgroundSelected = SystemColors.Highlight;

        /// <summary> Border of
        /// </summary>
        public Color DayBorder = SystemColors.ControlDark;

        /// <summary> Background color of day headers.
        /// </summary>
        public Color DayHeaderBackground = Combine(SystemColors.ControlDark, SystemColors.Control);

        /// <summary> Color of text of day headers
        /// </summary>
        public Color DayHeaderText = SystemColors.GrayText;

        /// <summary> Color of secondary text in headers
        /// </summary>
        public Color DayHeaderSecondaryText = SystemColors.GrayText;

        /// <summary> Color of border of the top part of the days
        /// </summary>
        /// <remarks>
        /// The DayTop is the zone of the calendar where items that lasts all or more are placed.
        /// </remarks>
        public Color DayTopBorder = SystemColors.ControlDark;

        /// <summary> Color of border of the top parth of the days when selected
        /// </summary>
        /// <remarks>
        /// The DayTop is the zone of the calendar where items that lasts all or more are placed.
        /// </remarks>
        public Color DayTopSelectedBorder = SystemColors.ControlDark;

        /// <summary> Background color of day tops.
        /// </summary>
        /// <remarks>
        /// The DayTop is the zone of the calendar where items that lasts all or more are placed.
        /// </remarks>
        public Color DayTopBackground = SystemColors.ControlLight;

        /// <summary> Background color of selected day tops.
        /// </summary>
        /// <remarks>
        /// The DayTop is the zone of the calendar where items that lasts all or more are placed.
        /// </remarks>
        public Color DayTopSelectedBackground = SystemColors.Highlight;

        /// <summary> Color of items borders
        /// </summary>
        public Color ItemBorder = SystemColors.ControlText;

        /// <summary> Background color of items
        /// </summary>
        public Color ItemBackground = SystemColors.Window;

        /// <summary> Forecolor of items
        /// </summary>
        public Color ItemText = SystemColors.WindowText;

        /// <summary> Color of secondary text on items (Dates and times)
        /// </summary>
        public Color ItemSecondaryText = SystemColors.GrayText;

        /// <summary> Color of items shadow
        /// </summary>
        public Color ItemShadow = Color.FromArgb(50, Color.Black);

        /// <summary> Color of items selected border
        /// </summary>
        public Color ItemSelectedBorder = SystemColors.Highlight;

        /// <summary> Background color of selected items
        /// </summary>
        public Color ItemSelectedBackground = Combine(SystemColors.Highlight, SystemColors.HighlightText);

        /// <summary> Forecolor of selected items
        /// </summary>
        public Color ItemSelectedText = SystemColors.WindowText;

        /// <summary> Background color of week headers
        /// </summary>
        public Color WeekHeaderBackground = Combine(SystemColors.ControlDark, SystemColors.Control);

        /// <summary> Border color of week headers
        /// </summary>
        public Color WeekHeaderBorder = SystemColors.ControlDark;

        /// <summary> Forecolor of week headers
        /// </summary>
        public Color WeekHeaderText = SystemColors.ControlText;

        /// <summary> Forecolor of day names
        /// </summary>
        public Color WeekDayName = SystemColors.ControlText;

        /// <summary> Border color of today day
        /// </summary>
        public Color TodayBorder = Color.Orange;

        /// <summary> Background color of today's DayTop
        /// </summary>
        public Color TodayTopBackground = Color.Orange;

        /// <summary> Color of lines in timescale
        /// </summary>
        public Color TimeScaleLine = SystemColors.ControlDark;

        /// <summary> Color of text representing hours on the timescale
        /// </summary>
        public Color TimeScaleHours = SystemColors.GrayText;

        /// <summary> Color of text representing minutes on the timescale
        /// </summary>
        public Color TimeScaleMinutes = SystemColors.GrayText;

        /// <summary> Background color of time units
        /// </summary>
        public Color TimeUnitBackground = SystemColors.Control;

        /// <summary> Background color of highlighted time units
        /// </summary>
        public Color TimeUnitHighlightedBackground = FromBaseColor(Combine(SystemColors.Control, SystemColors.ControlLightLight), 190);

        /// <summary> Background color of selected time units
        /// </summary>
        public Color TimeUnitSelectedBackground = SystemColors.Highlight;

        /// <summary> Color of light border of time units
        /// </summary>
        public Color TimeUnitBorderLight = SystemColors.ControlDark;

        /// <summary> Color of dark border of time units
        /// </summary>
        public Color TimeUnitBorderDark = SystemColors.ControlDarkDark;

        /// <summary> Border color of the overflow indicators
        /// </summary>
        public Color DayOverflowBorder = Color.White;

        /// <summary> Background color of the overflow indicators
        /// </summary>
        public Color DayOverflowBackground = SystemColors.ControlLight;

        /// <summary>Background color of selected overflow indicators
        /// </summary>
        public Color DayOverflowSelectedBackground = Color.Orange;

        /// <summary> Backgound color of the Time Unit when it is not available
        /// </summary>
        public Color TimeUnitNotAvailable = FromBaseColor(Color.LightSkyBlue, 190, 0, 0, 0);

        /// <summary> Backgound color of the Time Unit when it is highlighted and not available
        /// </summary>
        public Color TimeUnitNotAvailableHighlighted = FromBaseColor(Color.LightBlue, 190);

        /// <summary> Backgound color of the Time Unit when it is selected and not available
        /// </summary>
        public Color TimeUnitNotAvailableSelected = Combine(FromBaseColor(Color.LightBlue, 190), SystemColors.Highlight);

        #endregion

    }
}

