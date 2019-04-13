﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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

        private async void SearchBox_Search(SearchBox sender, string text)
        {
            await ResultBox.SearchAsync(text);
        }

        private void SearchBox_SuggestionsRecieved(SearchBox sender, string text, string json)
        {
            Console.WriteLine("{0} - {1}", text, json);
        }
    }

    
}
