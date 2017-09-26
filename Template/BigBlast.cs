using SharpDX;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project4
{
    class BigBlast : Blast
    {

        public BigBlast( Vector3 pos, Vector3 vel, Texture2D laser )
        {
            position = pos;
            velocity = vel*2;
            laserImage = laser;
            timeToLive = 5;
            isAlive = true;
        }

        internal bool HitsRock(Asteroid asteroid)
        {
            //Is this the right distance? @author Brent Lefever
            float distance = Vector3.Distance(position, asteroid.position);
            BoundingSphere rockBound = asteroid.model.CalculateBounds();
            rockBound.Radius *= asteroid.size; //scale
            rockBound.Radius /= 3; //seems to be a good scale!?
            if (distance < 50 + rockBound.Radius)
                return true;
            return false;
        }
    }
}
