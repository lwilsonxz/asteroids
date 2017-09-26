using SharpDX;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project4
{
    class Asteroid
    {
        public Vector3 position;
        public Vector3 velocity;
        public Model model;
        Texture2D texture; //model and texture are not static to support more types of asteroids
        public bool isAlive;
        public float size;  //.25 for smallest, .5 for medium, 1 for large
        public static Random random;
        public Vector3 angularRotations = Vector3.Zero; //each asteroid starts in the same rotational position
        public Vector3 angularRotationVelocities;
        public bool targeted;

        public Asteroid(Model rockModel, Texture2D rockTexture, Random generator)
        {
            random = generator;
            position = Vector3.Zero;
            velocity = new Vector3((float)random.NextDouble() * Asteroids.ASTEROID_SPEED, (float)random.NextDouble() * Asteroids.ASTEROID_SPEED, (float)random.NextDouble() * Asteroids.ASTEROID_SPEED);
            model = rockModel;
            texture = rockTexture;
            isAlive = true;
            size = 1;
            targeted = false;
        }

        public Asteroid( Model rockModel, Texture2D rockTexture )
        {
            this.RandomizePosition();
            this.RandomizeSpin();
            velocity = new Vector3((float)random.NextDouble() * Asteroids.ASTEROID_SPEED, (float)random.NextDouble() * Asteroids.ASTEROID_SPEED, (float)random.NextDouble() * Asteroids.ASTEROID_SPEED);
            model = rockModel;
            texture = rockTexture;
            isAlive = true;
            size = 1;
            targeted = false;
        }

        public Asteroid(Model rockModel, Texture2D rockTexture, Vector3 pos, Vector3 vel, float size) //constructor for baby asteroids
        {
            model = rockModel;
            texture = rockTexture;
            position = pos;
            velocity = vel;
            this.size = size;
            targeted = false;
            isAlive = true;
        }

        public Asteroid(Model rockModel, Vector3 pos, Vector3 vel ) //for the victory spheres
        {
            model = rockModel;
            texture = null;
            position = pos;
            velocity = vel;
            size = 1;
            isAlive = true;
            angularRotations = Vector3.Zero;
            angularRotationVelocities = Vector3.Zero;
        }

        public void RandomizePosition() //randomizes the position of the asteroid in case we need that
        {
            position = new Vector3((float)random.NextDouble() * 1350 - 675, (float)random.NextDouble() * 1350 - 675, (float)random.NextDouble() * 1350 - 675);
        }

        public void RandomizeSpeed()
        {
            velocity = new Vector3((float)random.NextDouble() * Asteroids.ASTEROID_SPEED, (float)random.NextDouble() * Asteroids.ASTEROID_SPEED, (float)random.NextDouble() * Asteroids.ASTEROID_SPEED);
        }

        public void RandomizeSpin() //randomizes the angular velocity of asteroids
        {
            angularRotationVelocities.X = (float)(random.NextDouble() * 2 - 1);
            angularRotationVelocities.Y = (float)(random.NextDouble() * 2 - 1);
            angularRotationVelocities.Z = (float)(random.NextDouble() * 2 - 1);
        }

        public void Update( float time, Vector3 shipPosition )
        {
            position += velocity * time;
            position -= shipPosition;

            angularRotations += angularRotationVelocities * time;

            //wrap-around toroidal physics
            if (position.X > Asteroids.RANGE)
                position.X = -Asteroids.RANGE;
            else if ( position.X < -Asteroids.RANGE )
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

        internal void Draw(GraphicsDevice GraphicsDevice, Matrix world, Matrix view, Matrix projection, BasicEffect basicEffect)
        {
            model.Draw(GraphicsDevice, world, view, projection, basicEffect);
        }

        public Asteroid Break( Vector3 blastVelocity)
        {
            if ( size <.5 ) //small asteroids
            {
                isAlive = false;
                //score increase
                //animation
                return null;
            }

            else if ( size < 1 ) //medium asteroids
            {
                BoundingSphere bounds = this.model.CalculateBounds();
                size = .25f;
                Asteroid temp = new Asteroid(model, texture, position, velocity, .25f);
                RandomizeSpin();
                temp.RandomizeSpin();

                Vector3 perp = Vector3.Cross( velocity, blastVelocity);
                perp.Normalize();
                //offset by correct amount. Since we are in the medium case, we must be making small asteroids
                position += perp * 2;
                temp.position -= perp * 2;
                RandomizeSpeed();
                temp.RandomizeSpeed();
                return temp;
            }

            else //full-grown asteroids
            {
                BoundingSphere bounds = this.model.CalculateBounds();
                size = .5f;
                Asteroid temp = new Asteroid(model, texture, position, velocity, .5f);
                RandomizeSpin();
                temp.RandomizeSpin();
                Vector3 perp = Vector3.Cross(velocity, blastVelocity);
                perp.Normalize();
                //now the asteroids are twice as large. That's all.
                position += perp * 10;
                temp.position -= 10;
                RandomizeSpeed();
                temp.RandomizeSpeed();
                return temp;
            }
        }
    }
}
