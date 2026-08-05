using KFDtool.Container;
using KFDtool.P25.Constant;
using KFDtool.P25.Generator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KFDtool.Gui.Dialog
{
    /// <summary>
    /// Interaction logic for ContainerEdit.xaml
    /// </summary>
    public partial class ContainerEdit : Window
    {
        private string OriginalContainer;

        private bool SuppressSelectionHandlers { get; set; }

        private TabItem ActiveTabItem { get; set; }

        private KeyItem SelectedKeyItem { get; set; }

        private Container.GroupItem SelectedGroupItem { get; set; }

        private bool PreferHiddenKeyMaterial { get; set; }

        public static RoutedCommand InsertCommand = new RoutedCommand();
        public static RoutedCommand DeleteCommand = new RoutedCommand();

        public ContainerEdit()
        {
            InitializeComponent();

            InsertCommand.InputGestures.Add(new KeyGesture(Key.Insert));
            DeleteCommand.InputGestures.Add(new KeyGesture(Key.Delete));

            OriginalContainer = ContainerUtilities.SerializeInnerContainer(Settings.ContainerInner).OuterXml;
            PreferHiddenKeyMaterial = true;

            keysListView.ItemsSource = Settings.ContainerInner.Keys;
            keysListView.SelectionChanged += KeysListView_SelectionChanged;

            groupsListView.ItemsSource = Settings.ContainerInner.Groups;
            groupsListView.SelectionChanged += GroupsListView_SelectionChanged;

            ActiveTabItem = containerTabControl.SelectedItem as TabItem;

            RefreshKeyConflictIndicators();
        }

        private void Tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SuppressSelectionHandlers || !(e.Source is TabControl))
            {
                return;
            }

            TabItem requestedTab = containerTabControl.SelectedItem as TabItem;

            if (requestedTab == ActiveTabItem)
            {
                return;
            }

            if (!TryResolvePendingKeyChanges())
            {
                SuppressSelectionHandlers = true;

                try
                {
                    containerTabControl.SelectedItem = ActiveTabItem;
                }
                finally
                {
                    SuppressSelectionHandlers = false;
                }

                return;
            }

            ActiveTabItem = requestedTab;
            ClearSelections();
        }

        private void KeysListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SuppressSelectionHandlers)
            {
                return;
            }

            KeyItem requestedKey = keysListView.SelectedItem as KeyItem;

            if (requestedKey == SelectedKeyItem)
            {
                return;
            }

            if (!TryResolvePendingKeyChanges())
            {
                SetKeySelection(SelectedKeyItem, false);
                return;
            }

            SetKeySelection(requestedKey, false);
        }

        private void GroupsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SuppressSelectionHandlers)
            {
                return;
            }

            Container.GroupItem requestedGroup = groupsListView.SelectedItem as Container.GroupItem;

            if (requestedGroup == SelectedGroupItem)
            {
                return;
            }

            if (!TryResolvePendingKeyChanges())
            {
                SetGroupSelection(SelectedGroupItem);
                return;
            }

            SetGroupSelection(requestedGroup);
        }

        private void DetachCurrentKeyEditor()
        {
            ContainerEditKeyControl keyEdit = ItemView.Content as ContainerEditKeyControl;

            if (keyEdit != null)
            {
                keyEdit.DirtyStateChanged -= KeyEdit_DirtyStateChanged;
                keyEdit.HidePreferenceChanged -= KeyEdit_HidePreferenceChanged;
                keyEdit.Saved -= KeyEdit_Saved;
            }
        }

        private void ShowKeyEditor(KeyItem keyItem, bool focusNameOnLoad)
        {
            DetachCurrentKeyEditor();

            if (keyItem == null)
            {
                ItemView.Content = null;
                return;
            }

            ContainerEditKeyControl keyEdit = new ContainerEditKeyControl(keyItem, PreferHiddenKeyMaterial, focusNameOnLoad);
            keyEdit.DirtyStateChanged += KeyEdit_DirtyStateChanged;
            keyEdit.HidePreferenceChanged += KeyEdit_HidePreferenceChanged;
            keyEdit.Saved += KeyEdit_Saved;

            ItemView.Content = keyEdit;
        }

        private void ShowGroupEditor(Container.GroupItem groupItem)
        {
            DetachCurrentKeyEditor();

            if (groupItem == null)
            {
                ItemView.Content = null;
                return;
            }

            ItemView.Content = new ContainerEditGroupControl(groupItem);
        }

        private void SetKeySelection(KeyItem keyItem, bool focusNameOnLoad)
        {
            SuppressSelectionHandlers = true;

            try
            {
                SelectedGroupItem = null;
                groupsListView.SelectedItem = null;

                SelectedKeyItem = keyItem;
                keysListView.SelectedItem = keyItem;
                ShowKeyEditor(keyItem, focusNameOnLoad);
            }
            finally
            {
                SuppressSelectionHandlers = false;
            }
        }

        private void SetGroupSelection(Container.GroupItem groupItem)
        {
            SuppressSelectionHandlers = true;

            try
            {
                SelectedKeyItem = null;
                keysListView.SelectedItem = null;

                SelectedGroupItem = groupItem;
                groupsListView.SelectedItem = groupItem;
                ShowGroupEditor(groupItem);
            }
            finally
            {
                SuppressSelectionHandlers = false;
            }
        }

        private void ClearSelections()
        {
            SuppressSelectionHandlers = true;

            try
            {
                SelectedKeyItem = null;
                SelectedGroupItem = null;
                keysListView.SelectedItem = null;
                groupsListView.SelectedItem = null;
                DetachCurrentKeyEditor();
                ItemView.Content = null;
            }
            finally
            {
                SuppressSelectionHandlers = false;
            }
        }

        private void KeyEdit_DirtyStateChanged(object sender, EventArgs e)
        {
            // Reserved for future UI polish if the dialog needs to surface editor dirty state.
        }

        private void KeyEdit_HidePreferenceChanged(object sender, EventArgs e)
        {
            ContainerEditKeyControl keyEdit = sender as ContainerEditKeyControl;

            if (keyEdit != null)
            {
                PreferHiddenKeyMaterial = keyEdit.IsKeyMaterialHidden;
            }
        }

        private void KeyEdit_Saved(object sender, EventArgs e)
        {
            ContainerEditKeyControl keyEdit = sender as ContainerEditKeyControl;

            if (keyEdit != null)
            {
                PreferHiddenKeyMaterial = keyEdit.IsKeyMaterialHidden;
            }

            RefreshKeyConflictIndicators();
        }

        private bool TryResolvePendingKeyChanges()
        {
            ContainerEditKeyControl keyEdit = ItemView.Content as ContainerEditKeyControl;

            if (keyEdit == null)
            {
                return true;
            }

            PreferHiddenKeyMaterial = keyEdit.IsKeyMaterialHidden;

            if (!keyEdit.HasUnsavedChanges)
            {
                return true;
            }

            if (!Properties.Settings.Default.PromptSavePendingKeyChanges)
            {
                return true;
            }

            MessageBoxResult res = MessageBox.Show(
                "The selected key has unsaved changes. Save before leaving?",
                "Unsaved Key Changes",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning
            );

            if (res == MessageBoxResult.Yes)
            {
                return keyEdit.TrySaveChanges();
            }

            return res != MessageBoxResult.Cancel;
        }

        private void RefreshKeyConflictIndicators()
        {
            KeyConflictHelper.RefreshConflictStates(Settings.ContainerInner.Keys);
            keysListView.Items.Refresh();
        }

        private static string GenerateKeyMaterialForAlgorithm(int algorithmId)
        {
            List<byte> keyBytes = null;

            if (algorithmId == (byte)AlgorithmId.AES256)
            {
                keyBytes = KeyGenerator.GenerateVarKey(32);
            }
            else if (algorithmId == (byte)AlgorithmId.DESOFB || algorithmId == (byte)AlgorithmId.DESXL)
            {
                keyBytes = KeyGenerator.GenerateSingleDesKey();
            }
            else if (algorithmId == (byte)AlgorithmId.ADP)
            {
                keyBytes = KeyGenerator.GenerateVarKey(5);
            }

            if (keyBytes == null)
            {
                return string.Empty;
            }

            return BitConverter.ToString(keyBytes.ToArray()).Replace("-", string.Empty);
        }

        private KeyItem CreateNewKey()
        {
            KeyItem key = new KeyItem();

            key.Id = Settings.ContainerInner.NextKeyNumber;
            key.Name = string.Format("Key {0}", Settings.ContainerInner.NextKeyNumber);
            Settings.ContainerInner.NextKeyNumber++;

            if (SelectedKeyItem != null)
            {
                key.ActiveKeyset = SelectedKeyItem.ActiveKeyset;
                key.KeysetId = SelectedKeyItem.KeysetId;
                key.KeyTypeAuto = SelectedKeyItem.KeyTypeAuto;
                key.KeyTypeTek = SelectedKeyItem.KeyTypeTek;
                key.KeyTypeKek = SelectedKeyItem.KeyTypeKek;
                key.AlgorithmId = SelectedKeyItem.AlgorithmId;

                // Keep the selected key's effective keyset, but advance the fields that
                // are most likely to require uniqueness inside that keyset.
                key.Sln = KeyConflictHelper.GetNextAvailableValue(
                    Settings.ContainerInner.Keys,
                    SelectedKeyItem.ActiveKeyset,
                    SelectedKeyItem.KeysetId,
                    SelectedKeyItem.Sln,
                    existingKey => existingKey.Sln
                );

                key.KeyId = KeyConflictHelper.GetNextAvailableValue(
                    Settings.ContainerInner.Keys,
                    SelectedKeyItem.ActiveKeyset,
                    SelectedKeyItem.KeysetId,
                    SelectedKeyItem.KeyId,
                    existingKey => existingKey.KeyId
                );

                key.Key = GenerateKeyMaterialForAlgorithm(key.AlgorithmId);
            }
            else
            {
                key.ActiveKeyset = true;
                key.KeysetId = 1;
                key.KeyTypeAuto = true;
                key.KeyTypeTek = false;
                key.KeyTypeKek = false;
                key.KeyId = KeyConflictHelper.GetNextAvailableValue(
                    Settings.ContainerInner.Keys,
                    true,
                    1,
                    0,
                    existingKey => existingKey.KeyId
                );
                key.AlgorithmId = 0x84;
                key.Sln = KeyConflictHelper.GetNextAvailableValue(
                    Settings.ContainerInner.Keys,
                    true,
                    1,
                    0,
                    existingKey => existingKey.Sln
                );
                key.Key = GenerateKeyMaterialForAlgorithm(key.AlgorithmId);
            }

            return key;
        }

        void ContainerEdit_Closing(object sender, CancelEventArgs e)
        {
            if (!TryResolvePendingKeyChanges())
            {
                e.Cancel = true;
                return;
            }

            string currentContainer = ContainerUtilities.SerializeInnerContainer(Settings.ContainerInner).OuterXml;

            if (OriginalContainer != currentContainer)
            {
                Settings.ContainerSaved = false;
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            if (containerTabControl.SelectedItem == keysTabItem)
            {
                if (!TryResolvePendingKeyChanges())
                {
                    return;
                }

                KeyItem key = CreateNewKey();
                Settings.ContainerInner.Keys.Add(key);

                RefreshKeyConflictIndicators();
                SetKeySelection(key, true);
            }
            else if (containerTabControl.SelectedItem == groupsTabItem)
            {
                Container.GroupItem group = new Container.GroupItem();
                group.Id = Settings.ContainerInner.NextGroupNumber;
                group.Name = string.Format("Group {0}", Settings.ContainerInner.NextGroupNumber);
                Settings.ContainerInner.NextGroupNumber++;
                group.Keys = new List<int>();
                Settings.ContainerInner.Groups.Add(group);

                SetGroupSelection(group);
            }
        }

        private void Up_Click(object sender, RoutedEventArgs e)
        {
            if (containerTabControl.SelectedItem == keysTabItem)
            {
                if (keysListView.SelectedItem != null)
                {
                    int index = keysListView.SelectedIndex;

                    if (index - 1 >= 0)
                    {
                        Settings.ContainerInner.Keys.Move(index, index - 1);
                    }
                }
            }
            else if (containerTabControl.SelectedItem == groupsTabItem)
            {
                if (groupsListView.SelectedItem != null)
                {
                    int index = groupsListView.SelectedIndex;

                    if (index - 1 >= 0)
                    {
                        Settings.ContainerInner.Groups.Move(index, index - 1);
                    }
                }
            }
        }

        private void Down_Click(object sender, RoutedEventArgs e)
        {
            if (containerTabControl.SelectedItem == keysTabItem)
            {
                if (keysListView.SelectedItem != null)
                {
                    int index = keysListView.SelectedIndex;

                    if (index + 1 < Settings.ContainerInner.Keys.Count)
                    {
                        Settings.ContainerInner.Keys.Move(index, index + 1);
                    }
                }
            }
            else if (containerTabControl.SelectedItem == groupsTabItem)
            {
                if (groupsListView.SelectedItem != null)
                {
                    int index = groupsListView.SelectedIndex;

                    if (index + 1 < Settings.ContainerInner.Groups.Count)
                    {
                        Settings.ContainerInner.Groups.Move(index, index + 1);
                    }
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (containerTabControl.SelectedItem == keysTabItem)
            {
                if (keysListView.SelectedItem != null)
                {
                    int index = keysListView.SelectedIndex;
                    int id = Settings.ContainerInner.Keys[index].Id;

                    SuppressSelectionHandlers = true;

                    try
                    {
                        if (SelectedKeyItem == Settings.ContainerInner.Keys[index])
                        {
                            SelectedKeyItem = null;
                            DetachCurrentKeyEditor();
                            ItemView.Content = null;
                        }

                        foreach (Container.GroupItem groupItem in Settings.ContainerInner.Groups)
                        {
                            if (groupItem.Keys.Contains(id))
                            {
                                groupItem.Keys.Remove(id);
                            }
                        }

                        Settings.ContainerInner.Keys.RemoveAt(index);
                        keysListView.SelectedItem = null;
                    }
                    finally
                    {
                        SuppressSelectionHandlers = false;
                    }

                    RefreshKeyConflictIndicators();
                }
            }
            else if (containerTabControl.SelectedItem == groupsTabItem)
            {
                if (groupsListView.SelectedItem != null)
                {
                    int index = groupsListView.SelectedIndex;

                    SuppressSelectionHandlers = true;

                    try
                    {
                        SelectedGroupItem = null;
                        ItemView.Content = null;
                        Settings.ContainerInner.Groups.RemoveAt(index);
                        groupsListView.SelectedItem = null;
                    }
                    finally
                    {
                        SuppressSelectionHandlers = false;
                    }
                }
            }
        }
    }
}
