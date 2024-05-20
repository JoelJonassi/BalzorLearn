
namespace ServerLibrary.Helpers
{
    /// <summary>
    /// Global class that handle exceptions messages.
    /// </summary>
    internal static class RuntimeMessages
    {
        internal static string Success { get { return "Operação realizada com sucesso!"; } }

        internal static string EmptyDataStruct { get { return "A estrutura de dados não contém informação!"; } }

        internal static string RowsAffected { get { return "A operação foi realizada em "; } }

        internal static string RollbackTransaction { get { return "Não foi possível realizar a operação e a informação não foi armazenada!\n\nPor favor repita a operação."; } }

        internal static string RollbackTransaction2 { get { return "Não foi possivel realizar a operação e a informação não foi armazenada!"; } }

        internal static string WrongLogin { get { return "O utilizador e/ou a password estão incorrectos."; } }

        internal static string NoLogin { get { return "Ainda não realizou o login."; } }

        internal static string NoData { get { return "Há dados em falta, por favor contacte o administrador do sistema."; } }

        internal static string NullParameter { get { return "Um ou mais parâmetros estão vazios ou nulos."; } }

        internal static string WrongIds { get { return "Os ID's fornecidos estão incorrectos ou não existem!"; } }

        internal static string InvalidDate { get { return "Escolha uma data que seja diferente da data actual!"; } }
        
        internal static string BiggerDate { get { return "Escolha uma data que igual ou superior à data actual!"; } }

        internal static string InvalidSession { get { return "A sessão do utilizador expirou ou não é válida!"; } }
        internal static string DuplicatedFile { get { return "O ficheiro fornecido é duplicado!"; } }
    }
}