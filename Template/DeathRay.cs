using SharpDX;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project4
{
    class DeathRay : Blast
    {

        public DeathRay( Vector3 pos, Vector3 vel, Texture2D laser )
        {
            position = pos;
            velocity = vel;
            laserImage = laser;
            timeToLive = 6;
            isAlive = true;
        }
    }
}
