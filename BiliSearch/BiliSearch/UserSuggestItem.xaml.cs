using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace BiliSearch
{
    /// <summary>
    /// UserSuggestItem.xaml 的交互逻辑
    /// </summary>
    public partial class UserSuggestItem : UserControl
    {
        public UserSuggestItem(SearchBox.UserSuggest userSuggest)
        {
            InitializeComponent();

            if (TitleInline.Text != null)
                TitleInline.Text = userSuggest.Title;

            FansInline.Text = string.Format("{0:0}粉丝", FormatNum(userSuggest.Fans, false)).PadRight(10, ' ');

            ArchivesInline.Text = string.Format("{0:0}个视频", FormatNum(userSuggest.Archives, false));

            this.Loaded += async delegate (object senderD, RoutedEventArgs eD)
            {
                System.Drawing.Bitmap bitmap = await userSuggest.GetCoverAsync();
                ImageBox.Source = BitmapToImageSource(bitmap);
            };
        }

        private BitmapSource BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            IntPtr ip = bitmap.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return bitmapSource;
        }

        public static string FormatNum(long number, bool decimalPlaces)
        {
            if(number < 10000)
            {
                return number.ToString();
            }
            else if(number < 100000000)
            {
                if(decimalPlaces)
                    return ((double)number / 10000) + "万";
                else
                    return (number / 10000) + "万";
            }
            else
            {
                if (decimalPlaces)
                    return ((double)number / 100000000) + "亿";
                else
                    return (number / 100000000) + "亿";
            }
        }
    }
}
