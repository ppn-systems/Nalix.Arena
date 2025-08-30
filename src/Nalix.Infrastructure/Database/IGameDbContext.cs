using Microsoft.EntityFrameworkCore;
using Nalix.Communication.Security;

namespace Nalix.Infrastructure.Database;

public interface IGameDbContext
{
    DbSet<Credentials> Accounts { get; set; }
}