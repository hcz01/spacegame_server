using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spacegame_server
{
    internal class step
    {
        public step(long sec, float x, float y)
        {
            this.sec = sec;
            this.x = x;
            this.y = y;
        }
        public long sec { get; set; }    
        public float x { get; set; }
        public float y { get; set; }
        public string tostring()
        {
            return sec.ToString()+";"+x.ToString()+";"+y.ToString();
        }
    }
}
