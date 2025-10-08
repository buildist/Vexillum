using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.Entities;

namespace Vexillum.physics
{
    class ContactFilter
    {
        public bool shouldCollide(Entity e1, Entity e2)
        {
            if (e1 == null || e2 == null) return false;
            if (e1 is Projectile && e2 == ((Projectile)e1).GetOwner())
            {
                return false;
            }
            else if (e2 is Projectile && e1 == ((Projectile)e2).GetOwner())
            {
                return false;
            }
            else if(e1 is Rocket && e2 is Rocket)
                return false;
            else if (e1 is HumanoidEntity && e2 is GrapplingHook)
                return false;
            else if (e2 is HumanoidEntity && e1 is GrapplingHook)
                return false;
            else if (e1.isPlayer && e2.isPlayer)
                return false;
            else if (e1.disabled || e2.disabled || !e1.enablePhysics || !e2.enablePhysics)
                return false;
            else
                return true;
        }
    }
}
