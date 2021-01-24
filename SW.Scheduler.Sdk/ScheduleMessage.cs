namespace SW.Scheduler.Model
{
    public class ScheduleMessage
    {
        public string Id { get; set; }
        public string MessageTypeName { get; set; }
        public string MessageSerialized { get; set; }
        public string Schedule { get; set; }
        public bool Delete { get; set; }
    }
}