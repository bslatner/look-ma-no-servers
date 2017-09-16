using System;

namespace NoServers.DataAccess
{
    public class Event
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsActive { get; set; }
    }
}