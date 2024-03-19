using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace demo_semantic.Plugins.ReservePlugin
{
    public class BookMeetingRoom
    {
        [KernelFunction("GetFreeRoom")]
        [Description ("可以取得到目前未使用的會議室")]
        public string GetFreeRoom(DateTime queryDate)
        {
            Random rnd = new Random();
            var result = rnd.Next(10) + 1;
            var rooms = new List<ConferenceRoom>();
            for (int i = 1; i <= result; i++)
            {
                char c = (char)i;
                rooms.Add(new ConferenceRoom { RoomId = i, RoomName = "Room " + c, IsAvailable = true, StartTime = queryDate, EndTime = queryDate.AddHours(2) });
            }
            return JsonSerializer.Serialize(rooms);
        }
    }


    public class ConferenceRoom
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}