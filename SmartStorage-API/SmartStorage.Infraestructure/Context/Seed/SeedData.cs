using Microsoft.EntityFrameworkCore;
using SmartStorage.Shared.Enum;
using SmartStorage_Shared.Model;

namespace SmartStorage_API.Model.Context.Seed;

/// <summary>
/// Carga inicial aplicada via HasData, ou seja, junto com as migrations. Como o
/// servico migrator roda antes das APIs subirem, um banco novo ja nasce com um
/// administrador e um estoque de exemplo.
///
/// Duas regras valem para tudo aqui: as chaves primarias sao explicitas e as
/// datas sao constantes. Qualquer valor calculado em tempo de execucao (um
/// DateTime.Now, por exemplo) faria o EF enxergar diferenca no modelo a cada
/// "migrations add" e gerar um UPDATE novo sem que nada tivesse mudado.
/// </summary>
internal static class SeedData
{
    private static readonly DateTime CadastroFuncionarios = new(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime CadastroPrateleiras = new(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime CadastroProdutos = new(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime DataEntrada = new(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc);

    internal static void Seed(this ModelBuilder modelBuilder)
    {
        modelBuilder.SeedUsers();
        modelBuilder.SeedEmployees();
        modelBuilder.SeedShelves();
        modelBuilder.SeedProducts();
        modelBuilder.SeedEnters();
    }

    /// <summary>
    /// Administrador inicial: admin / admin123. A senha vai gravada ja com o
    /// hash produzido pelo Sha256PasswordHasher da AuthenticationAPI, que e um
    /// SHA-256 puro em hexadecimal - sem salt, portanto deterministico, o que e
    /// justamente o que permite fixar o valor aqui.
    /// </summary>
    private static void SeedUsers(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            Username = "admin",
            FullName = "Administrador do Sistema",
            Password = "240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9",
            UseType = TipoUsuario.Administrador
        });
    }

    private static void SeedEmployees(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>().HasData(
            new Employee { EmpId = 1, EmpName = "Ana Paula Ribeiro", EmpCpf = "52998224725", EmpRg = "MG1234567", EmpDateRegister = CadastroFuncionarios },
            new Employee { EmpId = 2, EmpName = "Bruno Carvalho Lima", EmpCpf = "11144477735", EmpRg = "SP2345678", EmpDateRegister = CadastroFuncionarios },
            new Employee { EmpId = 3, EmpName = "Carla Menezes Souza", EmpCpf = "39053344705", EmpRg = "RJ3456789", EmpDateRegister = CadastroFuncionarios },
            new Employee { EmpId = 4, EmpName = "Diego Ferreira Alves", EmpCpf = "16899535009", EmpRg = "MG4567890", EmpDateRegister = CadastroFuncionarios },
            new Employee { EmpId = 5, EmpName = "Eduarda Nogueira Pinto", EmpCpf = "40442820850", EmpRg = "SP5678901", EmpDateRegister = CadastroFuncionarios });
    }

    /// <summary>
    /// As dez prateleiras sao as categorias do estoque. Nem todas recebem
    /// produto no seed - as vazias existem para dar destino a cadastros novos.
    /// </summary>
    private static void SeedShelves(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Shelf>().HasData(
            new Shelf { SheId = 1, SheName = "Prateleira A1 - Ferramentas Elétricas", SheDataRegister = CadastroPrateleiras },
            new Shelf { SheId = 2, SheName = "Prateleira A2 - Ferramentas Manuais", SheDataRegister = CadastroPrateleiras },
            new Shelf { SheId = 3, SheName = "Prateleira B1 - Equipamentos de Proteção", SheDataRegister = CadastroPrateleiras },
            new Shelf { SheId = 4, SheName = "Prateleira B2 - Materiais Elétricos", SheDataRegister = CadastroPrateleiras },
            new Shelf { SheId = 5, SheName = "Prateleira C1 - Fixadores e Parafusos", SheDataRegister = CadastroPrateleiras },
            new Shelf { SheId = 6, SheName = "Prateleira C2 - Medição e Precisão", SheDataRegister = CadastroPrateleiras },
            new Shelf { SheId = 7, SheName = "Prateleira D1 - Pintura e Acabamento", SheDataRegister = CadastroPrateleiras },
            new Shelf { SheId = 8, SheName = "Prateleira D2 - Hidráulica", SheDataRegister = CadastroPrateleiras },
            new Shelf { SheId = 9, SheName = "Prateleira E1 - Jardinagem", SheDataRegister = CadastroPrateleiras },
            new Shelf { SheId = 10, SheName = "Prateleira E2 - Estoque Geral", SheDataRegister = CadastroPrateleiras });
    }

    private static void SeedProducts(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(
            Produto(1, "Furadeira de Impacto 750W", "Furadeira de impacto com mandril de 13mm, velocidade variável e reversão.", 24, 1),
            Produto(2, "Parafusadeira sem Fio 12V", "Parafusadeira a bateria com duas velocidades, maleta e duas baterias de lítio.", 18, 1),
            Produto(3, "Serra Circular 1400W", "Serra circular com disco de 184mm e guia paralela para cortes retos.", 9, 2),
            Produto(4, "Martelo Unha 27mm", "Martelo com cabeça de aço forjado e cabo de fibra de vidro antiderrapante.", 40, 2),
            Produto(5, "Jogo de Chaves de Fenda", "Conjunto com seis chaves de fenda e philips com cabo emborrachado.", 35, 3),
            Produto(6, "Trena a Laser 40 Metros", "Medidor de distância a laser com precisão de 2mm e cálculo de área.", 12, 3),
            Produto(7, "Capacete de Segurança Branco", "Capacete de proteção classe B com carneira ajustável e certificado pelo CA.", 60, 4),
            Produto(8, "Luva de Proteção Nitrílica", "Par de luvas revestidas em nitrilo para manuseio de peças e ferramentas.", 150, 4),
            Produto(9, "Óculos de Proteção Incolor", "Óculos de segurança com lente antirrisco e proteção contra impactos.", 90, 5),
            Produto(10, "Fita Isolante 20 Metros", "Fita isolante antichama de 19mm por 20 metros para emendas elétricas.", 200, 5));
    }

    private static Product Produto(int id, string nome, string descricao, int quantidade, int funcionarioId) => new()
    {
        ProId = id,
        ProName = nome,
        ProDescription = descricao,
        ProQntd = quantidade,
        ProEmpId = funcionarioId,
        ProDateRegister = CadastroProdutos,
        ProImage = SeedImages.ForProduct(id)
    };

    /// <summary>
    /// Nao existe FK de produto para prateleira: quem liga os dois e o Enter, a
    /// entrada do produto no estoque. Sem estas linhas as prateleiras ficariam
    /// criadas mas vazias, e nenhum produto apareceria categorizado.
    /// </summary>
    private static void SeedEnters(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Enter>().HasData(
            Entrada(1, produto: 1, prateleira: 1, quantidade: 24, preco: 289.90m),
            Entrada(2, produto: 2, prateleira: 1, quantidade: 18, preco: 349.00m),
            Entrada(3, produto: 3, prateleira: 1, quantidade: 9, preco: 529.90m),
            Entrada(4, produto: 4, prateleira: 2, quantidade: 40, preco: 45.50m),
            Entrada(5, produto: 5, prateleira: 2, quantidade: 35, preco: 79.90m),
            Entrada(6, produto: 6, prateleira: 6, quantidade: 12, preco: 219.00m),
            Entrada(7, produto: 7, prateleira: 3, quantidade: 60, preco: 32.90m),
            Entrada(8, produto: 8, prateleira: 3, quantidade: 150, preco: 12.40m),
            Entrada(9, produto: 9, prateleira: 3, quantidade: 90, preco: 18.75m),
            Entrada(10, produto: 10, prateleira: 4, quantidade: 200, preco: 8.90m));
    }

    private static Enter Entrada(int id, int produto, int prateleira, int quantidade, decimal preco) => new()
    {
        EntId = id,
        EntProId = produto,
        EntSheId = prateleira,
        EntQntd = quantidade,
        EntPrice = preco,
        EntDateEnter = DataEntrada
    };
}
