using System;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConnectToGoServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private Stopwatch stopWatch;

        delegate void UpdateCrossThreadCallback(string e);

        private static int counterSent = 1000;

        private static int counterReceived;

        private void SendPacket(object sender, EventArgs e)
        {
            stopWatch = new Stopwatch();

            stopWatch.Start();

            Parallel.For(0, counterSent, i =>
            {
                //Console.WriteLine(i);
                Task.Factory.StartNew(() => Send("", ReceiveResponse));
            });
        }

        private void Send(string word, UpdateCrossThreadCallback cb)
        {
            try
            {
                //var request = (HttpWebRequest)WebRequest.Create(String.Format(@"http://localhost:8080/myfunc/{0}", textBox1.Text));
                //var request = (HttpWebRequest)WebRequest.Create(@"http://10.1.9.202:8080/key/");
                var request = (HttpWebRequest)WebRequest.Create(@"http://localhost:8080/secure/");


                request.Proxy = null;
                request.Credentials = CredentialCache.DefaultCredentials;

                var HttpWResp = (HttpWebResponse)request.GetResponse();

                // Gets the stream associated with the response.
                var receiveStream = HttpWResp.GetResponseStream();

                var encode = Encoding.GetEncoding("utf-8");

                // Pipes the stream to a higher level stream reader with the required encoding format. 
                var readStream = new StreamReader(receiveStream, encode);

                cb.Invoke("Response stream received.\r\n");

                Char[] read = new Char[256];
                // Reads 256 characters at a time.     
                int count = readStream.Read(read, 0, 256);

                while (count > 0)
                {
                    // Dumps the 256 characters on a string and displays the string to the console.
                    String str = new String(read, 0, count);

                    cb.Invoke(str);

                    Console.WriteLine(str);

                    count = readStream.Read(read, 0, 256);
                }

                counterReceived += 1;

                cb.Invoke("\r\n");

                // Releases the resources of the response.
                HttpWResp.Close();
                // Releases the resources of the Stream.
                readStream.Close();
            }
            catch (Exception ex)
            {
                cb.Invoke(ex.Message);
            }
        }

        private void ReceiveResponse(string e)
        {
            if (textBox2.InvokeRequired)
            {
                UpdateCrossThreadCallback d = new UpdateCrossThreadCallback(ReceiveResponse);

                this.Invoke(d, new object[] { e });
            }
            else
            {
                //textBox2.Text += e;

                //label1.Text = String.Format("Sent {0}, received {1}", counterSent, counterReceived);

                if (counterReceived == counterSent)
                {
                    stopWatch.Stop();

                    TimeSpan ts = stopWatch.Elapsed;

                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

                    label1.Text = elapsedTime; // String.Format("Sent {0}, received {1}", counterSent, counterReceived);
                }

                Application.DoEvents();
            }
        }
    }
}
