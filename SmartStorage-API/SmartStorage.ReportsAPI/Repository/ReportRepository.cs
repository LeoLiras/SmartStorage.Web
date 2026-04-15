using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ScottPlot;
using SmartStorage.ReportsAPI.Repository.IRepository;
using SmartStorage_API.Model.Context;
using System.Globalization;

namespace SmartStorage.ReportsAPI.Repository
{
    public class ReportRepository : IReportRepository
    {
        #region Properties

        private readonly SmartStorageContext _context;

        #endregion

        #region Constructors

        public ReportRepository(SmartStorageContext context)
        {
            _context = context;
        }

        #endregion

        #region Methods

        public async Task<byte[]> GenerateExcel()
        {
            var sales = _context.Sales
                .Include(s => s.Enter)
                .ThenInclude(s => s.Shelf)
                .Include(s => s.Enter)
                .ThenInclude(s => s.Product)
                .Where(s => s.SalDateSale.Month == DateTime.Now.Month).ToList();

            if (sales is null)
                throw new Exception("Ainda não há vendas no mês corrente.");

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add($"Vendas {DateTime.Now.Month}-{DateTime.Now.Year}");

            int row = 1;

            ws.Cell(row, 1).Value = "Produto";
            ws.Cell(row, 2).Value = "Prateleira";
            ws.Cell(row, 3).Value = "Data";
            ws.Cell(row, 4).Value = "Quantidade";
            ws.Cell(row, 5).Value = "Preço de Venda";
            ws.Cell(row, 6).Value = "Total";

            ws.Range(row, 1, row, 6).Style.Font.Bold = true;
            ws.Range(row, 1, row, 6).Style.Fill.BackgroundColor = XLColor.LightGray;

            row++;

            foreach (var sale in sales)
            {
                ws.Cell(row, 1).Value = sale.Enter.Product.ProName;
                ws.Cell(row, 2).Value = sale.Enter.Shelf.SheName;
                ws.Cell(row, 3).Value = sale.SalDateSale;
                ws.Cell(row, 4).Value = sale.SalQntd;
                ws.Cell(row, 5).Value = sale.Enter.EntPrice;
                ws.Cell(row, 5).Style.NumberFormat.Format = "R$ #,##0.00";
                ws.Cell(row, 6).Value = sale.Enter.EntPrice * sale.SalQntd;
                ws.Cell(row, 6).Style.NumberFormat.Format = "R$ #,##0.00";

                row++;
            }

            ws.Columns().AdjustToContents();

            ws.Range(1, 1, row - 1, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(1, 1, row - 1, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, row - 1, 6).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            using var ms = new MemoryStream();
            wb.SaveAs(ms);

            return ms.ToArray();
        }

        public async Task<byte[]> GeneratePdf()
        {
            var sales = _context.Sales
                .Include(s => s.Enter)
                .ThenInclude(s => s.Shelf)
                .Include(s => s.Enter)
                .ThenInclude(s => s.Product)
                .Where(s => s.SalDateSale.Month == DateTime.Now.Month).ToList();

            if (sales is null)
                throw new Exception("Ainda não há vendas no mês corrente.");

            //Table grade
            Func<IContainer, IContainer> cellStyle = c =>
                c.Border(1)
                .BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2)
                .Padding(5);

            //var reportAi = await AnalyseAI("Faça um resumo das minhas vendas em texto corrente (somente um texto normal, sem tópicos ou tabelas), para que eu coloque no meu relatório. Apenas me dê o resumo, sem saudações. Não cite lucro bruto total ou qualquer valor que seja igual a R$ 0.0.");

            //============================= Chart: Most saled in the month =============================

            var mostSaledMonth = sales
                .GroupBy(x => x.Enter.Product.ProName)
                .Select(p => new
                {
                    Product = p.Key,
                    Quantity = p.Sum(x => x.SalQntd)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(10)
                .ToList();

            Plot mostSaledMonthPlot = new();

            mostSaledMonthPlot.Title("Produtos mais vendidos do mês.", size: 25);

            mostSaledMonthPlot.Add.Bars(mostSaledMonth.Select(p => (double)p.Quantity).ToArray());
            mostSaledMonthPlot.Axes.Margins(bottom: 0);

            Tick[] mostSaledMonthTicks = mostSaledMonth
                .Select((x, index) => new Tick(index, x.Product))
                .ToArray();

            mostSaledMonthPlot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(mostSaledMonthTicks);
            mostSaledMonthPlot.Axes.Bottom.TickLabelStyle.Rotation = 45;
            mostSaledMonthPlot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleLeft;
            mostSaledMonthPlot.Axes.Bottom.TickLabelStyle.FontSize = 20;

            float largestLabelWidth = 0;
            using Paint paint = Paint.NewDisposablePaint();
            foreach (Tick tick in mostSaledMonthTicks)
            {
                PixelSize size = mostSaledMonthPlot.Axes.Bottom.TickLabelStyle.Measure(tick.Label, paint).Size;
                largestLabelWidth = Math.Max(largestLabelWidth, size.Width);
            }

            mostSaledMonthPlot.Axes.Bottom.MinimumSize = largestLabelWidth;
            mostSaledMonthPlot.Axes.Right.MinimumSize = largestLabelWidth;

            var mostSaledMonthChart = mostSaledMonthPlot.GetImageBytes(1000, 600);

            //============================= Chart: Most saled in the month =============================

            //============================= Chart: Total Saled =============================

            var yearSales = _context.Sales
                .Include(s => s.Enter)
                .Where(s => s.SalDateSale.Year.Equals(DateTime.Now.Year))
                .GroupBy(s => s.SalDateSale.Month)
                .Select(s => new
                {
                    Month = s.Key,
                    Total = s.Sum(x => x.Enter.EntPrice * x.SalQntd)
                })
                .OrderBy(s => s.Month)
                .ToList();

            double[] totals = yearSales.Select(x => (double)x.Total).ToArray();
            double[] months = yearSales.Select(x => (double)(x.Month - 1)).ToArray();

            string[] monthNames = { "Jan", "Fev", "Mar", "Abr", "Mai", "Jun", "Jul", "Ago", "Set", "Out", "Nov", "Dez" };

            Tick[] totalSaledTicks = yearSales.Select(x => new Tick(x.Month - 1, monthNames[x.Month - 1])).ToArray();

            var totalSaledPlot = new Plot();
            totalSaledPlot.Title($"Total de Vendas por Mês");
            totalSaledPlot.YLabel("Valor Total (R$)");
            totalSaledPlot.XLabel("Mês");

            totalSaledPlot.Add.ScatterLine(months, totals);
            totalSaledPlot.Axes.Margins(0.1, 0.1);

            totalSaledPlot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(totalSaledTicks);

            var totalSaledChart = totalSaledPlot.GetImageBytes(1000, 600);

            //============================= Chart: Total Saled =============================

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(QuestPDF.Helpers.Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .AlignRight()
                        .Text(x => { x.CurrentPageNumber(); });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Item().AlignCenter().Text($"Relatório de Vendas").Bold().FontSize(20);

                            x.Spacing(20);

                            x.Item().AlignLeft().Text($"Emitido em: {DateTime.Now.ToString("F", new CultureInfo("pt-BR"))}");

                            x.Item().PageBreak();

                            //x.Item().AlignCenter().Text(reportAi);

                            x.Item().Image(mostSaledMonthChart).FitWidth();

                            x.Spacing(20);

                            x.Item().Image(totalSaledChart).FitWidth();

                            x.Item().PageBreak();

                            x.Item().Table(t =>
                            {
                                t.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn();
                                    c.RelativeColumn();
                                    c.RelativeColumn();
                                    c.RelativeColumn();
                                    c.RelativeColumn();
                                    c.RelativeColumn();
                                });

                                t.Header(h =>
                                {
                                    h.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten1).Element(cellStyle).Text("Produto").Bold();
                                    h.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten1).Element(cellStyle).Text("Prateleira").Bold();
                                    h.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten1).Element(cellStyle).Text("Data").Bold();
                                    h.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten1).Element(cellStyle).Text("Quantidade").Bold();
                                    h.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten1).Element(cellStyle).Text("Preço de Venda").Bold();
                                    h.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten1).Element(cellStyle).Text("Total da Venda").Bold();
                                });

                                foreach (var sale in sales)
                                {
                                    t.Cell().Element(cellStyle).Text(sale.Enter.Product.ProName);
                                    t.Cell().Element(cellStyle).Text(sale.Enter.Shelf.SheName);
                                    t.Cell().Element(cellStyle).Text(sale.SalDateSale.ToString("d"));
                                    t.Cell().Element(cellStyle).Text(sale.SalQntd.ToString());
                                    t.Cell().Element(cellStyle).Text($"R$ {sale.Enter.EntPrice.ToString()}");
                                    t.Cell().Element(cellStyle).Text($"R$ {sale.SalQntd * sale.Enter.EntPrice}");
                                }
                            });
                        });


                });
            })
            .GeneratePdf();
        }

        #endregion


    }
}
