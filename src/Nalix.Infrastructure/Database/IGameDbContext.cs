using Microsoft.EntityFrameworkCore;
using Nalix.NetCore.Security;

namespace Nalix.Infrastructure.Database;

public interface IGameDbContext
{
    DbSet<Credentials> Accounts { get; set; }
}