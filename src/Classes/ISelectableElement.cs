using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Baldini.Controls.Calendar
{
    /// <summary>
    /// Represents a clickable element ofMonthView control
    /// </summary>
    public interface ISelectableElement
    {

        /// <summary>
        /// Gets the bounds of the element
        /// </summary>
        Rectangle Bounds { get; }

        /// <summary>
        /// Gets if the element is currently selected
        /// </summary>
        bool Selected { get; }
    }
}
