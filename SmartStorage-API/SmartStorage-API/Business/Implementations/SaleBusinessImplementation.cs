using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Google.GenAI;
using SmartStorage_API.Data.Converter.Implementations;
using SmartStorage_API.Data.VO;
using SmartStorage_API.Model.Context;
using SmartStorage_Shared.Model;

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
            var client = new Client(apiKey: System.Environment.GetEnvironmentVariable("GOOGLE_API_KEY"));

            var response = await client.Models.GenerateContentAsync(
              model: "gemini-2.5-flash", contents: $"{text}. Apenas texto, não utilize tabelas e separe em tópicos."
            );

            return response?.Candidates?[0]?.Content?.Parts?[0]?.Text ?? "";
        }

        public byte[] GenerateExcel()
        {
            var sales = FindAllSales().Where(s => s.DateSale.Month == DateTime.Now.Month).Take(100).ToList();

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

        #endregion
    }
}
