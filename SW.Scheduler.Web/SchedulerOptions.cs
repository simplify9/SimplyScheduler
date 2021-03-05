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
        public string Path { get; set; } = "";

        public string CustomCssUrl { get; set; }

        public bool LazyInit { get; set; }

        public int TimelineSpanInMinutes { get; set; } = 60;
    }
}