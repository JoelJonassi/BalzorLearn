using BaseLibrary.Dtos;
using BaseLibrary.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLibrary.Repositories.Contracts
{
    public interface IUserAccount
    {
        Task<GeneralResponse> CreateAsync(Register user);
        Task<LoginResponse> SingInAsync(Login user);
        Task<LoginResponse> RefreshTokenAsyn(RefreshToken token);
    }
}
