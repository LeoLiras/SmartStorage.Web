using System;

namespace SmartStorage.Shared.Validation
{
    /// <summary>
    /// Recusa de regra de negócio: a mensagem vai para o cliente como está, então precisa ser
    /// escrita para o usuário, em português e sem detalhe interno.
    /// </summary>
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message)
        {
        }

        public BusinessException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
