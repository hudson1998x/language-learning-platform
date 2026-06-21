using LLE.Kernel.Attributes;

namespace LLE.SQLiteAdapter.Configurations;

[Configuration]
public class SqliteConfiguration
{
    public string AppDbFile { get; set; } = "var/lle.db";
}
