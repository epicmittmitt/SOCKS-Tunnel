using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SOCKS_Tunnel {
    public partial class Form1 : Form {
        string PlinkPath = Path.Combine(Path.GetTempPath(), "plink.exe");
        byte tries = 0;

        public Form1() {
            InitializeComponent();

            Shown += delegate { (new Thread(CheckForPlinkUpdates)).Start(); };
        }

        public void AddLogMessage(string message) {
            textBox1.Invoke((MethodInvoker)delegate {
                textBox1.AppendText(message + Environment.NewLine);
            });
        }

        public void CheckForPlinkUpdates() {
            AddLogMessage("Checking for binary updates ...");
            WebClient wc = new WebClient();
            if (File.Exists(PlinkPath)) {
                if (tries++ > 3) {
                    AddLogMessage("Attempts to update the binary failed. Will use existing binary.");
                    return;
                }
                AddLogMessage("Existing binary found.");
                string LocalMD5 = string.Empty;
                AddLogMessage("Generating MD5 checksum of existing binary ...");
                using (var cs = MD5.Create()) {
                    using (var stream = File.OpenRead(PlinkPath)) {
                        LocalMD5 = BitConverter.ToString(cs.ComputeHash(stream)).Replace("-", "‌​").ToLower().ToString().Trim();
                        AddLogMessage("   " + LocalMD5);
                    }
                }
                AddLogMessage("Fetching MD5 checksum of online binary ...");
                string RemoteMD5 = Regex.Match(wc.DownloadString(@"https://the.earth.li/~sgtatham/putty/0.67/md5sums"), @"(.*?)[ ]*x86\/plink\.exe").Groups[1].Value.ToString().Trim();
                AddLogMessage("   " + RemoteMD5);
                if (LocalMD5 == RemoteMD5) {
                    AddLogMessage("No updates found." + Environment.NewLine);
                    return;
                }
                AddLogMessage("Update available. Downloading ...");
            } else { AddLogMessage("Binary not present. Downloading ..."); }
            wc.DownloadFile(@"https://the.earth.li/~sgtatham/putty/latest/x86/plink.exe", PlinkPath);
            CheckForPlinkUpdates();
        }
    }
}
