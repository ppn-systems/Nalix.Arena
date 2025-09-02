using Microsoft.EntityFrameworkCore;
using Nalix.Communication.Models;

namespace Nalix.Infrastructure.Database;

public interface IGameDbContext
{
    DbSet<Credentials> Accounts { get; set; }
}