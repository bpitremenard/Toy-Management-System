using System.Globalization;
using Microsoft.Extensions.Localization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TMS_SharedLibrary.Models;

namespace TMSTeacher.Services
{
    public class ReportsPDFService
    {
        private readonly IStringLocalizer _localizer;

        public ReportsPDFService(IStringLocalizerFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _localizer = factory.Create("Views.Shared._Layout", typeof(Program).Assembly.GetName().Name!);
        }

        public virtual byte[] GenerateBorrowHistoryPdf(IEnumerable<ToyLoan> loans, string studentName, bool languageOption)
        {
            if (loans == null)
            {
                throw new ArgumentNullException(nameof(loans));
            }

            if (string.IsNullOrWhiteSpace(studentName))
            {
                studentName = "-";
            }

            QuestPDF.Settings.License = LicenseType.Community;

            var loanList = loans.ToList();

            // true = English, false = French
            var selectedCulture = new CultureInfo(languageOption ? "en" : "fr-CA");

            var originalCulture = CultureInfo.CurrentCulture;
            var originalUICulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = selectedCulture;
                CultureInfo.CurrentUICulture = selectedCulture;

                var overdueText = _localizer["Overdue"].Value;
                var activeText = _localizer["Active"].Value;
                var returnedText = _localizer["Returned"].Value;

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        page.Header()
                            .Text(studentName)
                            .SemiBold()
                            .FontSize(20)
                            .FontColor(Colors.Blue.Medium);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(column =>
                            {
                                column.Spacing(5);

                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Border(1).Background(Colors.Grey.Lighten2)
                                        .Padding(5).Text(_localizer["ToyName"].Value).SemiBold();

                                    row.RelativeItem().Border(1).Background(Colors.Grey.Lighten2)
                                        .Padding(5).Text(_localizer["BorrowedDate"].Value).SemiBold();

                                    row.RelativeItem().Border(1).Background(Colors.Grey.Lighten2)
                                        .Padding(5).Text(_localizer["DueDate"].Value).SemiBold();

                                    row.RelativeItem().Border(1).Background(Colors.Grey.Lighten2)
                                        .Padding(5).Text(_localizer["ReturnedDate"].Value).SemiBold();

                                    row.RelativeItem().Border(1).Background(Colors.Grey.Lighten2)
                                        .Padding(5).Text(_localizer["Status"].Value).SemiBold();
                                });

                                foreach (var item in loanList)
                                {
                                    column.Item().Row(row =>
                                    {
                                        row.RelativeItem().Border(1).Padding(5)
                                            .Text(item.Toy?.Name ?? "-");

                                        row.RelativeItem().Border(1).Padding(5)
                                            .Text(item.BorrowDate.ToString("yyyy-MM-dd"));

                                        row.RelativeItem().Border(1).Padding(5)
                                            .Text(item.DueDate.ToString("yyyy-MM-dd"));

                                        var returnedLate = item.ReturnDate.HasValue && item.ReturnDate.Value > item.DueDate;

                                        row.RelativeItem().Border(1).Padding(5).Text(text =>
                                        {
                                            var span = text.Span(item.ReturnDate?.ToString("yyyy-MM-dd") ?? "-");

                                            if (returnedLate)
                                            {
                                                span.FontColor(Colors.Red.Medium);
                                            }
                                        });

                                        var status = item.ReturnDate == null
                                            ? (item.DueDate < DateOnly.FromDateTime(DateTime.Today) ? overdueText : activeText)
                                            : returnedText;

                                        var statusColor = status == overdueText ? Colors.Red.Medium :
                                                          status == activeText ? Colors.Orange.Medium :
                                                          Colors.Green.Medium;

                                        row.RelativeItem().Border(1).Padding(5)
                                            .Text(status)
                                            .FontColor(statusColor)
                                            .SemiBold();
                                    });
                                }

                                if (!loanList.Any())
                                {
                                    column.Item().Text(_localizer["NoBorrowHistoryFound"].Value)
                                        .Italic()
                                        .FontColor(Colors.Grey.Medium);
                                }
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span($"{_localizer["GeneratedOn"].Value} ");
                                x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).SemiBold();
                            });
                    });
                });

                return document.GeneratePdf();
            } finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUICulture;
            }
        }
    }
}