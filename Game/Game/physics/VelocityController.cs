using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.Entities;
using Microsoft.Xna.Framework;
using Vexillum.util;

namespace Vexillum.physics
{
    class VelocityController : PhysicsController
    {
        private Vec2 velocity;
        public VelocityController(Entity e, Vec2 velocity) : base(e)
        {
            this.velocity = velocity;
        }
        public override void run()
        {
            entity.Velocity = velocity;
        }
    }
}
