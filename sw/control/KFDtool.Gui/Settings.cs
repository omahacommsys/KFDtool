using KFDtool.Container;
using KFDtool.P25.TransferConstructs;
using System.Diagnostics;
using System.Reflection;
using System;
using FramePFX.Themes;

namespace KFDtool.Gui
{
    class Settings
    {
        public static string AssemblyVersion { get; private set; }

        public static string AssemblyFileVersion { get; private set; }

        public static string AssemblyInformationalVersion { get; private set; }

        public static string ScreenCurrent { get; set; }

        public static bool ScreenInProgress { get; set; }

        public static bool ContainerOpen { get; set; }

        public static bool ContainerSaved { get; set; }

        public static string ContainerPath { get; set; }

        public static byte[] ContainerKey { get; set; }

        public static OuterContainer ContainerOuter { get; set; }

        public static InnerContainer ContainerInner { get; set; }

        public static BaseDevice SelectedDevice { get; set; }

        public enum ThemeMode
        {
            System,
            Dark,
            Light
        }

        public static ThemeMode SelectedTheme { get; set; }

        static Settings()
        {
            //AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            //AssemblyInformationalVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

            // Versions are now pulled from the git tags
            AssemblyVersion = ThisAssembly.Git.BaseVersion.Major + "." + ThisAssembly.Git.BaseVersion.Minor + "." + ThisAssembly.Git.BaseVersion.Patch;
            AssemblyFileVersion = ThisAssembly.Git.SemVer.Major + "." + ThisAssembly.Git.SemVer.Minor + "." + ThisAssembly.Git.SemVer.Patch;
            AssemblyInformationalVersion = ThisAssembly.Git.BaseVersion.Major + "." +
                ThisAssembly.Git.BaseVersion.Minor + "." +
                ThisAssembly.Git.BaseVersion.Patch + "." +
                ThisAssembly.Git.Commits + "-" +
                ThisAssembly.Git.Branch + "+" +
                ThisAssembly.Git.Commit;

            ScreenCurrent = string.Empty;
            ScreenInProgress = false;
            ContainerOpen = false;
            ContainerSaved = false;
            ContainerPath = string.Empty;
            ContainerKey = null;
            ContainerInner = null;
            ContainerOuter = null;

            SelectedDevice = new BaseDevice();

            SelectedDevice.TwiKfdtoolDevice = new TwiKfdtoolDevice();
            SelectedDevice.DliIpDevice = new DliIpDevice();
            SelectedDevice.DliIpDevice.Protocol = DliIpDevice.ProtocolOptions.UDP;

            SelectedTheme = ThemeMode.System;

            //LoadSettings();
        }

        public static void InitSettings()
        {
            Properties.Settings.Default.TwiComPort = "";
            Properties.Settings.Default.DliHostname = "192.168.128.1";
            Properties.Settings.Default.DliPort = 49644;
            Properties.Settings.Default.DliVariant = "Motorola";
            Properties.Settings.Default.DeviceType = "TwiKfdDevice";
            Properties.Settings.Default.KfdDeviceType = "Kfdshield";
            Properties.Settings.Default.SelectedTheme = "System";
            Properties.Settings.Default.Save();
        }

        public static void SaveSettings()
        {
            Properties.Settings.Default.TwiComPort = SelectedDevice.TwiKfdtoolDevice.ComPort;
            Properties.Settings.Default.DliHostname = SelectedDevice.DliIpDevice.Hostname;
            Properties.Settings.Default.DliPort = SelectedDevice.DliIpDevice.Port;
            Properties.Settings.Default.DliVariant = SelectedDevice.DliIpDevice.Variant.ToString();
            Properties.Settings.Default.DeviceType = SelectedDevice.DeviceType.ToString();
            Properties.Settings.Default.KfdDeviceType = SelectedDevice.KfdDeviceType.ToString();
            Properties.Settings.Default.SelectedTheme = SelectedTheme.ToString();
            Properties.Settings.Default.Save();
        }

        public static void LoadSettings()
        {
            SelectedDevice.TwiKfdtoolDevice.ComPort = Properties.Settings.Default.TwiComPort;
            SelectedDevice.DliIpDevice.Hostname = Properties.Settings.Default.DliHostname;
            SelectedDevice.DliIpDevice.Port = Properties.Settings.Default.DliPort;
            SelectedDevice.DliIpDevice.Variant = (DliIpDevice.VariantOptions)Enum.Parse(typeof(DliIpDevice.VariantOptions), Properties.Settings.Default.DliVariant);
            SelectedDevice.DeviceType = (BaseDevice.DeviceTypeOptions)Enum.Parse(typeof(BaseDevice.DeviceTypeOptions), Properties.Settings.Default.DeviceType);
            SelectedDevice.KfdDeviceType = (Adapter.Device.TwiKfdDevice)Enum.Parse(typeof(Adapter.Device.TwiKfdDevice), Properties.Settings.Default.KfdDeviceType);
            SelectedTheme = (ThemeMode)Enum.Parse(typeof(ThemeMode), Properties.Settings.Default.SelectedTheme);
        }
    }
}
