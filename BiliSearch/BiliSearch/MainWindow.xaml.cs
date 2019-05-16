using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Data;

namespace BiliSearch
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void SearchBox_Search(SearchBox sender, string text)
        {
            ResultBox.SearchAsync(text);
        }
    }

    public class RectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double w = (double)values[0];
            double h = (double)values[1];
            Rect rect;
            if (w > 0 && h > 0)
                rect = new Rect(0, 0, w, h);
            else
                rect = new Rect(0, 0, 1, 1);
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
            Rect rect;
            if (w > 2 && h > 2)
                rect = new Rect(borderThickness, borderThickness, w, h);
            else
                rect = new Rect(borderThickness, borderThickness, 2, 2);
            return rect;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


}
