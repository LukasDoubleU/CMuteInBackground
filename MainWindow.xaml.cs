using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Input;

namespace CMuteInBackground
{
    public partial class MainWindow
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly WinEventDelegate _dele;
        private readonly BindingList<string> _runningAudioApps = new BindingList<string>();
        private const uint WineventOutofcontext = 0;
        private const uint EventSystemForeground = 3;

        public MainWindow()
        {
            InitializeComponent();

            VolumeMixer.SetApplicationMute("Idle", true);

            _dele = WinEventProc;
            SetWinEventHook(EventSystemForeground, EventSystemForeground, IntPtr.Zero, _dele, 0, 0,
                WineventOutofcontext);

            Apps.ItemsSource = _runningAudioApps;
            Muted.ItemsSource = MutedWindowStorage.Instance.Binding();
            RefreshActiveApps();
        }

        /// <summary>
        /// Called whenever the active window changes
        /// </summary>
        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject,
            int idChild,
            uint dwEventThread, uint dwmsEventTime)
        {
            // Refresh List of Active Audio Apps
            RefreshActiveApps();

            // Mute apps
            var focused = WindowUtil.GetActiveProcessName();
            foreach (var app in MutedWindowStorage.Instance.Binding())
            {
                VolumeMixer.SetApplicationMute(app, !app.Equals(focused));
            }
        }

        private void RefreshActiveApps()
        {
            _runningAudioApps.Clear();
            foreach (var audioProcess in VolumeMixer.GetAudioProcesses())
            {
                _runningAudioApps.Add(audioProcess);
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
            WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject,
            int idChild,
            uint dwEventThread, uint dwmsEventTime);

        private void OnAppViewClicked(object sender, MouseButtonEventArgs e)
        {
            MutedWindowStorage.Instance.Add((sender as DataGrid)?.CurrentItem as string);
        }

        private void OnMutedViewClicked(object sender, MouseButtonEventArgs e)
        {
            MutedWindowStorage.Instance.Remove((sender as DataGrid)?.CurrentItem as string);
        }
    }
}