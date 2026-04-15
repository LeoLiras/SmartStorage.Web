using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using SmartStorage.ReportsAPI.Repository.IRepository;
using SmartStorage_API.Model.Context;

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

        public byte[] GenerateExcel()
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

        public Task<byte[]> GeneratePdf()
        {
            throw new NotImplementedException();
        }

        #endregion


    }
}
