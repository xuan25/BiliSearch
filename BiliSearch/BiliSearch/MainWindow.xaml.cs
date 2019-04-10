using System;
using System.Windows;

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

        private void SearchBox_Search(SearchBox sender, string text)
        {
            LogBox.Text = text;
        }

        private void SearchBox_SuggestionsRecieved(SearchBox sender, string text, string json)
        {
            Console.WriteLine("{0} - {1}", text, json);
        }
    }

    
}
