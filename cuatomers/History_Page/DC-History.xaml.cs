using cuatomers.DAL;
using cuatomers;
using napeans.dal;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace cuatomers
{
    /// <summary>
    /// Interaction logic for DC_History.xaml
    /// </summary>
    public partial class DC_History : Page
    {
        ProcessData _processData;
        public DC_History(IAdoHelper adoHelper)
        {
            InitializeComponent();
            _processData = new ProcessData(adoHelper);
            LoadChallanDataGrid();
        }
        private void LoadChallanDataGrid()
        {
            var ds = _processData.GetDeliveryChallanData(); // Your method that returns a DataSet

            if (ds != null && ds.Tables.Count > 0)
            {
                challanDataGrid.ItemsSource = ds.Tables[0].DefaultView;
            }
        }


    }
}


