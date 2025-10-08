using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace  Vexillum.Entities
{
    public abstract class Projectile : BasicEntity
    {
        public abstract Entity GetOwner();
        public abstract void Setup(float angle, Entity owner);
    }
}
