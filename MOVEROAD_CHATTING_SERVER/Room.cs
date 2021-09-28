using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOVEROAD_CHATTING_SERVER
{
    public class Room
    {
        public int index { get; set; }
        public int userid { get; set; }
        public int toid { get; set; }
        public Room(int index, int userid, int toid)
        {
            this.index = index;
            this.userid = userid;
            this.toid = toid;
        }
    }
}
