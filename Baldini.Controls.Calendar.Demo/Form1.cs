using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Baldini.Controls.Calendar.Demo
{
    public partial class Form1 : Form
    {

        private List<ICalendarItem> data = new List<ICalendarItem>();
        private Color[] colorList = 
        { 
            Color.Red, Color.Blue, Color.Purple, Color.Green, Color.GreenYellow, Color.Gold, Color.White, Color.Thistle, 
            Color.Salmon, Color.Yellow, Color.Pink, Color.LightBlue, Color.SkyBlue, Color.LightCoral, Color.Coral,
            Color.Brown, Color.Black, Color.DarkOrchid, Color.Orange, Color.DarkOrange, Color.DarkGreen
        };

        public Form1()
        {
            InitializeComponent();
            DateTime apptDate = DateTime.Now.Date.AddMonths(-3);
            for (int i = 0; i < 2048; i++)
            {
                if (i > 0 && i % 8 == 0)
                    apptDate = apptDate.AddDays(1);

                Random rnd = new Random(i);
                var dt = apptDate.Date.AddHours(rnd.Next(23));
                rnd = new Random(rnd.Next());
                dt = dt.AddMinutes(rnd.Next(12) * 5);
                rnd = new Random(rnd.Next());
                data.Add(new DataItem()
                {
                    AppointmentID = i,
                    BackColor = colorList[rnd.Next(colorList.Length - 1)],
                    BeginTime = dt,
                    EditString = "Appointment id : " + i.ToString(),
                    EndTime = dt.AddMinutes(rnd.Next(25) * 5),
                    Notes = "Notes for appointment id : " + i.ToString(),
                    ResourceID = 0,
                    Tag = i,
                    TagID = i
                });
            }

            this.calendar1.DataSource = data.ToArray();
        }

    }

    class DataItem : ICalendarItem
    {

        public int AppointmentID { get; set; }

        public int TagID { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }

        public bool IsAllDay { get; set; }

        public string Display { get { return EditString; } }

        public string EditString { get; set; }

        public string Notes { get; set; }

        public int ResourceID { get; set; }

        public object Tag { get; set; }

        public Color BackColor { get; set; }

        public Color ForeColor { get; set; }

        public Image BackImage { get; set; }



    }

}
