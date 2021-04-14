using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;

namespace CreateQRCodeDemo
{
    public partial class Form1 : Form
    {
        bool IsCorrect;
        public Form1()
        {
            InitializeComponent();
        }
        FilterInfoCollection filterInfoCollection;
        VideoCaptureDevice captureDevice;

        private void btnStart_Click(object sender, EventArgs e)
        {
            captureDevice = new VideoCaptureDevice(filterInfoCollection[cboDevice.SelectedIndex].MonikerString);
            captureDevice.NewFrame += CaptureDevice_NewFrame;
            captureDevice.Start();
            timer1.Start();
        }

        private void CaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo filterInfo in filterInfoCollection)
                cboDevice.Items.Add(filterInfo.Name);
            cboDevice.SelectedIndex = 0;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (captureDevice.IsRunning)
                captureDevice.Stop();    
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(pictureBox1.Image != null)
            {
                BarcodeReader barcodeReader = new BarcodeReader();
                Result result = barcodeReader.Decode((Bitmap)pictureBox1.Image);
                if (result != null)
                {
                    txtQRcode.Text = result.ToString();
                    timer1.Stop();
                    Process.Start(txtQRcode.Text);
                    if (captureDevice.IsRunning)
                        captureDevice.Stop();
                }    
            }    
        }
        
        
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            var url = string.Format("http://chart.apis.google.com/chart?cht=qr&chs={1}x{2}&chl={0}", txtLink.Text, txtWidth.Text, txtHeight.Text);
            WebResponse response = default(WebResponse);
            Stream remoteStream = default(Stream);
            StreamReader readStream = default(StreamReader);
            WebRequest request = WebRequest.Create(url);
            response = request.GetResponse();
            remoteStream = response.GetResponseStream();
            readStream = new StreamReader(remoteStream);
            Image img = Image.FromStream(remoteStream);
            if (IsUrlValid(txtLink.Text))
            {
                IsCorrect = true;
            }
            else
            {
                MessageBox.Show("Format Error!!", "Failed");
                IsCorrect = false;
                return;
            }
            img.Save(@"D:\" + txtLink.Text + ".png");
            picQRcode.Image = img;
            picQRcode.SizeMode = PictureBoxSizeMode.StretchImage;
            response.Close();
            remoteStream.Close();
            readStream.Close();
            txtLink.Text = string.Empty;
            txtWidth.Text = string.Empty;
            txtHeight.Text = string.Empty;
            labelMessage.Text = "The QR Code is generated!";
        }

        private void txtImport_Click(object sender, EventArgs e)
        {
            if (!IsCorrect)
            {
                return;
            }
            BarcodeReader barcodeReader = new BarcodeReader();
            Result result = barcodeReader.Decode((Bitmap)picQRcode.Image);
            txtQRcode.Text = result.ToString();
            Process.Start(xuliLink(txtQRcode.Text));
       
        }
        private String xuliLink(String s)
        {
            String s1 = "http://www";
            if (s.IndexOf(s1) < 0)
            {
                s = s1 + s;
            }
            return s;
        }
        private bool IsUrlValid(string url)
        {
            string pattern = @"^(http|https|ftp|)\://|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";
            Regex reg = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return reg.IsMatch(url);
        }
    }
}
