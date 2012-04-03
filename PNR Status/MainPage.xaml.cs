using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections;
namespace PNR_Status
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            getResult("http://pnrapi.alagu.net/api/v1.0/pnr/" + pnrBox.Text);
        }
        public string var_dump(object obj, int recursion)
        {
            StringBuilder result = new StringBuilder();

            // Protect the method against endless recursion
            if (recursion < 5)
            {
                // Determine object type
                Type t = obj.GetType();

                // Get array with properties for this object
                PropertyInfo[] properties = t.GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    try
                    {
                        // Get the property value
                        object value = property.GetValue(obj, null);

                        // Create indenting string to put in front of properties of a deeper level
                        // We'll need this when we display the property name and value
                        string indent = String.Empty;
                        string spaces = "|   ";
                        string trail = "|...";

                        if (recursion > 0)
                        {
                            indent = new StringBuilder(trail).Insert(0, spaces, recursion - 1).ToString();
                        }

                        if (value != null)
                        {
                            // If the value is a string, add quotation marks
                            string displayValue = value.ToString();
                            if (value is string) displayValue = String.Concat('"', displayValue, '"');

                            // Add property name and value to return string
                            result.AppendFormat("{0}{1} = {2}\n", indent, property.Name, displayValue);

                            try
                            {
                                if (!(value is ICollection))
                                {
                                    // Call var_dump() again to list child properties
                                    // This throws an exception if the current property value
                                    // is of an unsupported type (eg. it has not properties)
                                    result.Append(var_dump(value, recursion + 1));
                                }
                                else
                                {
                                    // 2009-07-29: added support for collections
                                    // The value is a collection (eg. it's an arraylist or generic list)
                                    // so loop through its elements and dump their properties
                                    int elementCount = 0;
                                    foreach (object element in ((ICollection)value))
                                    {
                                        string elementName = String.Format("{0}[{1}]", property.Name, elementCount);
                                        indent = new StringBuilder(trail).Insert(0, spaces, recursion).ToString();

                                        // Display the collection element name and type
                                        result.AppendFormat("{0}{1} = {2}\n", indent, elementName, element.ToString());

                                        // Display the child properties
                                        result.Append(var_dump(element, recursion + 2));
                                        elementCount++;
                                    }

                                    result.Append(var_dump(value, recursion + 1));
                                }
                            }
                            catch { }
                        }
                        else
                        {
                            // Add empty (null) property to return string
                            result.AppendFormat("{0}{1} = {2}\n", indent, property.Name, "null");
                        }
                    }
                    catch
                    {
                        // Some properties will throw an exception on property.GetValue()
                        // I don't know exactly why this happens, so for now i will ignore them...
                    }
                }
            }

            return result.ToString();
        }
        [DataContract]
        public class pnrClass
        {
            [DataMember]
            public string status { get; set; }
            [DataMember]
            public pnrData data { get; set; }
        }

        [DataContract]
        public class pnrData
        {
            [DataMember]
            public string pnr_number { get; set; }
            [DataMember]
            public string message { get; set; }
            [DataMember]
            public string train_name { get; set; }
            [DataMember]
            public string train_number { get; set; }
            [DataMember]
            public Destination from { get; set; }
            [DataMember]
            public Destination to { get; set; }
            [DataMember]
            public Destination alight { get; set; }
            [DataMember]
            public Destination board { get; set; }
            [DataMember(Name = "class")]
            public string ticket_class { get; set; }
            [DataMember]
            public string travel_date { get; set; }
            [DataMember]
            public Seat[] passenger { get; set; }
            [DataMember]
            public string chart_prepared { get; set; }
        }
        [DataContract]
        public class Destination
        {
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public string code { get; set; }
            public string ToString() 
            {
                return name + " - " + code;
            }
        }
        [DataContract]
        public class Seat
        {
            [DataMember]
            public string seat_number { get; set; }
            [DataMember]
            public string status { get; set; }
        }
        public void getResult(string websiteURL)
        {
            WebClient c = new WebClient();
            c.DownloadStringAsync(new Uri(websiteURL));
            c.DownloadStringCompleted += new DownloadStringCompletedEventHandler(c_DownloadStringCompleted);
        }

        void c_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            
            if (!e.Cancelled)
            {
                String jsonString = e.Result;
                textBlock1.Text = jsonString;
                
                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(jsonString)))
                {
                    string information = "";
                    //parse into jsonser
                    var ser = new DataContractJsonSerializer(typeof(pnrClass));
                    pnrClass obj = ser.ReadObject(ms) as pnrClass;
                    //textBlock1.Text = var_dump(obj, 0);
                    if (obj.status == "OK")
                    {
                        if (obj.data.train_name != null)
                            information += "Train Name: " + obj.data.train_name + Environment.NewLine;

                        if (obj.data.train_number != null)
                            information += "Train Number: " + obj.data.train_number + Environment.NewLine;

                        if (obj.data.from != null)
                            information += "From: " + obj.data.from.ToString() + Environment.NewLine;

                        if (obj.data.to != null)
                            information += "To: " + obj.data.to.ToString() + Environment.NewLine;

                        if (obj.data.alight != null)
                            information += "Alight: " + obj.data.alight.ToString() + Environment.NewLine;

                        if (obj.data.board != null)
                            information += "Board: " + obj.data.board.ToString() + Environment.NewLine;

                        if (obj.data.ticket_class != null)
                            information += "Class: " + obj.data.ticket_class + Environment.NewLine;

                        if (obj.data.travel_date != null)
                            information += "Trave Date: " + obj.data.travel_date + Environment.NewLine;


                        if (obj.data.chart_prepared != "false")
                        {
                            information += "Chart Prepared: " + obj.data.travel_date + Environment.NewLine;
                            int i = 1;
                            foreach (Seat s in obj.data.passenger)
                            {
                                information += "Seat " + Convert.ToString(i) + ": Seat Number = " + s.seat_number + ", Status = " + s.status + Environment.NewLine;
                                i++;
                            }
                        }
                    }
                    else
                    {
                        information += obj.data.message;
                    }
                    //textBlock1.Text += var_dump(obj.data, 0);
                    textBlock1.Text = information;
                }
            }
        }
    }
}