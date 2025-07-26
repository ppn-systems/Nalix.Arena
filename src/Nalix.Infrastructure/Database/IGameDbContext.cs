using Microsoft.EntityFrameworkCore;
using Nalix.CrossPlatform.Security;

namespace Nalix.Infrastructure.Database;

public interface IGameDbContext
{
    DbSet<Credentials> Accounts { get; set; }
}