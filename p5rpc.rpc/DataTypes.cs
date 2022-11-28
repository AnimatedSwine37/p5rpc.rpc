using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p5rpc.rpc
{
    public enum TimeOfDay
    {
        Morning,
        Lunchtime,
        Afternoon,
        Daytime,
        After_School,
        Evening,
        Late_Night
    }

    public enum PartyMember
    {
        None,
        Joker,
        Skull,
        Mona,
        Panther,
        Fox,
        Queen,
        Noir,
        Oracle,
        Crow,
        Violet
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
