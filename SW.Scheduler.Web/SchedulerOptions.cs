namespace SW.Scheduler.Web
{
    public class SchedulerOptions
    {
        public const string ConfigurationSection = "Scheduler";

        public const string DatabaseTypeMsSql = "mssql";
        public const string DatabaseTypeMySql = "mysql";
        public const string DatabaseTypeSqlite = "sqlite";
        public const string DatabaseTypePgSql = "pgsql";

        public string DatabaseType { get; set; } = DatabaseTypePgSql;
        public int MaxConcurrency { get; set; } = 10;
    }
}