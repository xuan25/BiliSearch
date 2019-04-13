using Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

            public Video(IJson json)
            {
                Pic = "https:" + Regex.Unescape(json.GetValue("pic").ToString());
                Title = System.Net.WebUtility.HtmlDecode(Regex.Unescape(json.GetValue("title").ToString()));
                Play = json.GetValue("play").ToLong();
                Pubdate = json.GetValue("pubdate").ToLong();
                Author = Regex.Unescape(json.GetValue("author").ToString());
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

        public class Bangumi
        {
            public string Cover;
            public string Title;
            public string Styles;
            public string Areas;
            public long Pubtime;
            public string Cv;
            public string Description;

            public Bangumi(IJson json)
            {
                Cover = "https:" + Regex.Unescape(json.GetValue("cover").ToString());
                Title = System.Net.WebUtility.HtmlDecode(Regex.Unescape(json.GetValue("title").ToString()));
                Styles = Regex.Unescape(json.GetValue("styles").ToString());
                Areas = Regex.Unescape(json.GetValue("areas").ToString());
                Pubtime = json.GetValue("pubtime").ToLong();
                Cv = Regex.Unescape(json.GetValue("cv").ToString());
                Description = Regex.Unescape(json.GetValue("desc").ToString());
            }

            public Task<System.Drawing.Bitmap> GetCoverAsync()
            {
                Task<System.Drawing.Bitmap> task = new Task<System.Drawing.Bitmap>(() =>
                {
                    return GetCover();
                });
                task.Start();
                return task;
            }

            public System.Drawing.Bitmap GetCover()
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Cover);
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

        public string SearchText;
        public string NavType;

        private CancellationTokenSource cancellationTokenSource;
        public Task SearchAsync(string text)
        {
            if (cancellationTokenSource != null)
                try
                {
                    cancellationTokenSource.Cancel();
                }
                catch (Exception)
                {

                }
                
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            Task task = new Task(() =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    ContentPanel.Children.Clear();
                    LoadingPrompt.Visibility = Visibility.Visible;
                }));
                string type = NavType;
                IJson json = GetResult(text, type);
                switch (type)
                {
                    case "video":
                        foreach (IJson v in json.GetValue("data").GetValue("result"))
                        {
                            Video video = new Video(v);
                            Dispatcher.Invoke(new Action(() =>
                            {
                                ContentPanel.Children.Add(new SearchResultVideo(video));
                            }));
                        }
                        break;
                    case "media_bangumi":
                        foreach (IJson v in json.GetValue("data").GetValue("result"))
                        {
                            Bangumi bangumi = new Bangumi(v);
                            Dispatcher.Invoke(new Action(() =>
                            {
                                ContentPanel.Children.Add(new SearchResultBangumi(bangumi));
                            }));
                            
                        }
                        break;
                    case "media_ft":
                        break;
                    case "bili_user":
                        break;
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    LoadingPrompt.Visibility = Visibility.Hidden;
                }));
            }, cancellationTokenSource.Token);
            task.Start();
            return task;
        }

        public void Search(string text)
        {
            ContentPanel.Children.Clear();
            string type = NavType;
            IJson json = GetResult(text, type);
            switch (type)
            {
                case "video":
                    foreach (IJson v in json.GetValue("data").GetValue("result"))
                    {
                        Video video = new Video(v);
                        ContentPanel.Children.Add(new SearchResultVideo(video));
                    }
                    break;
                case "media_bangumi":
                    foreach (IJson v in json.GetValue("data").GetValue("result"))
                    {
                        Bangumi bangumi = new Bangumi(v);
                        ContentPanel.Children.Add(new SearchResultBangumi(bangumi));
                    }
                    break;
                case "media_ft":
                    break;
                case "bili_user":
                    break;
            }
        }

        private IJson GetResult(string text, string type)
        {
            SearchText = text;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("jsonp", "jsonp");
            dic.Add("highlight", "1");
            dic.Add("search_type", type);
            dic.Add("keyword", text);
            string baseUrl = "https://api.bilibili.com/x/web-interface/search/type";
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

            IJson json = JsonParser.Parse(result);
            return json;
        }

        private async void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            NavType = ((RadioButton)sender).Tag.ToString();
            if (SearchText != null && SearchText != "")
            {
                await SearchAsync(SearchText);
            }
        }
    }
}
