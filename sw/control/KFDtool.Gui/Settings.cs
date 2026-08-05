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
            AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            AssemblyInformationalVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
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
            Properties.Settings.Default.KfdDeviceType = "KfdShield";
            Properties.Settings.Default.SelectedTheme = "System";
            Properties.Settings.Default.PromptSavePendingKeyChanges = true;
            Properties.Settings.Default.PromptDuplicateKeyConflicts = true;
            Properties.Settings.Default.PromptWeakKeyWarnings = true;
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
            SelectedDevice.DliIpDevice.Hostname = string.IsNullOrWhiteSpace(Properties.Settings.Default.DliHostname) ? "192.168.128.1" : Properties.Settings.Default.DliHostname;
            SelectedDevice.DliIpDevice.Port = Properties.Settings.Default.DliPort <= 0 ? 49644 : Properties.Settings.Default.DliPort;
            SelectedDevice.DliIpDevice.Variant = Enum.TryParse(Properties.Settings.Default.DliVariant, out DliIpDevice.VariantOptions dliVariant) ? dliVariant : DliIpDevice.VariantOptions.Motorola;
            SelectedDevice.DeviceType = Enum.TryParse(Properties.Settings.Default.DeviceType, out BaseDevice.DeviceTypeOptions deviceType) ? deviceType : BaseDevice.DeviceTypeOptions.TwiKfdDevice;
            SelectedDevice.KfdDeviceType = Enum.TryParse(Properties.Settings.Default.KfdDeviceType, out Adapter.Device.TwiKfdDevice kfdDeviceType) ? kfdDeviceType : Adapter.Device.TwiKfdDevice.KfdShield;
            SelectedTheme = Enum.TryParse(Properties.Settings.Default.SelectedTheme, out ThemeMode selectedTheme) ? selectedTheme : ThemeMode.System;
        }
    }
}

namespace KFDtool.Gui.Properties
{
    internal sealed partial class Settings
    {
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool PromptSavePendingKeyChanges
        {
            get
            {
                return ((bool)(this["PromptSavePendingKeyChanges"]));
            }
            set
            {
                this["PromptSavePendingKeyChanges"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool PromptDuplicateKeyConflicts
        {
            get
            {
                return ((bool)(this["PromptDuplicateKeyConflicts"]));
            }
            set
            {
                this["PromptDuplicateKeyConflicts"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool PromptWeakKeyWarnings
        {
            get
            {
                return ((bool)(this["PromptWeakKeyWarnings"]));
            }
            set
            {
                this["PromptWeakKeyWarnings"] = value;
            }
        }
    }
}
