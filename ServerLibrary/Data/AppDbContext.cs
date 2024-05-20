using BaseLibrary.Entities;
using Microsoft.EntityFrameworkCore;

namespace ServerLibrary.Data {

    /// <summary>
    /// Method used for database tables creation
    /// </summary>
    /// <param name="options"></param>
    public class AppDbContext(DbContextOptions options) : DbContext(options){
           public DbSet<ApplicationUser> ApplicationUser {get; set;}
           public DbSet<SystemRole> SystemRoles {get; set;}
           public DbSet<UserRole> UserRoles {get; set;}
           public DbSet<RefreshTokenInfo> RefreshTokenInfos { get; set;} 
    }
}