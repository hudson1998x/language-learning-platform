using LLE.Kernel.Attributes;

namespace LLE.SQLiteAdapter.Configurations;

[Configuration("Developer", 1)]
public class SqliteConfiguration
{
    public string AppDbFile { get; set; } = "var/lle.db";
}
