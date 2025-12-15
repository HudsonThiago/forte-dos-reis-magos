using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.Entities
{
    public interface INavigation : IScroller
    {
        public void navigation(Vector2 value);
    }
}
