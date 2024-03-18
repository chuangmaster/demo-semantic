using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace demo_semantic.Plugin.DateTimePlugin
{
    public class DateTimePlugin
    {

        [KernelFunction("GetCurrentUtcDateTime")]
        [Description("Get the current date and time at UTC+0")]
        public string GetCurrentUtcDateTime()
        {
            return DateTime.UtcNow.ToString();
        }


        [KernelFunction("GetCurrentDateOfWeek")]
        [Description("Get the current day of week base on UTC+0")]
        public string GetCurrentDateOfWeek()
        {
            return DateTime.UtcNow.DayOfWeek.ToString();
        }
    }

}