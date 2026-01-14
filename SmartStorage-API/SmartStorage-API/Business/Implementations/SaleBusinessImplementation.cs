using ClosedXML.Excel;
using Google.GenAI;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ScottPlot;
using SmartStorage_API.Data.Converter.Implementations;
using SmartStorage_API.Data.VO;
using SmartStorage_API.Model.Context;
using SmartStorage_Shared.Model;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace SmartStorage_API.Service.Implementations
{
    public class SaleBusinessImplementation : ISaleBusiness
    {
        #region Propriedades

        private readonly SmartStorageContext _context;

        private readonly SaleConverter _converter;

        #endregion

        #region Construtores

        public SaleBusinessImplementation(SmartStorageContext context)
        {
            _context = context;
            _converter = new SaleConverter(_context);
        }

        #endregion

        #region Métodos

        public List<SaleVO> FindAllSales()
        {
            return _converter.Parse(_context.Sales.OrderBy(s => s.SalId).ToList());
        }

        public SaleVO FindSaleById(int saleId)
        {
            var sale = _context.Sales.FirstOrDefault(s => s.SalId.Equals(saleId));

            if (sale is null)
                throw new Exception("Venda não encontrada com o ID informado");

            return _converter.Parse(sale);
        }

        public SaleVO CreateNewSale(int enterId, int saleQntd, DateTime dateSale)
        {
            var enter = _context.Enters.FirstOrDefault(e => e.EntId.Equals(enterId));

            if (enter is null)
                throw new Exception("Entrada não encontrada com o ID do Produto informado.");

            if (enter.EntQntd >= saleQntd)
            {
                enter.EntQntd -= saleQntd;

                var sale = new Sale
                {
                    SalEntId = enter.EntId,
                    SalQntd = saleQntd,
                    SalDateSale = dateSale,
                };

                _context.Sales.Add(sale);
                _context.SaveChanges();

                return _converter.Parse(sale);
            }
            else
            {
                throw new Exception("Quantidade indisponível para venda.");
            }
        }

        public SaleVO UpdateSale(int saleId, int saleQntd)
        {
            var sale = _context.Sales.FirstOrDefault(s => s.SalId == saleId);

            if (sale == null)
                throw new Exception("Venda não encontrada com o ID informado");

            var enter = _context.Enters.FirstOrDefault(e => e.EntId.Equals(sale.SalEntId));

            if (enter == null)
                throw new Exception("Entrada não encontrada com o ID de Venda informado");

            if (saleQntd < sale.SalQntd)
                enter.EntQntd += saleQntd;
            else
            {
                var rest = (saleQntd - sale.SalQntd);

                if (enter.EntQntd > rest)
                    enter.EntQntd -= rest;
                else
                    throw new Exception("Não há quantidade suficiente na entrada do Produto para realizar essa atualização");
            }

            sale.SalQntd = saleQntd;

            _context.SaveChanges();

            var shelf = _context.Shelves.FirstOrDefault(s => s.SheId == enter.EntSheId);

            return _converter.Parse(sale);
        }

        public SaleVO DeleteSale(int saleId)
        {
            var sale = _context.Sales.FirstOrDefault(s => s.SalId.Equals(saleId));

            if (sale is null)
                throw new Exception("Venda não encontrada com o ID informado");

            var enter = _context.Enters.FirstOrDefault(e => e.EntId.Equals(sale.SalEntId));

            if (enter is null)
                throw new Exception("Entrada não encontrada com o ID de Venda informado.");

            enter.EntQntd += sale.SalQntd;

            _context.Sales.Remove(sale);
            _context.SaveChanges();

            return _converter.Parse(sale);
        }

        public async Task<string> AnalyseAI(string text)
        {
            var salesToPrompt = FindAllSales().OrderByDescending(s => s.DateSale).Take(10);
            var json = JsonSerializer.Serialize(salesToPrompt);

            var client = new Client(apiKey: System.Environment.GetEnvironmentVariable("GOOGLE_API_KEY"));

            var response = await client.Models.GenerateContentAsync(
              model: "gemini-2.5-flash", contents: $"{text}: {json}"
            );

            return response?.Candidates?[0]?.Content?.Parts?[0]?.Text ?? "";
        }

        public byte[] GenerateExcel()
        {
            var sales = FindAllSales().Where(s => s.DateSale.Month == DateTime.Now.Month).ToList();

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
                ws.Cell(row, 1).Value = sale.ProductName;
                ws.Cell(row, 2).Value = sale.ShelfName;
                ws.Cell(row, 3).Value = sale.DateSale;
                ws.Cell(row, 4).Value = sale.Qntd;
                ws.Cell(row, 5).Value = sale.EnterPrice;
                ws.Cell(row, 5).Style.NumberFormat.Format = "R$ #,##0.00";
                ws.Cell(row, 6).Value = sale.SaleTotal;
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
            //Table grade
            Func<IContainer, IContainer> cellStyle = c =>
                c.Border(1)
                .BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2)
                .Padding(5);

            //var reportAi = await AnalyseAI("Faça um resumo das minhas vendas em texto corrente (somente um texto normal, sem tópicos ou tabelas), para que eu coloque no meu relatório. Apenas me dê o resumo, sem saudações.");

            var sales = FindAllSales().Where(s => s.DateSale.Month == DateTime.Now.Month).ToList();

            //============================= Chart: Most saled int the month =============================

            var mostSaledMonth = sales
                .GroupBy(x => x.ProductName)
                .Select(p => new
                {
                    Product = p.Key,
                    Quantity = p.Sum(x => x.Qntd)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(10)
                .ToList();

            Plot mostSaledMonthPlot = new();

            mostSaledMonthPlot.Add.Bars(mostSaledMonth.Select(p => (double)p.Quantity).ToArray());
            mostSaledMonthPlot.Axes.Margins(bottom: 0);

            Tick[] ticks = mostSaledMonth
                .Select((x, index) => new Tick(index, x.Product))
                .ToArray();

            mostSaledMonthPlot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks);
            mostSaledMonthPlot.Axes.Bottom.TickLabelStyle.Rotation = 45;
            mostSaledMonthPlot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleLeft;
            mostSaledMonthPlot.Axes.Bottom.TickLabelStyle.FontSize = 20;

            float largestLabelWidth = 0;
            using Paint paint = Paint.NewDisposablePaint();
            foreach (Tick tick in ticks)
            {
                PixelSize size = mostSaledMonthPlot.Axes.Bottom.TickLabelStyle.Measure(tick.Label, paint).Size;
                largestLabelWidth = Math.Max(largestLabelWidth, size.Width);
            }

            mostSaledMonthPlot.Axes.Bottom.MinimumSize = largestLabelWidth;
            mostSaledMonthPlot.Axes.Right.MinimumSize = largestLabelWidth;

            var mostSaledMonthChart = mostSaledMonthPlot.GetImageBytes(1000, 600);

            //============================= Chart: Most saled int the month =============================

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

                            x.Spacing(20);

                            //x.Item().AlignCenter().Text(reportAi);

                            x.Spacing(20);

                            x.Item().Image(mostSaledMonthChart).FitWidth();

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

                                foreach(var sale in sales)
                                {
                                    t.Cell().Element(cellStyle).Text(sale.ProductName);
                                    t.Cell().Element(cellStyle).Text(sale.ShelfName);
                                    t.Cell().Element(cellStyle).Text(sale.DateSale.ToString("d"));
                                    t.Cell().Element(cellStyle).Text(sale.Qntd.ToString());
                                    t.Cell().Element(cellStyle).Text($"R$ {sale.EnterPrice.ToString()}");
                                    t.Cell().Element(cellStyle).Text($"R$ {sale.SaleTotal.ToString()}");
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
