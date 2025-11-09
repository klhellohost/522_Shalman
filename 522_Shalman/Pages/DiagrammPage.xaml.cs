using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;

namespace _522_Shalman.Pages
{
    /// <summary>
    /// Логика взаимодействия для DiagrammPage.xaml
    /// </summary>
    public partial class DiagrammPage : Page
    {
        private SeriesChartType currentType;
        public DiagrammPage()
        {
            InitializeComponent();
            ChartPayments.ChartAreas.Add(new ChartArea("Main"));
            var currentSeries = new Series("Платежи")
            {
                IsValueShownAsLabel = true,
            };
            ChartPayments.Series.Add(currentSeries);

            cmbUsers.ItemsSource = Entities.GetContext().User.ToList();
            cmbDiagramm.ItemsSource = Enum.GetValues(typeof(SeriesChartType));
        }

        private void UpdateData()
        {
            if (cmbUsers.SelectedItem is User currentUser)
            {
                Series currentSeries = ChartPayments.Series.FirstOrDefault();

                currentSeries.ChartType = currentType;
                currentSeries.Points.Clear();

                var categoriesList = Entities.GetContext().Category.ToList();
                foreach (var category in categoriesList)
                {
                    currentSeries.Points.AddXY(category.Name,
                    Entities.GetContext().Payment.ToList().Where(u => u.UserID ==
                    currentUser.ID && u.CategoryID == category.ID).Sum(u => u.Price * u.Num));
                }
            }
        }

        public string GetNewFileName(string filePath, string fileName, string ext)
        {
            string fullPath = System.IO.Path.Combine(filePath, $"{fileName}№{1}{ext}");
            int counter = 1;

            while (File.Exists(fullPath))
            {
                fullPath = System.IO.Path.Combine(filePath, $"{fileName}№{counter}{ext}");
                counter++;
            }

            return fullPath;
        }


        private void excelBtn_Click(object sender, RoutedEventArgs e)
        {
            Excel.Application application = null;
            Excel.Workbook workbook = null;
            try
            {
                var allUsers = Entities.GetContext().User.ToList();
                var allCategories = Entities.GetContext().Category.ToList();

                // Initialize Excel application
                application = new Excel.Application();
                workbook = application.Workbooks.Add();
                application.Visible = true; // Make Excel visible (optional)

                // Process each user
                for (int i = 0; i < allUsers.Count; i++)
                {
                    int startRowIndex = 1;
                    Excel.Worksheet worksheet = workbook.Worksheets.Add();
                    worksheet.Name = allUsers[i].FIO.Length > 31 ? allUsers[i].FIO.Substring(0, 31) : allUsers[i].FIO; // Excel sheet name limit

                    // Set column headers
                    worksheet.Cells[1, startRowIndex] = "Дата платежа";
                    worksheet.Cells[1, startRowIndex + 1] = "Название";
                    worksheet.Cells[1, startRowIndex + 2] = "Стоимость";
                    worksheet.Cells[1, startRowIndex + 3] = "Количество";
                    worksheet.Cells[1, startRowIndex + 4] = "Сумма";

                    Excel.Range columnHeaderRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 5]];
                    columnHeaderRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    columnHeaderRange.Font.Bold = true;
                    startRowIndex++;

                    // Group payments by category
                    var userCategories = allUsers[i].Payment
                        .OrderBy(u => u.Date)
                        .GroupBy(u => u.Category)
                        .OrderBy(u => u.Key.Name);

                    decimal userTotal = 0; // Track total for the user

                    foreach (var categoryGroup in userCategories)
                    {
                        // Add category header
                        Excel.Range headerRange = worksheet.Range[worksheet.Cells[startRowIndex, 1], worksheet.Cells[startRowIndex, 5]];
                        headerRange.Merge();
                        headerRange.Value = categoryGroup.Key.Name;
                        headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                        headerRange.Font.Italic = true;
                        startRowIndex++;

                        // Add payment details
                        foreach (var payment in categoryGroup)
                        {
                            worksheet.Cells[startRowIndex, 1] = payment.Date.ToString();
                            worksheet.Cells[startRowIndex, 2] = payment.Name;
                            worksheet.Cells[startRowIndex, 3] = payment.Price;
                            (worksheet.Cells[startRowIndex, 3] as Excel.Range).NumberFormat = "0.00";
                            worksheet.Cells[startRowIndex, 4] = payment.Num;
                            worksheet.Cells[startRowIndex, 5].Formula = $"=C{startRowIndex}*D{startRowIndex}";
                            (worksheet.Cells[startRowIndex, 5] as Excel.Range).NumberFormat = "0.00";
                            userTotal += (decimal)(payment.Price * payment.Num);
                            startRowIndex++;
                        }

                        // Add category total
                        Excel.Range sumRange = worksheet.Range[worksheet.Cells[startRowIndex, 1], worksheet.Cells[startRowIndex, 4]];
                        sumRange.Merge();
                        sumRange.Value = "ИТОГО:";
                        sumRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                        worksheet.Cells[startRowIndex, 5].Formula = $"=SUM(E{startRowIndex - categoryGroup.Count()}:E{startRowIndex - 1})";
                        sumRange.Font.Bold = true;
                        (worksheet.Cells[startRowIndex, 5] as Excel.Range).Font.Bold = true;
                        startRowIndex++;

                        // Add borders to the range
                        Excel.Range rangeBorders = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[startRowIndex - 1, 5]];
                        rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle =
                        rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle =
                        rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle =
                        rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle =
                        rangeBorders.Borders[Excel.XlBordersIndex.xlInsideHorizontal].LineStyle =
                        rangeBorders.Borders[Excel.XlBordersIndex.xlInsideVertical].LineStyle =
                        Excel.XlLineStyle.xlContinuous;
                    }

                    worksheet.Columns.AutoFit();
                }

                // Add summary sheet
                Excel.Worksheet summarySheet = workbook.Worksheets.Add(After: workbook.Worksheets[workbook.Worksheets.Count]);
                summarySheet.Name = "Общий итог";

                // Calculate grand total (assuming grandTotal is the sum of all payments)
                decimal grandTotal = (decimal)allUsers.SelectMany(u => u.Payment)
                    .Sum(p => p.Price * p.Num);

                // Write summary
                summarySheet.Cells[1, 1] = "Общий итог:";
                summarySheet.Cells[1, 2] = grandTotal;
                (summarySheet.Cells[1, 2] as Excel.Range).NumberFormat = "0.00";

                // Format summary
                Excel.Range summaryRange = summarySheet.Range[summarySheet.Cells[1, 1], summarySheet.Cells[1, 2]];
                summaryRange.Font.Color = Excel.XlRgbColor.rgbRed;
                summaryRange.Font.Bold = true;
                summarySheet.Columns.AutoFit();
                Excel.Worksheet defaultSheet = workbook.Worksheets["Лист1"];
                defaultSheet.Delete();
                // Save the workbook to the desktop
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = GetNewFileName(desktop, $"Расходы пользователя {DateTime.Today.ToString("dd-MM-yyyy")}", ".xlsx");

                workbook.SaveAs(filePath);

                MessageBox.Show($"Отчет сохранен на рабочем столе: {filePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании отчета: {ex.Message}");
            }
            finally
            {
                // Clean up Excel objects
                if (workbook != null)
                {
                    workbook.Close();
                    Marshal.ReleaseComObject(workbook);
                }
                if (application != null)
                {
                    application.Quit();
                    Marshal.ReleaseComObject(application);
                }
            }
        }

        private void wordBtn_Click(object sender, RoutedEventArgs e)
        {
            var allUsers = Entities.GetContext().User.ToList();
            var allCategories = Entities.GetContext().Category.ToList();

            var application = new Word.Application();
            Word.Document document = application.Documents.Add();

            foreach (var user in allUsers)
            {
                // Создаем абзац для ФИО пользователя
                Word.Paragraph userParagraph = document.Paragraphs.Add();
                Word.Range userRange = userParagraph.Range;
                userRange.Text = user.FIO;

                // Пробуем разные варианты стилей
                try { userParagraph.set_Style("Заголовок 1"); }
                catch
                {
                    try { userParagraph.set_Style("Заголовок"); }
                    catch { userParagraph.set_Style("Heading 1"); }
                }

                userRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                userRange.InsertParagraphAfter();

                document.Paragraphs.Add(); // Пустая строка

                // Создаем абзац для таблицы
                Word.Paragraph tableParagraph = document.Paragraphs.Add();
                Word.Range tableRange = tableParagraph.Range;
                Word.Table paymentsTable = document.Tables.Add(tableRange, allCategories.Count() + 1, 2);
                paymentsTable.Borders.InsideLineStyle = paymentsTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                paymentsTable.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                // Заголовки таблицы
                Word.Range cellRange;
                cellRange = paymentsTable.Cell(1, 1).Range;
                cellRange.Text = "Категория";
                cellRange = paymentsTable.Cell(1, 2).Range;
                cellRange.Text = "Сумма расходов";

                paymentsTable.Rows[1].Range.Font.Name = "Times New Roman";
                paymentsTable.Rows[1].Range.Font.Size = 14;
                paymentsTable.Rows[1].Range.Bold = 1;
                paymentsTable.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                // Заполняем таблицу данными
                for (int i = 0; i < allCategories.Count(); i++)
                {
                    var currentCategory = allCategories[i];

                    cellRange = paymentsTable.Cell(i + 2, 1).Range;
                    cellRange.Text = currentCategory.Name;
                    cellRange.Font.Name = "Times New Roman";
                    cellRange.Font.Size = 12;

                    cellRange = paymentsTable.Cell(i + 2, 2).Range;
                    var categorySum = user.Payment
                        .Where(p => p.CategoryID == currentCategory.ID)
                        .Sum(p => p.Num * p.Price);

                    // Форматируем сумму правильно
                    string formattedSum = string.Format("{0:N2} руб.", categorySum);
                    cellRange.Text = formattedSum;
                    cellRange.Font.Name = "Times New Roman";
                    cellRange.Font.Size = 12;
                }

                document.Paragraphs.Add(); // Пустая строка после таблицы

                // Максимальный платеж
                Payment maxPayment = user.Payment.OrderByDescending(u => u.Price * u.Num).FirstOrDefault();
                if (maxPayment != null)
                {
                    Word.Paragraph maxPaymentParagraph = document.Paragraphs.Add();
                    Word.Range maxPaymentRange = maxPaymentParagraph.Range;

                    // Форматируем числа и дату правильно
                    string maxPaymentAmount = string.Format("{0:N2}", maxPayment.Price * maxPayment.Num);
                    string maxPaymentDate = maxPayment.Date.ToString("dd.MM.yyyy");
                    maxPaymentRange.Text = $"Самый дорогостоящий платеж - {maxPayment.Name} за {maxPaymentAmount} руб. от {maxPaymentDate}";

                    // Пробуем разные варианты стилей
                    try { maxPaymentParagraph.set_Style("Заголовок 2"); }
                    catch
                    {
                        try { maxPaymentParagraph.set_Style("Подзаголовок"); }
                        catch { maxPaymentParagraph.set_Style("Heading 2"); }
                    }

                    maxPaymentRange.Font.Color = Word.WdColor.wdColorDarkRed;
                    maxPaymentRange.InsertParagraphAfter();
                }

                document.Paragraphs.Add(); // Пустая строка

                // Минимальный платеж
                Payment minPayment = user.Payment.OrderBy(u => u.Price * u.Num).FirstOrDefault();
                if (minPayment != null)
                {
                    Word.Paragraph minPaymentParagraph = document.Paragraphs.Add();
                    Word.Range minPaymentRange = minPaymentParagraph.Range;

                    // Форматируем числа и дату правильно
                    string minPaymentAmount = string.Format("{0:N2}", minPayment.Price * minPayment.Num);
                    string minPaymentDate = minPayment.Date.ToString("dd.MM.yyyy");
                    minPaymentRange.Text = $"Самый дешевый платеж - {minPayment.Name} за {minPaymentAmount} руб. от {minPaymentDate}";

                    // Пробуем разные варианты стилей
                    try { minPaymentParagraph.set_Style("Заголовок 2"); }
                    catch
                    {
                        try { minPaymentParagraph.set_Style("Подзаголовок"); }
                        catch { minPaymentParagraph.set_Style("Heading 2"); }
                    }

                    minPaymentRange.Font.Color = Word.WdColor.wdColorDarkGreen;
                    minPaymentRange.InsertParagraphAfter();
                }

                // Разрыв страницы (кроме последнего пользователя)
                if (user != allUsers.LastOrDefault())
                {
                    object breakType = Word.WdBreakType.wdPageBreak;
                    document.Words.Last.InsertBreak(ref breakType);
                }
            }

            // Добавляем колонтитулы
            foreach (Word.Section section in document.Sections)
            {
                // Нижний колонтитул с номерами страниц
                Word.HeaderFooter footer = section.Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                footer.PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberCenter);

                // Верхний колонтитул с датой
                Word.Range headerRange = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                headerRange.Font.ColorIndex = Word.WdColorIndex.wdBlack;
                headerRange.Font.Size = 10;
                headerRange.Text = DateTime.Now.ToString("dd/MM/yyyy");
            }

            // Сохранение файлов
            string fileNameDocx = GetNewFileName(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"Расходы пользователя {DateTime.Today:dd-MM-yyyy}",
                ".docx");

            string fileNamePdf = GetNewFileName(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"Расходы пользователя {DateTime.Today:dd-MM-yyyy}",
                ".pdf");

            application.Visible = true;
            document.SaveAs2(fileNameDocx);
            document.ExportAsFixedFormat(fileNamePdf, Word.WdExportFormat.wdExportFormatPDF);

            document.Close();
            application.Quit();

            MessageBox.Show("Отчеты сохранены на рабочем столе");
        }



        private void cmbUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateData();
        }

        private void cmbDiagramm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDiagramm.SelectedItem is SeriesChartType selectedType)
            {
                currentType = selectedType;
                UpdateData();
            }
        }
    }
}
