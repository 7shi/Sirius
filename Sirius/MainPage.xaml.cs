using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Sirius
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();

            Loaded += (sender, e) =>
            {
                HtmlPage.Plugin.Focus();
                textBox1.Focus();
            };

            InitBox(cap1b, cap1f, textBox1);
            InitBox(cap2b, cap2f, textBox2);
            InitBox(cap3b, cap3f, textBox3);
            InitBox(cap4b, cap4f, textBox4);
        }

        public void Write(string format, params object[] args)
        {
            textBox1.Text += string.Format(format, args);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != true) return;

            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();
            var sb3 = new StringBuilder();
            var sb4 = new StringBuilder();
            var sbx = sb1;
            try
            {
                using (var fs = ofd.File.OpenRead())
                using (var br = new BinaryReader(fs))
                {
                    var elf = new ELF64();
                    elf.Read(sb1, br);
                    sbx = sb2;
                    elf.Disassemble(sb2, br);
                }
            }
            catch (Exception ex)
            {
                sbx.AppendLine(ex.Message);
                sbx.AppendLine("読み込みに失敗しました。");
            }
            textBox1.Text = sb1.ToString();
            textBox2.Text = sb2.ToString();
            textBox3.Text = sb3.ToString();
            textBox4.Text = sb4.ToString();
        }

        private void InitBox(Grid bg, TextBlock fg, TextBox tb)
        {
            InactivateCaption(bg, fg);
            tb.GotFocus += (sender, e) => ActivateCaption(bg, fg);
            tb.LostFocus += (sender, e) => InactivateCaption(bg, fg);
            bg.MouseLeftButtonDown += (sender, e) => tb.Focus();
            new WheelObserver(tb);
        }

        private void ActivateCaption(Grid grid, TextBlock text)
        {
            grid.Background = new SolidColorBrush(SystemColors.ActiveCaptionColor);
            text.Foreground = new SolidColorBrush(SystemColors.ActiveCaptionTextColor);
        }

        private void InactivateCaption(Grid grid, TextBlock text)
        {
            grid.Background = new SolidColorBrush(SystemColors.InactiveCaptionColor);
            text.Foreground = new SolidColorBrush(SystemColors.InactiveCaptionTextColor);
        }
    }
}
