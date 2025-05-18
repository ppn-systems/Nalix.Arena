using Microsoft.EntityFrameworkCore;
using Nalix.Game.Shared.Security;

namespace Nalix.Game.Infrastructure.Database;

public interface IGameDbContext
{
    DbSet<Credentials> Accounts { get; set; }
}