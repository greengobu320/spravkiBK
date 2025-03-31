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
using System.IO;
using System.Xml;
using System.Globalization;
using Newtonsoft.Json;
using System.Security.Policy;

namespace spravkiBK
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private List<DataSet> DatasetsList = new List<DataSet>();

        private void WorkingFile(String xmlContent)
        {
            var GetSpravkaBKData = new GetSpravkaBKData();
            DatasetsTreeView.Items.Clear();
            DatasetsList.Clear();
            DatasetsList = GetSpravkaBKData.GetData(DatasetsList, xmlContent);           
            foreach (DataSet ds in DatasetsList)      
            {      
                TreeViewItem personNode = new TreeViewItem();
                personNode.Header = ds.DataSetName;                
                foreach (DataTable table in ds.Tables)
                {
                    TreeViewItem tableNode = new TreeViewItem();
                    tableNode.Header = table.TableName;
                    personNode.Items.Add(tableNode);
                }
                DatasetsTreeView.Items.Add(personNode);               
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var GetSpravkaBKData = new GetSpravkaBKData();
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "XSB files (*.xsb)|*.xsb|All files (*.*)|*.*",
                Title = "Выберите XSB файл"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                Console.WriteLine("Выбран файл: " + filePath);
                Dictionary <string, string> dict = GetSpravkaBKData.DecodeXsbFile(filePath);
                if (dict["result"] == "error")
                {
                    MessageBox.Show($"{dict["data"]}", "Ошибка декодирования", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else {WorkingFile(dict["data"]);}
                
            }
        }
         private void DatasetsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            var treeView = sender as TreeView;
            if (treeView == null || treeView.SelectedItem == null)
                return;

            // Получаем TreeViewItem по объекту
            TreeViewItem selectedTreeViewItem = GetTreeViewItemFromObject(treeView, treeView.SelectedItem);

            if (selectedTreeViewItem == null) return;

            string selectedHeader = selectedTreeViewItem.Header?.ToString() ?? "";
            string parentHeader = (ItemsControl.ItemsControlFromItemContainer(selectedTreeViewItem) as TreeViewItem)?.Header?.ToString() ?? "";

            if (!string.IsNullOrEmpty(parentHeader))
            {
                var table = DatasetsList
                    .FirstOrDefault(ds => ds.DataSetName == parentHeader)?
                    .Tables
                    .Cast<DataTable>()
                    .FirstOrDefault(t => t.TableName == selectedHeader);

                if (table != null)
                    DataGridView.ItemsSource = table.DefaultView;
            }

        }
        private TreeViewItem GetTreeViewItemFromObject(ItemsControl container, object item)
        {
            if (container == null) return null;

            foreach (object i in container.Items)
            {
                TreeViewItem treeItem = container.ItemContainerGenerator.ContainerFromItem(i) as TreeViewItem;
                if (treeItem != null)
                {
                    if (i == item)
                        return treeItem;

                    TreeViewItem child = GetTreeViewItemFromObject(treeItem, item);
                    if (child != null)
                        return child;
                }
            }
            return null;
        }
         private async void GetDataFromAis_Button_Click(object sender, RoutedEventArgs e)
        {
            var GetEIAPData = new GetEIAPData();
            string result= await   searchData("Шаманьков Кирилл Анатольевич", "1984-07-02");
            if (result== null) { return; }
            DataSet EiapDataset = new DataSet
            {
                DataSetName = "EiapDataset"
            };
            List <string> urlList = new List<string>();
            urlList.Add("http://ekpiprom.ais3.tax.nalog.ru:9400/inias/csc/pf-is/app-dnpw/service/dnpwcatalogfl/api/v1/bd/full");
            urlList.Add("http://ekpiprom.ais3.tax.nalog.ru:9400/inias/csc/pf-is/app-dnpw/service/dnpwcatalogfl/api/v1/nedvizhdata");
            urlList.Add("http://ekpiprom.ais3.tax.nalog.ru:9400/inias/csc/pf-is/app-dnpw/service/dnpwcatalogfl/api/v1/transportdata");
            urlList.Add("http://ekpiprom.ais3.tax.nalog.ru:9400/inias/csc/pf-is/app-dnpw/service/dnpwcatalogfl/api/v1/vodnvozddata");
            urlList.Add("http://ekpiprom.ais3.tax.nalog.ru:9400/inias/csc/pf-is/app-dnpw/service/dnpwcatalogfl/api/v1/bankaccounts");
           
            foreach (string url in urlList)
            {
                var resultSearch = await GetEIAPData.GetAsyncDetailData(url, result);
                if (resultSearch == null) { continue; }
                if (resultSearch["result"] != "success") { Console.WriteLine($"Ошибка получения данных: {resultSearch["message"]}"); continue; }
                Console.WriteLine(resultSearch["value"]);
                Console.WriteLine("-----------------------------");
            }
            
            
            Console.WriteLine();


        }

        private async Task <string>  searchData(string name, string BrithDate)
        {
            string result = null;
            var GetEIAPData = new GetEIAPData();
            var resultSearch = await GetEIAPData.GetAsyncData(name);
            if (resultSearch == null) { return result; }
            if (resultSearch["result"] != "success") { Console.WriteLine($"Ошибка получения данных: {resultSearch["message"]}"); return result; }
            Dictionary<string, Object> oneDict = JsonConvert.DeserializeObject<Dictionary<string, Object>>(resultSearch["value"].ToString());
            if (oneDict == null) { Console.WriteLine("Ошибка дессириализации"); }
            if (Convert.ToInt32(oneDict["Total"]) == 0) { Console.WriteLine("Ошибка поиска данных. Найдено записей 0"); return result; }
            string DictValue = oneDict["ResultItems"].ToString();
            DataTable ResultItems = JsonConvert.DeserializeObject<DataTable>(DictValue);
            foreach (DataRow row in ResultItems.Rows )
            {                
                if (FormatDate(row["BirthDate"].ToString()) == BrithDate)
                {
                    result = row["FId"].ToString();
                    break;
                }
            }
            return result;
        }

        private static string FormatDate(string value)
        {
            if(DateTime.TryParse(value, out DateTime parseDate))
            {
                return parseDate.ToString("yyyy-MM-dd");
            }
            return null;
        }
    }



}
