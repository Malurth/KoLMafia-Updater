
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace KoLMafia_Updater {
    partial class Form1 {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private System.Windows.Forms.Label label;
        private System.Windows.Forms.ProgressBar progressBar;
        private string KoLMafia;
        private Uri downloadUri;
        private string localLatestFullPath;
        private FileStream log;

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.label = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(54, 101);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(599, 46);
            this.progressBar.TabIndex = 0;
            // 
            // label
            // 
            this.label.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label.Location = new System.Drawing.Point(-447, 33);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(1600, 32);
            this.label.TabIndex = 1;
            this.label.Text = "label1";
            this.label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(44)))));
            this.ClientSize = new System.Drawing.Size(706, 180);
            this.Controls.Add(this.label);
            this.Controls.Add(this.progressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "KoLMafia Updater";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private void launchMafia() {
            Process p = new Process();
            p.StartInfo.FileName = "javaw";
            if (localLatestFullPath.Contains("KoLmafia")) {
                p.StartInfo.Arguments = "-jar \"" + localLatestFullPath + "\"";
            } else {
                string[] jars = Directory.GetFiles(Directory.GetCurrentDirectory(), "KoLmafia*.jar");
                if (jars.Length == 1) {
                    p.StartInfo.Arguments = "-jar \"" + jars[0] + "\"";
                } else if (jars.Length == 0) {
                    WriteLog("Unable to find valid KoLmafia .jar to launch. Aborting.");
                    return;
                } else {
                    WriteLog("Found multiple valid KoLmafia .jars to launch, and I don't feel like implementing a function that makes an educated guess on which to launch. Aborting.");
                    return;
                }
            }
            try {
                p.Start();
            } catch (Exception e) {
                WriteLog("An error occured when attempting to start KoLmafia.");
                WriteLog("Exception Message: " + e.Message);
            }
        }

       private bool checkForUpdate() {
            using (WebClient wc = new WebClient()) {
                byte[] page = wc.DownloadData("https://ci.kolmafia.us/job/Kolmafia/lastBuild/");
                string webData = System.Text.Encoding.UTF8.GetString(page);
                string regex = @"KoLmafia-\d+.jar";
                if (Regex.IsMatch(webData, regex)) {
                    KoLMafia = Regex.Match(webData, regex).Value;
                    localLatestFullPath = Directory.GetCurrentDirectory() + "\\" + KoLMafia;
                } else {
                    WriteLog("Failed to find the text we were looking for on the builds page.");
                    done();
                }
                
                if (File.Exists(KoLMafia)) {
                    label.Text = "You're already up to date.";
                    done();
                    return false;
                }
                downloadUri = new Uri("https://ci.kolmafia.us/job/Kolmafia/lastBuild/artifact/dist/" + KoLMafia);
                return true;
            }
        }

        private void done() {
            launchMafia();
            log.Close();
            Application.Exit();
        }

        public void WriteLog(string value) {
            Console.WriteLine(value);
            value = value + "\r\n";
            byte[] info = new UTF8Encoding(true).GetBytes(value);
            log.Write(info, 0, info.Length);
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            progressBar.Value = int.Parse(Math.Truncate(percentage).ToString());
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {
            label.Text = "Download complete!";
            deleteOldCopies();
            done();
        }

        void deleteOldCopies() {
            string[] jars = Directory.GetFiles(Directory.GetCurrentDirectory(), "KoLmafia*.jar");
            if (jars.Length <= 1) {
                return;
            } else {
                foreach (string file in jars) {
                    if (!file.Contains(KoLMafia)) {
                        File.Delete(file);
                    }
                }
            }
        }

        private void downloadFile(Uri uri, string filename) {
            using (WebClient wc = new WebClient()) {
                wc.DownloadFileAsync(uri, filename);
                wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                wc.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                label.Text = $"Downloading latest version ({filename})...";
            }
        }
    }
}

