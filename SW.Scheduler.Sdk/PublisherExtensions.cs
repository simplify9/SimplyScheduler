using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SW.PrimitiveTypes;

namespace SW.Scheduler.Model
{
    public static class PublisherExtensions
    {
        public static ScheduleMessage GetScheduleMessage(this object o, string scheduleId, string scheduleCron)
            => new ScheduleMessage
            {
                Id = scheduleId,
                Delete = false,
                Schedule = scheduleCron,
                MessageSerialized = JsonConvert.SerializeObject(o),
                MessageTypeName = o.GetType().Name
            };

        public static ScheduleMessage GetDeleteScheduleMessage(this Type messageType, string scheduleId)
            => new ScheduleMessage
            {
                Id = scheduleId,
                Delete = true,
                MessageTypeName = messageType.Name
            };

        
        public static Task Schedule<T>(this IPublish publisher, T message, string scheduleId, string scheduleCron) =>
            publisher.Publish(message.GetScheduleMessage(scheduleId, scheduleCron));
        
        public static Task DeleteSchedule<T>(this IPublish publisher, string scheduleId) =>
            publisher.Publish(typeof(T).GetDeleteScheduleMessage(scheduleId));

    }
}