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

        private void WorkingFile(String FileName)
        {
            DatasetsList.Clear();
            string xmlContent = File.ReadAllText(FileName);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlContent);
            XmlNodeList spravki = doc.SelectNodes("//Справка");
            int index = 1;
            foreach (XmlNode spravka in spravki)
            {
                DataSet ds = new DataSet($"Person{index}");
                ds.DataSetName = $"Person{index}";
                // Личные данные                
                ds.Tables.Add(Personal(spravka));
                // Расходы
                ds.Tables.Add(Expenses(spravka));
                // Доходы               
                ds.Tables.Add(Incomes(spravka));
                // Счета                
                ds.Tables.Add(Accounts(spravka));

                //Недвижимое Имущество
                ds.Tables.Add(RealEstate(spravka));
                //Недвижимое имущество в собственности
                ds.Tables.Add(UsageRealEstate(spravka));
                //Транспортные средства
                ds.Tables.Add(Vehicles(spravka));
                //Цифровые финансы
                ds.Tables.Add(DigitalFinancialAssets(spravka));
                //Цифровая валюта
                ds.Tables.Add(DigitalCurrencies(spravka));
                //Ценные Бумаги
                ds.Tables.Add(Securities(spravka));
                //Срочные Обязательства
                ds.Tables.Add(GetLiabilities(spravka));
                //Подарки
                ds.Tables.Add(GetGifts(spravka));
                // Здесь можно сохранить или обработать DataSet
                Console.WriteLine($"Создан DataSet для Person{index} с {ds.Tables.Count} таблицами.");
                DatasetsList.Add(ds);

                TreeViewItem personNode = new TreeViewItem();
                personNode.Header = ds.DataSetName;

                // Добавляем таблицы как дочерние узлы
                foreach (DataTable table in ds.Tables)
                {
                    TreeViewItem tableNode = new TreeViewItem();
                    tableNode.Header = table.TableName;
                    personNode.Items.Add(tableNode);
                }
                DatasetsTreeView.Items.Add(personNode);
                index++;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "XSB files (*.xsb)|*.xsb|All files (*.*)|*.*",
                Title = "Выберите XSB файл"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                Console.WriteLine("Выбран файл: " + filePath);
                WorkingFile(filePath);
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


        private static DataTable Personal(XmlNode spravka)
        {
            DataTable personal = new DataTable("ЛичныеДанные");
            personal.Columns.Add("Тип"); // кто это: Заявитель / Супруг / Ребёнок
            personal.Columns.Add("Фамилия");
            personal.Columns.Add("Имя");
            personal.Columns.Add("Отчество");
            personal.Columns.Add("ДатаРождения");
            personal.Columns.Add("СНИЛС");
            personal.Columns.Add("ТипПаспорта");
            personal.Columns.Add("СерияПаспорта");
            personal.Columns.Add("НомерПаспорта");
            personal.Columns.Add("ДатаВыдачиПаспорта");
            personal.Columns.Add("ОрганВыдачиПаспорта");
            personal.Columns.Add("Пол");
            personal.Columns.Add("Регистрация");
            personal.Columns.Add("Должность");
            personal.Columns.Add("Организация");

            // Все возможные блоки
            var personalBlocks = new (string XPath, string Тип)[]
            {
    (".//Атрибут[@Имя='ЛичныеДанныеЗаявителя']/ЛичныеДанные", "Заявитель"),
    (".//Атрибут[@Имя='ЛичныеДанные']/ЛичныеДанные", "Супруг/Супруга"),
            };

            // Если ребёнок — в некоторых справках будет только один ЛичныеДанные
            if (!spravka.OuterXml.Contains("ЛичныеДанныеЗаявителя"))
            {
                personalBlocks = new (string, string)[] {
        (".//Атрибут[@Имя='ЛичныеДанные']/ЛичныеДанные", "Ребёнок")
    };
            }

            foreach (var (xpath, тип) in personalBlocks)
            {
                var node = spravka.SelectSingleNode(xpath);
                if (node == null) continue;

                var row = personal.NewRow();
                row["Тип"] = тип;

                // Документ: Паспорт или Свидетельство
                var docNode = node.SelectSingleNode("./Атрибут[@Имя='ДокументУдостоверяющийЛичность']/*");
                row["Фамилия"] = docNode?.SelectSingleNode("./Атрибут[@Имя='Фамилия']")?.InnerText;
                row["Имя"] = docNode?.SelectSingleNode("./Атрибут[@Имя='Имя']")?.InnerText;
                row["Отчество"] = docNode?.SelectSingleNode("./Атрибут[@Имя='Отчество']")?.InnerText;
                row["ДатаРождения"] = docNode?.SelectSingleNode("./Атрибут[@Имя='ДатаРождения']")?.InnerText;
                row["СНИЛС"] = node.SelectSingleNode("./Атрибут[@Имя='СНИЛС']")?.InnerText;
                var адрес = node.SelectSingleNode("./Атрибут[@Имя='Регистрация']/Адрес/Атрибут[@Имя='ДляПечати']");
                row["Регистрация"] = адрес?.InnerText;
                var местоРаботы = node.SelectSingleNode("./Атрибут[@Имя='МестоРаботы']/МестоРаботы");
                row["Должность"] = местоРаботы?.SelectSingleNode("./Атрибут[@Имя='Должность']")?.InnerText;
                row["Организация"] = местоРаботы?.SelectSingleNode("./Атрибут[@Имя='НазваниеОрганизации']")?.InnerText;
                row["ТипПаспорта"] = docNode?.SelectSingleNode("./Атрибут[@Имя='Тип']")?.InnerText;
                row["СерияПаспорта"] = docNode?.SelectSingleNode("./Атрибут[@Имя='Серия']")?.InnerText;
                row["НомерПаспорта"] = docNode?.SelectSingleNode("./Атрибут[@Имя='Номер']")?.InnerText;
                row["ДатаВыдачиПаспорта"] = docNode?.SelectSingleNode("./Атрибут[@Имя='ДатаВыдачи']")?.InnerText;
                row["ОрганВыдачиПаспорта"] = docNode?.SelectSingleNode("./Атрибут[@Имя='ОрганВыдавшийДокумент']")?.InnerText;
                row["Пол"] = docNode?.SelectSingleNode("./Атрибут[@Имя='Пол']")?.InnerText;
                personal.Rows.Add(row);
            }
            return personal;

        }
        private static DataTable Expenses(XmlNode spravka)
        {
            DataTable expenses = new DataTable("Расходы");

            // Основные колонки
            expenses.Columns.Add("ОснованиеПриобретения");
            expenses.Columns.Add("Наименование");
            expenses.Columns.Add("СуммаСделки");
            expenses.Columns.Add("СуммаСделкиДляПечати");
            expenses.Columns.Add("ИсточникСредств");
            expenses.Columns.Add("ВидНедвижимогоИмущества");
            expenses.Columns.Add("Площадь");
            expenses.Columns.Add("ПлощадьДляПечати");
            expenses.Columns.Add("ВидАктива");

            // Адресные колонки
            expenses.Columns.Add("АдресДляПечати");
            expenses.Columns.Add("Страна");
            expenses.Columns.Add("Индекс");
            expenses.Columns.Add("Регион");
            expenses.Columns.Add("Город");
            expenses.Columns.Add("Улица");
            expenses.Columns.Add("Дом");
            expenses.Columns.Add("Корпус");
            expenses.Columns.Add("Квартира");
            expenses.Columns.Add("ДополнительнаяИнформация");

            // Колонки для источников средств
            expenses.Columns.Add("Источник1");
            expenses.Columns.Add("Сумма1");
            expenses.Columns.Add("Источник2");
            expenses.Columns.Add("Сумма2");
            expenses.Columns.Add("Кредитор");
            expenses.Columns.Add("ИННКредитора");
            expenses.Columns.Add("ОГРНКредитора");
            expenses.Columns.Add("АдресКредитора");
            expenses.Columns.Add("КредитныйДоговор");
            expenses.Columns.Add("СуммаКредита");
            expenses.Columns.Add("ОстатокКредита");
            expenses.Columns.Add("УсловияКредита");

            XmlNodeList расходы = spravka.SelectNodes(".//Расход");
            foreach (XmlNode расход in расходы)
            {
                var row = expenses.NewRow();

                // Основные данные
                row["ОснованиеПриобретения"] = расход.SelectSingleNode("./Атрибут[@Имя='ОснованиеПриобретения']")?.InnerText;
                row["Наименование"] = расход.SelectSingleNode("./Атрибут[@Имя='Наименование']")?.InnerText;
                row["СуммаСделки"] = расход.SelectSingleNode("./Атрибут[@Имя='СуммаСделки']")?.InnerText;
                row["СуммаСделкиДляПечати"] = расход.SelectSingleNode("./Атрибут[@Имя='СуммаСделкиДляПечати']")?.InnerText;
                row["ИсточникСредств"] = расход.SelectSingleNode("./Атрибут[@Имя='ИсточникСредств']")?.InnerText;
                row["ВидНедвижимогоИмущества"] = расход.SelectSingleNode("./Атрибут[@Имя='ВидНедвижимогоИмущества']")?.InnerText;
                row["Площадь"] = расход.SelectSingleNode("./Атрибут[@Имя='Площадь']")?.InnerText;
                row["ПлощадьДляПечати"] = расход.SelectSingleNode("./Атрибут[@Имя='ПлощадьДляПечати']")?.InnerText;
                row["ВидАктива"] = расход.SelectSingleNode("./Атрибут[@Имя='ВидАктива']")?.InnerText;

                // Адресные данные
                var адрес = расход.SelectSingleNode("./Атрибут[@Имя='МестонахождениеИмущества']/Адрес");
                if (адрес != null)
                {
                    row["АдресДляПечати"] = адрес.SelectSingleNode("./Атрибут[@Имя='ДляПечати']")?.InnerText;
                    row["Страна"] = адрес.SelectSingleNode("./Атрибут[@Имя='Страна']")?.InnerText;
                    row["Индекс"] = адрес.SelectSingleNode("./Атрибут[@Имя='Индекс']")?.InnerText;
                    row["Регион"] = адрес.SelectSingleNode("./Атрибут[@Имя='Регион']")?.InnerText;
                    row["Город"] = адрес.SelectSingleNode("./Атрибут[@Имя='Город']")?.InnerText;
                    row["Улица"] = адрес.SelectSingleNode("./Атрибут[@Имя='Улица']")?.InnerText;
                    row["Дом"] = адрес.SelectSingleNode("./Атрибут[@Имя='Дом']")?.InnerText;
                    row["Корпус"] = адрес.SelectSingleNode("./Атрибут[@Имя='Корпус']")?.InnerText;
                    row["Квартира"] = адрес.SelectSingleNode("./Атрибут[@Имя='Квартира']")?.InnerText;
                    row["ДополнительнаяИнформация"] = адрес.SelectSingleNode("./Атрибут[@Имя='ДополнительнаяИнформация']")?.InnerText;
                }

                // Источники средств
                var источники = расход.SelectNodes("./Атрибут[@Имя='ИсточникиСредств']/ИсточникСредств");
                if (источники != null && источники.Count > 0)
                {
                    // Первый источник
                    var источник1 = источники[0];
                    row["Источник1"] = источник1.SelectSingleNode("./Атрибут[@Имя='Источник']")?.InnerText;
                    row["Сумма1"] = источник1.SelectSingleNode("./Атрибут[@Имя='СуммаСредств']")?.InnerText;

                    // Второй источник (если есть)
                    if (источники.Count > 1)
                    {
                        var источник2 = источники[1];
                        row["Источник2"] = источник2.SelectSingleNode("./Атрибут[@Имя='Источник']/СрочноеОбязательство/Атрибут[@Имя='СодержаниеОбязательства']")?.InnerText;
                        row["Сумма2"] = источник2.SelectSingleNode("./Атрибут[@Имя='СуммаСредств']")?.InnerText;

                        var кредит = источник2.SelectSingleNode("./Атрибут[@Имя='Источник']/СрочноеОбязательство");
                        if (кредит != null)
                        {
                            row["Кредитор"] = кредит.SelectSingleNode("./Атрибут[@Имя='ВтораяСторонаОбязательстваДляПечати']")?.InnerText;
                            row["ИННКредитора"] = кредит.SelectSingleNode("./Атрибут[@Имя='ИНН']")?.InnerText;
                            row["ОГРНКредитора"] = кредит.SelectSingleNode("./Атрибут[@Имя='ОГРН']")?.InnerText;
                            row["АдресКредитора"] = кредит.SelectSingleNode("./Атрибут[@Имя='Адрес']/Адрес/Атрибут[@Имя='ДляПечати']")?.InnerText;
                            row["КредитныйДоговор"] = кредит.SelectSingleNode("./Атрибут[@Имя='ОснованиеВозникновения']")?.InnerText;
                            row["СуммаКредита"] = кредит.SelectSingleNode("./Атрибут[@Имя='СуммаОбязательства']")?.InnerText;
                            row["ОстатокКредита"] = кредит.SelectSingleNode("./Атрибут[@Имя='РазмерОбязательства']")?.InnerText;        
                            row["УсловияКредита"] = кредит.SelectSingleNode("./Атрибут[@Имя='УсловиеОбязательства']")?.InnerText;
                        }
                    }
                }

                expenses.Rows.Add(row);
            }

            return expenses;
        }
        private static DataTable Incomes(XmlNode spravka)
        {
            DataTable incomes = new DataTable("Доходы");
            incomes.Columns.Add("ВидДохода");
            incomes.Columns.Add("ВеличинаДохода");
            incomes.Columns.Add("СведениеОДоходе");
            incomes.Columns.Add("КатегорияДохода");
            incomes.Columns.Add("ВидИногоДохода");
            incomes.Columns.Add("ИмяДарителя");
            incomes.Columns.Add("ДатаРожденияДарителя");
            incomes.Columns.Add("Пояснения");

            XmlNodeList доходы = spravka.SelectNodes(".//Доход");
            foreach (XmlNode доход in доходы)
            {
                var row = incomes.NewRow();
                row["ВидДохода"] = доход.SelectSingleNode("./Атрибут[@Имя='ВидДохода']")?.InnerText;
                row["ВеличинаДохода"] = доход.SelectSingleNode("./Атрибут[@Имя='ВеличинаДохода']")?.InnerText;
                row["СведениеОДоходе"] = доход.SelectSingleNode("./Атрибут[@Имя='СведениеОДоходе']")?.InnerText;
                row["КатегорияДохода"] = доход.SelectSingleNode("./Атрибут[@Имя='КатегорияДохода']")?.InnerText;
                row["ВидИногоДохода"] = доход.SelectSingleNode("./Атрибут[@Имя='ВидИногоДохода']")?.InnerText;

                // Особые поля для доходов от дарения
                row["ИмяДарителя"] = доход.SelectSingleNode("./Атрибут[@Имя='Имя']")?.InnerText;
                row["ДатаРожденияДарителя"] = доход.SelectSingleNode("./Атрибут[@Имя='ДатаРождения']")?.InnerText;
                row["Пояснения"] = доход.SelectSingleNode("./Атрибут[@Имя='Пояснения']")?.InnerText;

                incomes.Rows.Add(row);
            }
            return incomes;        
        }
        private static DataTable Accounts(XmlNode spravka)
        {
            DataTable accounts = new DataTable("Счета");

            // Основные колонки
            accounts.Columns.Add("ВидСчета");
            accounts.Columns.Add("ВалютаСчета");
            accounts.Columns.Add("ДатаОткрытия", typeof(DateTime));
            accounts.Columns.Add("ДатаОткрытияДляПечати");
            accounts.Columns.Add("КредитнаяОрганизация");
            accounts.Columns.Add("КредитнаяОрганизацияДляПечати");
            accounts.Columns.Add("ОстатокНаСчете", typeof(decimal));
            accounts.Columns.Add("ОстатокНаСчетеДляПечати");
            accounts.Columns.Add("СуммаПоступившихСредств", typeof(decimal));
            accounts.Columns.Add("СуммаПоступившихСредствДляПечати");
            accounts.Columns.Add("СуммаНеПревышаетОбщийДоход");
            accounts.Columns.Add("ДатаВыписки", typeof(DateTime));
            accounts.Columns.Add("Идентификатор");
            accounts.Columns.Add("Пояснения");
            accounts.Columns.Add("ВидСчетаИВалютаДляПечати");

            XmlNodeList счета = spravka.SelectNodes(".//Счет");
            foreach (XmlNode счет in счета)
            {
                var row = accounts.NewRow();

                // Основные данные
                row["ВидСчета"] = счет.SelectSingleNode("./Атрибут[@Имя='ВидСчета']")?.InnerText;
                row["ВалютаСчета"] = счет.SelectSingleNode("./Атрибут[@Имя='ВалютаСчета']")?.InnerText;

                DateTime date;
                if (DateTime.TryParse(счет.SelectSingleNode("./Атрибут[@Имя='ДатаОткрытияСчета']")?.InnerText, out date))
                    row["ДатаОткрытия"] = date;
                row["ДатаОткрытияДляПечати"] = счет.SelectSingleNode("./Атрибут[@Имя='ДатаОткрытияСчетаДляПечати']")?.InnerText;

                row["КредитнаяОрганизация"] = счет.SelectSingleNode("./Атрибут[@Имя='КредитнаяОрганизация']")?.InnerText;
                row["КредитнаяОрганизацияДляПечати"] = счет.SelectSingleNode("./Атрибут[@Имя='КредитнаяОрганизацияДляПечати']")?.InnerText;

                decimal balance;
                if (decimal.TryParse(счет.SelectSingleNode("./Атрибут[@Имя='ОстатокНаСчете']")?.InnerText,
                                   NumberStyles.Any, CultureInfo.InvariantCulture, out balance))
                    row["ОстатокНаСчете"] = balance;
                row["ОстатокНаСчетеДляПечати"] = счет.SelectSingleNode("./Атрибут[@Имя='ОстатокНаСчетеДляПечати']")?.InnerText;

                decimal income;
                if (decimal.TryParse(счет.SelectSingleNode("./Атрибут[@Имя='СуммаПоступившихСредств']")?.InnerText,
                                   NumberStyles.Any, CultureInfo.InvariantCulture, out income))
                    row["СуммаПоступившихСредств"] = income;
                row["СуммаПоступившихСредствДляПечати"] = счет.SelectSingleNode("./Атрибут[@Имя='СуммаПоступившихСредствДляПечати']")?.InnerText;

                row["СуммаНеПревышаетОбщийДоход"] = счет.SelectSingleNode("./Атрибут[@Имя='СуммаНеПревышаетОбщийДоход']")?.InnerText;

                if (DateTime.TryParse(счет.SelectSingleNode("./Атрибут[@Имя='ДатаВыписки']")?.InnerText, out date))
                    row["ДатаВыписки"] = date;

                row["Идентификатор"] = счет.SelectSingleNode("./Атрибут[@Имя='Идентификатор']")?.InnerText;
                row["Пояснения"] = счет.SelectSingleNode("./Атрибут[@Имя='Пояснения']")?.InnerText;
                row["ВидСчетаИВалютаДляПечати"] = счет.SelectSingleNode("./Атрибут[@Имя='ВидСчетаИВалютаДляПечати']")?.InnerText;

                accounts.Rows.Add(row);
            }

            return accounts;
        }
        private static DataTable RealEstate(XmlNode spravka)
        {
            DataTable realEstate = new DataTable("НедвижимоеИмущество");

            // Основные колонки
            realEstate.Columns.Add("ВидИмущества");
            realEstate.Columns.Add("ТипАктива");
            realEstate.Columns.Add("Наименование");
            realEstate.Columns.Add("Площадь", typeof(double));
            realEstate.Columns.Add("ПлощадьДляПечати");
            realEstate.Columns.Add("ВидСобственности");
            realEstate.Columns.Add("ВидСобственностиДляПечати");
            realEstate.Columns.Add("ОснованиеПриобретения");
            realEstate.Columns.Add("ПолноеОписание");

            // Адресные колонки
            realEstate.Columns.Add("АдресДляПечати");
            realEstate.Columns.Add("Страна");
            realEstate.Columns.Add("Индекс");
            realEstate.Columns.Add("Регион");
            realEstate.Columns.Add("Город");
            realEstate.Columns.Add("Улица");
            realEstate.Columns.Add("Дом");
            realEstate.Columns.Add("Корпус");
            realEstate.Columns.Add("Квартира");
            realEstate.Columns.Add("ДополнительнаяИнформация");

            // Колонки для совладельцев
            realEstate.Columns.Add("Совладельцы");
            realEstate.Columns.Add("ДатыРожденияСовладельцев");
            realEstate.Columns.Add("КоличествоСовладельцев", typeof(int));

            XmlNodeList собственности = spravka.SelectNodes(".//Собственность");
            foreach (XmlNode собственность in собственности)
            {
                var row = realEstate.NewRow();

                // Основные данные
                row["ВидИмущества"] = собственность.SelectSingleNode(".//Атрибут[@Имя='ВидНедвижимогоИмущества']")?.InnerText;
                row["ТипАктива"] = собственность.SelectSingleNode(".//Атрибут[@Имя='ВидАктива']")?.InnerText;
                row["Наименование"] = собственность.SelectSingleNode(".//Атрибут[@Имя='НаименованиеНедвижимогоИмущества']")?.InnerText;

                double площадь;
                if (double.TryParse(собственность.SelectSingleNode(".//Атрибут[@Имя='Площадь']")?.InnerText,
                                  NumberStyles.Any, CultureInfo.InvariantCulture, out площадь))
                    row["Площадь"] = площадь;
                row["ПлощадьДляПечати"] = собственность.SelectSingleNode(".//Атрибут[@Имя='ПлощадьДляПечати']")?.InnerText;

                row["ВидСобственности"] = собственность.SelectSingleNode("./Атрибут[@Имя='ВидСобственности']")?.InnerText;
                row["ВидСобственностиДляПечати"] = собственность.SelectSingleNode("./Атрибут[@Имя='ВидСобственностиДляПечати']")?.InnerText;
                row["ОснованиеПриобретения"] = собственность.SelectSingleNode("./Атрибут[@Имя='ОснованиеПриобретенияИИсточникСредств']")?.InnerText;
                row["ПолноеОписание"] = собственность.SelectSingleNode(".//Атрибут[@Имя='НаименованиеИмущества']")?.InnerText;

                // Адресные данные
                var адрес = собственность.SelectSingleNode(".//Адрес");
                if (адрес != null)
                {
                    row["АдресДляПечати"] = адрес.SelectSingleNode("./Атрибут[@Имя='ДляПечати']")?.InnerText;
                    row["Страна"] = адрес.SelectSingleNode("./Атрибут[@Имя='Страна']")?.InnerText;
                    row["Индекс"] = адрес.SelectSingleNode("./Атрибут[@Имя='Индекс']")?.InnerText;
                    row["Регион"] = адрес.SelectSingleNode("./Атрибут[@Имя='Регион']")?.InnerText;
                    row["Город"] = адрес.SelectSingleNode("./Атрибут[@Имя='Город']")?.InnerText;
                    row["Улица"] = адрес.SelectSingleNode("./Атрибут[@Имя='Улица']")?.InnerText;
                    row["Дом"] = адрес.SelectSingleNode("./Атрибут[@Имя='Дом']")?.InnerText;
                    row["Корпус"] = адрес.SelectSingleNode("./Атрибут[@Имя='Корпус']")?.InnerText;
                    row["Квартира"] = адрес.SelectSingleNode("./Атрибут[@Имя='Квартира']")?.InnerText;
                    row["ДополнительнаяИнформация"] = адрес.SelectSingleNode("./Атрибут[@Имя='ДополнительнаяИнформация']")?.InnerText;
                }

                // Данные о совладельцах
                var совладельцы = собственность.SelectNodes("./Атрибут[@Имя='ИныеЛицы']/ФизическоеЛицо");
                if (совладельцы != null && совладельцы.Count > 0)
                {
                    var имена = new List<string>();
                    var датыРождения = new List<string>();

                    foreach (XmlNode совладелец in совладельцы)
                    {
                        var имя = совладелец.SelectSingleNode("./Атрибут[@Имя='Имя']")?.InnerText;
                        var дата = совладелец.SelectSingleNode("./Атрибут[@Имя='ДатаРождения']")?.InnerText;

                        if (!string.IsNullOrEmpty(имя)) имена.Add(имя);
                        if (!string.IsNullOrEmpty(дата)) датыРождения.Add(дата);
                    }

                    row["Совладельцы"] = string.Join("; ", имена);
                    row["ДатыРожденияСовладельцев"] = string.Join("; ", датыРождения);
                    row["КоличествоСовладельцев"] = совладельцы.Count;
                }
                else
                {
                    row["КоличествоСовладельцев"] = 0;
                }

                realEstate.Rows.Add(row);
            }

            return realEstate;
        }

        private static DataTable UsageRealEstate(XmlNode spravka)
        {
            DataTable usage = new DataTable("Недвижимое имущество в пользовании");

            usage.Columns.Add("Вид пользования", typeof(string));
            usage.Columns.Add("Период (начало)", typeof(string));
            usage.Columns.Add("Период (конец)", typeof(string));
            usage.Columns.Add("Сроки пользования", typeof(string));
            usage.Columns.Add("Основание", typeof(string));
            usage.Columns.Add("Тип имущества", typeof(string));
            usage.Columns.Add("Наименование", typeof(string));
            usage.Columns.Add("Площадь (кв.м)", typeof(string));
            usage.Columns.Add("Адрес", typeof(string));
            usage.Columns.Add("Предоставившее лицо", typeof(string));
            usage.Columns.Add("Дата рождения предоставившего", typeof(string));

            var nodes = spravka.SelectNodes(".//Атрибут[@Имя='НедвижимоеИмуществоВПользовании']/Пользование");
            if (nodes == null) return usage;

            foreach (XmlNode node in nodes)
            {
                var row = usage.NewRow();

                row["Вид пользования"] = GetNodeValue(node, "./Атрибут[@Имя='ВидПользования']");
                row["Период (начало)"] = GetNodeValue(node, "./Атрибут[@Имя='Начало']");
                row["Период (конец)"] = GetNodeValue(node, "./Атрибут[@Имя='Конец']");
                row["Сроки пользования"] = GetNodeValue(node, "./Атрибут[@Имя='ВидИСрокиПользованияДляПечати']");
                row["Основание"] = GetNodeValue(node, "./Атрибут[@Имя='ОснованиеПользования']");

                row["Тип имущества"] = GetNodeValue(node, "./Атрибут[@Имя='ВидНедвижимогоИмущества']");
                row["Наименование"] = GetNodeValue(node, "./Атрибут[@Имя='НаименованиеНедвижимогоИмуществаСПояснением']");
                row["Площадь (кв.м)"] = GetNodeValue(node, "./Атрибут[@Имя='ПлощадьДляПечати']");

                var addressNode = node.SelectSingleNode("./Атрибут[@Имя='МестонахождениеИмущества']/Адрес");
                row["Адрес"] = GetNodeValue(addressNode, "./Атрибут[@Имя='ДляПечати']");

                row["Предоставившее лицо"] = GetNodeValue(node, "./Атрибут[@Имя='ФИО']");
                row["Дата рождения предоставившего"] = GetNodeValue(node, "./Атрибут[@Имя='ДатаРождения']");

                usage.Rows.Add(row);
            }

            return usage;
        }
        private static DataTable Vehicles(XmlNode spravka)
        {
            DataTable vehicles = new DataTable("Транспортные средства");

            vehicles.Columns.Add("Тип ТС", typeof(string));
            vehicles.Columns.Add("Марка", typeof(string));
            vehicles.Columns.Add("Модель", typeof(string));
            vehicles.Columns.Add("Год выпуска", typeof(int));
            vehicles.Columns.Add("Место регистрации", typeof(string));
            vehicles.Columns.Add("Форма собственности", typeof(string));
            vehicles.Columns.Add("Описание", typeof(string));
            vehicles.Columns.Add("Полное наименование", typeof(string));

            var nodes = spravka.SelectNodes(".//Атрибут[@Имя='ТранспортныеСредства']/Собственность");
            if (nodes == null) return vehicles;

            foreach (XmlNode node in nodes)
            {
                var row = vehicles.NewRow();

                row["Тип ТС"] = GetNodeValue(node, ".//Атрибут[@Имя='ВидТранспортногоСредства']");
                row["Марка"] = GetNodeValue(node, ".//Атрибут[@Имя='МаркаТранспортногоСредства']");
                row["Модель"] = GetNodeValue(node, ".//Атрибут[@Имя='МодельТранспортногоСредства']");

                if (int.TryParse(GetNodeValue(node, ".//Атрибут[@Имя='ГодВыпуска']"), out int year))
                    row["Год выпуска"] = year;

                row["Место регистрации"] = GetNodeValue(node, ".//Атрибут[@Имя='МестоРегистрации']");
                row["Форма собственности"] = GetNodeValue(node, ".//Атрибут[@Имя='ВидСобственности']");
                row["Описание"] = GetNodeValue(node, ".//Атрибут[@Имя='Наименование']");
                row["Полное наименование"] = GetNodeValue(node, ".//Атрибут[@Имя='НаименованиеИМаркаТранспортногоСредстваДляПечати']");

                vehicles.Rows.Add(row);
            }

            return vehicles;
        }
        private static DataTable DigitalFinancialAssets(XmlNode spravka)
        {
            DataTable table = new DataTable("Цифровые финансовые активы");

            table.Columns.Add("Наименование актива", typeof(string));
            table.Columns.Add("Дата приобретения", typeof(DateTime));
            table.Columns.Add("Количество", typeof(decimal));
            table.Columns.Add("Оператор", typeof(string));
            table.Columns.Add("Страна регистрации", typeof(string));
            table.Columns.Add("Рег. номер", typeof(string));

            var nodes = spravka.SelectNodes(".//Цифровые_финансовые_активы");
            if (nodes == null) return table;

            foreach (XmlNode node in nodes)
            {
                var row = table.NewRow();

                row["Наименование актива"] = GetNodeValue(node, "./Атрибут[@Имя='Наименование_цифрового_финансового_актива']");

                if (DateTime.TryParse(GetNodeValue(node, "./Атрибут[@Имя='Дата_приобретения']"), out DateTime date))
                    row["Дата приобретения"] = date;

                if (decimal.TryParse(GetNodeValue(node, "./Атрибут[@Имя='Общее_количество']"), out decimal quantity))
                    row["Количество"] = quantity;

                row["Оператор"] = GetNodeValue(node, "./Атрибут[@Имя='Наименование_оператора']");
                row["Страна регистрации"] = GetNodeValue(node, "./Атрибут[@Имя='Страна_регистрации']");
                row["Рег. номер"] = GetNodeValue(node, "./Атрибут[@Имя='Регистрационный_номер']");

                table.Rows.Add(row);
            }

            return table;
        }
        private static DataTable DigitalCurrencies(XmlNode spravka)
        {
            DataTable table = new DataTable("Цифровые валюты");

            table.Columns.Add("Наименование", typeof(string));
            table.Columns.Add("Дата приобретения", typeof(DateTime));
            table.Columns.Add("Количество", typeof(decimal));

            var nodes = spravka.SelectNodes(".//Цифровая_валюта");
            if (nodes == null) return table;

            foreach (XmlNode node in nodes)
            {
                var row = table.NewRow();

                row["Наименование"] = GetNodeValue(node, "./Атрибут[@Имя='Наименование_цифровой_валюты']");

                if (DateTime.TryParse(GetNodeValue(node, "./Атрибут[@Имя='Дата_приобретения']"), out DateTime date))
                    row["Дата приобретения"] = date;

                if (decimal.TryParse(GetNodeValue(node, "./Атрибут[@Имя='Общее_количество']"), out decimal quantity))
                    row["Количество"] = quantity;

                table.Rows.Add(row);
            }

            return table;
        }
        private static DataTable Securities(XmlNode spravka)
        {
            DataTable table = new DataTable("Ценные бумаги");

            table.Columns.Add("Наименование организации", typeof(string));
            table.Columns.Add("Организационная форма", typeof(string));
            table.Columns.Add("Адрес", typeof(string));
            table.Columns.Add("Тип акции", typeof(string));
            table.Columns.Add("Количество", typeof(int));
            table.Columns.Add("Номинальная стоимость", typeof(decimal));
            table.Columns.Add("Общая стоимость", typeof(decimal));
            table.Columns.Add("Основание владения", typeof(string));
            table.Columns.Add("Доля участия", typeof(string));

            var nodes = spravka.SelectNodes(".//УчастиеВКоммерческихОрганизацияхИФондах");
            if (nodes == null) return table;

            foreach (XmlNode node in nodes)
            {
                var row = table.NewRow();

                row["Наименование организации"] = GetNodeValue(node, ".//Атрибут[@Имя='Имя']");
                row["Организационная форма"] = GetNodeValue(node, ".//Атрибут[@Имя='ОрганизационноПравоваяФорма']");
                row["Адрес"] = GetNodeValue(node, ".//Атрибут[@Имя='ЮридическийАдрес']/Адрес/Атрибут[@Имя='ДляПечати']");
                row["Тип акции"] = GetNodeValue(node, "./Атрибут[@Имя='ВидАкции']");

                if (int.TryParse(GetNodeValue(node, "./Атрибут[@Имя='КоличествоАкций']"), out int quantity))
                    row["Количество"] = quantity;

                if (decimal.TryParse(GetNodeValue(node, "./Атрибут[@Имя='НоминальнаяСтоимостьАкции']"), out decimal nominal))
                    row["Номинальная стоимость"] = nominal;

                if (decimal.TryParse(GetNodeValue(node, "./Атрибут[@Имя='ОбщаяСтоимость']"), out decimal total))
                    row["Общая стоимость"] = total;

                row["Основание владения"] = GetNodeValue(node, "./Атрибут[@Имя='ОснованиеУчастия']");
                row["Доля участия"] = GetNodeValue(node, "./Атрибут[@Имя='ДоляУчастияДляПечати']");

                table.Rows.Add(row);
            }

            return table;
        }
        private static DataTable GetLiabilities(XmlNode spravka)
        {
            DataTable table = new DataTable("Обязательства");

            table.Columns.Add("Тип обязательства", typeof(string));
            table.Columns.Add("Контрагент", typeof(string));
            table.Columns.Add("ИНН", typeof(string));
            table.Columns.Add("ОГРН", typeof(string));
            table.Columns.Add("Основание", typeof(string));
            table.Columns.Add("Сумма", typeof(decimal));
            table.Columns.Add("Остаток", typeof(decimal));
            table.Columns.Add("Условия", typeof(string));
            table.Columns.Add("Статус", typeof(string));

            var nodes = spravka.SelectNodes(".//СрочноеОбязательство");
            if (nodes == null) return table;

            foreach (XmlNode node in nodes)
            {
                var row = table.NewRow();

                row["Тип обязательства"] = GetNodeValue(node, "./Атрибут[@Имя='ВидОбязательства']");
                row["Контрагент"] = GetNodeValue(node, "./Атрибут[@Имя='ЮрЛицо']");
                row["ИНН"] = GetNodeValue(node, "./Атрибут[@Имя='ИНН']");
                row["ОГРН"] = GetNodeValue(node, "./Атрибут[@Имя='ОГРН']");
                row["Основание"] = GetNodeValue(node, "./Атрибут[@Имя='ОснованиеВозникновения']");

                if (decimal.TryParse(GetNodeValue(node, "./Атрибут[@Имя='СуммаОбязательства']"), out decimal amount))
                    row["Сумма"] = amount;

                if (decimal.TryParse(GetNodeValue(node, "./Атрибут[@Имя='РазмерОбязательства']"), out decimal balance))
                    row["Остаток"] = balance;

                row["Условия"] = GetNodeValue(node, "./Атрибут[@Имя='УсловиеОбязательства']");
                row["Статус"] = GetNodeValue(node, "./Атрибут[@Имя='КредиторИлиДолжник']");

                table.Rows.Add(row);
            }

            return table;
        }
        private static DataTable GetGifts(XmlNode spravka)
        {
            DataTable table = new DataTable("Подарки");

            table.Columns.Add("Даритель", typeof(string));
            table.Columns.Add("Дата рождения", typeof(DateTime));
            table.Columns.Add("Тип имущества", typeof(string));
            table.Columns.Add("Наименование", typeof(string));
            table.Columns.Add("Площадь", typeof(decimal));
            table.Columns.Add("Адрес", typeof(string));
            table.Columns.Add("Основание", typeof(string));
            table.Columns.Add("Тип документа", typeof(string));
            table.Columns.Add("Документ", typeof(string));
            table.Columns.Add("Дата выдачи", typeof(DateTime));
            table.Columns.Add("Кем выдан", typeof(string));

            var nodes = spravka.SelectNodes(".//Подарок");
            if (nodes == null) return table;

            foreach (XmlNode node in nodes)
            {
                var row = table.NewRow();

                row["Даритель"] = GetNodeValue(node, "./Атрибут[@Имя='ФИО']");

                if (DateTime.TryParse(GetNodeValue(node, "./Атрибут[@Имя='ДатаРождения']"), out DateTime birthDate))
                    row["Дата рождения"] = birthDate;

                row["Тип имущества"] = GetNodeValue(node, "./Атрибут[@Имя='ВидИмущества']");
                row["Наименование"] = GetNodeValue(node, "./Атрибут[@Имя='НаименованиеНедвижимогоИмущества']");

                if (decimal.TryParse(GetNodeValue(node, "./Атрибут[@Имя='Площадь']"), out decimal area))
                    row["Площадь"] = area;

                row["Адрес"] = GetNodeValue(node, "./Атрибут[@Имя='МестонахождениеИмущества']/Адрес/Атрибут[@Имя='ДляПечати']");
                row["Основание"] = GetNodeValue(node, "./Атрибут[@Имя='ОснованиеОтчуждения']");
                row["Тип документа"] = GetNodeValue(node, "./Атрибут[@Имя='ТипДокумента']");

                string series = GetNodeValue(node, "./Атрибут[@Имя='Серия']");
                string number = GetNodeValue(node, "./Атрибут[@Имя='Номер']");
                row["Документ"] = $"{series} {number}".Trim();

                if (DateTime.TryParse(GetNodeValue(node, "./Атрибут[@Имя='ДатаВыдачи']"), out DateTime issueDate))
                    row["Дата выдачи"] = issueDate;

                row["Кем выдан"] = GetNodeValue(node, "./Атрибут[@Имя='КемВыдан']");

                table.Rows.Add(row);
            }

            return table;
        }
        private static string GetNodeValue(XmlNode parentNode, string xpath)
        {
            var node = parentNode.SelectSingleNode(xpath);
            return node?.InnerText ?? string.Empty;
        }


    }



}
