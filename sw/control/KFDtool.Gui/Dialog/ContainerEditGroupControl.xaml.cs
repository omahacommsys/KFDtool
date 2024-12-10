using KFDtool.Container;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KFDtool.Gui.Dialog
{
    /// <summary>
    /// Interaction logic for ContainerEditGroupControl.xaml
    /// </summary>
    public partial class ContainerEditGroupControl : UserControl
    {
        private Container.GroupItem LocalGroup { get; set; }

        private List<int> Keys;

        public ObservableCollection<KeyValuePair<int, string>> Available { get; set; }
        public ObservableCollection<KeyValuePair<int, string>> Selected { get; set; }

        public ContainerEditGroupControl(Container.GroupItem groupItem)
        {
            InitializeComponent();

            LocalGroup = groupItem;

            Keys = new List<int>();
            Keys.AddRange(groupItem.Keys);

            Available = new ObservableCollection<KeyValuePair<int, string>>();
            Selected = new ObservableCollection<KeyValuePair<int, string>>();

            txtName.Text = groupItem.Name;

            this.DataContext = this;

            UpdateColumns();
        }

        private void UpdateColumns()
        {
            Debug.Print("Updaing key group columns");
            Available.Clear();

            foreach (KeyItem keyItem in Settings.ContainerInner.Keys)
            {
                Available.Add(new KeyValuePair<int, string>(keyItem.Id, keyItem.Name));
            }

            Selected.Clear();

            foreach (int key in Keys)
            {
                Selected.Add(new KeyValuePair<int, string>(key, Available.SingleOrDefault(i => i.Key == key).Value));
            }

            foreach (KeyValuePair<int, string> selected in Selected)
            {
                Available.Remove(Available.SingleOrDefault(i => i.Key == selected.Key));
            }
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            if (lbAvailable.SelectedItem != null)
            {
                foreach( KeyValuePair<int, string> selectedKey in lbAvailable.SelectedItems)
                {
                    Keys.Add(selectedKey.Key);
                }

                UpdateColumns();
            }
        }

        private void Remove_Button_Click(object sender, RoutedEventArgs e)
        {
            if (lbSelected.SelectedItem != null)
            {
                foreach( KeyValuePair<int, string> selectedKey in lbSelected.SelectedItems)
                {
                    Keys.Remove(selectedKey.Key);
                }

                UpdateColumns();
            }
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            if (txtName.Text.Length == 0)
            {
                MessageBox.Show("Group name required", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (txtName.Text != LocalGroup.Name)
            {
                foreach (Container.GroupItem groupItem in Settings.ContainerInner.Groups)
                {
                    if (txtName.Text == groupItem.Name)
                    {
                        MessageBox.Show("Group name must be unique", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }

            LocalGroup.Name = txtName.Text;
            LocalGroup.Keys.Clear();
            LocalGroup.Keys.AddRange(Keys);
        }
    }
}
