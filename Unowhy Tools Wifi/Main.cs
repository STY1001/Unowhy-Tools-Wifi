using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Web.UI.Design.WebControls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace Unowhy_Tools_Wifi
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WebClient wc = new WebClient();
            string sn = serial.Text;
            string g = wc.DownloadString($"https://idf.hisqool.com/conf/devices/{sn}/configuration");
            string jsonString = g;
            List<dynamic> dataList = JsonConvert.DeserializeObject<List<dynamic>>(jsonString);

            List<string> urlList = new List<string>();

            foreach (var data in dataList)
            {
                string url = data.url;
                urlList.Add(url);
            }

            // Supposons que vous avez une liste de réponses JSON sous forme de chaînes de caractères
            List<string> jsonResponseList = new List<string>();

            // Créer un objet JObject vide pour stocker les informations fusionnées
            JObject mergedJson = new JObject();

            foreach (var url in urlList)
            {
                var client = new WebClient();
                JObject json = JObject.Parse(client.DownloadString(url));
                mergedJson.Merge(json);
            }
            
            DataTable dt = new DataTable();
            dt.Columns.Add("SSID", typeof(string));
            dt.Columns.Add("Password", typeof(string));
            dt.Columns.Add("Security Type", typeof(string));
            dt.Columns.Add("Hidden", typeof(string));
            dt.Columns.Add("Proxy Type", typeof(string));
            dt.Columns.Add("Proxy URL (Automatic)", typeof(string));
            dt.Columns.Add("Proxy Address (Manual)", typeof(string));
            dt.Columns.Add("Proxy Port (Manual)", typeof(string));

            

            foreach (JProperty property in mergedJson.Properties())
            {
                if (property.Name.StartsWith("conf/network/all/"))
                {
                    DataRow row = dt.NewRow();
                    JToken payload = property.Value["payload"];
                    JToken options = property.Value["options"];
                    JToken proxy = options["proxy"];
                    if (payload != null && options != null && proxy != null)
                    {
                        Console.WriteLine("SSID: " + options["ssid"]);
                        Console.WriteLine("Password: " + options["password"]);
                        Console.WriteLine("Security: " + options["securityType"]);
                        Console.WriteLine("Hidden: " + options["hidden"]);
                        Console.WriteLine("Proxy: " + proxy["type"]);

                        row["SSID"] = options["ssid"];
                        row["Password"] = options["password"];
                        row["Security Type"] = options["securityType"];
                        row["Hidden"] = options["hidden"];
                        row["Proxy Type"] = proxy["type"];

                        if (proxy["type"].ToString() == "none")
                        {
                            Console.WriteLine("No proxy");
                        }
                        else if (proxy["type"].ToString() == "manual")
                        {
                            Console.WriteLine("Manual Proxy");
                            Console.WriteLine("Port: " + proxy["proxyPort"]);
                            Console.WriteLine("IP: " + proxy["proxyHostName"]);
                            row["Proxy Address (Manual)"] = proxy["proxyHostName"];
                            row["Proxy Port (Manual)"] = proxy["proxyPort"];
                        }
                        else if (proxy["type"].ToString() == "automatic")
                        {
                            Console.WriteLine("Auto Proxy");
                            Console.WriteLine("URL: " + proxy["autoProxyUrl"]);
                            row["Proxy URL (Automatic)"] = proxy["autoProxyUrl"];
                        }
                        
                        dt.Rows.Add(row);
                    }

                    
                }
            }
            
            dataGridView1.DataSource = dt;
        }
    }
}
