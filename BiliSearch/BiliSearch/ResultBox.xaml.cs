﻿using Json;
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
    public partial class ResultBox : UserControl
    {
        public delegate void SelectedDel(long id);
        public event SelectedDel VedioSelected;
        public event SelectedDel SeasonSelected;

        public class Video
        {
            public string Pic;
            public string Title;
            public long Play;
            public long Pubdate;
            public string Author;
            public long Aid;

            public Video(IJson json)
            {
                Pic = "https:" + Regex.Unescape(json.GetValue("pic").ToString());
                Title = System.Net.WebUtility.HtmlDecode(Regex.Unescape(json.GetValue("title").ToString()));
                Play = json.GetValue("play").ToLong();
                if(json.Contains("pubdate"))
                    Pubdate = json.GetValue("pubdate").ToLong();
                else
                    Pubdate = json.GetValue("created").ToLong();
                Author = Regex.Unescape(json.GetValue("author").ToString());
                Aid = json.GetValue("aid").ToLong();
            }

            public Task<System.Drawing.Bitmap> GetPicAsync()
            {
                return BiliApi.GetImageAsync(Pic);
            }
        }

        public class Season
        {
            public string Cover;
            public string Title;
            public string Styles;
            public string Areas;
            public long Pubtime;
            public string Cv;
            public string Description;
            public long SeasonId;
            public string SeasonTypeName;
            public string OrgTitle;

            public Season(IJson json, IJson cardsJson)
            {
                Cover = "https:" + Regex.Unescape(json.GetValue("cover").ToString());
                Title = System.Net.WebUtility.HtmlDecode(Regex.Unescape(json.GetValue("title").ToString()));
                Styles = Regex.Unescape(json.GetValue("styles").ToString());
                Areas = Regex.Unescape(json.GetValue("areas").ToString());
                Pubtime = json.GetValue("pubtime").ToLong();
                Cv = Regex.Unescape(json.GetValue("cv").ToString());
                Description = Regex.Unescape(json.GetValue("desc").ToString());
                SeasonId = json.GetValue("season_id").ToLong();
                SeasonTypeName = cardsJson.GetValue("result").GetValue(SeasonId.ToString()).GetValue("season_type_name").ToString();
                OrgTitle = System.Net.WebUtility.HtmlDecode(Regex.Unescape(json.GetValue("org_title").ToString()));
            }

            public Task<System.Drawing.Bitmap> GetCoverAsync()
            {
                return BiliApi.GetImageAsync(Cover);
            }
        }

        public class User
        {
            public long Mid;
            public string Upic;
            public string Uname;
            public long Videos;
            public long Fans;
            public string Usign;

            public User(IJson json)
            {
                Mid = json.GetValue("mid").ToLong();
                Upic = "https:" + Regex.Unescape(json.GetValue("upic").ToString());
                Uname = Regex.Unescape(json.GetValue("uname").ToString());
                Videos = json.GetValue("videos").ToLong();
                Fans = json.GetValue("fans").ToLong();
                Usign = Regex.Unescape(json.GetValue("usign").ToString());
            }

            public Task<System.Drawing.Bitmap> GetPicAsync()
            {
                return BiliApi.GetImageAsync(Upic);
            }
        }

        public ResultBox()
        {
            InitializeComponent();
        }

        public string SearchText;
        public string NavType;
        public RadioButton TypeBtn;

        private CancellationTokenSource cancellationTokenSource;
        public Task SearchAsync(string text)
        {
            ContentViewer.ScrollToHome();
            TypeBtn.IsChecked = true;
            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();
                
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            ContentPanel.Children.Clear();
            LoadingPrompt.Visibility = Visibility.Visible;
            Task task = new Task(() =>
            {
                string type = NavType;
                IJson json = GetResult(text, type);
                Dispatcher.Invoke(new Action(() =>
                {
                    ShowResult(json, type);
                    LoadingPrompt.Visibility = Visibility.Hidden;
                }));
            }, cancellationTokenSource.Token);
            task.Start();
            return task;
        }

        public void Search(string text)
        {
            ContentViewer.ScrollToHome();
            TypeBtn.IsChecked = true;
            ContentPanel.Children.Clear();
            string type = NavType;
            IJson json = GetResult(text, type);
            ShowResult(json, type);
        }

        private IJson GetResult(string text, string type)
        {
            SearchText = text;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("jsonp", "jsonp");
            dic.Add("highlight", "1");
            dic.Add("search_type", type);
            dic.Add("keyword", text);
            IJson json = BiliApi.GetJsonResult("https://api.bilibili.com/x/web-interface/search/type", dic);
            return json;
        }

        private async void ShowResult(IJson json, string type)
        {
            if(((JsonArray)json.GetValue("data").GetValue("result")).Count > 0)
                switch (type)
                {
                    case "video":
                        foreach (IJson v in json.GetValue("data").GetValue("result"))
                        {
                            Video video = new Video(v);
                            ResultVideo resultVideo = new ResultVideo(video);
                            resultVideo.PreviewMouseLeftButtonDown += ResultVideo_PreviewMouseLeftButtonDown;
                            ContentPanel.Children.Add(resultVideo);
                        }
                        break;
                    case "media_bangumi":
                        StringBuilder stringBuilder = new StringBuilder();
                        foreach (IJson v in json.GetValue("data").GetValue("result"))
                        {
                            stringBuilder.Append(',');
                            stringBuilder.Append(v.GetValue("season_id").ToString());
                        }
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        dic.Add("season_ids", stringBuilder.ToString().Substring(1));
                        IJson cardsJson = await BiliApi.GetJsonResultAsync("https://api.bilibili.com/pgc/web/season/cards", dic);
                        foreach (IJson v in json.GetValue("data").GetValue("result"))
                        {
                            Season season = new Season(v, cardsJson);
                            ResultSeason resultSeason = new ResultSeason(season);
                            resultSeason.PreviewMouseLeftButtonDown += ResultSeason_PreviewMouseLeftButtonDown;
                            ContentPanel.Children.Add(resultSeason);
                        }
                        break;
                    case "media_ft":
                        StringBuilder stringBuilder1 = new StringBuilder();
                        foreach (IJson v in json.GetValue("data").GetValue("result"))
                        {
                            stringBuilder1.Append(',');
                            stringBuilder1.Append(v.GetValue("season_id").ToString());
                        }
                        Dictionary<string, string> dic1 = new Dictionary<string, string>();
                        dic1.Add("season_ids", stringBuilder1.ToString().Substring(1));
                        IJson cardsJson1 = await BiliApi.GetJsonResultAsync("https://api.bilibili.com/pgc/web/season/cards", dic1);
                        foreach (IJson v in json.GetValue("data").GetValue("result"))
                        {
                            Season season = new Season(v, cardsJson1);
                            ResultSeason resultSeason = new ResultSeason(season);
                            resultSeason.PreviewMouseLeftButtonDown += ResultSeason_PreviewMouseLeftButtonDown;
                            ContentPanel.Children.Add(new ResultSeason(season));
                        }
                        break;
                    case "bili_user":
                        foreach (IJson v in json.GetValue("data").GetValue("result"))
                        {
                            User user = new User(v);
                            ResultUser resultUser = new ResultUser(user);
                            resultUser.PreviewMouseLeftButtonDown += ResultUser_PreviewMouseLeftButtonDown;
                            ContentPanel.Children.Add(resultUser);
                        }
                        break;
                }
        }

        private void ResultUser_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TypeBtn.IsChecked = false;
            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();

            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            ContentPanel.Children.Clear();
            LoadingPrompt.Visibility = Visibility.Visible;
            Task task = new Task(() =>
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("mid", ((ResultUser)sender).Mid.ToString());
                dic.Add("pagesize", "30");
                dic.Add("page", "1");
                IJson json = BiliApi.GetJsonResult("https://space.bilibili.com/ajax/member/getSubmitVideos", dic);
                Dispatcher.Invoke(new Action(() =>
                {
                    foreach(IJson v in json.GetValue("data").GetValue("vlist"))
                    {
                        Video video = new Video(v);
                        ResultVideo resultVideo = new ResultVideo(video);
                        resultVideo.PreviewMouseLeftButtonDown += ResultVideo_PreviewMouseLeftButtonDown;
                        ContentPanel.Children.Add(resultVideo);
                    }
                    LoadingPrompt.Visibility = Visibility.Hidden;
                }));
            }, cancellationTokenSource.Token);
            task.Start();
        }

        private void ResultSeason_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SeasonSelected?.Invoke(((ResultSeason)sender).SeasonId);
        }

        private void ResultVideo_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            VedioSelected?.Invoke(((ResultVideo)sender).Aid);
        }

        private async void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            TypeBtn = (RadioButton)sender;
            NavType = ((RadioButton)sender).Tag.ToString();
            if (SearchText != null && SearchText != "")
            {
                await SearchAsync(SearchText);
            }
        }
    }
}
