using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartStorage_API.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Employee",
                columns: new[] { "EmpId", "EmpCpf", "EmpDateRegister", "EmpName", "EmpRg" },
                values: new object[,]
                {
                    { 1, "52998224725", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Ana Paula Ribeiro", "MG1234567" },
                    { 2, "11144477735", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Bruno Carvalho Lima", "SP2345678" },
                    { 3, "39053344705", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Carla Menezes Souza", "RJ3456789" },
                    { 4, "16899535009", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Diego Ferreira Alves", "MG4567890" },
                    { 5, "40442820850", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Eduarda Nogueira Pinto", "SP5678901" }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Shelf",
                columns: new[] { "SheId", "SheDataRegister", "SheName" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Prateleira A1 - Ferramentas Elétricas" },
                    { 2, new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Prateleira A2 - Ferramentas Manuais" },
                    { 3, new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Prateleira B1 - Equipamentos de Proteção" },
                    { 4, new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Prateleira B2 - Materiais Elétricos" },
                    { 5, new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Prateleira C1 - Fixadores e Parafusos" },
                    { 6, new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Prateleira C2 - Medição e Precisão" },
                    { 7, new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Prateleira D1 - Pintura e Acabamento" },
                    { 8, new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Prateleira D2 - Hidráulica" },
                    { 9, new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Prateleira E1 - Jardinagem" },
                    { 10, new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Prateleira E2 - Estoque Geral" }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "User",
                columns: new[] { "id", "UseFullName", "UsePassword", "UseRefreshToken", "UseRefreshTokenExpiryTime", "UseType", "UseUsername" },
                values: new object[] { 1L, "Administrador do Sistema", "240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9", null, null, (byte)1, "admin" });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Product",
                columns: new[] { "ProId", "ProDateRegister", "ProDescription", "ProEmpId", "ProImage", "ProName", "ProQntd" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Furadeira de impacto com mandril de 13mm, velocidade variável e reversão.", 1, new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 128, 0, 0, 0, 128, 8, 2, 0, 0, 0, 76, 92, 246, 156, 0, 0, 1, 33, 73, 68, 65, 84, 120, 218, 237, 221, 177, 9, 128, 48, 16, 64, 209, 108, 228, 56, 86, 142, 99, 237, 56, 238, 149, 9, 4, 7, 16, 139, 120, 119, 152, 39, 191, 149, 132, 123, 85, 32, 104, 91, 247, 83, 137, 53, 35, 0, 0, 224, 110, 217, 14, 133, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 48, 23, 0, 0, 4, 0, 128, 0, 0, 16, 0, 0, 2, 0, 64, 0, 0, 8, 0, 0, 1, 0, 32, 0, 0, 30, 235, 163, 159, 128, 65, 100, 173, 14, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 239, 1, 34, 119, 60, 106, 173, 130, 252, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 144, 8, 144, 59, 29, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 248, 43, 64, 228, 91, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 48, 15, 64, 228, 33, 25, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 84, 190, 27, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 190, 150, 82, 231, 2, 104, 214, 198, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 192, 255, 3, 130, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 152, 11, 0, 0, 2, 0, 64, 0, 0, 8, 0, 0, 1, 0, 32, 0, 0, 4, 0, 128, 0, 0, 16, 0, 0, 250, 14, 64, 41, 1, 0, 48, 119, 23, 151, 196, 101, 55, 40, 70, 191, 79, 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130 }, "Furadeira de Impacto 750W", 24 },
                    { 2, new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Parafusadeira a bateria com duas velocidades, maleta e duas baterias de lítio.", 1, new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 128, 0, 0, 0, 128, 8, 2, 0, 0, 0, 76, 92, 246, 156, 0, 0, 1, 35, 73, 68, 65, 84, 120, 218, 237, 220, 209, 9, 128, 48, 12, 64, 193, 238, 213, 113, 186, 151, 227, 56, 80, 39, 16, 28, 64, 68, 106, 26, 235, 149, 247, 43, 106, 238, 43, 32, 150, 189, 53, 77, 172, 24, 1, 0, 0, 103, 91, 173, 10, 11, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 96, 46, 0, 0, 8, 0, 0, 1, 0, 32, 0, 0, 4, 0, 128, 0, 0, 16, 0, 0, 2, 0, 64, 0, 0, 92, 214, 71, 159, 81, 111, 155, 240, 193, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 224, 62, 64, 192, 188, 158, 221, 43, 237, 131, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 64, 54, 128, 185, 75, 50, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 172, 10, 16, 121, 85, 158, 41, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 192, 219, 0, 145, 75, 242, 39, 166, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 107, 127, 27, 154, 118, 163, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 85, 255, 150, 210, 115, 28, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 136, 4, 208, 228, 77, 88, 0, 0, 8, 0, 0, 1, 0, 32, 0, 0, 4, 0, 128, 0, 0, 16, 0, 0, 2, 0, 64, 0, 0, 8, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 230, 5, 0, 192, 191, 59, 0, 40, 224, 31, 9, 35, 139, 65, 114, 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130 }, "Parafusadeira sem Fio 12V", 18 },
                    { 3, new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Serra circular com disco de 184mm e guia paralela para cortes retos.", 2, new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 128, 0, 0, 0, 128, 8, 2, 0, 0, 0, 76, 92, 246, 156, 0, 0, 1, 44, 73, 68, 65, 84, 120, 218, 237, 221, 193, 13, 128, 32, 16, 0, 65, 58, 179, 7, 155, 176, 7, 202, 177, 68, 43, 48, 177, 0, 227, 3, 238, 64, 198, 236, 215, 128, 204, 139, 4, 181, 28, 103, 85, 98, 197, 18, 0, 0, 240, 180, 213, 93, 97, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 172, 11, 0, 0, 2, 0, 64, 0, 0, 8, 0, 0, 1, 0, 32, 0, 0, 4, 0, 128, 0, 0, 16, 0, 0, 175, 93, 173, 175, 220, 209, 187, 206, 25, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 190, 3, 180, 90, 175, 209, 198, 10, 30, 29, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 18, 1, 2, 54, 201, 3, 26, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 64, 111, 128, 200, 187, 236, 132, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 176, 14, 64, 228, 38, 57, 247, 21, 187, 65, 207, 134, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 96, 61, 128, 220, 231, 156, 235, 208, 42, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 252, 224, 107, 41, 115, 77, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 252, 63, 32, 56, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 117, 1, 0, 64, 0, 0, 8, 0, 0, 1, 0, 32, 0, 0, 4, 0, 128, 0, 0, 16, 0, 0, 2, 0, 64, 253, 0, 148, 18, 0, 0, 107, 119, 3, 136, 165, 60, 228, 173, 192, 131, 29, 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130 }, "Serra Circular 1400W", 9 },
                    { 4, new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Martelo com cabeça de aço forjado e cabo de fibra de vidro antiderrapante.", 2, new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 128, 0, 0, 0, 128, 8, 2, 0, 0, 0, 76, 92, 246, 156, 0, 0, 1, 36, 73, 68, 65, 84, 120, 218, 237, 220, 203, 9, 128, 48, 16, 64, 193, 244, 99, 57, 222, 44, 199, 163, 229, 88, 90, 42, 8, 88, 128, 136, 152, 221, 72, 38, 188, 171, 31, 118, 114, 17, 212, 114, 238, 171, 18, 43, 70, 0, 0, 192, 213, 177, 45, 10, 11, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 96, 46, 0, 0, 8, 0, 0, 1, 0, 32, 0, 0, 4, 0, 128, 0, 0, 16, 0, 0, 2, 0, 64, 0, 0, 220, 86, 191, 94, 185, 51, 234, 122, 99, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 240, 28, 32, 114, 94, 1, 215, 202, 218, 25, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 144, 8, 16, 48, 130, 119, 231, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 95, 3, 4, 28, 21, 185, 51, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 96, 16, 128, 220, 17, 212, 49, 22, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 120, 55, 212, 71, 122, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 240, 183, 148, 222, 207, 207, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 48, 15, 128, 0, 0, 16, 0, 0, 2, 0, 64, 0, 0, 8, 0, 0, 1, 0, 32, 0, 0, 4, 0, 128, 0, 0, 16, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 35, 0, 40, 37, 0, 0, 230, 174, 1, 226, 104, 19, 47, 66, 190, 210, 2, 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130 }, "Martelo Unha 27mm", 40 },
                    { 5, new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Conjunto com seis chaves de fenda e philips com cabo emborrachado.", 3, new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 128, 0, 0, 0, 128, 8, 2, 0, 0, 0, 76, 92, 246, 156, 0, 0, 1, 42, 73, 68, 65, 84, 120, 218, 237, 221, 209, 13, 64, 48, 20, 64, 81, 155, 25, 196, 0, 6, 49, 128, 113, 236, 101, 2, 137, 1, 144, 168, 247, 90, 61, 114, 127, 165, 213, 243, 213, 144, 26, 150, 121, 83, 98, 131, 37, 0, 0, 224, 108, 26, 87, 133, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 176, 46, 0, 0, 8, 0, 0, 1, 0, 32, 0, 0, 4, 0, 128, 0, 0, 16, 0, 0, 2, 0, 64, 0, 0, 92, 182, 151, 190, 114, 71, 255, 116, 206, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 240, 28, 160, 212, 122, 21, 31, 43, 114, 98, 153, 59, 97, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 120, 253, 156, 165, 54, 156, 21, 110, 119, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 32, 17, 32, 242, 174, 128, 218, 123, 37, 9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 159, 2, 68, 110, 146, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 64, 87, 223, 134, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 64, 36, 64, 181, 167, 165, 180, 53, 49, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 240, 255, 128, 102, 78, 75, 17, 0, 0, 2, 0, 64, 0, 0, 8, 0, 0, 1, 0, 32, 0, 0, 4, 0, 128, 0, 0, 16, 0, 0, 2, 0, 64, 0, 0, 8, 0, 0, 1, 0, 32, 0, 0, 116, 7, 160, 148, 0, 0, 232, 187, 3, 19, 121, 27, 85, 72, 247, 198, 88, 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130 }, "Jogo de Chaves de Fenda", 35 },
                    { 6, new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Medidor de distância a laser com precisão de 2mm e cálculo de área.", 3, new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 128, 0, 0, 0, 128, 8, 2, 0, 0, 0, 76, 92, 246, 156, 0, 0, 1, 35, 73, 68, 65, 84, 120, 218, 237, 220, 177, 9, 128, 48, 16, 64, 81, 55, 113, 28, 199, 177, 115, 21, 59, 135, 204, 4, 130, 3, 136, 69, 184, 187, 196, 23, 126, 43, 145, 123, 85, 136, 184, 108, 231, 165, 196, 22, 35, 0, 0, 224, 105, 221, 15, 133, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 48, 23, 0, 0, 4, 0, 128, 0, 0, 16, 0, 0, 2, 0, 64, 0, 0, 8, 0, 0, 1, 0, 32, 0, 0, 94, 107, 189, 87, 192, 32, 178, 94, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 190, 3, 68, 206, 171, 215, 94, 185, 198, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 80, 13, 160, 215, 89, 180, 224, 148, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 32, 17, 32, 224, 169, 22, 184, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 160, 8, 64, 228, 33, 121, 244, 187, 82, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 152, 224, 219, 80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 152, 21, 96, 196, 191, 165, 76, 117, 37, 9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 64, 0, 0, 8, 0, 0, 1, 0, 32, 0, 0, 4, 0, 128, 0, 0, 16, 0, 0, 2, 0, 64, 0, 0, 8, 0, 0, 1, 0, 32, 0, 0, 4, 0, 0, 0, 0, 197, 0, 148, 18, 0, 0, 255, 238, 6, 239, 183, 89, 94, 228, 211, 41, 174, 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130 }, "Trena a Laser 40 Metros", 12 },
                    { 7, new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Capacete de proteção classe B com carneira ajustável e certificado pelo CA.", 4, new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 128, 0, 0, 0, 128, 8, 2, 0, 0, 0, 76, 92, 246, 156, 0, 0, 1, 37, 73, 68, 65, 84, 120, 218, 237, 220, 177, 13, 128, 32, 16, 64, 81, 54, 113, 28, 183, 113, 28, 123, 199, 113, 29, 39, 48, 113, 0, 99, 1, 199, 33, 207, 252, 214, 64, 120, 213, 5, 99, 57, 247, 85, 29, 43, 142, 0, 0, 128, 167, 99, 91, 20, 22, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 192, 185, 0, 0, 32, 0, 0, 4, 0, 128, 0, 0, 16, 0, 0, 2, 0, 64, 0, 0, 8, 0, 0, 1, 0, 240, 218, 85, 251, 233, 187, 122, 211, 61, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 192, 119, 128, 90, 231, 149, 109, 173, 224, 141, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 64, 71, 128, 128, 33, 57, 33, 63, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 180, 6, 136, 124, 107, 136, 97, 27, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 170, 0, 68, 14, 201, 163, 95, 109, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 192, 191, 191, 13, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 161, 1, 210, 254, 45, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 38, 4, 16, 0, 0, 2, 0, 64, 0, 0, 8, 0, 0, 1, 0, 32, 0, 0, 4, 0, 128, 0, 0, 16, 0, 0, 2, 0, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 32, 3, 128, 186, 4, 0, 192, 220, 221, 55, 155, 253, 68, 175, 156, 54, 173, 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130 }, "Capacete de Segurança Branco", 60 },
                    { 8, new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Par de luvas revestidas em nitrilo para manuseio de peças e ferramentas.", 4, new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 128, 0, 0, 0, 128, 8, 2, 0, 0, 0, 76, 92, 246, 156, 0, 0, 1, 28, 73, 68, 65, 84, 120, 218, 237, 220, 193, 9, 128, 48, 12, 64, 209, 46, 228, 56, 94, 93, 192, 61, 92, 192, 77, 59, 129, 224, 0, 34, 82, 146, 96, 159, 252, 171, 88, 251, 78, 65, 105, 59, 215, 67, 137, 53, 91, 0, 0, 192, 221, 190, 108, 10, 11, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 96, 95, 0, 0, 16, 0, 0, 2, 0, 64, 0, 0, 8, 0, 0, 1, 0, 32, 0, 0, 4, 0, 128, 0, 0, 120, 172, 143, 190, 70, 189, 109, 193, 133, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 192, 123, 128, 128, 253, 250, 246, 172, 178, 11, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 106, 0, 163, 102, 81, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 144, 117, 151, 79, 146, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 152, 7, 32, 114, 72, 54, 9, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 127, 67, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 192, 105, 41, 142, 171, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 192, 239, 1, 4, 0, 128, 0, 0, 16, 0, 0, 2, 0, 64, 0, 0, 8, 0, 0, 1, 0, 32, 0, 0, 4, 0, 128, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 168, 0, 160, 148, 0, 0, 152, 187, 11, 0, 152, 159, 156, 175, 251, 6, 195, 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130 }, "Luva de Proteção Nitrílica", 150 },
                    { 9, new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Óculos de segurança com lente antirrisco e proteção contra impactos.", 5, new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 128, 0, 0, 0, 128, 8, 2, 0, 0, 0, 76, 92, 246, 156, 0, 0, 1, 35, 73, 68, 65, 84, 120, 218, 237, 220, 203, 9, 128, 64, 12, 64, 193, 109, 202, 38, 236, 193, 10, 188, 217, 138, 157, 90, 129, 96, 1, 34, 18, 147, 128, 35, 239, 42, 171, 153, 211, 226, 103, 44, 219, 174, 194, 134, 17, 0, 0, 112, 53, 205, 171, 210, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 152, 11, 0, 0, 2, 0, 64, 0, 0, 8, 0, 0, 1, 0, 32, 0, 0, 4, 0, 128, 0, 0, 16, 0, 0, 183, 29, 209, 71, 212, 221, 54, 188, 48, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 120, 14, 144, 48, 175, 119, 107, 181, 189, 48, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 232, 6, 16, 181, 23, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 85, 103, 37, 60, 146, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 38, 0, 153, 155, 228, 218, 79, 236, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 192, 187, 161, 61, 167, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 254, 150, 82, 178, 58, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 124, 7, 32, 0, 0, 4, 0, 128, 0, 0, 16, 0, 0, 2, 0, 64, 0, 0, 8, 0, 0, 1, 0, 32, 0, 0, 4, 0, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 64, 7, 0, 149, 4, 0, 192, 191, 59, 1, 23, 98, 240, 184, 58, 104, 92, 238, 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130 }, "Óculos de Proteção Incolor", 90 },
                    { 10, new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Fita isolante antichama de 19mm por 20 metros para emendas elétricas.", 5, new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 128, 0, 0, 0, 128, 8, 2, 0, 0, 0, 76, 92, 246, 156, 0, 0, 1, 36, 73, 68, 65, 84, 120, 218, 237, 221, 177, 13, 128, 32, 16, 64, 81, 54, 114, 4, 199, 112, 4, 119, 112, 28, 71, 100, 2, 19, 123, 27, 133, 59, 34, 143, 252, 214, 160, 60, 27, 18, 137, 229, 220, 55, 37, 86, 44, 1, 0, 0, 119, 199, 186, 40, 44, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 117, 1, 0, 64, 0, 0, 8, 0, 0, 1, 0, 32, 0, 0, 4, 0, 128, 0, 0, 16, 0, 0, 2, 0, 224, 109, 245, 121, 4, 60, 109, 109, 61, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 96, 16, 128, 172, 135, 137, 55, 254, 62, 23, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 252, 96, 39, 220, 124, 138, 174, 239, 19, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 76, 11, 16, 121, 21, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 204, 3, 16, 185, 73, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 190, 13, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 67, 122, 67, 157, 223, 203, 186, 49, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 240, 255, 128, 224, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 214, 5, 0, 0, 1, 0, 32, 0, 0, 4, 0, 128, 0, 0, 16, 0, 0, 2, 0, 64, 0, 0, 8, 0, 0, 245, 3, 80, 74, 0, 0, 204, 221, 5, 131, 182, 120, 137, 62, 84, 33, 162, 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130 }, "Fita Isolante 20 Metros", 200 }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Enter",
                columns: new[] { "EntId", "EntDateEnter", "EntPrice", "EntProId", "EntQntd", "EntSheId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc), 289.90m, 1, 24, 1 },
                    { 2, new DateTime(2026, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc), 349.00m, 2, 18, 1 },
                    { 3, new DateTime(2026, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc), 529.90m, 3, 9, 1 },
                    { 4, new DateTime(2026, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc), 45.50m, 4, 40, 2 },
                    { 5, new DateTime(2026, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc), 79.90m, 5, 35, 2 },
                    { 6, new DateTime(2026, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc), 219.00m, 6, 12, 6 },
                    { 7, new DateTime(2026, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc), 32.90m, 7, 60, 3 },
                    { 8, new DateTime(2026, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc), 12.40m, 8, 150, 3 },
                    { 9, new DateTime(2026, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc), 18.75m, 9, 90, 3 },
                    { 10, new DateTime(2026, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc), 8.90m, 10, 200, 4 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Enter",
                keyColumn: "EntId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Enter",
                keyColumn: "EntId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Enter",
                keyColumn: "EntId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Enter",
                keyColumn: "EntId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Enter",
                keyColumn: "EntId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Enter",
                keyColumn: "EntId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Enter",
                keyColumn: "EntId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Enter",
                keyColumn: "EntId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Enter",
                keyColumn: "EntId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Enter",
                keyColumn: "EntId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Shelf",
                keyColumn: "SheId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Shelf",
                keyColumn: "SheId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Shelf",
                keyColumn: "SheId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Shelf",
                keyColumn: "SheId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Shelf",
                keyColumn: "SheId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "User",
                keyColumn: "id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Shelf",
                keyColumn: "SheId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Shelf",
                keyColumn: "SheId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Shelf",
                keyColumn: "SheId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Shelf",
                keyColumn: "SheId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Shelf",
                keyColumn: "SheId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Employee",
                keyColumn: "EmpId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Employee",
                keyColumn: "EmpId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Employee",
                keyColumn: "EmpId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Employee",
                keyColumn: "EmpId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Employee",
                keyColumn: "EmpId",
                keyValue: 5);
        }
    }
}
