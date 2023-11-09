using KFDtool.P25.Constant;
using KFDtool.P25.Generator;
using KFDtool.P25.TransferConstructs;
using KFDtool.P25.Validator;
using KFDtool.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using KFDtool.KMF.KMFConnection;
using KFDtool.KMF.TransferConstructs;

namespace KFDtool.Gui.Control
{
    /// <summary>
    /// Interaction logic for P25ViewKeyInfo.xaml
    /// </summary>
    public partial class KMFViewKeys : UserControl
    {
        public KMFViewKeys()
        {
            InitializeComponent();
        }

        private async void View_KeyItems_Click(object sender, RoutedEventArgs e)
        {
            IKMFConnection kmf = new OPKMFConnection("https://localhost:7138", "");
            List<KMFKeyItem> keys = await kmf.GetAllKeys();

            KeyItems.ItemsSource = keys;

            KeyItems.Items.SortDescriptions.Add(new SortDescription("KeysetId", ListSortDirection.Ascending));
            KeyItems.Items.SortDescriptions.Add(new SortDescription("Sln", ListSortDirection.Ascending));
        }
    }
}
