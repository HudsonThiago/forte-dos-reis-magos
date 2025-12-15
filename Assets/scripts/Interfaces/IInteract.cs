using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Entities
{
    public interface IInteraction : IScroller
    {
        public void interact(int value = 0);
    }
}
