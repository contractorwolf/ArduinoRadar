using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;

namespace ArduinoRadar
{
    public partial class ArduinoRadarForm : Form
    {

        private List<RadarDot> dots = new List<RadarDot>();
        public static SerialPort arduino = null;

        public static byte[] rx_data;


        public static int bytesToRead = 0;
        public static int read_count = 0;
        public static int count = 0;
        public static double freq = 0;

        private int totalCount = 0;
        private int missCount = 0;
        private static int totalMissedByteLength = 0;


        private string lastMessage = "";
        private string message = "";



        private byte _terminator = 0x4; 

        public ArduinoRadarForm()
        {
            InitializeComponent();
        }




        private void btnStart_Click(object sender, EventArgs e)
        {
            arduino = new SerialPort(cbPorts.SelectedValue.ToString(), 9600);
            arduino.Open();
            arduino.DataReceived += new SerialDataReceivedEventHandler(Arduino_DataReceived);
        }


        private void Arduino_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            try
            {
                rx_data = new byte[arduino.BytesToRead];

                // read the data
                read_count = arduino.Read(rx_data, 0, rx_data.Length);


                lastMessage += Encoding.ASCII.GetString(rx_data, 0, read_count);




                if (lastMessage.IndexOf("*") > -1) 
                {


                    message = lastMessage;
                    lastMessage = "";
                    this.Invoke(new EventHandler(WriteData));


                } 












            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cbPorts.DataSource = SerialPort.GetPortNames();
            cbPorts.Update();
        }

        private void WriteData(object sender, EventArgs e)
        {

            txtIncoming.Text = ParseMessage(message);



        }



        private string ParseMessage(string message)
        {
            if (message.IndexOf("*") != -1)
            {//then * exists

                message = message.Substring(0, message.IndexOf("*"));

                if (message.IndexOf(",") != -1)
                {//then , exists


                    string[] pair = message.Split(',');

                    if (pair.Length == 2)
                    {
                        string strangle = pair[0];
                        string strdistance = pair[1];



                        int distance = 0;
                        int angle = 0;

                        int.TryParse(strangle, out angle);
                        int.TryParse(strdistance, out distance);



                        //covert servo to 360 
                        angle = (int)(angle / 4.444);
                        //and extend distance for visualization
                        distance = distance * 5;

                        //and clear the collection on every other pass
                        if (angle == 0)
                        {
                            dots.Clear();
                        }



                        dots.Add(new RadarDot(angle, distance));


                        panel1.Refresh();




                    }
                    else
                    {//stub?
                        message = "error";
                    }




                }
                else
                {//stub?
                    message = "error";
                }

            }
            else
            {//stub?
                    message = "error";

            }

            return (message);


        }


        public class RadarDot
        {
            public int Distance { get; set; }
            public int Angle { get; set; }


            public RadarDot(int angle, int distance)
            {
                Angle = angle;
                Distance = distance;
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            DrawRadar(e.Graphics);
        }


        private void DrawRadar(Graphics g)
        {
            int offset = 5;


            int width = panel1.Width - offset;// 50;
            int height = panel1.Height - offset;//50;
            int radius = (width - offset) / 2;


            g.FillEllipse(new SolidBrush(System.Drawing.Color.White), 0, 0, width, height);

            Point center = new Point((width / 2) + (offset / 2), (height / 2) + (offset / 2));


            //center dot
            g.FillRectangle(new SolidBrush(System.Drawing.Color.Black), center.X, center.Y, 5, 5);


            DrawDotCollection(g, center);

        }


        private void DrawDotCollection(Graphics g, Point center)
        {
            SolidBrush greenBrush = new SolidBrush(System.Drawing.Color.Green);
            foreach (RadarDot dot in dots)
            {

                Point endpoint = FindEndpointOnCircle(center, dot.Distance, dot.Angle);

                g.FillRectangle(greenBrush, endpoint.X, endpoint.Y, 10, 10);

            }
        }




        private Point FindEndpointOnCircle(Point origin, int radius, int angle)
        {
            double x = origin.X + (radius * Math.Cos(angle * Math.PI / 180F));
            double y = origin.Y - (radius * Math.Sin(angle * Math.PI / 180F));

            return new Point((int)x, (int)y);
        }
    }
}
