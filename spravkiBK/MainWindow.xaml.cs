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
                //Ценные Бумаги
                ds.Tables.Add(Securities(spravka));
                //Срочные Обязательства
                ds.Tables.Add(GetLiabilities(spravka));
                //Подарки
                ds.Tables.Add(GetGifts(spravka));


                // Здесь можно сохранить или обработать DataSet
                Console.WriteLine($"Создан DataSet для Person{index} с {ds.Tables.Count} таблицами.");
                index++;
            }
            Console.WriteLine();
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
        private static DataTable Incomes(XmlNode spravka)
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
        private static DataTable Accounts(XmlNode spravka)
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
        private static DataTable Expenses(XmlNode spravka)
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
        private static DataTable RealEstate(XmlNode spravka)
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
        private static DataTable UsageRealEstate(XmlNode spravka)
        {
            DataTable usage = new DataTable("Недвижимое имущество в собственности");

            usage.Columns.Add("Вид Пользования");         // ВидПользования (например, Аренда)
            usage.Columns.Add("Начало");         // Начало
            usage.Columns.Add("Конец");           // Конец
            usage.Columns.Add("Вид И Сроки Пользования");     // ВидИСрокиПользованияДляПечати
            usage.Columns.Add("Основание Пользования");             // ОснованиеПользования
            usage.Columns.Add("Вид Недвижимого Имущества");      // ВидНедвижимогоИмущества
            usage.Columns.Add("Наименование Имущества");      // НаименованиеИмущества
            usage.Columns.Add("Площадь");              // Площадь
            usage.Columns.Add("Адрес");           // Адрес (ДляПечати)
            usage.Columns.Add("ФИО предоставившего");     // ФИО предоставившего
            usage.Columns.Add("Дата Рождения");    // ДатаРождения

            var nodes = spravka.SelectNodes(".//Атрибут[@Имя='НедвижимоеИмуществоВПользовании']/Пользование");

            foreach (XmlNode node in nodes)
            {
                var row = usage.NewRow();

                row["Вид Пользования"] = node.SelectSingleNode("./Атрибут[@Имя='ВидПользования']")?.InnerText;
                row["Начало"] = node.SelectSingleNode("./Атрибут[@Имя='Начало']")?.InnerText;
                row["Конец"] = node.SelectSingleNode("./Атрибут[@Имя='Конец']")?.InnerText;
                row["Вид И Сроки Пользования"] = node.SelectSingleNode("./Атрибут[@Имя='ВидИСрокиПользованияДляПечати']")?.InnerText;
                row["Основание Пользования"] = node.SelectSingleNode("./Атрибут[@Имя='ОснованиеПользования']")?.InnerText;

                row["Вид Недвижимого Имущества"] = node.SelectSingleNode("./Атрибут[@Имя='ВидНедвижимогоИмущества']")?.InnerText;
                row["Наименование Имущества"] = node.SelectSingleNode("./Атрибут[@Имя='Наименование']")?.InnerText;
                row["Площадь"] = node.SelectSingleNode("./Атрибут[@Имя='Площадь']")?.InnerText;
                row["Адрес"] = node.SelectSingleNode("./Атрибут[@Имя='МестонахождениеИмущества']/Адрес/Атрибут[@Имя='ДляПечати']")?.InnerText;

                row["ФИО предоставившего"] = node.SelectSingleNode("./Атрибут[@Имя='ФИО']")?.InnerText;
                row["Дата Рождения"] = node.SelectSingleNode("./Атрибут[@Имя='ДатаРождения']")?.InnerText;

                usage.Rows.Add(row);
            }

            return usage;
        }
        private static DataTable Vehicles(XmlNode spravka)
        {
            DataTable vehicles = new DataTable("Транспортные средства");

            vehicles.Columns.Add("Вид Транспортного Средства");         // ВидТранспортногоСредства
            vehicles.Columns.Add("Марка Транспортного Средства");               // МаркаТранспортногоСредства
            vehicles.Columns.Add("Модель Транспортного Средства");               // МодельТранспортногоСредства
            vehicles.Columns.Add("Год Выпуска");                // ГодВыпуска
            vehicles.Columns.Add("Место Регистрации");   // МестоРегистрации
            vehicles.Columns.Add("Вид Собственности");       // ВидСобственности
            vehicles.Columns.Add("Наименование");         // Наименование
            vehicles.Columns.Add("Наименование И Марка Транспортного Средства");         // НаименованиеИМаркаТранспортногоСредстваДляПечати

            var transportNodes = spravka.SelectNodes(".//Атрибут[@Имя='ТранспортныеСредства']/Собственность");

            foreach (XmlNode node in transportNodes)
            {
                var row = vehicles.NewRow();

                // Основной блок
                row["Вид Транспортного Средства"] = node.SelectSingleNode(".//Атрибут[@Имя='ВидТранспортногоСредства']")?.InnerText;
                row["Марка Транспортного Средства"] = node.SelectSingleNode(".//Атрибут[@Имя='МаркаТранспортногоСредства']")?.InnerText;
                row["Модель Транспортного Средства"] = node.SelectSingleNode(".//Атрибут[@Имя='МодельТранспортногоСредства']")?.InnerText;
                row["Год Выпуска"] = node.SelectSingleNode(".//Атрибут[@Имя='ГодВыпуска']")?.InnerText;
                row["Место Регистрации"] = node.SelectSingleNode(".//Атрибут[@Имя='МестоРегистрации']")?.InnerText;
                row["Вид Собственности"] = node.SelectSingleNode(".//Атрибут[@Имя='ВидСобственности']")?.InnerText;
                row["Наименование"] = node.SelectSingleNode(".//Атрибут[@Имя='Наименование']")?.InnerText;
                row["Наименование И Марка Транспортного Средства"] = node.SelectSingleNode(".//Атрибут[@Имя='НаименованиеИМаркаТранспортногоСредстваДляПечати']")?.InnerText;

                vehicles.Rows.Add(row);
            }

            return vehicles;
        }
        private static DataTable DigitalFinancialAssets(XmlNode spravka)
        {
            DataTable table = new DataTable("Цифровые финансы");
            table.Columns.Add("Наименование цифрового финансового актива");
            table.Columns.Add("Дата приобретения");
            table.Columns.Add("Общее количество");
            table.Columns.Add("Наименование оператора");
            table.Columns.Add("Страна регистрации");
            table.Columns.Add("Регистрационный номер");

            var nodes = spravka.SelectNodes(".//Цифровые_финансовые_активы");
            foreach (XmlNode node in nodes)
            {
                var row = table.NewRow();
                row["Наименование цифрового финансового актива"] = node.SelectSingleNode("./Атрибут[@Имя='Наименование_цифрового_финансового_актива']")?.InnerText;
                row["Дата приобретения"] = node.SelectSingleNode("./Атрибут[@Имя='Дата_приобретения']")?.InnerText;
                row["Общее количество"] = node.SelectSingleNode("./Атрибут[@Имя='Общее_количество']")?.InnerText;
                row["Наименование оператора"] = node.SelectSingleNode("./Атрибут[@Имя='Наименование_оператора']")?.InnerText;
                row["Страна регистрации"] = node.SelectSingleNode("./Атрибут[@Имя='Страна_регистрации']")?.InnerText;
                row["Регистрационный номер"] = node.SelectSingleNode("./Атрибут[@Имя='Регистрационный_номер']")?.InnerText;
                table.Rows.Add(row);
            }
            return table;
        }
        private static DataTable DigitalCurrencies(XmlNode spravka)
        {
            DataTable table = new DataTable("Цифровая валюта");
            table.Columns.Add("Наименование цифровой валюты");
            table.Columns.Add("Дата приобретения");
            table.Columns.Add("Общее количество");

            var nodes = spravka.SelectNodes(".//Цифровая_валюта");
            foreach (XmlNode node in nodes)
            {
                var row = table.NewRow();
                row["Наименование цифровой валюты"] = node.SelectSingleNode("./Атрибут[@Имя='Наименование_цифровой_валюты']")?.InnerText;
                row["Дата приобретения"] = node.SelectSingleNode("./Атрибут[@Имя='Дата_приобретения']")?.InnerText;
                row["Общее количество"] = node.SelectSingleNode("./Атрибут[@Имя='Общее_количество']")?.InnerText;
                table.Rows.Add(row);
            }
            return table;
        }
        private static DataTable Securities(XmlNode spravka)
        {
            DataTable table = new DataTable("Ценные Бумаги");
            table.Columns.Add("Имя");
            table.Columns.Add("Организационно Правовая Форма");
            table.Columns.Add("Юридический Адрес");
            table.Columns.Add("Вид Акции");
            table.Columns.Add("Количество Акций");
            table.Columns.Add("Номинальная Стоимость Акции");
            table.Columns.Add("Общая Стоимость");
            table.Columns.Add("Основание Участия");

            var nodes = spravka.SelectNodes(".//УчастиеВКоммерческихОрганизацияхИФондах");
            foreach (XmlNode node in nodes)
            {
                var row = table.NewRow();
                row["Имя"] = node.SelectSingleNode(".//Атрибут[@Имя='Имя']")?.InnerText;
                row["Организационно Правовая Форма"] = node.SelectSingleNode(".//Атрибут[@Имя='ОрганизационноПравоваяФорма']")?.InnerText;
                row["Юридический Адрес"] = node.SelectSingleNode(".//Атрибут[@Имя='ЮридическийАдрес']/Адрес/Атрибут[@Имя='ДляПечати']")?.InnerText;
                row["Вид Акции"] = node.SelectSingleNode("./Атрибут[@Имя='ВидАкции']")?.InnerText;
                row["Количество Акций"] = node.SelectSingleNode("./Атрибут[@Имя='КоличествоАкций']")?.InnerText;
                row["Номинальная Стоимость Акции"] = node.SelectSingleNode("./Атрибут[@Имя='НоминальнаяСтоимостьАкции']")?.InnerText;
                row["Общая Стоимость"] = node.SelectSingleNode("./Атрибут[@Имя='ОбщаяСтоимость']")?.InnerText;
                row["Основание Участия"] = node.SelectSingleNode("./Атрибут[@Имя='ОснованиеУчастия']")?.InnerText;
                table.Rows.Add(row);
            }
            return table;
        }
        private static DataTable GetLiabilities(XmlNode spravka)
        {
            DataTable table = new DataTable("Срочные Обязательства");
            table.Columns.Add("Вид Обязательства");
            table.Columns.Add("ЮрЛицо");
            table.Columns.Add("ИНН");
            table.Columns.Add("ОГРН");
            table.Columns.Add("Основание Возникновения");
            table.Columns.Add("Сумма Обязательства");
            table.Columns.Add("Размер Обязательства");
            table.Columns.Add("Условие Обязательства");

            var nodes = spravka.SelectNodes(".//СрочноеОбязательство");
            foreach (XmlNode node in nodes)
            {
                var row = table.NewRow();
                row["Вид Обязательства"] = node.SelectSingleNode("./Атрибут[@Имя='ВидОбязательства']")?.InnerText;
                row["ЮрЛицо"] = node.SelectSingleNode("./Атрибут[@Имя='ЮрЛицо']")?.InnerText;
                row["ИНН"] = node.SelectSingleNode("./Атрибут[@Имя='ИНН']")?.InnerText;
                row["ОГРН"] = node.SelectSingleNode("./Атрибут[@Имя='ОГРН']")?.InnerText;
                row["Основание Возникновения"] = node.SelectSingleNode("./Атрибут[@Имя='ОснованиеВозникновения']")?.InnerText;
                row["Сумма Обязательства"] = node.SelectSingleNode("./Атрибут[@Имя='СуммаОбязательства']")?.InnerText;
                row["Размер Обязательства"] = node.SelectSingleNode("./Атрибут[@Имя='РазмерОбязательства']")?.InnerText;
                row["Условие Обязательства"] = node.SelectSingleNode("./Атрибут[@Имя='УсловиеОбязательства']")?.InnerText;
                table.Rows.Add(row);
            }
            return table;
        }
        private static DataTable GetGifts(XmlNode spravka)
        {
            DataTable table = new DataTable("Подарки");
            table.Columns.Add("ФИО");
            table.Columns.Add("Дата Рождения");
            table.Columns.Add("Вид Имущества");
            table.Columns.Add("Наименование Недвижимого Имущества");
            table.Columns.Add("Площадь");
            table.Columns.Add("Местонахождение Имущества");
            table.Columns.Add("Основание Отчуждения");
            table.Columns.Add("Тип Документа");
            table.Columns.Add("Серия");
            table.Columns.Add("Дата Выдачи");
            table.Columns.Add("Кем Выдан");

            var nodes = spravka.SelectNodes(".//Подарок");
            foreach (XmlNode node in nodes)
            {
                var row = table.NewRow();
                row["ФИО"] = node.SelectSingleNode("./Атрибут[@Имя='ФИО']")?.InnerText;
                row["Дата Рождения"] = node.SelectSingleNode("./Атрибут[@Имя='ДатаРождения']")?.InnerText;
                row["Вид Имущества"] = node.SelectSingleNode("./Атрибут[@Имя='ВидИмущества']")?.InnerText;
                row["Наименование Недвижимого Имущества"] = node.SelectSingleNode("./Атрибут[@Имя='НаименованиеНедвижимогоИмущества']")?.InnerText;
                row["Площадь"] = node.SelectSingleNode("./Атрибут[@Имя='Площадь']")?.InnerText;
                row["Местонахождение Имущества"] = node.SelectSingleNode("./Атрибут[@Имя='МестонахождениеИмущества']/Адрес/Атрибут[@Имя='ДляПечати']")?.InnerText;
                row["Основание Отчуждения"] = node.SelectSingleNode("./Атрибут[@Имя='ОснованиеОтчуждения']")?.InnerText;
                row["Тип Документа"] = node.SelectSingleNode("./Атрибут[@Имя='ТипДокумента']")?.InnerText;
                row["Серия"] = $"{node.SelectSingleNode("./Атрибут[@Имя='Серия']")?.InnerText} {node.SelectSingleNode("./Атрибут[@Имя='Номер']")?.InnerText}";
                row["Дата Выдачи"] = node.SelectSingleNode("./Атрибут[@Имя='ДатаВыдачи']")?.InnerText;
                row["Кем Выдан"] = node.SelectSingleNode("./Атрибут[@Имя='КемВыдан']")?.InnerText;
                table.Rows.Add(row);
            }
            return table;
        }



    }



}
