using KFDtool.Container;
using KFDtool.P25.Constant;
using KFDtool.P25.Generator;
using KFDtool.P25.Validator;
using KFDtool.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KFDtool.Gui.Dialog
{
    internal static class KeyConflictHelper
    {
        public static void RefreshConflictStates(IEnumerable<KeyItem> keys)
        {
            List<KeyItem> keyList = keys.ToList();

            foreach (KeyItem key in keyList)
            {
                List<string> conflicts = GetConflictMessages(keyList, key.ActiveKeyset, key.KeysetId, key.Sln, key.KeyId, key);

                key.HasConflict = conflicts.Count > 0;
                key.ConflictSummary = string.Join(Environment.NewLine, conflicts);
            }
        }

        public static List<string> GetConflictMessages(IEnumerable<KeyItem> keys, bool activeKeyset, int keysetId, int sln, int keyId, KeyItem excludedKey)
        {
            List<string> conflicts = new List<string>();

            bool duplicateSln = keys.Any(key =>
                !object.ReferenceEquals(key, excludedKey) &&
                IsSameEffectiveKeyset(key, activeKeyset, keysetId) &&
                key.Sln == sln);

            if (duplicateSln)
            {
                conflicts.Add("Another key already uses this SLN/CKR in the same effective keyset.");
            }

            bool duplicateKeyId = keys.Any(key =>
                !object.ReferenceEquals(key, excludedKey) &&
                IsSameEffectiveKeyset(key, activeKeyset, keysetId) &&
                key.KeyId == keyId);

            if (duplicateKeyId)
            {
                conflicts.Add("Another key already uses this Key ID in the same effective keyset.");
            }

            return conflicts;
        }

        public static int GetNextAvailableValue(IEnumerable<KeyItem> keys, bool activeKeyset, int keysetId, int currentValue, Func<KeyItem, int> selector)
        {
            HashSet<int> usedValues = new HashSet<int>(
                keys
                    .Where(key => IsSameEffectiveKeyset(key, activeKeyset, keysetId))
                    .Select(selector)
            );

            for (int offset = 1; offset <= 0x10000; offset++)
            {
                int candidate = (currentValue + offset) & 0xFFFF;

                if (!usedValues.Contains(candidate))
                {
                    return candidate;
                }
            }

            return currentValue;
        }

        private static bool IsSameEffectiveKeyset(KeyItem key, bool activeKeyset, int keysetId)
        {
            if (activeKeyset)
            {
                return key.ActiveKeyset;
            }

            return !key.ActiveKeyset && key.KeysetId == keysetId;
        }
    }

    /// <summary>
    /// Interaction logic for ContainerEditKeyControl.xaml
    /// </summary>
    public partial class ContainerEditKeyControl : UserControl
    {
        private sealed class KeyEditorState
        {
            public string Name { get; set; }

            public bool UseActiveKeyset { get; set; }

            public string KeysetIdHex { get; set; }

            public int KeyTypeSelection { get; set; }

            public string SlnHex { get; set; }

            public string KeyIdHex { get; set; }

            public string AlgorithmHex { get; set; }

            public string Key { get; set; }

            public bool SemanticallyEquals(KeyEditorState other)
            {
                if (other == null)
                {
                    return false;
                }

                if (!string.Equals(Name, other.Name, StringComparison.Ordinal))
                {
                    return false;
                }

                if (UseActiveKeyset != other.UseActiveKeyset)
                {
                    return false;
                }

                if (KeyTypeSelection != other.KeyTypeSelection)
                {
                    return false;
                }

                if (!UseActiveKeyset && !HexValuesEqual(KeysetIdHex, other.KeysetIdHex))
                {
                    return false;
                }

                if (!HexValuesEqual(SlnHex, other.SlnHex))
                {
                    return false;
                }

                if (!HexValuesEqual(KeyIdHex, other.KeyIdHex))
                {
                    return false;
                }

                if (!HexValuesEqual(AlgorithmHex, other.AlgorithmHex))
                {
                    return false;
                }

                return string.Equals(NormalizeText(Key), NormalizeText(other.Key), StringComparison.OrdinalIgnoreCase);
            }

            private static bool HexValuesEqual(string left, string right)
            {
                int leftValue;
                int rightValue;

                if (int.TryParse(left, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out leftValue) &&
                    int.TryParse(right, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out rightValue))
                {
                    return leftValue == rightValue;
                }

                return string.Equals(NormalizeHex(left), NormalizeHex(right), StringComparison.OrdinalIgnoreCase);
            }

            private static string NormalizeHex(string value)
            {
                string normalizedText = NormalizeText(value);

                if (normalizedText.Length == 0)
                {
                    return string.Empty;
                }

                string normalized = normalizedText.TrimStart('0');

                return normalized.Length == 0 ? "0" : normalized;
            }

            private static string NormalizeText(string value)
            {
                return (value ?? string.Empty).Trim();
            }
        }

        private readonly KeyItem LocalKey;

        private bool IsKek { get; set; }

        private bool FocusNameOnLoad { get; set; }

        private bool IsInitializing { get; set; }

        private bool InitialFocusApplied { get; set; }

        private KeyEditorState SavedState { get; set; }

        public bool HasUnsavedChanges { get; private set; }

        public bool IsKeyMaterialHidden
        {
            get { return cbHide.IsChecked == true; }
        }

        public event EventHandler DirtyStateChanged;

        public event EventHandler Saved;

        public event EventHandler HidePreferenceChanged;

        public ContainerEditKeyControl(KeyItem keyItem)
            : this(keyItem, true, false)
        {
        }

        public ContainerEditKeyControl(KeyItem keyItem, bool hideKeyMaterial, bool focusNameOnLoad)
        {
            InitializeComponent();

            LocalKey = keyItem;
            FocusNameOnLoad = focusNameOnLoad;

            Loaded += ContainerEditKeyControl_Loaded;
            txtName.TextChanged += PersistentFieldChanged;
            txtKeyVisible.TextChanged += PersistentFieldChanged;
            txtKeyHidden.PasswordChanged += KeyHidden_PasswordChanged;

            IsInitializing = true;

            try
            {
                LoadKeyState(keyItem, hideKeyMaterial);
                SavedState = CaptureEditorState();
                HasUnsavedChanges = false;
            }
            finally
            {
                IsInitializing = false;
            }
        }

        private void ContainerEditKeyControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (FocusNameOnLoad && !InitialFocusApplied)
            {
                txtName.Focus();
                txtName.SelectAll();
                InitialFocusApplied = true;
            }
        }

        private void LoadKeyState(KeyItem keyItem, bool hideKeyMaterial)
        {
            txtName.Text = keyItem.Name;

            cbActiveKeyset.IsChecked = keyItem.ActiveKeyset;

            if (!keyItem.ActiveKeyset)
            {
                txtKeysetIdDec.Text = keyItem.KeysetId.ToString();
                UpdateKeysetIdDec();
            }

            if (keyItem.KeyTypeAuto)
            {
                cboType.SelectedIndex = 0;
            }
            else if (keyItem.KeyTypeTek)
            {
                cboType.SelectedIndex = 1;
            }
            else if (keyItem.KeyTypeKek)
            {
                cboType.SelectedIndex = 2;
            }
            else
            {
                throw new Exception("invalid key type");
            }

            txtSlnDec.Text = keyItem.Sln.ToString();
            UpdateSlnDec();

            txtKeyIdDec.Text = keyItem.KeyId.ToString();
            UpdateKeyIdDec();

            if (keyItem.AlgorithmId == 0x84)
            {
                cboAlgo.SelectedIndex = 0;
            }
            else if (keyItem.AlgorithmId == 0x81)
            {
                cboAlgo.SelectedIndex = 1;
            }
            else if (keyItem.AlgorithmId == 0x9F)
            {
                cboAlgo.SelectedIndex = 2;
            }
            else if (keyItem.AlgorithmId == 0xAA)
            {
                cboAlgo.SelectedIndex = 3;
            }
            else
            {
                cboAlgo.SelectedIndex = 4;

                txtAlgoDec.Text = keyItem.AlgorithmId.ToString();
                UpdateAlgoDec();
            }

            txtKeyHidden.Password = keyItem.Key;
            txtKeyVisible.Text = keyItem.Key;
            cbHide.IsChecked = hideKeyMaterial;
            SetHideState(hideKeyMaterial);
        }

        private void PersistentFieldChanged(object sender, TextChangedEventArgs e)
        {
            UpdateDirtyState();
        }

        private void KeyHidden_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateDirtyState();
        }

        private void UpdateDirtyState()
        {
            if (IsInitializing)
            {
                return;
            }

            bool hasUnsavedChanges = !SavedState.SemanticallyEquals(CaptureEditorState());

            if (HasUnsavedChanges != hasUnsavedChanges)
            {
                HasUnsavedChanges = hasUnsavedChanges;
                DirtyStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private KeyEditorState CaptureEditorState()
        {
            return new KeyEditorState
            {
                Name = txtName.Text,
                UseActiveKeyset = cbActiveKeyset.IsChecked == true,
                KeysetIdHex = txtKeysetIdHex.Text,
                KeyTypeSelection = cboType.SelectedIndex,
                SlnHex = txtSlnHex.Text,
                KeyIdHex = txtKeyIdHex.Text,
                AlgorithmHex = txtAlgoHex.Text,
                Key = GetKey()
            };
        }

        private void UpdateKeysetIdDec()
        {
            int num;

            if (int.TryParse(txtKeysetIdDec.Text, out num))
            {
                txtKeysetIdHex.Text = string.Format("{0:X}", num);
            }
            else
            {
                txtKeysetIdHex.Text = string.Empty;
            }
        }

        private void UpdateKeysetIdHex()
        {
            int num;

            if (int.TryParse(txtKeysetIdHex.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num))
            {
                txtKeysetIdDec.Text = num.ToString();
            }
            else
            {
                txtKeysetIdDec.Text = string.Empty;
            }
        }

        private void KeysetIdDec_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtKeysetIdDec.IsFocused)
            {
                UpdateKeysetIdDec();
            }

            UpdateDirtyState();
        }

        private void KeysetIdHex_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtKeysetIdHex.IsFocused)
            {
                UpdateKeysetIdHex();
            }

            UpdateDirtyState();
        }

        private void OnActiveKeysetChecked(object sender, RoutedEventArgs e)
        {
            txtKeysetIdDec.Text = string.Empty;
            txtKeysetIdHex.Text = string.Empty;
            txtKeysetIdDec.IsEnabled = false;
            txtKeysetIdHex.IsEnabled = false;

            UpdateDirtyState();
        }

        private void OnActiveKeysetUnchecked(object sender, RoutedEventArgs e)
        {
            txtKeysetIdDec.IsEnabled = true;
            txtKeysetIdHex.IsEnabled = true;

            UpdateDirtyState();
        }

        private void UpdateSlnDec()
        {
            int num;

            if (int.TryParse(txtSlnDec.Text, out num))
            {
                txtSlnHex.Text = string.Format("{0:X}", num);
            }
            else
            {
                txtSlnHex.Text = string.Empty;
            }

            UpdateType();
        }

        private void UpdateSlnHex()
        {
            int num;

            if (int.TryParse(txtSlnHex.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num))
            {
                txtSlnDec.Text = num.ToString();
            }
            else
            {
                txtSlnDec.Text = string.Empty;
            }

            UpdateType();
        }

        private void SlnDec_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSlnDec.IsFocused)
            {
                UpdateSlnDec();
            }

            UpdateDirtyState();
        }

        private void SlnHex_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSlnHex.IsFocused)
            {
                UpdateSlnHex();
            }

            UpdateDirtyState();
        }

        private void UpdateType()
        {
            if (cboType.SelectedItem != null)
            {
                string name = ((ComboBoxItem)cboType.SelectedItem).Name as string;

                if (name == "AUTO")
                {
                    int num;

                    if (int.TryParse(txtSlnHex.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num))
                    {
                        if (num >= 0 && num <= 61439)
                        {
                            lblType.Content = "TEK";
                            IsKek = false;
                        }
                        else if (num >= 61440 && num <= 65535)
                        {
                            lblType.Content = "KEK";
                            IsKek = true;
                        }
                        else
                        {
                            lblType.Content = "Auto";
                        }
                    }
                    else
                    {
                        lblType.Content = "Auto";
                    }
                }
                else if (name == "TEK")
                {
                    lblType.Content = "TEK";
                    IsKek = false;
                }
                else if (name == "KEK")
                {
                    lblType.Content = "KEK";
                    IsKek = true;
                }
            }
        }

        private void OnTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateType();
            UpdateDirtyState();
        }

        private void UpdateKeyIdDec()
        {
            int num;

            if (int.TryParse(txtKeyIdDec.Text, out num))
            {
                txtKeyIdHex.Text = string.Format("{0:X}", num);
            }
            else
            {
                txtKeyIdHex.Text = string.Empty;
            }
        }

        private void UpdateKeyIdHex()
        {
            int num;

            if (int.TryParse(txtKeyIdHex.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num))
            {
                txtKeyIdDec.Text = num.ToString();
            }
            else
            {
                txtKeyIdDec.Text = string.Empty;
            }
        }

        private void KeyIdDec_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtKeyIdDec.IsFocused)
            {
                UpdateKeyIdDec();
            }

            UpdateDirtyState();
        }

        private void KeyIdHex_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtKeyIdHex.IsFocused)
            {
                UpdateKeyIdHex();
            }

            UpdateDirtyState();
        }

        private void UpdateAlgoDec()
        {
            int num;

            if (int.TryParse(txtAlgoDec.Text, out num))
            {
                txtAlgoHex.Text = string.Format("{0:X}", num);
            }
            else
            {
                txtAlgoHex.Text = string.Empty;
            }
        }

        private void UpdateAlgoHex()
        {
            int num;

            if (int.TryParse(txtAlgoHex.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num))
            {
                txtAlgoDec.Text = num.ToString();
            }
            else
            {
                txtAlgoDec.Text = string.Empty;
            }
        }

        private void AlgoDec_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtAlgoDec.IsFocused)
            {
                UpdateAlgoDec();
            }

            UpdateDirtyState();
        }

        private void AlgoHex_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtAlgoHex.IsFocused)
            {
                UpdateAlgoHex();
            }

            UpdateDirtyState();
        }

        private void OnAlgoChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboAlgo.SelectedItem != null)
            {
                string name = ((ComboBoxItem)cboAlgo.SelectedItem).Name as string;

                if (name == "AES256")
                {
                    txtAlgoHex.Text = "84";
                    UpdateAlgoHex();
                    txtAlgoDec.IsEnabled = false;
                    txtAlgoHex.IsEnabled = false;
                }
                else if (name == "DESOFB")
                {
                    txtAlgoHex.Text = "81";
                    UpdateAlgoHex();
                    txtAlgoDec.IsEnabled = false;
                    txtAlgoHex.IsEnabled = false;
                }
                else if (name == "DESXL")
                {
                    txtAlgoHex.Text = "9F";
                    UpdateAlgoHex();
                    txtAlgoDec.IsEnabled = false;
                    txtAlgoHex.IsEnabled = false;
                }
                else if (name == "ADP")
                {
                    txtAlgoHex.Text = "AA";
                    UpdateAlgoHex();
                    txtAlgoDec.IsEnabled = false;
                    txtAlgoHex.IsEnabled = false;
                }
                else
                {
                    txtAlgoDec.Text = string.Empty;
                    txtAlgoHex.Text = string.Empty;
                    txtAlgoDec.IsEnabled = true;
                    txtAlgoHex.IsEnabled = true;
                }
            }

            UpdateDirtyState();
        }

        private void Generate_Button_Click(object sender, RoutedEventArgs e)
        {
            int algId = 0;

            try
            {
                algId = Convert.ToInt32(txtAlgoHex.Text, 16);
            }
            catch (Exception)
            {
                MessageBox.Show("Error Parsing Algorithm ID", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!FieldValidator.IsValidAlgorithmId(algId))
            {
                MessageBox.Show("Algorithm ID invalid - valid range 0 to 255 (dec), 0x00 to 0xFF (hex)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            List<byte> key = new List<byte>();

            if (algId == (byte)AlgorithmId.AES256)
            {
                key = KeyGenerator.GenerateVarKey(32);
            }
            else if (algId == (byte)AlgorithmId.DESOFB || algId == (byte)AlgorithmId.DESXL)
            {
                key = KeyGenerator.GenerateSingleDesKey();
            }
            else if (algId == (byte)AlgorithmId.ADP)
            {
                key = KeyGenerator.GenerateVarKey(5);
            }
            else
            {
                MessageBox.Show(string.Format("No key generator exists for algorithm ID 0x{0:X2}", algId), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SetKey(BitConverter.ToString(key.ToArray()).Replace("-", string.Empty));
            UpdateDirtyState();
        }

        private void SetHideState(bool hideKeyMaterial)
        {
            string currentKey = string.IsNullOrEmpty(txtKeyVisible.Text) ? txtKeyHidden.Password : txtKeyVisible.Text;

            if (hideKeyMaterial)
            {
                txtKeyHidden.Password = currentKey;
                txtKeyVisible.Text = string.Empty;
                txtKeyVisible.Visibility = Visibility.Hidden;
                txtKeyHidden.Visibility = Visibility.Visible;
            }
            else
            {
                txtKeyVisible.Text = currentKey;
                txtKeyHidden.Password = string.Empty;
                txtKeyVisible.Visibility = Visibility.Visible;
                txtKeyHidden.Visibility = Visibility.Hidden;
            }
        }

        private void OnHideChecked(object sender, RoutedEventArgs e)
        {
            SetHideState(true);

            if (!IsInitializing)
            {
                HidePreferenceChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnHideUnchecked(object sender, RoutedEventArgs e)
        {
            SetHideState(false);

            if (!IsInitializing)
            {
                HidePreferenceChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private string GetKey()
        {
            if (cbHide.IsChecked == true)
            {
                return txtKeyHidden.Password;
            }
            else
            {
                return txtKeyVisible.Text;
            }
        }

        private void SetKey(string str)
        {
            if (cbHide.IsChecked == true)
            {
                txtKeyHidden.Password = str;
            }
            else
            {
                txtKeyVisible.Text = str;
            }
        }

        public bool TrySaveChanges()
        {
            int keysetId;
            int sln;
            int keyId;
            int algId;
            List<byte> key;

            bool useActiveKeyset = cbActiveKeyset.IsChecked == true;

            if (useActiveKeyset)
            {
                keysetId = 1; // to pass validation, will not get used
            }
            else
            {
                try
                {
                    keysetId = Convert.ToInt32(txtKeysetIdHex.Text, 16);
                }
                catch (Exception)
                {
                    MessageBox.Show("Error Parsing Keyset ID", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            try
            {
                sln = Convert.ToInt32(txtSlnHex.Text, 16);
            }
            catch (Exception)
            {
                MessageBox.Show("Error Parsing SLN", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            try
            {
                keyId = Convert.ToInt32(txtKeyIdHex.Text, 16);
            }
            catch (Exception)
            {
                MessageBox.Show("Error Parsing Key ID", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            try
            {
                algId = Convert.ToInt32(txtAlgoHex.Text, 16);
            }
            catch (Exception)
            {
                MessageBox.Show("Error Parsing Algorithm ID", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            try
            {
                key = Utility.ByteStringToByteList(GetKey());
            }
            catch (Exception)
            {
                MessageBox.Show("Error Parsing Key", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            Tuple<ValidateResult, string> validateResult = FieldValidator.KeyloadValidate(keysetId, sln, IsKek, keyId, algId, key);

            if (validateResult.Item1 == ValidateResult.Warning)
            {
                bool suppressWeakKeyWarning = !Properties.Settings.Default.PromptWeakKeyWarnings &&
                                              IsWeakKeyWarning(validateResult.Item2);

                if (!suppressWeakKeyWarning)
                {
                    if (MessageBox.Show(string.Format("{1}{0}{0}Continue?", Environment.NewLine, validateResult.Item2), "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    {
                        return false;
                    }
                }
            }
            else if (validateResult.Item1 == ValidateResult.Error)
            {
                MessageBox.Show(validateResult.Item2, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (txtName.Text.Length == 0)
            {
                MessageBox.Show("Key name required", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (txtName.Text != LocalKey.Name)
            {
                foreach (KeyItem keyItem in Settings.ContainerInner.Keys)
                {
                    if (txtName.Text == keyItem.Name)
                    {
                        MessageBox.Show("Key name must be unique", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
            }

            List<string> duplicateConflicts = KeyConflictHelper.GetConflictMessages(Settings.ContainerInner.Keys, useActiveKeyset, keysetId, sln, keyId, LocalKey);

            if (duplicateConflicts.Count > 0 && Properties.Settings.Default.PromptDuplicateKeyConflicts)
            {
                string message = string.Format(
                    "Possible key configuration conflict:{0}{0}{1}{0}{0}Save anyway?",
                    Environment.NewLine,
                    string.Join(Environment.NewLine, duplicateConflicts)
                );

                if (MessageBox.Show(message, "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    return false;
                }
            }

            LocalKey.Name = txtName.Text;
            LocalKey.ActiveKeyset = useActiveKeyset;
            LocalKey.KeysetId = keysetId;
            LocalKey.Sln = sln;

            if (cboType.SelectedIndex == 0)
            {
                LocalKey.KeyTypeAuto = true;
                LocalKey.KeyTypeTek = false;
                LocalKey.KeyTypeKek = false;
            }
            else if (cboType.SelectedIndex == 1)
            {
                LocalKey.KeyTypeAuto = false;
                LocalKey.KeyTypeTek = true;
                LocalKey.KeyTypeKek = false;
            }
            else if (cboType.SelectedIndex == 2)
            {
                LocalKey.KeyTypeAuto = false;
                LocalKey.KeyTypeTek = false;
                LocalKey.KeyTypeKek = true;
            }
            else
            {
                throw new Exception("invalid key type");
            }

            LocalKey.KeyId = keyId;
            LocalKey.AlgorithmId = algId;
            LocalKey.Key = BitConverter.ToString(key.ToArray()).Replace("-", string.Empty);

            SavedState = CaptureEditorState();
            UpdateDirtyState();
            Saved?.Invoke(this, EventArgs.Empty);

            return true;
        }

        private static bool IsWeakKeyWarning(string message)
        {
            return message.IndexOf("cryptographically weak", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   message.IndexOf("easily guessable", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            TrySaveChanges();
        }
    }
}
