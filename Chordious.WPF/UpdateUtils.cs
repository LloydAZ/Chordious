﻿// 
// UpdateUtils.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015 Jon Thysell <http://jonthysell.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using System.Windows;

using com.jonthysell.Chordious.Core.ViewModel;

namespace com.jonthysell.Chordious.WPF
{
    public class UpdateUtils
    {
        public static AppViewModel AppVM
        {
            get
            {
                return AppViewModel.Instance;
            }
        }

        public static void UpdateCheck(bool confirmUpdate, bool showUpToDate)
        {
            List<InstallerInfo> installerInfos = GetLatestInstallerInfos();

            ReleaseChannel targetReleaseChannel = GetDefaultReleaseChannel();

            ulong currentVersion = LongVersion(AppVM.FullVersion);
            
            InstallerInfo latestVersion = null;

            bool updateAvailable = false;
            foreach (InstallerInfo installerInfo in installerInfos)
            {
                if (installerInfo.ReleaseChannel == targetReleaseChannel)
                {
                    ulong installerVersion = LongVersion(installerInfo.Version);

                    if (installerVersion > currentVersion)
                    {
                        updateAvailable = true;
                        latestVersion = installerInfo;
                    }
                }
            }

            bool doUpdate = false;

            if (!updateAvailable)
            {
                if (showUpToDate)
                {
                    MessageBox.Show("Chordious is up-to-date.", "Chordious", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                doUpdate = true;
                if (confirmUpdate)
                {
                    string message = String.Format("Chordious {0} is available. Would you like to update now?", latestVersion.Version);
                    MessageBoxResult result = MessageBox.Show(message, "Chordious", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    doUpdate = (result == MessageBoxResult.Yes);
                }
            }

            if (doUpdate)
            {
                Update(latestVersion);
            }
        }

        private static void Update(InstallerInfo installerInfo)
        {
            if (null == installerInfo)
            {
                throw new ArgumentNullException("installerInfo");
            }

            string tempPath = Path.GetTempPath();

            string msiPath = Path.Combine(tempPath, "ChordiousSetup.msi");

            if (File.Exists(msiPath))
            {
                File.Delete(msiPath);
            }

            using (WebClient client = new WebClient())
            {
                client.DownloadFile(installerInfo.Url, msiPath);
            }

            string cmdFile = Path.Combine(tempPath, "UpdateChordious.cmd");

            using (StreamWriter sw = new StreamWriter(new FileStream(cmdFile, FileMode.Create)))
            {
                sw.WriteLine("msiexec /i \"{0}\"", msiPath);
            }

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo("cmd.exe", String.Format("/c {0}", cmdFile));
            p.StartInfo.CreateNoWindow = true;
            p.Start();

            App.Current.Shutdown();
        }

        public static List<InstallerInfo> GetLatestInstallerInfos()
        {
            List<InstallerInfo> installerInfos = new List<InstallerInfo>();

            using (XmlReader reader = XmlReader.Create(WebRequest.Create(_updateUrl).GetResponse().GetResponseStream()))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        if (reader.Name == "update")
                        {
                            string version = reader.GetAttribute("version");
                            string url = reader.GetAttribute("url");
                            ReleaseChannel releaseChannel = (ReleaseChannel)Enum.Parse(typeof(ReleaseChannel), reader.GetAttribute("channel"));
                            installerInfos.Add(new InstallerInfo(version, url, releaseChannel));
                        }
                    }
                }
            }

            return installerInfos;
        }

        public static ulong LongVersion(string version)
        {
            if (String.IsNullOrWhiteSpace(version))
            {
                throw new ArgumentNullException("version");
            }

            ulong vers = 0;

            string[] parts = version.Trim().Split('.');

            for (int i = 0; i < parts.Length; i++)
            {
                vers |= (ulong.Parse(parts[i]) << ((4 - (i + 1)) * 16));
            }

            return vers;
        }

        public static ReleaseChannel GetDefaultReleaseChannel()
        {
            ReleaseChannel result;

            if (Enum.TryParse<ReleaseChannel>(AppVM.GetSetting("app.releasechannel"), out result))
            {
                return result;
            }

            return ReleaseChannel.Official;
        }

        public static bool GetUpdateCheckOnStart()
        {
            bool result;

            if (Boolean.TryParse(AppVM.GetSetting("app.checkupdateonstart"), out result))
            {
                return result;
            }

            return false;
        }

        private const string _updateUrl = "http://update.chordious.com";
    }

    public class InstallerInfo
    {
        public string Version { get; private set; }

        public string Url { get; private set; }

        public ReleaseChannel ReleaseChannel { get; private set; }

        public InstallerInfo(string version, string url, ReleaseChannel releaseChannel)
        {
            if (String.IsNullOrWhiteSpace(version))
            {
                throw new ArgumentNullException("version");
            }

            if (String.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException("url");
            }

            Version = version.Trim();
            Url = url.Trim();
            ReleaseChannel = releaseChannel;
        }
    }

    public enum ReleaseChannel
    {
        Preview,
        Official
    }
}