using SharpDX;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project4
{
    class Missile : Blast
    {
        public Asteroid target;

        public Missile( Texture2D missileTexture, Asteroid mark)
        {
            laserImage = missileTexture;
            target = mark;
            velocity = Vector3.Normalize(mark.position) * Asteroids.BLAST_SPEED / 2; //send it at the target
        }

        public void Update(float time, Vector3 shipPosition)
        {
            velocity = Vector3.Normalize(target.position - position) * Asteroids.BLAST_SPEED;
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
    }
}
