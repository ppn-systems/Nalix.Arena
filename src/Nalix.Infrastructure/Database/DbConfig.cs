using Nalix.Common.Environment;
using Nalix.Framework.Configuration.Binding;
using System.IO;

namespace Nalix.Infrastructure.Database;

public sealed class DbConfig : ConfigurationLoader
{
    public System.String Provider { get; set; } = "SQLite";
    public System.String ConnectionString { get; set; } =
            $"Data Source={Path.Combine(Directories.DatabaseDirectory, "nalix.db")}";
}
