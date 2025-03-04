using FramePFX.Themes;
using KFDtool.Adapter.Device;
using KFDtool.Container;
using KFDtool.Gui.Dialog;
using KFDtool.P25.TransferConstructs;
using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;

namespace KFDtool.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        private AutoDetection AppDet;

        public MainWindow()
        {
            InitializeComponent();

            UpdateContainerText();
            try
            {
                Settings.LoadSettings();
            }
            catch
            {
                MessageBox.Show("Saved settings invalid, resetting to default", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                Settings.InitSettings();
                Settings.LoadSettings();
            }

            InitAppDet();

            // Load selected theme
            UpdateWindowTheme();

            // on load select the type from settings
            switch (Settings.SelectedDevice.DeviceType)
            {
                case BaseDevice.DeviceTypeOptions.DliIp:
                    {
                        SwitchType(TypeDliIp);
                        break;
                    }

                case BaseDevice.DeviceTypeOptions.TwiKfdDevice:
                default:
                    {
                        // Select the appropriate device type based on loaded settings
                        switch (Settings.SelectedDevice.KfdDeviceType)
                        {
                            case TwiKfdDevice.KfdShield:
                                {
                                    SwitchType(TypeTwiKfdShield);
                                    break;
                                }
                            case TwiKfdDevice.KfdTool:
                                {
                                    SwitchType(TypeTwiKfdTool);
                                    break;
                                }
                            case TwiKfdDevice.KfdUsb:
                                {
                                    SwitchType(TypeTwiKfdUsb);
                                    break;
                                }
                            default:
                                {
                                    SwitchType(TypeTwiKfdTool);
                                    break;
                                }
                        }
                        // Select proper com port from loaded settings
                        foreach (MenuItem port in DeviceMenu.Items)
                        {
                            if (port.Name == Settings.SelectedDevice.TwiKfdtoolDevice.ComPort)
                            {
                                SelectDevice(port);
                            }
                        }
                        break;
                    }
            }

            // on load select the P25 Keyload function
            SwitchScreen("NavigateP25Keyload", true);
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            // prevent exit if operation is in progress
            if (Settings.ScreenInProgress)
            {
                SwitchScreen(Settings.ScreenCurrent, false);
                MessageBox.Show("Unable to exit - please stop the current operation", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Cancel = true;
            }

            // ask user if container should be saved before exiting
            if (Settings.ContainerOpen)
            {
                if (!Settings.ContainerSaved)
                {
                    MessageBoxResult res = MessageBox.Show("Container is unsaved - save before closing?", "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                    if (res == MessageBoxResult.Yes)
                    {
                        ContainerSave();

                        ContainerClose();
                    }
                    else if (res == MessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }

            // have to stop the WMI watcher or a RCW exception will be thrown
            StopAppDet();
        }

        #region navigate
        private void ClearAllSelections()
        {
            foreach (MenuItem item in P25KfdMenu.Items)
            {
                item.IsChecked = false;
            }

            foreach (MenuItem item in P25MrMenu.Items)
            {
                item.IsChecked = false;
            }

            foreach (MenuItem item in UtilityMenu.Items)
            {
                item.IsChecked = false;
            }
        }

        private void UpdateTitle(string s)
        {
#if DEBUG
            this.Title = string.Format("KFDtool DEBUG {0} [{1}]", Settings.AssemblyInformationalVersion, s);
#else
            this.Title = string.Format("KFDtool {0} [{1}]", Settings.AssemblyVersion, s);
#endif
        }

        private void SwitchScreen(string item, bool changeControl)
        {
            ClearAllSelections();

            object control;

            if (item == "NavigateP25MultipleKeyload")
            {
                control = new Control.P25MultipleKeyload();
                NavigateP25MultipleKeyload.IsChecked = true;
                Settings.ScreenCurrent = item;
                UpdateTitle("P25 KFD - Multiple Keyload");
            }
            else if (item == "NavigateP25Keyload")
            {
                control = new Control.P25Keyload();
                NavigateP25Keyload.IsChecked = true;
                Settings.ScreenCurrent = item;
                UpdateTitle("P25 KFD - Keyload");
            }
            else if (item == "NavigateP25KeyErase")
            {
                control = new Control.P25KeyErase();
                NavigateP25KeyErase.IsChecked = true;
                Settings.ScreenCurrent = item;
                UpdateTitle("P25 KFD - Key Erase");
            }
            else if (item == "NavigateP25EraseAllKeys")
            {
                control = new Control.P25EraseAllKeys();
                NavigateP25EraseAllKeys.IsChecked = true;
                Settings.ScreenCurrent = item;
                UpdateTitle("P25 KFD - Erase All Keys");
            }
            else if (item == "NavigateP25ViewKeyInfo")
            {
                control = new Control.P25ViewKeyInfo();
                NavigateP25ViewKeyInfo.IsChecked = true;
                Settings.ScreenCurrent = item;
                UpdateTitle("P25 KFD - View Key Info");
            }
            else if (item == "NavigateP25ViewKeysetInfo")
            {
                control = new Control.P25ViewKeysetInfo();
                NavigateP25ViewKeysetInfo.IsChecked = true;
                Settings.ScreenCurrent = item;
                UpdateTitle("P25 KFD - View Keyset Info");
            }
            else if (item == "NavigateP25ViewRsiConfig")
            {
                control = new Control.P25ViewRsiConfig();
                NavigateP25ViewRsiConfig.IsChecked = true;
                Settings.ScreenCurrent = item;
                UpdateTitle("P25 KFD - RSI Configuration");
            }
            else if (item == "NavigateP25KmfConfig")
            {
                control = new Control.P25KmfConfig();
                NavigateP25KmfConfig.IsChecked = true;
                Settings.ScreenCurrent = item;
                UpdateTitle("P25 KFD - KMF Configuration");
            }
            else if (item == "NavigateP25MrEmulator")
            {
                control = new Control.P25MrEmulator();
                NavigateP25MrEmulator.IsChecked = true;
                Settings.ScreenCurrent = item;
                UpdateTitle("P25 MR - Emulator");
            }
            else if (item == "NavigateUtilityFixDesKeyParity")
            {
                control = new Control.UtilFixDesKeyParity();
                NavigateUtilityFixDesKeyParity.IsChecked = true;
                Settings.ScreenCurrent = item;
                UpdateTitle("Utility - Fix DES Key Parity");
            }
            else if (item == "NavigateUtilityAdapterSelfTest")
            {
                control = new Control.UtilAdapterSelfTest();
                NavigateUtilityAdapterSelfTest.IsChecked = true;
                Settings.ScreenCurrent = item;
                UpdateTitle("Utility - Adapter Self Test");
            }
            else
            {
                throw new Exception(string.Format("unknown item passed to SwitchScreen - {0}", item));
            }

            if (changeControl)
            {
                AppView.Content = control;
            }
        }

        private void Navigate_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;

            if (mi != null)
            {
                if (Settings.ScreenInProgress)
                {
                    SwitchScreen(Settings.ScreenCurrent, false);
                    MessageBox.Show("Unable to change screens - please stop the current operation", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (!Settings.ContainerOpen && mi.Name == "NavigateP25MultipleKeyload")
                {
                    SwitchScreen(Settings.ScreenCurrent, false);
                    MessageBox.Show("No container open", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    SwitchScreen(mi.Name, true);
                }
            }
        }
        #endregion

        #region container
        public void UpdateContainerText()
        {
            if (Settings.ContainerOpen)
            {
                lblSelectedContainer.Text = string.Format("{0}{1}", Settings.ContainerSaved ? string.Empty : "[UNSAVED] ", Settings.ContainerPath);
            }
            else
            {
                lblSelectedContainer.Text = "Not Opened";
            }
        }

        private void Container_New_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.ContainerOpen)
            {
                MessageBox.Show("A container is already open", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ContainerSetPassword containerSetPassword = new ContainerSetPassword();
            containerSetPassword.Style = Window.GetWindow(this).Style;
            containerSetPassword.Owner = this; // for centering in parent window
            containerSetPassword.ShowDialog();

            if (containerSetPassword.PasswordSet)
            {
                string password = containerSetPassword.PasswordText;

                Settings.ContainerOpen = true;
                Settings.ContainerSaved = false;
                Settings.ContainerPath = string.Empty;
                (Settings.ContainerOuter, Settings.ContainerKey) = ContainerUtilities.CreateOuterContainer(password);
                Settings.ContainerInner = ContainerUtilities.CreateInnerContainer();

                UpdateContainerText();
            }
        }

        private void Container_Open_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.ContainerOpen)
            {
                MessageBox.Show("A container is already open", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Encrypted Key Container (*.ekc)|*.ekc|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                if (filePath.Equals(string.Empty))
                {
                    MessageBox.Show("No file selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                ContainerEnterPassword containerEnterPassword = new ContainerEnterPassword();
                containerEnterPassword.Style = Window.GetWindow(this).Style;
                containerEnterPassword.Owner = this; // for centering in parent window
                containerEnterPassword.ShowDialog();

                if (containerEnterPassword.PasswordSet)
                {
                    string password = containerEnterPassword.PasswordText;

                    byte[] fileContents;

                    try
                    {
                        fileContents = File.ReadAllBytes(filePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Failed to read file: {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    OuterContainer outerContainer;
                    InnerContainer innerContainer;
                    byte[] key;

                    try
                    {
                        (outerContainer, innerContainer, key) = ContainerUtilities.DecryptOuterContainer(fileContents, password);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Failed to decrypt container: {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    Settings.ContainerOpen = true;
                    Settings.ContainerSaved = true;
                    Settings.ContainerPath = filePath;
                    Settings.ContainerKey = key;
                    Settings.ContainerOuter = outerContainer;
                    Settings.ContainerInner = innerContainer;

                    UpdateContainerText();
                }
            }
        }

        private void Container_Edit_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.ContainerOpen)
            {
                ContainerEdit containerEdit = new ContainerEdit();
                containerEdit.Style = Window.GetWindow(this).Style;
                containerEdit.Owner = this; // for centering in parent window

                try
                {
                    containerEdit.ShowDialog();
                }
                catch (Exception ex)
                {
                    containerEdit.Close();
                    MessageBox.Show(string.Format("Error modifying key container - {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ContainerClose();
                    return;
                }

                UpdateContainerText();

                if (!Settings.ContainerSaved && Settings.ScreenCurrent == "NavigateP25MultipleKeyload")
                {
                    SwitchScreen(Settings.ScreenCurrent, true);
                    MessageBox.Show("Multiple keyload selections reset due to key container edit", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("No container open", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Container_Change_Password_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.ContainerOpen)
            {
                ContainerSetPassword containerSetPassword = new ContainerSetPassword();
                containerSetPassword.Style = Window.GetWindow(this).Style;
                containerSetPassword.Owner = this; // for centering in parent window
                containerSetPassword.ShowDialog();

                if (containerSetPassword.PasswordSet)
                {
                    string password = containerSetPassword.PasswordText;

                    (Settings.ContainerOuter, Settings.ContainerKey) = ContainerUtilities.CreateOuterContainer(password);

                    Settings.ContainerSaved = false;

                    UpdateContainerText();

                    MessageBox.Show("Password Changed", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("No container open", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ContainerWrite(string path)
        {
            byte[] contents;

            try
            {
                contents = ContainerUtilities.EncryptOuterContainer(Settings.ContainerOuter, Settings.ContainerInner, Settings.ContainerKey);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Failed to encrypt container: {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                File.WriteAllBytes(path, contents);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Failed to write file: {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Settings.ContainerPath = path;
            Settings.ContainerSaved = true;

            UpdateContainerText();
        }

        private void ContainerSaveAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Encrypted Key Container (*.ekc)|*.ekc";

            if (saveFileDialog.ShowDialog() == true)
            {
                ContainerWrite(saveFileDialog.FileName);
            }
        }

        private void ContainerSave()
        {
            if (Settings.ContainerPath != string.Empty)
            {
                ContainerWrite(Settings.ContainerPath);
            }
            else
            {
                ContainerSaveAs();
            }
        }

        private void Container_Save_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.ContainerOpen)
            {
                ContainerSave();
            }
            else
            {
                MessageBox.Show("No container open", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Container_Save_As_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.ContainerOpen)
            {
                ContainerSaveAs();
            }
            else
            {
                MessageBox.Show("No container open", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ContainerClose()
        {
            if (Settings.ScreenCurrent == "NavigateP25MultipleKeyload")
            {
                SwitchScreen("NavigateP25Keyload", true);
            }

            Settings.ContainerOpen = false;
            Settings.ContainerSaved = false;
            Settings.ContainerPath = string.Empty;
            Settings.ContainerKey = null;
            Settings.ContainerOuter = null;
            Settings.ContainerInner = null;

            UpdateContainerText();
        }

        private void Container_Close_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.ContainerOpen)
            {
                if (!Settings.ContainerSaved)
                {
                    MessageBoxResult res = MessageBox.Show("Container is unsaved - save before closing?", "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                    if (res == MessageBoxResult.Yes)
                    {
                        ContainerSave();
                    }
                    else if (res == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }

                ContainerClose();
            }
            else
            {
                MessageBox.Show("No container open", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        private void SwitchType(MenuItem mi)
        {
            foreach (MenuItem item in TypeMenu.Items)
            {
                item.IsChecked = false;
            }

            mi.IsChecked = true;

            if (mi.Name == "TypeTwiKfdTool")
            {
                DeviceMenu.Items.Clear();

                Settings.SelectedDevice.DeviceType = BaseDevice.DeviceTypeOptions.TwiKfdDevice;
                Settings.SelectedDevice.KfdDeviceType = TwiKfdDevice.KfdTool;

                ResetTwiDeviceInfo();

                StartAppDet();
            }
            else if (mi.Name == "TypeTwiKfdShield")
            {
                DeviceMenu.Items.Clear();

                Settings.SelectedDevice.DeviceType = BaseDevice.DeviceTypeOptions.TwiKfdDevice;
                Settings.SelectedDevice.KfdDeviceType = TwiKfdDevice.KfdShield;

                ResetTwiDeviceInfo();

                StartAppDet();
            }
            else if (mi.Name == "TypeTwiKfdUsb")
            {
                DeviceMenu.Items.Clear();

                Settings.SelectedDevice.DeviceType = BaseDevice.DeviceTypeOptions.TwiKfdDevice;
                Settings.SelectedDevice.KfdDeviceType = TwiKfdDevice.KfdUsb;

                ResetTwiDeviceInfo();

                StartAppDet();
            }
            else if (mi.Name == "TypeDliIp")
            {
                StopAppDet();

                DeviceMenu.Items.Clear();

                Settings.SelectedDevice.DeviceType = BaseDevice.DeviceTypeOptions.DliIp;

                MenuItem DliIpEdit = new MenuItem();
                DliIpEdit.Header = "_[Edit]";
                DliIpEdit.Click += DliIpEdit_MenuItem_Click;

                DeviceMenu.Items.Add(DliIpEdit);

                UpdateDeviceDliIp();
            }
        }

        private void UpdateDeviceDliIp()
        {
            lblSelectedDevice.Text = string.Format(
                "DLI (IP) - {0}:{1}, {2}, Variant: {3}",
                Settings.SelectedDevice.DliIpDevice.Hostname,
                Settings.SelectedDevice.DliIpDevice.Port.ToString(),
                Settings.SelectedDevice.DliIpDevice.Protocol.ToString(),
                Settings.SelectedDevice.DliIpDevice.Variant.ToString()
            );

            // Save config
            Settings.SaveSettings();
        }

        private void Type_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;

            SwitchType(mi);

            // Save config
            Settings.SaveSettings();
        }

        private void DliIpEdit_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DliIpDeviceEdit wnd = new DliIpDeviceEdit();
            wnd.Owner = this; // for centering in parent window
            wnd.ShowDialog();

            UpdateDeviceDliIp();
        }

        private void InitAppDet()
        {
            AppDet = new AutoDetection();
            AppDet.DevicesChanged += CheckConnectedDevices;
        }

        private void StartAppDet()
        {
            AppDet.Start();
        }

        private void StopAppDet()
        {
            AppDet.Stop();
        }

        private void ResetTwiDeviceInfo()
        {
            lblSelectedDevice.Text = string.Format(
                "TWI ({0}) - None",
                Settings.SelectedDevice.KfdDeviceType.ToString()
            );
        }

        private void CheckConnectedDevices(object sender, EventArgs e)
        {
            Log.Debug("device list updated");

            // needed to access UI elements from different thread
            this.Dispatcher.Invoke(() =>
            {
                //bool first = true;

                List<string> ports = AppDet.Devices;

                // sort ports lowest to highest (COM6,COM12,COM26)
                ports.Sort();

                DeviceMenu.Items.Clear();

                // no devices detected
                if (ports.Count == 0)
                {
                    Settings.SelectedDevice.TwiKfdtoolDevice.ComPort = string.Empty;

                    lblSelectedDevice.Text = string.Format(
                        "TWI ({0}) - None",
                        Settings.SelectedDevice.KfdDeviceType.ToString()
                    );

                    MenuItem item = new MenuItem();

                    item.Header = "No devices found";
                    item.IsCheckable = false;
                    item.IsEnabled = false;

                    DeviceMenu.Items.Add(item);
                }

                // one or more devices detected
                foreach (string port in ports)
                {
                    MenuItem item = new MenuItem();

                    item.Name = port;
                    item.Header = port;
                    item.IsCheckable = true;
                    item.Click += Device_MenuItem_Click;

                    DeviceMenu.Items.Add(item);

                    // Removed auto-select on startup to get rid of the annoying timeout error
                    /*
                    // there was a change in the device list, but the device that was previously selected is still connected
                    if (port == Settings.SelectedDevice.TwiKfdtoolDevice.ComPort)
                    {
                        SelectDevice(item);
                        first = false;
                    }

                    // select the lowest numbered device if there is no device currently selected
                    // or if the device that was previously selected was disconnected
                    if (first)
                    {
                        SelectDevice(item);
                        first = false;
                    }*/
                }
            });
        }

        private void Device_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;

            SelectDevice(mi);
        }

        private void SelectDevice(MenuItem mi)
        {
            if (mi != null)
            {
                foreach (MenuItem item in DeviceMenu.Items)
                {
                    item.IsChecked = false;
                }

                mi.IsChecked = true;

                Settings.SelectedDevice.TwiKfdtoolDevice.ComPort = mi.Name;

                string apVerStr = string.Empty;

                // Save new selection
                Settings.SaveSettings();

                try
                {
                    apVerStr = Interact.ReadAdapterProtocolVersion(Settings.SelectedDevice);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Error -- {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Version apVersion = new Version(apVerStr);

                if (apVersion.Major != 2)    // TODO: handle this better
                {
                    MessageBox.Show(string.Format(
                        "Adapter protocol version not compatible ({0})\n\n" +
                        "This version of KFDtool supports adapter protocol versions 2.x.x.\n\n" +
                        "Please update your adapter firmware.",
                        apVerStr), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string fwVersion = string.Empty;
                string uniqueId = string.Empty;
                string model = string.Empty;
                string hwRev = string.Empty;
                string serialNum = string.Empty;

                try
                {
                    fwVersion = Interact.ReadFirmwareVersion(Settings.SelectedDevice);
                    uniqueId = Interact.ReadUniqueId(Settings.SelectedDevice);
                    model = Interact.ReadModel(Settings.SelectedDevice);
                    hwRev = Interact.ReadHardwareRevision(Settings.SelectedDevice);
                    serialNum = Interact.ReadSerialNumber(Settings.SelectedDevice);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Error -- {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                lblSelectedDevice.Text = string.Format(
                    "TWI ({0}) - {1}, Model {2}, HW {3}, Serial {4}, UID {5}, FW {6}",
                    Settings.SelectedDevice.KfdDeviceType.ToString(),
                    Settings.SelectedDevice.TwiKfdtoolDevice.ComPort,
                    model,
                    hwRev,
                    serialNum,
                    uniqueId,
                    fwVersion
                );
            }
        }

        private void Exit_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


        private void About_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            string aboutText = string.Format(
                "KFDtool Control Application{0}{0}Copyright 2019-2020 Ellie Dugger{0}Copyright 2021-2024 Natalie Moore{0}Copyright 2023-2024 Ilya Smirnov{0}Copyright 2023-2024 Patrick McDonnell"
                #if DEBUG
                + " DEBUG"
                #endif
                + "{0}{0}",
                Environment.NewLine
            );
            AboutPage about = new AboutPage(aboutText);
            about.Style = this.Style;
            about.Owner = this;
            about.Show();
        }

        private void UpdateWindowTheme()
        {
            // Reset checks
            NavigateUtilityChangeThemeDark.IsChecked = false;
            NavigateUtilityChangeThemeLight.IsChecked = false;
            NavigateUtilityChangeThemeSystem.IsChecked = false;
            // Change theme
            switch (Settings.SelectedTheme)
            {
                // System theme, detect light/dark mode
                case Settings.ThemeMode.System:
                    NavigateUtilityChangeThemeSystem.IsChecked = true;
                    if (IsSystemLightTheme())
                    {
                        //ThemesController.SetTheme(ThemeType.LightTheme);
                        ThemesController.ClearTheme();
                        this.Style = new Style();
                    }
                    else
                    {
                        ThemesController.SetTheme(ThemeType.SoftDark);
                        this.Style = (Style)FindResource("CustomWindowStyle");
                    }
                    break;
                // Light theme
                case Settings.ThemeMode.Light:
                    NavigateUtilityChangeThemeLight.IsChecked = true;
                    //ThemesController.SetTheme(ThemeType.LightTheme);
                    ThemesController.ClearTheme();
                    this.Style = new Style();
                    break;
                // Dark Theme
                case Settings.ThemeMode.Dark:
                    NavigateUtilityChangeThemeDark.IsChecked = true;
                    ThemesController.SetTheme(ThemeType.SoftDark);
                    this.Style = (Style)FindResource("CustomWindowStyle");
                    break;
            }
        }

        private void NavigateUtilityChangeThemeDark_Click(object sender, RoutedEventArgs e)
        {
            // Update settings
            Settings.SelectedTheme = Settings.ThemeMode.Dark;
            Settings.SaveSettings();
            // Update
            UpdateWindowTheme();
        }

        private void NavigateUtilityChangeThemeLight_Click(object sender, RoutedEventArgs e)
        {
            // Update settings
            Settings.SelectedTheme = Settings.ThemeMode.Light;
            Settings.SaveSettings();
            // Update
            UpdateWindowTheme();
        }

        private void NavigateUtilityChangeThemeSystem_Click(object sender, RoutedEventArgs e)
        {
            // Update settings
            Settings.SelectedTheme = Settings.ThemeMode.System;
            Settings.SaveSettings();
            // Update
            UpdateWindowTheme();
        }

        public static bool IsSystemLightTheme()
        {
            int res = (int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", -1);
            if (res == 0)
            {
                return false;
            }
            else
            {
                if (res == -1)
                {
                    MessageBox.Show("Unable to determine system theme mode, defaulting to light mode", "Unable to Detect Theme");
                }
                return true;
            }
        }
    }
}
