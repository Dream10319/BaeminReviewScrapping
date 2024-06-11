using Nancy.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BaeminReviewScrapping
{
    public partial class Form1 : Form
    {
        Thread th;
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            th.Abort();
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                th = new Thread(new ThreadStart(() =>
                {
                    string filePath = @"locationinfo.txt"; // Adjust the path to where your file is stored.
                    Regex delimiterRegex = new Regex(@"[\s\t]+");
                    List<(string Latitude, string Longitude)> coordinates = new List<(string, string)>();

                    try
                    {
                        using (StreamReader reader = new StreamReader(filePath))
                        {
                            string line;
                            bool isFirstLine = true;

                            while ((line = reader.ReadLine()) != null)
                            {
                                if (isFirstLine)
                                {
                                    isFirstLine = false;
                                    continue;
                                }

                                string[] parts = delimiterRegex.Split(line.Trim());
                                if (parts.Length >= 2)
                                {
                                    coordinates.Add((parts[0], parts[1]));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error reading locationinfo.txt: " + ex.Message);
                        return; // Exit the thread if there's an error
                    }

                    string strUrl = "https://shopdp-api.baemin.com/display-groups/BAEMIN?latitude=37.5450159&longitude=127.1368066&sessionId=b4e3292329dfd570f054c8&carrier=302780&site=7jWXRELC2e&dvcid=OPUD6086af457479a7bb&adid=aede849f-5e9c-499f-827f-cb4e5c65d801&deviceModel=SM-G9500&appver=12.23.0&oscd=2&osver=32&dongCode=11140102&zipCode=04522&ActionTrackingKey=Organic";

                    RestClient client = new RestClient(strUrl);
                    RestRequest request = new RestRequest();
                    request.AddHeader("Accept-Encoding", "gzip, deflate");
                    request.AddHeader("Connection", "Keep-Alive");
                    request.AddHeader("Host", "shopdp-api.baemin.com");
                    request.AddHeader("User-Agent", "and1_12.23.0");
                    request.AddHeader("USER-BAEDAL", "W/OnG34HSvOVmxn4McyeRzEK3Ldc9+ruPokFIKgQcm0zVU8aOlNuihy2TNW+7I7ZBORlK3kvRun7bOtwlyMA9PnUeLy01xw69qCQLwVBmJdm/hJB8mRTF8vkzRUt/1qkIjb9Tto92g2qIH9ldixRCvPKlFkepp+bOCN6lWvdTIvEx8s0W2jVWA4NWbjnwqqLvKR0wjQxP9pPG3heaCdvvA==");

                    try
                    {
                        string strReturn = client.ExecuteGet(request).Content;
                        JavaScriptSerializer jss = new JavaScriptSerializer();
                        dynamic data = jss.Deserialize<dynamic>(strReturn);
                        dynamic categories = data["data"]["displayCategories"];

                        int locationNum = 1;
                        foreach (var (Latitude, Longitude) in coordinates)
                        {
                            locationNum++;
                            this.Invoke(new Action(() =>
                            {
                                Lat.Text = Latitude.ToString();
                                Lon.Text = Longitude.ToString();
                                LocationNum.Text = "Location" + locationNum.ToString();
                            }));
                            File.WriteAllText("log.txt", Environment.NewLine + Lat.Text + ", " + Lon.Text);

                            foreach (var category in categories)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    Category.Text = category["text"].ToString();
                                }));
                                int shopcount = 2000;
                                int totalcount = 0;
                                for (int i = 0; i <= (int)(shopcount / 25); i++)
                                {
                                    strUrl = string.Format("https://shopdp-api.baemin.com/v3/BAEMIN/shops?displayCategory={3}&longitude={0}&latitude={1}&sort=SORT__DEFAULT&filter=&offset={2}&limit=25&extension=&perseusSessionId=1718023403008.788454282780365941.FWy8AA9FNv&memberNumber=000000000000&sessionId=b4e3292329dfd570f054c8&carrier=302780&site=7jWXRELC2e&dvcid=OPUD6086af457479a7bb&adid=aede849f-5e9c-499f-827f-cb4e5c65d801&deviceModel=SM-G9500&appver=12.23.0&oscd=2&osver=32&dongCode=11140102&zipCode=04522&ActionTrackingKey=Organic", Longitude.ToString(), Latitude.ToString(), 25 * i, category["code"].ToString());
                                    client = new RestClient(strUrl);
                                    strReturn = client.ExecuteGet(request).Content;
                                    if(strReturn.Contains("SUCCESS"))
                                    {
                                        data = jss.Deserialize<dynamic>(strReturn);
                                        dynamic shops = data["data"]["shops"];
                                        shopcount = (int)data["data"]["totalCount"];

                                        foreach (var shop in shops)
                                        {
                                            totalcount++;
                                            string shopnumber = shop["shopInfo"]["shopNumber"].ToString();
                                            if (!string.IsNullOrEmpty(shopnumber))
                                            {
                                                strUrl = string.Format("https://review-api.baemin.com/v1/shops/{0}/reviews/info?sessionId=1447226b282d5e40f677b5a1d37&carrier=302780&site=7jWXRELC2e&dvcid=OPUD6086af457479a7bb&adid=aede849f-5e9c-499f-827f-cb4e5c65d801&deviceModel=SM-G9500&appver=12.23.0&oscd=2&osver=32&dongCode=11140102&zipCode=04522&ActionTrackingKey=Organic", shopnumber);
                                                client = new RestClient(strUrl);
                                                RestRequest reviewRequest = new RestRequest();
                                                reviewRequest.AddHeader("Accept-Encoding", "gzip, deflate");
                                                reviewRequest.AddHeader("Authorization", "bearer guest");
                                                reviewRequest.AddHeader("Connection", "Keep-Alive");
                                                reviewRequest.AddHeader("Host", "review-api.baemin.com");
                                                reviewRequest.AddHeader("User-Agent", "and1_12.23.0");
                                                reviewRequest.AddHeader("USER-BAEDAL", "W/OnG34HSvOVmxn4McyeRzEK3Ldc9+ruPokFIKgQcm0zVU8aOlNuihy2TNW+7I7ZBORlK3kvRun7bOtwlyMA9PnUeLy01xw69qCQLwVBmJdm/hJB8mRTF8vkzRUt/1qkIjb9Tto92g2qIH9ldixRCvPKlFkepp+bOCN6lWvdTIvEx8s0W2jVWA4NWbjnwqqLvKR0wjQxP9pPG3heaCdvvA==");
                                                strReturn = client.ExecuteGet(reviewRequest).Content;
                                                if (strReturn.Contains("SUCCESS"))
                                                {
                                                    data = jss.Deserialize<dynamic>(strReturn);
                                                    int totalnum = (int)data["data"]["stats"]["reviewCount"];

                                                    for (int j = 0; j <= totalnum / 25; j++)
                                                    {
                                                        strUrl = string.Format("https://review-api.baemin.com/v1/shops/{0}/reviews?sort=RECOMMENDATION&filter=ALL&offset={1}&limit=25&sessionId=1447226b282d5e40f677b5a1d37&carrier=302780&site=7jWXRELC2e&dvcid=OPUD6086af457479a7bb&adid=aede849f-5e9c-499f-827f-cb4e5c65d801&deviceModel=SM-G9500&appver=12.23.0&oscd=2&osver=32&dongCode=11140102&zipCode=04522&ActionTrackingKey=Organic", shopnumber, j * 25);
                                                        client = new RestClient(strUrl);
                                                        string review = client.ExecuteGet(reviewRequest).Content;
                                                        if(review.Contains("SUCCESS"))
                                                        {
                                                            var dir = "Reviews";
                                                            if (!Directory.Exists(dir))
                                                                Directory.CreateDirectory(dir);
                                                            File.WriteAllText(string.Format(@"{0}\review-{1}-{2}-{3}.json", dir, locationNum.ToString(), shopnumber, j.ToString()), review);
                                                            this.Invoke(new Action(() =>
                                                            {
                                                                progressBar1.Value = (int)((10000 * totalcount) / shopcount);
                                                            }));
                                                        }       
                                                    }
                                                }

                                            }
                                        }
                                    }
                                    else
                                    {
                                        shopcount = 0;
                                    }
                                    
                                }

                            }

                        }
                        if (locationNum > 0)
                        {
                            MessageBox.Show("Successfully done!!!");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }

                }));
                th.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

        }
    }
}
