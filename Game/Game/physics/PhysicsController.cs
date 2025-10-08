using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.Entities;

namespace Vexillum.physics
{
    public abstract class PhysicsController
    {
        protected Entity entity;
        public PhysicsController(Entity e)
        {
            entity = e;
        }
        public abstract void run();
    }
}
