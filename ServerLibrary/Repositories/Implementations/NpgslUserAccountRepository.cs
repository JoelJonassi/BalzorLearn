using BaseLibrary.Dtos;
using BaseLibrary.Responses;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Npgsql;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;


namespace ServerLibrary.Repositories.Implementations
{
    public class NpgslUserAccountRepository : IUserAccount
    {

   

        public async Task<GeneralResponse> CreateAsync2(Register user)
        {
            try
            {
                using (var connection = NpgsqlDbContext.DBConnection())
                {
                    await connection.OpenAsync();
                    string query = $"Select registar_utilizador(@nome_utilizador, @abv_nome, @email, @password, @id_area, @activeCol);";

                    var command = new NpgsqlCommand(query, connection);
                    command.Parameters.AddWithValue("@nome_utilizador", user.FullName);
                    command.Parameters.AddWithValue("@abv_nome", user.ShortName);
                    command.Parameters.AddWithValue("@email", user.Email);
                    command.Parameters.AddWithValue("@password", user.Password);
                    command.Parameters.AddWithValue("@id_area", user.IdArea);
                    command.Parameters.AddWithValue("@activeCol", user.Active);
                    var result = await command.ExecuteScalarAsync();
                    connection.Close();
                    bool? resultBool = result as bool?;
                    return resultBool == true ? new GeneralResponse(true, "Account created") : new  GeneralResponse(false, "Account not created");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<GeneralResponse> CreateAsync(Register user)
        {
            try
            {
                var instance = new PostgreSQLTools(NpgsqlDbContext.GetConnectionString());
                NpgsqlParameter[] parameters = new NpgsqlParameter[] {
                    new NpgsqlParameter("nome_utilizador", user.FullName), //"@nome_utilizador"
                    new NpgsqlParameter("abv_nome", user.ShortName),
                    new NpgsqlParameter("email", user.Email),
                    new NpgsqlParameter("password", user.Password),
                    new NpgsqlParameter("id_area", user.IdArea),
                    new NpgsqlParameter("activeCol", user.Active)
                };
                int? resultBool = instance.SetDbDataScalar("registar_utilizador", parameters);
                return resultBool == 1 ? new GeneralResponse(true, "Account created") : new GeneralResponse(false, "Account not created");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Task<LoginResponse> RefreshTokenAsyn(RefreshToken token)
        {
            throw new NotImplementedException();
        }

        public Task<LoginResponse> SingInAsync(Login user)
        {
            throw new NotImplementedException();
        }

       
    }
}