using SharpDX;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project4
{
    class Blast
    {
        public Texture2D laserImage;
        public Vector3 position;
        public Vector3 velocity;
        public float timeToLive;
        public bool isAlive;

        public Blast()
        {
            position = Vector3.Zero;
            velocity = Vector3.Zero;
            timeToLive = 30;
            isAlive = true;
        }

        public Blast( Vector3 pos, Vector3 vel, Texture2D laser )
        {
            position = pos;
            velocity = vel;
            laserImage = laser;
            timeToLive = 6;
            isAlive = true;
        }
        
        public void Update( float time, Vector3 shipPosition)
        {
            position += velocity * time;
            position -= shipPosition;

            timeToLive -= time;
            if (timeToLive <= 0)
                isAlive = false;

            //wrap-around toroidal physics
            if (position.X > Asteroids.RANGE)
                position.X = -Asteroids.RANGE;
            else if (position.X < -Asteroids.RANGE)
                position.X = Asteroids.RANGE;

            if (position.Y > Asteroids.RANGE)
                position.Y = -Asteroids.RANGE;
            else if (position.Y < -Asteroids.RANGE)
                position.Y = Asteroids.RANGE;

            if (position.Z > Asteroids.RANGE)
                position.Z = -Asteroids.RANGE;
            else if (position.Z < -Asteroids.RANGE)
                position.Z = Asteroids.RANGE;
        }

        internal bool HitsRock(Asteroid asteroid)
        {
            //Is this the right distance? @author Brent Lefever
            float distance = Vector3.Distance(position, asteroid.position);
            BoundingSphere rockBound = asteroid.model.CalculateBounds();
            rockBound.Radius *= asteroid.size; //scale
            rockBound.Radius /= 3; //seems to be a good scale!?
            if (distance < 1 + rockBound.Radius)
                return true;
            return false;
        }
    }
}
