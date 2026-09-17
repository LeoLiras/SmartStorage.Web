using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using SmartStorage.EmailAPI.Config;
using SmartStorage.EmailAPI.Repository.Interfaces;
using SmartStorage_Shared.VO;

namespace SmartStorage.EmailAPI.Repository
{
    public class EmailRepository : IEmailRepository
    {
        #region Properties

        public string Provedor { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Destinatario { get; private set; }
        
        #endregion

        #region Constructros

        public EmailRepository(IOptions<EmailSettings> options)
        {
            Provedor = options.Value.Provedor;
            Username = options.Value.Username;
            Password = options.Value.Password;
            Destinatario = options.Value.Destinatario;
        }

        #endregion

        #region Methods

        public async Task NewProductEmail(ProductVO product)
        {
            var subject = $"Novo Produto Cadastrado";

            var body = "Prezado, um novo produto foi cadastrado no estoque, seguem os dados:\n\n" +
                $"Produto: {product.Name}\n" +
                $"Descrição: {product.Descricao}\n" +
                $"Quantidade: {product.Qntd}\n\n" +
                $"Por favor, realize a conferência.\n\n" +
                $"Atenciosamente.";

            await SendAsync(subject, body);
        }

        public async Task LowStockEmail(LowStockAlertVO alert)
        {
            var subject = $"Estoque abaixo do mínimo: {alert.ProductName}";

            var body = "Prezado, o estoque de um produto ficou abaixo do mínimo definido:\n\n" +
                $"Produto: {alert.ProductName}\n" +
                $"Estoque total: {alert.TotalQntd}\n" +
                $"Depósito: {alert.WarehouseQntd} · Prateleiras: {alert.ShelvesQntd}\n" +
                $"Estoque mínimo: {alert.MinimumStock}\n" +
                $"Movimentação que disparou o aviso: {alert.Origin}\n\n" +
                $"Por favor, providencie a reposição.\n\n" +
                $"Atenciosamente.";

            await SendAsync(subject, body);
        }

        private async Task SendAsync(string subject, string body)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("SmartStorage", Username));
            message.To.Add(new MailboxAddress("Destino", Destinatario));
            message.Subject = subject;

            message.Body = new TextPart("plain")
            {
                Text = body
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(Provedor, 587, MailKit.Security.SecureSocketOptions.StartTls);

                await client.AuthenticateAsync(Username, Password);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        #endregion

    }
}
