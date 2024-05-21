using BaseLibrary.Dtos;
using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Constants = ServerLibrary.Helpers.Constants;

namespace ServerLibrary.Repositories.Implementations
{
    public class UserAccountRespositiory(IOptions<JwtSection> config, AppDbContext appDbContext) : IUserAccount
    {
        public async Task<GeneralResponse> CreateAsync(Register user)
        {
            if (user == null) return new GeneralResponse(false, "Model is empty");
            var checkuser = await FindUserByEmail(user.Email);
            if (checkuser != null) return new GeneralResponse(false, "User registered already");

            var applicationsUser = await AddToDatabase(new ApplicationUser()
            {
                FullName = user.FullName,
                Email = user.Email,

                Password = BCrypt.Net.BCrypt.HashPassword(user.Password)
            });

            var checkAdminRole = await appDbContext.SystemRoles.FirstOrDefaultAsync(role => role.Name!.Equals(Constants.Admin));
            if(checkAdminRole is null)
            {
                var createAdminRole = await AddToDatabase(new SystemRole() { Name = Constants.Admin });
                await AddToDatabase(new UserRole() { RoleId = createAdminRole.Id, UserId = applicationsUser.Id });
                return new GeneralResponse(true, "Account created");
            }

            var checkUserRole = await appDbContext.SystemRoles.FirstOrDefaultAsync(role => role.Name!.Equals(Constants.User));
            SystemRole response = new();
            if (checkUserRole is null)
            {
                var createUserRole = await AddToDatabase(new SystemRole() { Name = Constants.User });
                await AddToDatabase(new UserRole() { RoleId = response.Id, UserId = applicationsUser.Id });          
            }
            else
            {
                await AddToDatabase(new UserRole() { RoleId = checkUserRole.Id, UserId = applicationsUser.Id });

            }
            return new GeneralResponse(true, "Account created");
        }

        /// <summary>
        /// Method to perdom signin
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<LoginResponse> SingInAsync(Login user)
        {
            if (user is null) return new LoginResponse(false, "Model empty");
            var applicationUser = await FindUserByEmail(user.Email);
            if (applicationUser is null) return new LoginResponse(false, "User not found");

            if (!BCrypt.Net.BCrypt.Verify(user.Password, applicationUser.Password))
                return new LoginResponse(false, "Invalid Credentials");
            var getUserRole = await FindUserRole(applicationUser.Id);
            if (getUserRole is null) return new LoginResponse(false, "User role not found");

            var getRoleName = await FindRoleName(getUserRole.RoleId);
            if (getUserRole is null) return new LoginResponse(false, "User role nof found");

            string jwtToken = GenerateTokenJWT(applicationUser, getRoleName!.Name);
            string refreshToken = GenerateRefreshToken();

            var findUser = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(refreshToken => refreshToken.UserId!.Equals(applicationUser.Id));
            if (findUser is not null)
            {
                findUser!.Token = refreshToken;
                await appDbContext.SaveChangesAsync();
            }
            else
            {
                await AddToDatabase(new RefreshTokenInfo() { Token = refreshToken, UserId = applicationUser.Id });
            }
            return new LoginResponse(true, "Login successfully", jwtToken, refreshToken);

        }



        private static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        private async Task<ApplicationUser> FindUserByEmail(string email) =>
            await appDbContext.ApplicationUser.FirstOrDefaultAsync(x => x.Email!.ToLower()!.Equals(email!.ToLower()));
    
        
        private async Task<T> AddToDatabase<T>(T model)
        {
            var result = appDbContext.Add(model!);
            await appDbContext.SaveChangesAsync();
            return (T)result.Entity;
        }

        /// <summary>
        /// Method to generate token according to user data
        /// </summary>
        /// <param name="idUser"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        private string GenerateTokenJWT(ApplicationUser user, string role)
        {
  
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Value.Key!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: config.Value.Issuer,
                audience: config.Value.Audience,
                expires: DateTime.Now.AddSeconds(2),
                claims: new Claim[]
                {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FullName!),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, role)
                },
                signingCredentials: credentials);
  
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<UserRole> FindUserRole(int userId) =>
            await appDbContext.UserRoles.FirstOrDefaultAsync(userRole => userRole.UserId == userId);


        private async Task<SystemRole> FindRoleName(int roleId) =>
            await appDbContext.SystemRoles.FirstOrDefaultAsync(userRole => userRole.Id == roleId);

        /// <summary>
        /// First time the user login or create the account the refresh token have to be kept
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<LoginResponse> RefreshTokenAsyn(RefreshToken token)
        {
            if (token is null) return new LoginResponse(false, "Model Empty");
            var findToken = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(refreshToken => refreshToken.Token!.Equals(token.Token));

            
            var user = findToken != null ?  await appDbContext.ApplicationUser.FirstOrDefaultAsync(appUser => appUser.Id == findToken!.UserId) : null;
            if (user is null) return new LoginResponse(false, "Refresh token could not be generated because user not found");
            
            var userRole = await FindUserRole(user.Id);
            var roleName = await FindRoleName(userRole.RoleId);
            var jwtToken = GenerateTokenJWT(user, roleName.Name);
            var refreshToken = GenerateRefreshToken();

            var updateRefreshToken = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(refreshToken => refreshToken.UserId == user.Id);
            if (updateRefreshToken is null) return new LoginResponse(false, "Refresh token could not be generated because user has not sign");

            updateRefreshToken.Token = refreshToken;
            await appDbContext.SaveChangesAsync();
            return new LoginResponse(true, "Token refreshed successfully", jwtToken, refreshToken);
                
        }

    }
}
