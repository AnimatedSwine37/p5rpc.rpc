using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace p5rpc.rpc
{
    internal unsafe class Sequence
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct SequenceInfo
        {
            internal int Field0 { get; }         
            internal SequenceType CurrentSequence { get; }
            internal SequenceType LastSequence { get; }
            internal int Field3 { get; }
            internal int Field4 { get; }
            internal int Field5 { get; }
            internal EventInfo* EventInfo { get; }
        }

        internal struct EventInfo
        {
            internal int Major { get; }
            internal int Minor { get; }
        }

        internal enum SequenceType : int
        {
            TITLE,
            TITLE_RAPID,
            LOAD,
            FIELD,
            BATTLE,
            FIELD_VIEWER,
            EVENT,
            EVENT_VIEWER,
            MOVIE,
            MOVIE_VIEWER,
            INIT_READ,
            CALENDAR,
            CALENDAR_RESET,
            DUNGEON_RESULT
        }
    }
}
