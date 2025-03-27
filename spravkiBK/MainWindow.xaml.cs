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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string xmlContent = File.ReadAllText("312.xml");           
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlContent);

            XmlNodeList spravki = doc.SelectNodes("//Справка");

            int index = 1;

            foreach (XmlNode spravka in spravki)
            {

                DataSet ds = new DataSet($"Person{index}");


                // Личные данные                
                ds.Tables.Add(Personal(spravka));
                // Доходы               
                ds.Tables.Add(Incomes(spravka));
                // Счета                
                ds.Tables.Add(Accounts(spravka));
                // Расходы
                ds.Tables.Add(Expenses(spravka));
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

                ds.Tables.Add(Securities(spravka));
                ds.Tables.Add(GetLiabilities(spravka));
                ds.Tables.Add(GetGifts(spravka));


                // Здесь можно сохранить или обработать DataSet
                Console.WriteLine($"Создан DataSet для Person{index} с {ds.Tables.Count} таблицами.");
                index++;
            }
            Console.WriteLine();
        }

        private DataTable Personal(XmlNode spravka)
        {
            DataTable personal = new DataTable("ЛичныеДанные");
            personal.Columns.Add("Тип"); // кто это: Заявитель / Супруг / Ребёнок
            personal.Columns.Add("Фамилия");
            personal.Columns.Add("Имя");
            personal.Columns.Add("Отчество");
            personal.Columns.Add("ДатаРождения");
            personal.Columns.Add("СНИЛС");
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

                // СНИЛС (если есть)
                row["СНИЛС"] = node.SelectSingleNode("./Атрибут[@Имя='СНИЛС']")?.InnerText;

                // Адрес
                var адрес = node.SelectSingleNode("./Атрибут[@Имя='Регистрация']/Адрес/Атрибут[@Имя='ДляПечати']");
                row["Регистрация"] = адрес?.InnerText;

                // Место работы (если есть)
                var местоРаботы = node.SelectSingleNode("./Атрибут[@Имя='МестоРаботы']/МестоРаботы");
                row["Должность"] = местоРаботы?.SelectSingleNode("./Атрибут[@Имя='Должность']")?.InnerText;
                row["Организация"] = местоРаботы?.SelectSingleNode("./Атрибут[@Имя='НазваниеОрганизации']")?.InnerText;

                personal.Rows.Add(row);
            }
            return personal;

        }
        private DataTable Incomes(XmlNode spravka)
        {
            DataTable incomes = new DataTable("Доходы");
            incomes.Columns.Add("ВидДохода");
            incomes.Columns.Add("ВеличинаДохода");

            XmlNodeList доходы = spravka.SelectNodes(".//Доход");
            foreach (XmlNode доход in доходы)
            {
                var row = incomes.NewRow();
                row["ВидДохода"] = доход.SelectSingleNode("./Атрибут[@Имя='ВидДохода']")?.InnerText;
                row["ВеличинаДохода"] = доход.SelectSingleNode("./Атрибут[@Имя='ВеличинаДохода']")?.InnerText;
                incomes.Rows.Add(row);
            }
            return incomes;
        }
        private DataTable Accounts(XmlNode spravka)
        {
            DataTable accounts = new DataTable("Счета");
            accounts.Columns.Add("ВидСчета");
            accounts.Columns.Add("ВалютаСчета");
            accounts.Columns.Add("ДатаОткрытия");
            accounts.Columns.Add("КредитнаяОрганизация");
            accounts.Columns.Add("ОстатокНаСчете");
            accounts.Columns.Add("СуммаНеПревышаетОбщийДоход");
            accounts.Columns.Add("ДатаВыписки");
            accounts.Columns.Add("Идентификатор");

            var счета = spravka.SelectNodes(".//Атрибут[@Имя='Счета']/Счет");
            foreach (XmlNode счет in счета)
            {
                var row = accounts.NewRow();
                row["ВидСчета"] = счет.SelectSingleNode("./Атрибут[@Имя='ВидСчета']")?.InnerText;
                row["ВалютаСчета"] = счет.SelectSingleNode("./Атрибут[@Имя='ВалютаСчета']")?.InnerText;
                row["ДатаОткрытия"] = счет.SelectSingleNode("./Атрибут[@Имя='ДатаОткрытияСчета']")?.InnerText;
                row["КредитнаяОрганизация"] = счет.SelectSingleNode("./Атрибут[@Имя='КредитнаяОрганизация']")?.InnerText;
                row["ОстатокНаСчете"] = счет.SelectSingleNode("./Атрибут[@Имя='ОстатокНаСчете']")?.InnerText;
                row["СуммаНеПревышаетОбщийДоход"] = счет.SelectSingleNode("./Атрибут[@Имя='СуммаНеПревышаетОбщийДоход']")?.InnerText;
                row["ДатаВыписки"] = счет.SelectSingleNode("./Атрибут[@Имя='ДатаВыписки']")?.InnerText;
                row["Идентификатор"] = счет.SelectSingleNode("./Атрибут[@Имя='Идентификатор']")?.InnerText;

                accounts.Rows.Add(row);
            }
            return accounts;
        }
        private DataTable Expenses(XmlNode spravka)
        {
            DataTable expenses = new DataTable("Расходы");
            expenses.Columns.Add("Наименование");
            expenses.Columns.Add("СуммаСделки");
            expenses.Columns.Add("Основание");
            expenses.Columns.Add("ИсточникСредств");
            expenses.Columns.Add("Площадь");
            expenses.Columns.Add("Адрес");
            expenses.Columns.Add("ТипИмущества");

            // Получаем список расходов
            var расходы = spravka.SelectNodes(".//Атрибут[@Имя='Расходы']/Расход");

            foreach (XmlNode расход in расходы)
            {
                var row = expenses.NewRow();

                row["Наименование"] = расход.SelectSingleNode("./Атрибут[@Имя='Наименование']")?.InnerText;
                row["СуммаСделки"] = расход.SelectSingleNode("./Атрибут[@Имя='СуммаСделки']")?.InnerText;
                row["Основание"] = расход.SelectSingleNode("./Атрибут[@Имя='ОснованиеПриобретения']")?.InnerText;
                row["ИсточникСредств"] = расход.SelectSingleNode("./Атрибут[@Имя='ИсточникСредств']")?.InnerText;
                row["Площадь"] = расход.SelectSingleNode("./Атрибут[@Имя='Площадь']")?.InnerText;
                row["ТипИмущества"] = расход.SelectSingleNode("./Атрибут[@Имя='ВидНедвижимогоИмущества']")?.InnerText;

                var адрес = расход.SelectSingleNode("./Атрибут[@Имя='МестонахождениеИмущества']/Адрес/Атрибут[@Имя='ДляПечати']");
                row["Адрес"] = адрес?.InnerText;

                expenses.Rows.Add(row);
            }
            return expenses;
        }
        private DataTable RealEstate(XmlNode spravka)
        {
            DataTable realEstate = new DataTable("Недвижимое Имущество");

            realEstate.Columns.Add("PropertyType");             // ВидНедвижимогоИмущества
            realEstate.Columns.Add("PropertyName");             // НаименованиеНедвижимогоИмущества
            realEstate.Columns.Add("FullDescription");          // НаименованиеИмущества
            realEstate.Columns.Add("Area");                     // Площадь
            realEstate.Columns.Add("OwnershipType");            // ВидСобственности
            realEstate.Columns.Add("AcquisitionBasis");         // ОснованиеПриобретенияИИсточникСредств
            realEstate.Columns.Add("Address");                  // Адрес для печати
            realEstate.Columns.Add("CoOwners");                 // ИныеЛица (имена)
            realEstate.Columns.Add("CoOwnersBirthDates");       // ИныеЛица (дата рождения)

            var properties = spravka.SelectNodes(".//Атрибут[@Имя='НедвижимоеИмущество']/Собственность");

            foreach (XmlNode prop in properties)
            {
                var row = realEstate.NewRow();

                // Тип и наименование
                row["PropertyType"] = prop.SelectSingleNode(".//Атрибут[@Имя='ВидНедвижимогоИмущества']")?.InnerText;
                row["PropertyName"] = prop.SelectSingleNode(".//Атрибут[@Имя='НаименованиеНедвижимогоИмущества']")?.InnerText;
                row["FullDescription"] = prop.SelectSingleNode(".//Атрибут[@Имя='НаименованиеИмущества']")?.InnerText;
                row["Area"] = prop.SelectSingleNode(".//Атрибут[@Имя='Площадь']")?.InnerText;
                row["OwnershipType"] = prop.SelectSingleNode(".//Атрибут[@Имя='ВидСобственности']")?.InnerText;
                row["AcquisitionBasis"] = prop.SelectSingleNode(".//Атрибут[@Имя='ОснованиеПриобретенияИИсточникСредств']")?.InnerText;

                // Адрес
                row["Address"] = prop.SelectSingleNode(".//Атрибут[@Имя='МестонахождениеИмущества']/Адрес/Атрибут[@Имя='ДляПечати']")?.InnerText;

                // Совладельцы
                var coOwners = prop.SelectNodes(".//Атрибут[@Имя='ИныеЛица']/ФизическоеЛицо");
                if (coOwners != null && coOwners.Count > 0)
                {
                    var names = new List<string>();
                    var birthDates = new List<string>();

                    foreach (XmlNode person in coOwners)
                    {
                        var name = person.SelectSingleNode("./Атрибут[@Имя='Имя']")?.InnerText;
                        var birth = person.SelectSingleNode("./Атрибут[@Имя='ДатаРождения']")?.InnerText;

                        if (!string.IsNullOrEmpty(name)) names.Add(name);
                        if (!string.IsNullOrEmpty(birth)) birthDates.Add(birth);
                    }

                    row["CoOwners"] = string.Join("; ", names);
                    row["CoOwnersBirthDates"] = string.Join("; ", birthDates);
                }

                realEstate.Rows.Add(row);
            }

            return realEstate;
        }
        private DataTable UsageRealEstate(XmlNode spravka)
        {
            DataTable usage = new DataTable("Недвижимое имущество в собственности");

            usage.Columns.Add("UsageType");         // ВидПользования (например, Аренда)
            usage.Columns.Add("StartYear");         // Начало
            usage.Columns.Add("EndYear");           // Конец
            usage.Columns.Add("FullUsageText");     // ВидИСрокиПользованияДляПечати
            usage.Columns.Add("Basis");             // ОснованиеПользования
            usage.Columns.Add("PropertyType");      // ВидНедвижимогоИмущества
            usage.Columns.Add("PropertyName");      // НаименованиеИмущества
            usage.Columns.Add("Area");              // Площадь
            usage.Columns.Add("Address");           // Адрес (ДляПечати)
            usage.Columns.Add("OwnerFullName");     // ФИО предоставившего
            usage.Columns.Add("OwnerBirthDate");    // ДатаРождения

            var nodes = spravka.SelectNodes(".//Атрибут[@Имя='НедвижимоеИмуществоВПользовании']/Пользование");

            foreach (XmlNode node in nodes)
            {
                var row = usage.NewRow();

                row["UsageType"] = node.SelectSingleNode("./Атрибут[@Имя='ВидПользования']")?.InnerText;
                row["StartYear"] = node.SelectSingleNode("./Атрибут[@Имя='Начало']")?.InnerText;
                row["EndYear"] = node.SelectSingleNode("./Атрибут[@Имя='Конец']")?.InnerText;
                row["FullUsageText"] = node.SelectSingleNode("./Атрибут[@Имя='ВидИСрокиПользованияДляПечати']")?.InnerText;
                row["Basis"] = node.SelectSingleNode("./Атрибут[@Имя='ОснованиеПользования']")?.InnerText;

                row["PropertyType"] = node.SelectSingleNode("./Атрибут[@Имя='ВидНедвижимогоИмущества']")?.InnerText;
                row["PropertyName"] = node.SelectSingleNode("./Атрибут[@Имя='Наименование']")?.InnerText;
                row["Area"] = node.SelectSingleNode("./Атрибут[@Имя='Площадь']")?.InnerText;
                row["Address"] = node.SelectSingleNode("./Атрибут[@Имя='МестонахождениеИмущества']/Адрес/Атрибут[@Имя='ДляПечати']")?.InnerText;

                row["OwnerFullName"] = node.SelectSingleNode("./Атрибут[@Имя='ФИО']")?.InnerText;
                row["OwnerBirthDate"] = node.SelectSingleNode("./Атрибут[@Имя='ДатаРождения']")?.InnerText;

                usage.Rows.Add(row);
            }

            return usage;
        }
        private DataTable Vehicles(XmlNode spravka)
        {
            DataTable vehicles = new DataTable("Транспортные средства");

            vehicles.Columns.Add("VehicleType");         // ВидТранспортногоСредства
            vehicles.Columns.Add("Brand");               // МаркаТранспортногоСредства
            vehicles.Columns.Add("Model");               // МодельТранспортногоСредства
            vehicles.Columns.Add("Year");                // ГодВыпуска
            vehicles.Columns.Add("RegistrationPlace");   // МестоРегистрации
            vehicles.Columns.Add("OwnershipType");       // ВидСобственности
            vehicles.Columns.Add("Description");         // Наименование
            vehicles.Columns.Add("DisplayName");         // НаименованиеИМаркаТранспортногоСредстваДляПечати

            var transportNodes = spravka.SelectNodes(".//Атрибут[@Имя='ТранспортныеСредства']/Собственность");

            foreach (XmlNode node in transportNodes)
            {
                var row = vehicles.NewRow();

                // Основной блок
                row["VehicleType"] = node.SelectSingleNode(".//Атрибут[@Имя='ВидТранспортногоСредства']")?.InnerText;
                row["Brand"] = node.SelectSingleNode(".//Атрибут[@Имя='МаркаТранспортногоСредства']")?.InnerText;
                row["Model"] = node.SelectSingleNode(".//Атрибут[@Имя='МодельТранспортногоСредства']")?.InnerText;
                row["Year"] = node.SelectSingleNode(".//Атрибут[@Имя='ГодВыпуска']")?.InnerText;
                row["RegistrationPlace"] = node.SelectSingleNode(".//Атрибут[@Имя='МестоРегистрации']")?.InnerText;
                row["OwnershipType"] = node.SelectSingleNode(".//Атрибут[@Имя='ВидСобственности']")?.InnerText;
                row["Description"] = node.SelectSingleNode(".//Атрибут[@Имя='Наименование']")?.InnerText;
                row["DisplayName"] = node.SelectSingleNode(".//Атрибут[@Имя='НаименованиеИМаркаТранспортногоСредстваДляПечати']")?.InnerText;

                vehicles.Rows.Add(row);
            }

            return vehicles;
        }
        private DataTable DigitalFinancialAssets(XmlNode spravka)
        {
            DataTable table = new DataTable("Цифровые финансы");
            table.Columns.Add("AssetName");
            table.Columns.Add("AcquisitionDate");
            table.Columns.Add("Quantity");
            table.Columns.Add("Operator");
            table.Columns.Add("Country");
            table.Columns.Add("RegistrationNumber");

            var nodes = spravka.SelectNodes(".//Цифровые_финансовые_активы");
            foreach (XmlNode node in nodes)
            {
                var row = table.NewRow();
                row["AssetName"] = node.SelectSingleNode("./Атрибут[@Имя='Наименование_цифрового_финансового_актива']")?.InnerText;
                row["AcquisitionDate"] = node.SelectSingleNode("./Атрибут[@Имя='Дата_приобретения']")?.InnerText;
                row["Quantity"] = node.SelectSingleNode("./Атрибут[@Имя='Общее_количество']")?.InnerText;
                row["Operator"] = node.SelectSingleNode("./Атрибут[@Имя='Наименование_оператора']")?.InnerText;
                row["Country"] = node.SelectSingleNode("./Атрибут[@Имя='Страна_регистрации']")?.InnerText;
                row["RegistrationNumber"] = node.SelectSingleNode("./Атрибут[@Имя='Регистрационный_номер']")?.InnerText;
                table.Rows.Add(row);
            }
            return table;
        }
        private DataTable DigitalCurrencies(XmlNode spravka)
        {
            DataTable table = new DataTable("Цифровая валюта");
            table.Columns.Add("CurrencyName");
            table.Columns.Add("AcquisitionDate");
            table.Columns.Add("Quantity");

            var nodes = spravka.SelectNodes(".//Цифровая_валюта");
            foreach (XmlNode node in nodes)
            {
                var row = table.NewRow();
                row["CurrencyName"] = node.SelectSingleNode("./Атрибут[@Имя='Наименование_цифровой_валюты']")?.InnerText;
                row["AcquisitionDate"] = node.SelectSingleNode("./Атрибут[@Имя='Дата_приобретения']")?.InnerText;
                row["Quantity"] = node.SelectSingleNode("./Атрибут[@Имя='Общее_количество']")?.InnerText;
                table.Rows.Add(row);
            }
            return table;
        }
        private DataTable Securities(XmlNode spravka)
        {
            DataTable table = new DataTable("Ценные Бумаги");
            table.Columns.Add("OrganizationName");
            table.Columns.Add("LegalForm");
            table.Columns.Add("LegalAddress");
            table.Columns.Add("ShareType");
            table.Columns.Add("SharesCount");
            table.Columns.Add("NominalPrice");
            table.Columns.Add("TotalValue");
            table.Columns.Add("OwnershipBasis");

            var nodes = spravka.SelectNodes(".//УчастиеВКоммерческихОрганизацияхИФондах");
            foreach (XmlNode node in nodes)
            {
                var row = table.NewRow();
                row["OrganizationName"] = node.SelectSingleNode(".//Атрибут[@Имя='Имя']")?.InnerText;
                row["LegalForm"] = node.SelectSingleNode(".//Атрибут[@Имя='ОрганизационноПравоваяФорма']")?.InnerText;
                row["LegalAddress"] = node.SelectSingleNode(".//Атрибут[@Имя='ЮридическийАдрес']/Адрес/Атрибут[@Имя='ДляПечати']")?.InnerText;
                row["ShareType"] = node.SelectSingleNode("./Атрибут[@Имя='ВидАкции']")?.InnerText;
                row["SharesCount"] = node.SelectSingleNode("./Атрибут[@Имя='КоличествоАкций']")?.InnerText;
                row["NominalPrice"] = node.SelectSingleNode("./Атрибут[@Имя='НоминальнаяСтоимостьАкции']")?.InnerText;
                row["TotalValue"] = node.SelectSingleNode("./Атрибут[@Имя='ОбщаяСтоимость']")?.InnerText;
                row["OwnershipBasis"] = node.SelectSingleNode("./Атрибут[@Имя='ОснованиеУчастия']")?.InnerText;
                table.Rows.Add(row);
            }
            return table;
        }
        public static DataTable GetLiabilities(XmlNode spravka)
        {
            DataTable table = new DataTable("Liabilities");
            table.Columns.Add("Type");
            table.Columns.Add("Creditor");
            table.Columns.Add("INN");
            table.Columns.Add("OGRN");
            table.Columns.Add("Contract");
            table.Columns.Add("Amount");
            table.Columns.Add("Outstanding");
            table.Columns.Add("Terms");

            var nodes = spravka.SelectNodes(".//СрочноеОбязательство");
            foreach (XmlNode node in nodes)
            {
                var row = table.NewRow();
                row["Type"] = node.SelectSingleNode("./Атрибут[@Имя='ВидОбязательства']")?.InnerText;
                row["Creditor"] = node.SelectSingleNode("./Атрибут[@Имя='ЮрЛицо']")?.InnerText;
                row["INN"] = node.SelectSingleNode("./Атрибут[@Имя='ИНН']")?.InnerText;
                row["OGRN"] = node.SelectSingleNode("./Атрибут[@Имя='ОГРН']")?.InnerText;
                row["Contract"] = node.SelectSingleNode("./Атрибут[@Имя='ОснованиеВозникновения']")?.InnerText;
                row["Amount"] = node.SelectSingleNode("./Атрибут[@Имя='СуммаОбязательства']")?.InnerText;
                row["Outstanding"] = node.SelectSingleNode("./Атрибут[@Имя='РазмерОбязательства']")?.InnerText;
                row["Terms"] = node.SelectSingleNode("./Атрибут[@Имя='УсловиеОбязательства']")?.InnerText;
                table.Rows.Add(row);
            }
            return table;
        }
        public static DataTable GetGifts(XmlNode spravka)
        {
            DataTable table = new DataTable("Gifts");
            table.Columns.Add("Donor");
            table.Columns.Add("BirthDate");
            table.Columns.Add("GiftType");
            table.Columns.Add("GiftName");
            table.Columns.Add("GiftArea");
            table.Columns.Add("GiftAddress");
            table.Columns.Add("Reason");
            table.Columns.Add("DocumentType");
            table.Columns.Add("DocumentNumber");
            table.Columns.Add("DocumentDate");
            table.Columns.Add("IssuedBy");

            var nodes = spravka.SelectNodes(".//Подарок");
            foreach (XmlNode node in nodes)
            {
                var row = table.NewRow();
                row["Donor"] = node.SelectSingleNode("./Атрибут[@Имя='ФИО']")?.InnerText;
                row["BirthDate"] = node.SelectSingleNode("./Атрибут[@Имя='ДатаРождения']")?.InnerText;
                row["GiftType"] = node.SelectSingleNode("./Атрибут[@Имя='ВидИмущества']")?.InnerText;
                row["GiftName"] = node.SelectSingleNode("./Атрибут[@Имя='НаименованиеНедвижимогоИмущества']")?.InnerText;
                row["GiftArea"] = node.SelectSingleNode("./Атрибут[@Имя='Площадь']")?.InnerText;
                row["GiftAddress"] = node.SelectSingleNode("./Атрибут[@Имя='МестонахождениеИмущества']/Адрес/Атрибут[@Имя='ДляПечати']")?.InnerText;
                row["Reason"] = node.SelectSingleNode("./Атрибут[@Имя='ОснованиеОтчуждения']")?.InnerText;
                row["DocumentType"] = node.SelectSingleNode("./Атрибут[@Имя='ТипДокумента']")?.InnerText;
                row["DocumentNumber"] = $"{node.SelectSingleNode("./Атрибут[@Имя='Серия']")?.InnerText} {node.SelectSingleNode("./Атрибут[@Имя='Номер']")?.InnerText}";
                row["DocumentDate"] = node.SelectSingleNode("./Атрибут[@Имя='ДатаВыдачи']")?.InnerText;
                row["IssuedBy"] = node.SelectSingleNode("./Атрибут[@Имя='КемВыдан']")?.InnerText;
                table.Rows.Add(row);
            }
            return table;
        }



    }



}
