using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SW.Scheduler.Sdk
{
    
    public static class CronValidator
    {
    
        public static (bool valid,string issue) Validate(string cron)
        {
            var cronExpression = CultureInfo.InvariantCulture.TextInfo.ToUpper(cron);
            try
            {
                var _ =new CronExpression(cronExpression);
            }
            catch (Exception e)
            {
                return (false, e.Message);
            }
            
            return (true,null);
        }

    }
}