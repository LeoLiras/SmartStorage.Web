using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace SmartStorage_API.Service.Nfe
{
    public class NfeDocument
    {
        public string Chave { get; set; }

        public string Numero { get; set; }

        public string Serie { get; set; }

        public string EmitenteCnpj { get; set; }

        public string EmitenteNome { get; set; }

        public DateTime DataEmissao { get; set; }

        public List<NfeItem> Items { get; set; } = new List<NfeItem>();
    }

    public class NfeItem
    {
        public int Numero { get; set; }

        public string CodigoProduto { get; set; }

        public string Codigo { get; set; }

        public string CodigoInvalido { get; set; }

        public string Descricao { get; set; }

        public string Unidade { get; set; }

        public decimal Quantidade { get; set; }

        public decimal ValorUnitario { get; set; }

        public decimal ValorTotal { get; set; }
    }

    public static class NfeXmlReader
    {
        #region Propriedades

        private static readonly XNamespace Ns = "http://www.portalfiscal.inf.br/nfe";

        #endregion

        #region Métodos

        public static NfeDocument Read(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                throw new Exception("O XML da nota fiscal é obrigatório.");

            XDocument document;

            try
            {
                document = XDocument.Parse(xml.TrimStart('﻿'), LoadOptions.None);
            }
            catch (XmlException)
            {
                throw new Exception("O arquivo não é um XML válido.");
            }

            var infNFe = document.Descendants(Ns + "infNFe").FirstOrDefault()
                ?? throw new Exception("O arquivo não é uma NF-e: o grupo infNFe não foi encontrado.");

            var ide = Required(infNFe, "ide");

            var model = Text(ide, "mod");

            if (model != "55")
                throw new Exception($"Só a NF-e modelo 55 é aceita; o arquivo é do modelo {model ?? "desconhecido"}.");

            var emit = Required(infNFe, "emit");

            var nfe = new NfeDocument
            {
                Chave = ReadChave(document, infNFe),
                Numero = RequiredText(ide, "nNF"),
                Serie = RequiredText(ide, "serie"),
                EmitenteCnpj = Text(emit, "CNPJ") ?? Text(emit, "CPF"),
                EmitenteNome = Truncate(RequiredText(emit, "xNome"), 60),
                DataEmissao = ReadDataEmissao(ide),
            };

            foreach (var det in infNFe.Elements(Ns + "det"))
            {
                var prod = Required(det, "prod");

                var (codigo, codigoInvalido) = ReadGtin(Text(prod, "cEAN"));

                nfe.Items.Add(new NfeItem
                {
                    Numero = int.TryParse((string)det.Attribute("nItem"), out var numero) ? numero : nfe.Items.Count + 1,
                    CodigoProduto = Text(prod, "cProd"),
                    Codigo = codigo,
                    CodigoInvalido = codigoInvalido,
                    Descricao = Truncate(RequiredText(prod, "xProd"), 120),
                    Unidade = Truncate(RequiredText(prod, "uCom"), 6),
                    Quantidade = RequiredDecimal(prod, "qCom"),
                    ValorUnitario = RequiredDecimal(prod, "vUnCom"),
                    ValorTotal = RequiredDecimal(prod, "vProd"),
                });
            }

            if (nfe.Items.Count == 0)
                throw new Exception("A NF-e não tem itens.");

            return nfe;
        }

        public static bool IsValidGtin(string code)
        {
            if (string.IsNullOrEmpty(code) || !code.All(char.IsAsciiDigit) || code.Length is not (8 or 12 or 13 or 14))
                return false;

            var sum = 0;

            for (var i = code.Length - 2; i >= 0; i--)
            {
                var digit = code[i] - '0';

                sum += (code.Length - 2 - i) % 2 == 0 ? digit * 3 : digit;
            }

            return (10 - sum % 10) % 10 == code[^1] - '0';
        }

        private static (string Codigo, string CodigoInvalido) ReadGtin(string cean)
        {
            if (string.IsNullOrWhiteSpace(cean) || cean.Equals("SEM GTIN", StringComparison.OrdinalIgnoreCase))
                return (null, null);

            return IsValidGtin(cean) ? (cean, null) : (null, cean);
        }

        private static string ReadChave(XDocument document, XElement infNFe)
        {
            var id = (string)infNFe.Attribute("Id");

            var chave = id is not null && id.StartsWith("NFe") ? id[3..] : Text(document.Descendants(Ns + "infProt").FirstOrDefault(), "chNFe");

            if (chave is null || chave.Length != 44 || !chave.All(char.IsAsciiDigit))
                throw new Exception("A chave de acesso da NF-e não foi encontrada ou não tem 44 dígitos.");

            return chave;
        }

        private static DateTime ReadDataEmissao(XElement ide)
        {
            var dhEmi = Text(ide, "dhEmi");

            if (dhEmi is not null && DateTimeOffset.TryParse(dhEmi, CultureInfo.InvariantCulture, DateTimeStyles.None, out var withOffset))
                return withOffset.DateTime;

            var dEmi = Text(ide, "dEmi");

            if (dEmi is not null && DateTime.TryParseExact(dEmi, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;

            throw new Exception("A data de emissão da NF-e não foi encontrada.");
        }

        private static XElement Required(XElement parent, string name)
        {
            return parent.Element(Ns + name)
                ?? throw new Exception($"A NF-e não tem o grupo obrigatório {name}.");
        }

        private static string Text(XElement parent, string name)
        {
            var value = parent?.Element(Ns + name)?.Value.Trim();

            return string.IsNullOrEmpty(value) ? null : value;
        }

        private static string RequiredText(XElement parent, string name)
        {
            return Text(parent, name)
                ?? throw new Exception($"A NF-e não tem o campo obrigatório {name}.");
        }

        private static decimal RequiredDecimal(XElement parent, string name)
        {
            if (!decimal.TryParse(RequiredText(parent, name), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value))
                throw new Exception($"O campo {name} da NF-e não é um número válido.");

            return value;
        }

        private static string Truncate(string value, int length)
        {
            return value.Length <= length ? value : value[..length];
        }

        #endregion
    }
}
