using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace demo_semantic.Plugins.BenefitRequestPlugin
{
    public class BenefitRequestPlugin
    {
        [KernelFunction]
        [Description("取得員工所擁有點數")]
        public decimal GetBenefit([Description("員工編號")] string empId)
        {
            decimal result = empId switch
            {
                "001" => 100,
                "002" => 200,
                "003" => 500,
                _ => 0
            };
            return result;
        }

        [KernelFunction]
        [Description("進行點數申請")]
        public bool DoBenefitApply([Description("員工編號")] string empId, [Description("活動項目")] string activity, [Description("使用點數")] decimal points)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{empId}申請活動[{activity}]，使用點數{points}");
            return true;
        }

        [KernelFunction]
        [Description("取得今天日期")]
        public DateTime GetCurrentDate()
        {
            return DateTime.Now;
        }
    }
}