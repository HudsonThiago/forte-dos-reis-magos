using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Entities
{
    public interface IScroller
    {
        public int field { get; set; }
        public int prevField { get; set; }
        public int totalFields { get; set; }
        public int scroller(float field); 
    }
}
