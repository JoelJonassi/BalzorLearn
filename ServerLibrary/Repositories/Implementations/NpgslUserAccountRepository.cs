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
                    string query = $"Select registar_utilizador(@utilizador_nome, @nome_abv, @email_parameter, @password_parameter, @area_id, @active);";
    
                    var command = new NpgsqlCommand(query, connection);
                    command.Parameters.AddWithValue("@utilizador_nome", user.FullName);
                   // command.Parameters.AddWithValue("@nome_abv", user.ShortName);
                    command.Parameters.AddWithValue("@email_parameter", user.Email);
                    command.Parameters.AddWithValue("@password_parameter", user.Password);
                  //  command.Parameters.AddWithValue("@area_id", user.IdArea);
                   // command.Parameters.AddWithValue("@active", user.Active);
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
                    new NpgsqlParameter("utilizador_nome", user.FullName), //"@nome_utilizador"
                  //  new NpgsqlParameter("nome_abv", user.ShortName),
                    new NpgsqlParameter("email_parameter", user.Email),
                    new NpgsqlParameter("password_parameter", user.Password),
                   // new NpgsqlParameter("area_id", user.IdArea),
                   // new NpgsqlParameter("active", user.Active)
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