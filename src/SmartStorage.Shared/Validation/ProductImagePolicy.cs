using System;

namespace SmartStorage.Shared.Validation
{
    public static class ProductImagePolicy
    {
        public const int MaxSizeInBytes = 10 * 1024 * 1024;

        public const string InvalidFormatMessage = "A imagem do produto precisa ser PNG ou JPEG.";

        public const string TooLargeMessage = "A imagem do produto não pode passar de 10 MB.";

        private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        private static readonly byte[] JpegSignature = { 0xFF, 0xD8, 0xFF };

        /// <returns>O motivo da recusa, ou null quando a imagem pode ser gravada.</returns>
        public static string Validate(byte[] image)
        {
            if (image is null || image.Length == 0)
                return null;

            if (image.Length > MaxSizeInBytes)
                return TooLargeMessage;

            // Pela assinatura porque o corpo da requisição só traz os bytes: a extensão
            // e o Content-Type do arquivo escolhido não chegam à API.
            if (!HasSignature(image, PngSignature) && !HasSignature(image, JpegSignature))
                return InvalidFormatMessage;

            return null;
        }

        private static bool HasSignature(byte[] image, byte[] signature)
            => image.AsSpan().StartsWith(signature);
    }
}
