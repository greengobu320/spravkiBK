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
            string xmlContent = File.ReadAllText("123.xml");           
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlContent);

            XmlNodeList spravki = doc.SelectNodes("//Справка");

            int index = 1;
            foreach (XmlNode spravka in spravki)
            {
                DataSet ds = new DataSet($"Person{index}");

                // Личные данные
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
                ds.Tables.Add(personal);

                // Доходы
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
                ds.Tables.Add(incomes);

                // Счета
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
                ds.Tables.Add(accounts);

                // Здесь можно сохранить или обработать DataSet
                Console.WriteLine($"Создан DataSet для Person{index} с {ds.Tables.Count} таблицами.");
                index++;
            }
            Console.WriteLine();
        }
    }

   

}
