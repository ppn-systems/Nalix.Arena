using Microsoft.EntityFrameworkCore;
using Nalix.Game.Shared.Security;

namespace Nalix.Infrastructure.Database;

public interface IGameDbContext
{
    DbSet<Credentials> Accounts { get; set; }
}