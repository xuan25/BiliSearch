using Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BiliSearch
{
    /// <summary>
    /// SearchResultBox.xaml 的交互逻辑
    /// </summary>
    public partial class SearchResultBox : UserControl
    {
        public delegate void SelectedDel(string msg);
        public event SelectedDel VedioSelected;
        public event SelectedDel BangumiSelected;
        public event SelectedDel FtSelected;
        public event SelectedDel UserSelected;

        public class Video
        {
            public string Pic;
            public string Title;
            public long Play;
            public long Pubdate;
            public string Author;

            public Video(dynamic json)
            {
                Pic = "https:" + Regex.Unescape(json.pic);
                Title = System.Net.WebUtility.HtmlDecode(Regex.Unescape(json.title));
                Play = json.play;
                Pubdate = json.pubdate;
                Author = Regex.Unescape(json.author);
            }

            public Task<System.Drawing.Bitmap> GetPicAsync()
            {
                Task<System.Drawing.Bitmap> task = new Task<System.Drawing.Bitmap>(() =>
                {
                    return GetPic();
                });
                task.Start();
                return task;
            }

            public System.Drawing.Bitmap GetPic()
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Pic);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(dataStream);
                response.Close();
                dataStream.Close();
                return bitmap;
            }
        }

        public SearchResultBox()
        {
            InitializeComponent();
        }

        public string SearchContent;
        public string NavType;

        public void Search(string text)
        {
            string type = NavType;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("jsonp", "jsonp");
            dic.Add("highlight", "1");
            dic.Add("search_type", type);
            dic.Add("keyword", text);
            string baseUrl = "https://api.bilibili.com/x/web-interface/search/all";
            string payloads = BiliApi.DicToParams(dic, true);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format("{0}?{1}", baseUrl, payloads));
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string result = reader.ReadToEnd();
            reader.Close();
            response.Close();
            dataStream.Close();

            Console.WriteLine(result);

            dynamic json = JsonParser.Parse(result);

            ContentPanel.Children.Clear();

            switch (type)
            {
                case "video":
                    foreach (dynamic v in json.data.result.video)
                    {
                        Video video = new Video(v);
                        ContentPanel.Children.Add(new SearchResultVideo(video));
                    }
                    break;
                case "media_bangumi":
                    break;
                case "media_ft":
                    break;
                case "bili_user":
                    break;
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            NavType = ((RadioButton)sender).Tag.ToString();
        }
    }
}
