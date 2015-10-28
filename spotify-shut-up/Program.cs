using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using api = SpotifyAPI.Local.SpotifyLocalAPI;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Local.Models;
using System.Runtime.InteropServices;
using NAudio;

namespace spotify_shut_up
{
    public partial class Program
    {
        string[] availCmds = { "help", "info", "tctrl", "get", "quit"};

        private SpotifyLocalAPI _spotify;
        private Track _currentTrack;
        private System.Timers.Timer checkAdStateTimer;
        public bool allowInput = true;

        #region Tray stuff
        //http://www.codeproject.com/Questions/354327/Minimizing-Console-Window-to-System-Tray

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();

        static NotifyIcon notifyIcon;
        static IntPtr processHandle;
        static IntPtr WinShell;
        static IntPtr WinDesktop;
        static MenuItem HideMenu;
        static MenuItem RestoreMenu;
        #endregion

        static void Main(string[] args)
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = Properties.Resources.Spotify_128;
            notifyIcon.Text = "Shut up, Spotify!";
            notifyIcon.Visible = true;

            ContextMenu menu = new ContextMenu();
            HideMenu = new MenuItem("Hide", new EventHandler(Minimize_Click));
            RestoreMenu = new MenuItem("Restore", new EventHandler(Maximize_Click));

            menu.MenuItems.Add(RestoreMenu);
            menu.MenuItems.Add(HideMenu);
            menu.MenuItems.Add(new MenuItem("Exit", new EventHandler(CleanExit)));

            notifyIcon.ContextMenu = menu;

            Task.Factory.StartNew(Init);

            processHandle = Process.GetCurrentProcess().MainWindowHandle;
            WinShell = GetShellWindow();
            WinDesktop = GetDesktopWindow();

            ResizeWindow(false);

            Application.Run();
        }

        static void Init()
        {
            new Program().Start();
        }

        public void Start()
        {
            if (!api.IsSpotifyRunning())
            {
                DialogResult q = MessageBox.Show("Spotify is not running! Would you like to start it now?", "Spotify not running!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (q == DialogResult.Yes)
                {
                    api.RunSpotify();
                    if (!api.IsSpotifyWebHelperRunning())
                        api.RunSpotifyWebHelper();

                }
                else if (q == DialogResult.No)
                {
                    Environment.Exit(0);
                }
            }

            _spotify = new api();

            _spotify.Connect();

            _spotify.OnTrackChange += new api.TrackChangeEventHandler(onTrackChange);
            _spotify.ListenForEvents = true;

            _currentTrack = _spotify.GetStatus().Track;

            WriteWithColor("+---------------------------+\n| ######################### |\n| ###### Spotify? ######### |\n| ######### Shut up! ###### |\n| ######################### |\n+---------------------------+", ConsoleColor.Gray);
            WriteWithColor("  Created by iUltimateLP @ GitHub [Compiled 10/28/15]", ConsoleColor.Gray);
            WriteWithColor("\nType 'help' for help.", ConsoleColor.Gray);

            checkAdStateTimer = new System.Timers.Timer();
            checkAdStateTimer.Interval = 500;
            checkAdStateTimer.Elapsed += new System.Timers.ElapsedEventHandler(CheckAdStateTimer_Elapsed);
            checkAdStateTimer.Enabled = true;

            KeepConsoleRunning();
        }

        private void CheckAdStateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_spotify.GetStatus().Track.IsAd())
            {
                SetVolume(0);
            }
            else
            {
                SetVolume(1);
            }
        }

        void onTrackChange(TrackChangeEventArgs e)
        {
            if (e.NewTrack.IsAd())
            {
                WriteWithColor("Ad detected! Muting..", ConsoleColor.Cyan);
            }
        }

        public void KeepConsoleRunning()
        {
            Console.Write("> ");
            string input = Console.ReadLine();
            List<string> args = new List<string>();

            args = input.Split(' ').ToList();

            if (availCmds.Contains(args[0]))
            {
                switch (args[0])
                {
                    case "help":
                        WriteWithColor("help - Displays this screen.", ConsoleColor.DarkYellow);
                        WriteWithColor("info - Displays info.", ConsoleColor.DarkYellow);
                        WriteWithColor("tctrl - Track control command", ConsoleColor.DarkYellow);
                        WriteWithColor("get - gets all infos available", ConsoleColor.DarkYellow);
                        WriteWithColor("quit - Quits this application.", ConsoleColor.DarkYellow);
                        break;
                    case "info":
                        WriteWithColor("Shut up, Spotify! Created by iUltimateLP @ GitHub", ConsoleColor.Cyan);
                        WriteWithColor("--- INFO ---", ConsoleColor.Gray);
                        WriteWithColor("isSpotifyRunning: " + api.IsSpotifyRunning(), ConsoleColor.Gray);
                        WriteWithColor("isSpotifyWebHelperRunning: " + api.IsSpotifyWebHelperRunning(), ConsoleColor.Gray);
                        break;
                    case "tctrl":
                        string[] subs = { "play", "pause", "next", "prev" };
                        if (args.Count == 1 || !subs.Contains(args[1]))
                        {
                            WriteWithColor("Wrong arguments! Usage: tctrl play/pause/next/prev", ConsoleColor.Red);
                            break;
                        }
                        switch (args[1])
                        {
                            case "play":
                                WriteWithColor("Sent play event!", ConsoleColor.Green);
                                _spotify.Play();
                                break;
                            case "pause":
                                WriteWithColor("Sent pause event!", ConsoleColor.Green);
                                _spotify.Pause();
                                break;
                            case "next":
                                WriteWithColor("Sent next track event!", ConsoleColor.Green);
                                _spotify.Skip();
                                break;
                            case "prev":
                                WriteWithColor("Sent previous track event!", ConsoleColor.Green);
                                _spotify.Previous();
                                break;
                            default:
                                break;
                        }
                        break;
                    case "get":
                        if (!_spotify.GetStatus().Track.IsAd())
                        {
                            WriteWithColor("Track Name: " + _currentTrack.TrackResource.Name, ConsoleColor.Green);
                            WriteWithColor("Track Album: " + _currentTrack.AlbumResource.Name, ConsoleColor.Green);
                            WriteWithColor("Track Artist: " + _currentTrack.ArtistResource.Name, ConsoleColor.Green);
                            WriteWithColor("\nPlayback Volume: " + _spotify.GetStatus().Volume, ConsoleColor.Green);
                            WriteWithColor("Client Version: " + _spotify.GetStatus().ClientVersion, ConsoleColor.Green);
                            WriteWithColor("Client online: " + _spotify.GetStatus().Online, ConsoleColor.Green);
                        }
                        else
                        {
                            WriteWithColor("Ad is running", ConsoleColor.Red);
                        }
                        break;
                    case "quit":
                        Environment.Exit(0);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                WriteWithColor("This command is not known! Run 'help' for help!", ConsoleColor.Red);
            }

            KeepConsoleRunning();
        }

        public void WriteWithColor(string str, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void SetVolume(int value)
        {     
            NAudio.CoreAudioApi.MMDeviceEnumerator MMDE = new NAudio.CoreAudioApi.MMDeviceEnumerator();
            NAudio.CoreAudioApi.MMDeviceCollection DevCol = MMDE.EnumerateAudioEndPoints(NAudio.CoreAudioApi.DataFlow.Render, NAudio.CoreAudioApi.DeviceState.Active);

            foreach (NAudio.CoreAudioApi.MMDevice dev in DevCol)
            {
                var sessions = dev.AudioSessionManager.Sessions;
                for(int i = 0; i < sessions.Count; i++)
                {
                    Process process = Process.GetProcessById((int)sessions[i].GetProcessID);
                    if (process.ProcessName == "Spotify")
                    {
                        sessions[i].SimpleAudioVolume.Mute = (value > 0) ? false : true;
                    }
                }
            }
        }

        private static void CleanExit(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
            Environment.Exit(1);
        }

        static void Minimize_Click(object sender, EventArgs e)
        {
            ResizeWindow(false);
        }

        static void Maximize_Click(object sender, EventArgs e)
        {
            ResizeWindow();
        }

        static void ResizeWindow(bool Restore = true)
        {
            if (Restore)
            {
                RestoreMenu.Enabled = false;
                HideMenu.Enabled = true;
                SetParent(processHandle, WinDesktop);
            }
            else
            {
                RestoreMenu.Enabled = true;
                HideMenu.Enabled = false;
                SetParent(processHandle, WinShell);
            }
        }

    }
}
