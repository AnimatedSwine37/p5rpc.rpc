using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p5rpc.rpc
{
    /// <summary>
    /// A class for getting data about fields and events to be displayed
    /// </summary>
    public class DataGetter
    {
        
    }


    public class Field
    {
        public Field()
        {
        }

        public int Major { get; set; }
        public int Minor { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string State { get; set; }
        public bool InMetaverse { get; set; } = false;
        public bool InBattle { get; set; }
        public string ImageKey { get; set; }

        public Field(int major, int minor, string name, string description, string state, bool inMetaverse, bool inBattle, string imageKey)
        {
            Major = major;
            Minor = minor;
            Name = name;
            Description = description;
            State = state;
            InMetaverse = inMetaverse;
            InBattle = inBattle;
            ImageKey = imageKey;
        }
    }

    public class Event
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string State { get; set; }
        public string ImageKey { get; set; }

        public Event()
        {
            
        }
        
        public Event(int major, int minor, string name, string description, string state, string imageKey)
        {
            Major = major;
            Minor = minor;
            Name = name;
            Description = description;
            State = state;
            ImageKey = imageKey;
        }
    }
}
