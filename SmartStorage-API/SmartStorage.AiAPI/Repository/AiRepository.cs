using Google.GenAI;
using SmartStorage.AIAPI.Repository.Interfaces;
using SmartStorage_API.Model.Context;
using System.Text.Json;

namespace SmartStorage.AIAPI.Repository
{
    public class AiRepository : IAiRepository
    {
        #region Propriedades

        private readonly SmartStorageContext _context;

        #endregion

        #region Construtores

        public AiRepository(SmartStorageContext context)
        {
            _context = context;
        }

        #endregion

        #region Métodos

        public async Task<string> CallAISales(string text)
        {
            var salesToPrompt = _context.Sales.OrderByDescending(s => s.SalDateSale).Take(10);
            var json = JsonSerializer.Serialize(salesToPrompt);

            var client = new Client(apiKey: Environment.GetEnvironmentVariable("GOOGLE_API_KEY"));

            var response = await client.Models.GenerateContentAsync(
              model: "gemini-2.5-flash", contents: $"{text}: {json}"
            );

            return response.Candidates?[0].Content?.Parts?[0].Text ?? "";
        }

        #endregion
    }
}
