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

            new WheelObserver(textBox1);
            new WheelObserver(textBox2);
            new WheelObserver(textBox3);
            new WheelObserver(textBox4);
        }

        public void Write(string format, params object[] args)
        {
            textBox1.Text += string.Format(format, args);
        }

        private byte[] data;

        private void ReadElf(Stream s)
        {
            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();
            var sbx = sb1;
            try
            {
                data = new byte[s.Length];
                s.Read(data, 0, data.Length);
                btnSave.IsEnabled = true;

                var elf = new ELF64();
                using (var ms = new MemoryStream(data))
                using (var br = new BinaryReader(ms))
                    elf.Read(sb1, br);
                sbx = sb2;
                elf.Disassemble(sb2);
            }
            catch (Exception ex)
            {
                sbx.AppendLine(ex.Message);
                sbx.AppendLine("読み込みに失敗しました。");
            }
            textBox1.Text = sb1.ToString();
            textBox2.Text = sb2.ToString();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != true) return;

            textBox1.Text = textBox2.Text = textBox3.Text = textBox4.Text = "";
            try
            {
                var fi = ofd.File;
                if (fi.Length > 200 * 1024)
                    throw new Exception("ファイルが大き過ぎます。上限は200KBです。");
                using (var fs = ofd.File.OpenRead())
                    ReadElf(fs);
            }
            catch (Exception ex)
            {
                textBox1.Text = ex.Message + Environment.NewLine +
                    "読み込みに失敗しました。" + Environment.NewLine;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            if (sfd.ShowDialog() != true) return;

            using (var fs = sfd.OpenFile())
                fs.Write(data, 0, data.Length);
        }

        private void ReadTest(int t)
        {
            var tb = textBox1;
            textBox1.Text = textBox2.Text = textBox3.Text = textBox4.Text = "";
            try
            {
                var uri1 = new Uri("Test/" + t, UriKind.Relative);
                using (var s = Application.GetResourceStream(uri1).Stream)
                    ReadElf(s);
                tb = textBox3;
                var uri2 = new Uri("Test/" + t + ".c", UriKind.Relative);
                using (var s = Application.GetResourceStream(uri2).Stream)
                using (var sr = new StreamReader(s))
                    textBox3.Text = sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                tb.Text = ex.Message + Environment.NewLine +
                    "読み込みに失敗しました。" + Environment.NewLine;
            }
        }

        private void btnTest1_Click(object sender, RoutedEventArgs e)
        {
            ReadTest(1);
        }
    }
}
