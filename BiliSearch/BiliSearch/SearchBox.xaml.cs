using Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace BiliSearch
{
    /// <summary>
    /// SearchBox.xaml 的交互逻辑
    /// </summary>
    public partial class SearchBox : UserControl
    {
        public delegate void SearchDel(SearchBox sender, string text);
        public event SearchDel Search;

        public delegate void SuggestionsRecievedDel(SearchBox sender, string text, string json);
        public event SuggestionsRecievedDel SuggestionsRecieved;

        public static readonly DependencyProperty SuggestDelayProperty = DependencyProperty.Register("SuggestDelay", typeof(int), typeof(SearchBox), new FrameworkPropertyMetadata(100));
        public int SuggestDelay
        {
            get
            {
                return (int)GetValue(SuggestDelayProperty);
            }
            set
            {
                SetValue(SuggestDelayProperty, value);
            }
        }
        private void SuggestDelayChanged(object sender, EventArgs e)
        {
            
        }

        public SearchBox()
        {
            InitializeComponent();

            DependencyPropertyDescriptor SuggestDelayPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(SuggestDelayProperty, typeof(SearchBox));
            SuggestDelayPropertyDescriptor.AddValueChanged(this, SuggestDelayChanged);
        }

        public class SeasonSuggest : Suggest
        {
            public string Cover;
            public string Uri;
            public long Ptime;
            public string SeasonTypeName;
            public string Area;
            public string Label;

            public SeasonSuggest(dynamic item)
            {
                Position = (uint)item.position;
                if (item.Contains("title"))
                    Title = item.title;
                else
                    Title = null;
                Keyword = item.keyword;
                Cover = Regex.Unescape("https:" + item.Cover);
                Uri = Regex.Unescape(item.uri);
                Ptime = item.ptime;
                SeasonTypeName = item.season_type_name;
                Area = item.area;
                if (item.Contains("label"))
                    Label = item.label;
                else
                    Label = null;
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

        public class UserSuggest : Suggest
        {
            public string Cover;
            public string Uri;
            public uint Level;
            public long Fans;
            public long Archives;

            public UserSuggest(dynamic item)
            {
                Position = (uint)item.position;
                Title = item.title;
                Keyword = item.keyword;
                Cover = Regex.Unescape("https:" + item.Cover);
                Uri = Regex.Unescape(item.uri);
                Level = (uint)item.level;
                Fans = item.fans;
                Archives = item.archives;
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

        public class Suggest
        {
            public uint Position;
            public string Title;
            public string Keyword;
            public string Type;

            public Suggest()
            {

            }

            public Suggest(dynamic item)
            {
                Position = (uint)item.position;

                if (item.Contains("title"))
                    Title = item.title;
                else
                    Title = null;

                Keyword = item.keyword;

                if (item.Contains("sug_type"))
                    Type = item.sug_type;
                else
                    Type = null;
            }
        }

        private BitmapSource BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            IntPtr ip = bitmap.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return bitmapSource;
        }

        private async void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsInitialized && InputBox.IsFocused)
            {
                List<Suggest> suggests = null;
                try
                {
                    suggests = await GetSuggestAsync(InputBox.Text, SuggestDelay);
                }
                catch (AggregateException ex)
                {
                    
                }
                
                SuggestList.Items.Clear();
                if (suggests != null)
                {
                    SuggestList.Visibility = Visibility.Visible;
                    foreach (Suggest suggest in suggests)
                    {
                        ListBoxItem listBoxItem = new ListBoxItem();
                        listBoxItem.VerticalAlignment = VerticalAlignment.Stretch;
                        if (suggest.GetType() == typeof(Suggest))
                        {
                            listBoxItem.Content = new SuggestItem(suggest);
                            listBoxItem.Tag = suggest.Keyword;
                        }
                        else if (suggest.GetType() == typeof(SeasonSuggest))
                        {
                            SeasonSuggest seasonSuggest = (SeasonSuggest)suggest;
                            listBoxItem.Content = new SeasonSuggestItem(seasonSuggest);
                            listBoxItem.Tag = seasonSuggest.Uri;
                        }
                        else if (suggest.GetType() == typeof(UserSuggest))
                        {
                            UserSuggest userSuggest = (UserSuggest)suggest;
                            listBoxItem.Content = new UserSuggestItem(userSuggest);
                            listBoxItem.Tag = userSuggest.Uri;
                        }
                        SuggestList.Items.Add(listBoxItem);
                    }
                }
                else
                    SuggestList.Visibility = Visibility.Hidden;
            }
            else
                SuggestList.Visibility = Visibility.Hidden;
        }

        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SearchBtn.Focus();
            InputBox.Text = ((ListBoxItem)sender).Tag.ToString();
            Search?.Invoke(this, InputBox.Text);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SuggestList.Visibility = Visibility.Hidden;
            Search?.Invoke(this, InputBox.Text);
        }

        private CancellationTokenSource cancellationTokenSource;
        private Task<List<Suggest>> GetSuggestAsync(string text, int delay)
        {
            if(cancellationTokenSource != null)
                cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            Task<List<Suggest>> task = new Task<List<Suggest>>(() =>
            {
                if (text == null || text == "")
                    return null;
                Thread.Sleep(delay);
                if (cancellationToken.IsCancellationRequested)
                    return null;
                List<Suggest> result = GetSuggest(text);
                if (cancellationToken.IsCancellationRequested)
                    return null;
                return result;
            }, cancellationTokenSource.Token);
            task.Start();
            return task;
        }

        private List<Suggest> GetSuggest(string text)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("highlight", "1");
            dic.Add("keyword", text);
            string baseUrl = "https://app.bilibili.com/x/v2/search/suggest3";
            string payloads = BiliApi.DicToParams(dic, true);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format("{0}?{1}", baseUrl, payloads));
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string result = reader.ReadToEnd();
            reader.Close();
            response.Close();
            dataStream.Close();

            SuggestionsRecieved?.Invoke(this, text, result);
            dynamic json = JsonParser.Parse(result);

            if (json.data.Contains("list"))
            {
                List<Suggest> suggests = new List<Suggest>();
                foreach (dynamic i in json.data.list)
                {
                    if (!i.Contains("sug_type"))
                    {
                        Suggest suggest = new Suggest(i);
                        suggests.Add(suggest);
                    }
                    else if(i.sug_type == "pgc")
                    {
                        SeasonSuggest seasonSuggest = new SeasonSuggest(i);
                        suggests.Add(seasonSuggest);
                    }
                    else if (i.sug_type == "user")
                    {
                        UserSuggest userSuggest = new UserSuggest(i);
                        suggests.Add(userSuggest);
                    }
                    else
                    {
                        Suggest suggest = new Suggest(i);
                        suggests.Add(suggest);
                    }
                }
                suggests.Sort((x, y) => x.Position.CompareTo(y.Position));
                return suggests;
            }
            return null;

        }

        private void InputBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                SuggestList.Focus();
                SuggestList.SelectedIndex = 0;
                e.Handled = true;
            }
            else if(e.Key == Key.Enter)
            {
                SearchBtn.Focus();
                SuggestList.Visibility = Visibility.Hidden;
                Search?.Invoke(this, InputBox.Text);
            }
        }

        private void SuggestList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                if(SuggestList.SelectedIndex < SuggestList.Items.Count - 1)
                    SuggestList.SelectedIndex++;
                e.Handled = true;
            }
            else if(e.Key == Key.Up)
            {
                SuggestList.SelectedIndex--;
                if (SuggestList.SelectedIndex == -1)
                    InputBox.Focus();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                SearchBtn.Focus();
                InputBox.Text = ((ListBox)sender).SelectedItem.ToString();
                Search?.Invoke(this, InputBox.Text);
                e.Handled = true;
            }
            e.Handled = true;
        }
    }

    public class RectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double w = (double)values[0];
            double h = (double)values[1];
            Rect rect = new Rect(0, 0, w, h);
            return rect;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class BorderRectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double borderThickness = 1;
            double w = (double)values[0] - 2 * borderThickness;
            double h = (double)values[1] - 2 * borderThickness;
            if (w > 2 && h > 2)
            {
                Rect rect = new Rect(borderThickness, borderThickness, w, h);
                return rect;
            }
            else
            {
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
