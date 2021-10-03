﻿using CoolapkLite.Core.Helpers;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls
{
    /// <summary>
    /// 基于 Cyenoch 的 MyRichTextBlock 控件和 Coolapk UWP 项目的 TextBlockEx 控件重制
    /// </summary>
    public sealed partial class TextBlockEx : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(TextBlockEx),
            new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnTextChanged))
        );

        /// <summary>
        /// 用于Hyperlink传递href参数
        /// </summary>
        public static readonly DependencyProperty HrefClickParam = DependencyProperty.Register(
            "HrefParam",
            typeof(string),
            typeof(Hyperlink),
            null
        );

        public event RoutedEventHandler LinkClicked;
        private void Hyperlink_Click(Hyperlink sender, HyperlinkClickEventArgs args) => LinkClicked?.Invoke(sender, args);

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public TextBlockEx() => InitializeComponent();

        private void GetTextBlock()
        {
            Paragraph paragraph = new Paragraph();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(Text.Replace("<!--break-->", string.Empty));
            HtmlNodeCollection nodes = doc.DocumentNode.ChildNodes;
            foreach (HtmlNode node in nodes)
            {
                switch (node.NodeType)
                {
                    case HtmlNodeType.Text:
                        Run run = new Run()
                        {
                            Text = WebUtility.HtmlDecode(node.InnerText)
                        };
                        paragraph.Inlines.Add(run);
                        break;
                    case HtmlNodeType.Element:
                        if (node.OriginalName == "a")
                        {
                            Hyperlink hyperlink = new Hyperlink { UnderlineStyle = UnderlineStyle.None };
                            string href = node.GetAttributeValue("href", string.Empty);
                            string type = node.GetAttributeValue("type", string.Empty);
                            string content = node.InnerText;
                            if (!string.IsNullOrEmpty(href))
                            {
                                ToolTipService.SetToolTip(hyperlink, new ToolTip { Content = href });
                            }
                            if (content.IndexOf('@') != 0 && content.IndexOf('#') != 0 && !(type == "user-detail"))
                            {
                                Run run2 = new Run { Text = "\uE167", FontFamily = new FontFamily("Segoe MDL2 Assets") };
                                hyperlink.Inlines.Add(run2);
                            }
                            else if (content == "查看图片" && (href.IndexOf("http://image.coolapk.com", StringComparison.Ordinal) == 0 || href.IndexOf("https://image.coolapk.com", StringComparison.Ordinal) == 0))
                            {
                                content = "查看图片";
                                Run run2 = new Run { Text = "\uE158", FontFamily = new FontFamily("Segoe MDL2 Assets") };
                                hyperlink.Inlines.Add(run2);
                            }
                            Run run3 = new Run { Text = WebUtility.HtmlDecode(content) };
                            hyperlink.Inlines.Add(run3);
                            hyperlink.Click += Hyperlink_Click;
                            hyperlink.SetValue(HrefClickParam, href);
                            paragraph.Inlines.Add(hyperlink);
                        }
                        break;
                }
            }
            RichTextBlock root = new RichTextBlock();
            root.Blocks.Add(paragraph);
            Content = root;
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBlockEx rtb = d as TextBlockEx;
            rtb.GetTextBlock();
        }
    }
}