using SmartStorage.Shared.Enum;
using SmartStorage_API.Model.Context;
using SmartStorage_API.Repository.Interfaces;
using SmartStorage_API.Service.Nfe;
using SmartStorage_Shared.Model;
using SmartStorage_Shared.VO;

namespace SmartStorage_API.Service.Implementations
{
    public class InvoiceBusinessImplementation : IInvoiceBusiness
    {
        #region Propriedades

        private readonly SmartStorageContext _context;

        private readonly IProductStockMovementRepository _movementRepository;

        #endregion

        #region Construtores

        public InvoiceBusinessImplementation(SmartStorageContext context, IProductStockMovementRepository movementRepository)
        {
            _context = context;
            _movementRepository = movementRepository;
        }

        #endregion

        #region Métodos

        public InvoicePreviewVO PreviewInvoice(string xml)
        {
            var nfe = NfeXmlReader.Read(xml);

            var codes = nfe.Items.Where(i => i.Codigo is not null).Select(i => i.Codigo).Distinct().ToList();

            var products = _context.Products
                .Where(p => codes.Contains(p.ProCodigo))
                .ToDictionary(p => p.ProCodigo);

            return new InvoicePreviewVO
            {
                Chave = nfe.Chave,
                Numero = nfe.Numero,
                Serie = nfe.Serie,
                EmitenteCnpj = nfe.EmitenteCnpj,
                EmitenteNome = nfe.EmitenteNome,
                DataEmissao = nfe.DataEmissao,
                ImportadaEm = _context.Invoices.Where(i => i.InvChave == nfe.Chave).Select(i => (DateTime?)i.InvDataImportacao).FirstOrDefault(),
                Items = nfe.Items.Select(item =>
                {
                    var product = item.Codigo is null ? null : products.GetValueOrDefault(item.Codigo);

                    var factor = product?.ProFatorConversao ?? 1;

                    var stockQuantity = item.Quantidade * factor;

                    return new InvoicePreviewItemVO
                    {
                        Numero = item.Numero,
                        CodigoProduto = item.CodigoProduto,
                        Codigo = item.Codigo,
                        CodigoInvalido = item.CodigoInvalido,
                        Descricao = item.Descricao,
                        Unidade = item.Unidade,
                        QntdNota = item.Quantidade,
                        ValorUnitario = item.ValorUnitario,
                        ValorTotal = item.ValorTotal,
                        ProductId = product?.ProId,
                        ProductName = product?.ProName,
                        FatorConversao = factor,
                        QntdEstoque = stockQuantity == decimal.Truncate(stockQuantity) && stockQuantity >= 1 ? (int)stockQuantity : null,
                    };
                }).ToList(),
            };
        }

        public InvoiceVO ImportInvoice(InvoiceImportVO import)
        {
            var nfe = NfeXmlReader.Read(import?.Xml);

            var imported = _context.Invoices.FirstOrDefault(i => i.InvChave == nfe.Chave);

            if (imported is not null)
                throw new Exception($"A NF-e {nfe.Numero}/{nfe.Serie} já foi importada em {imported.InvDataImportacao:dd/MM/yyyy HH:mm}.");

            var requests = import.Items ?? new List<InvoiceImportItemVO>();

            var unknown = requests.Select(r => r.Numero).Except(nfe.Items.Select(i => i.Numero)).ToList();

            if (unknown.Count > 0)
                throw new Exception($"A NF-e não tem o item {unknown[0]}.");

            var productIds = requests.Where(r => r.ProductId is not null).Select(r => r.ProductId.Value).Distinct().ToList();

            var existingProducts = _context.Products
                .Where(p => productIds.Contains(p.ProId))
                .ToDictionary(p => p.ProId);

            var codes = nfe.Items.Where(i => i.Codigo is not null).Select(i => i.Codigo).Distinct().ToList();

            var productsByCode = _context.Products
                .Where(p => codes.Contains(p.ProCodigo))
                .ToDictionary(p => p.ProCodigo);

            var newNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var newCodes = new HashSet<string>();

            var factorByProduct = new Dictionary<int, int>();

            var plan = new List<(NfeItem Item, InvoiceImportItemVO Request)>();

            foreach (var item in nfe.Items)
            {
                var itemName = $"Item {item.Numero} ({item.Descricao})";

                try
                {
                    var matching = requests.Where(r => r.Numero == item.Numero).ToList();

                    if (matching.Count == 0)
                        throw new Exception("escolha um produto existente ou cadastre um novo.");

                    if (matching.Count > 1)
                        throw new Exception("o item aparece mais de uma vez no envio.");

                    var request = matching[0];

                    if (request.FatorConversao < 1)
                        throw new Exception("o fator de conversão deve ser maior que zero.");

                    if (request.QntdEstoque < 1)
                        throw new Exception("a quantidade de entrada deve ser maior que zero.");

                    if ((request.ProductId is null) == (request.NewProduct is null))
                        throw new Exception("escolha um produto existente ou cadastre um novo, não os dois.");

                    if (request.ProductId is not null)
                        ValidateExistingProduct(item, request, existingProducts, productsByCode, factorByProduct);
                    else
                        ValidateNewProduct(item, request.NewProduct, productsByCode, newNames, newCodes);

                    plan.Add((item, request));
                }
                catch (Exception ex)
                {
                    throw new Exception($"{itemName}: {ex.Message}");
                }
            }

            var employeeIds = plan
                .Where(p => p.Request.NewProduct is not null)
                .Select(p => p.Request.NewProduct.EmployeeId ?? Employee.GenericEmployeeId)
                .Distinct()
                .ToList();

            var missingEmployee = employeeIds.Except(_context.Employees.Where(e => employeeIds.Contains(e.EmpId)).Select(e => e.EmpId)).FirstOrDefault();

            if (missingEmployee != 0)
                throw new Exception($"Colaborador {missingEmployee} não encontrado.");

            var userId = _movementRepository.FindAuthenticatedUserId();

            var movementDate = DateTime.Now;

            var reason = $"NF-e {nfe.Numero}/{nfe.Serie} de {nfe.EmitenteNome}";

            var invoice = new Invoice
            {
                InvChave = nfe.Chave,
                InvNumero = nfe.Numero,
                InvSerie = nfe.Serie,
                InvEmitenteCnpj = nfe.EmitenteCnpj,
                InvEmitenteNome = nfe.EmitenteNome,
                InvDataEmissao = nfe.DataEmissao,
                InvDataImportacao = movementDate,
                InvUseId = userId,
            };

            var createdProducts = 0;

            using (var transaction = _context.Database.BeginTransaction())
            {
                var totals = existingProducts.Keys.ToDictionary(id => id, id => _movementRepository.FindProductTotalBalance(id));

                foreach (var (item, request) in plan)
                {
                    Product product;

                    if (request.ProductId is not null)
                    {
                        product = existingProducts[request.ProductId.Value];

                        if (item.Codigo is not null && product.ProCodigo is null)
                            product.ProCodigo = item.Codigo;
                    }
                    else
                    {
                        product = CreateProduct(item, request.NewProduct);

                        totals[product.ProId] = 0;

                        createdProducts++;
                    }

                    product.ProFatorConversao = request.FatorConversao;

                    var unitCost = Math.Round(item.ValorTotal / request.QntdEstoque, 4);

                    product.ProCusto = AverageCost(product.ProCusto, totals[product.ProId], unitCost, request.QntdEstoque);

                    totals[product.ProId] += request.QntdEstoque;

                    _movementRepository.CreateNewStockMovement(
                        product.ProId,
                        shelfId: null,
                        TipoMovimentacao.Entrada,
                        request.QntdEstoque,
                        reason,
                        date: movementDate);

                    invoice.Items.Add(new InvoiceItem
                    {
                        IniNumero = item.Numero,
                        IniProId = product.ProId,
                        IniCodigo = item.Codigo,
                        IniDescricao = item.Descricao,
                        IniUnidade = item.Unidade,
                        IniQntdNota = item.Quantidade,
                        IniValorTotal = item.ValorTotal,
                        IniFatorConversao = request.FatorConversao,
                        IniQntdEstoque = request.QntdEstoque,
                        IniCustoUnitario = unitCost,
                    });
                }

                _context.Invoices.Add(invoice);
                _context.SaveChanges();

                transaction.Commit();
            }

            return new InvoiceVO
            {
                Id = invoice.InvId,
                Chave = invoice.InvChave,
                Numero = invoice.InvNumero,
                Serie = invoice.InvSerie,
                EmitenteNome = invoice.InvEmitenteNome,
                DataImportacao = invoice.InvDataImportacao,
                ItemsCount = invoice.Items.Count,
                CreatedProducts = createdProducts,
            };
        }

        public static decimal AverageCost(decimal? currentCost, int currentTotal, decimal entryCost, int entryQuantity)
        {
            if (currentCost is null || currentTotal <= 0)
                return entryCost;

            return Math.Round((currentCost.Value * currentTotal + entryCost * entryQuantity) / (currentTotal + entryQuantity), 4);
        }

        private static void ValidateExistingProduct(
            NfeItem item,
            InvoiceImportItemVO request,
            Dictionary<int, Product> existingProducts,
            Dictionary<string, Product> productsByCode,
            Dictionary<int, int> factorByProduct)
        {
            if (!existingProducts.TryGetValue(request.ProductId.Value, out var product))
                throw new Exception("produto não encontrado com o ID informado.");

            if (item.Codigo is not null)
            {
                if (product.ProCodigo is not null && product.ProCodigo != item.Codigo)
                    throw new Exception($"o produto {product.ProName} tem o código {product.ProCodigo}, e a nota traz {item.Codigo}.");

                if (productsByCode.TryGetValue(item.Codigo, out var owner) && owner.ProId != product.ProId)
                    throw new Exception($"o código {item.Codigo} já pertence ao produto {owner.ProName}.");
            }

            if (factorByProduct.TryGetValue(product.ProId, out var factor) && factor != request.FatorConversao)
                throw new Exception($"o produto {product.ProName} aparece em outro item com fator de conversão diferente.");

            factorByProduct[product.ProId] = request.FatorConversao;
        }

        private void ValidateNewProduct(
            NfeItem item,
            ProductVO newProduct,
            Dictionary<string, Product> productsByCode,
            HashSet<string> newNames,
            HashSet<string> newCodes)
        {
            var name = newProduct.Name?.Trim();

            if (string.IsNullOrEmpty(name) || name.Length < 5 || name.Length > 100)
                throw new Exception("o nome do produto deve ter entre 5 e 100 caracteres.");

            var description = string.IsNullOrWhiteSpace(newProduct.Descricao) ? item.Descricao : newProduct.Descricao.Trim();

            if (description.Length < 5 || description.Length > 300)
                throw new Exception("a descrição do produto deve ter entre 5 e 300 caracteres.");

            if (!newNames.Add(name) || _context.Products.Any(p => p.ProName == name))
                throw new Exception($"já existe um produto chamado {name}.");

            if (item.Codigo is not null)
            {
                if (productsByCode.TryGetValue(item.Codigo, out var owner))
                    throw new Exception($"o código {item.Codigo} já pertence ao produto {owner.ProName}; vincule o item a ele.");

                if (!newCodes.Add(item.Codigo))
                    throw new Exception($"outro item da nota já cadastra um produto com o código {item.Codigo}.");
            }

            if (newProduct.MinimumStock < 0)
                throw new Exception("o estoque mínimo não pode ser negativo.");

            if (newProduct.Volume <= 0)
                throw new Exception("o volume do produto deve ser maior que zero.");

            if (newProduct.PrecoInicial <= 0)
                throw new Exception("o preço inicial do produto deve ser maior que zero.");
        }

        private Product CreateProduct(NfeItem item, ProductVO newProduct)
        {
            var product = new Product
            {
                ProName = newProduct.Name.Trim(),
                ProDescription = string.IsNullOrWhiteSpace(newProduct.Descricao) ? item.Descricao : newProduct.Descricao.Trim(),
                ProDateRegister = DateTime.UtcNow,
                ProQntd = 0,
                ProMinimumStock = newProduct.MinimumStock,
                ProVolume = newProduct.Volume,
                ProPrecoInicial = newProduct.PrecoInicial,
                ProCodigo = item.Codigo,
                ProEmpId = newProduct.EmployeeId ?? Employee.GenericEmployeeId,
                ProImage = newProduct.ProImage,
            };

            _context.Products.Add(product);
            _context.SaveChanges();

            return product;
        }

        #endregion
    }
}
