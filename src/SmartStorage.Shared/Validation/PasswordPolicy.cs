using System.Linq;

namespace SmartStorage.Shared.Validation
{
    public static class PasswordPolicy
    {
        public const int MinLength = 8;

        // Uma mensagem só, com a regra inteira, para a tela usar o mesmo texto como ajuda do campo.
        public const string RuleMessage = "A senha precisa ter ao menos 8 caracteres, com uma letra, um número e um caractere especial.";

        /// <returns>O motivo da recusa, ou null quando a senha cumpre a regra.</returns>
        public static string Validate(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < MinLength)
                return RuleMessage;

            if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit) || !password.Any(IsSpecial))
                return RuleMessage;

            return null;
        }

        // Qualquer coisa fora de letra, número e espaço, para a tela não precisar listar símbolos.
        private static bool IsSpecial(char c)
            => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c);
    }
}
