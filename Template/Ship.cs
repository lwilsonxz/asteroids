using SharpDX;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project4
{
    class Ship
    {
        public Vector3 position;
        public Vector3 velocity;
        public Model model;
        public Model thrustModel;
        public Texture2D texture;
        public bool isAlive;
        public Vector3 up;
        public Vector3 right;
        public Vector3 forward;
        public Matrix rotation;

        public Ship( Model shipModel, Texture2D shipTexture )
        {
            position = Vector3.Zero;
            velocity = Vector3.Zero;
            model = shipModel;
            texture = shipTexture;
            isAlive = true;
            up = new Vector3(0, 1, 0);
            right = new Vector3(0, 0, -1);
            forward = new Vector3(-1, 0, 0);
            rotation = Matrix.Identity; //start at identity
        }

        internal void Draw(GraphicsDevice GraphicsDevice, Matrix world, Matrix view, Matrix projection, BasicEffect basicEffect)
        {
            model.Draw(GraphicsDevice, world, view, projection, basicEffect);
        }

        internal void fireThrusters( int level) //TODO: fix issue where ship can't move once it's at max speed
        {
            if (level < 5)//ship speed doubles level 6 on
            {
                velocity += Asteroids.SHIP_ACCELERATION * forward;
                if (velocity.Length() > Asteroids.MAX_SHIP_SPEED)
                    velocity = Vector3.Normalize(velocity) * Asteroids.MAX_SHIP_SPEED;
            }
            else
            {
                velocity += Asteroids.SHIP_ACCELERATION * 2 * forward;
                if (velocity.Length() > Asteroids.MAX_SHIP_SPEED * 2)
                    velocity = Vector3.Normalize(velocity) * Asteroids.MAX_SHIP_SPEED * 2;
            }
        }

        internal void stopThrusters() //TODO: Actually make this method work
        {
            //subtract the normalized velocity from the velocity. Scalar on normalized velocity to adjust right
            velocity -= Vector3.Normalize(velocity)*Asteroids.SHIP_ACCELERATION;
        }

        internal void RotateShip(bool zUp, bool zDown, bool yUp, bool yDown, bool xUp, bool xDown, float time)
        {
              if (xUp) //pitching
                rotation *= Matrix.RotationAxis(forward, Asteroids.TURN_SPEED*time);
            if (xDown)
                rotation *= Matrix.RotationAxis(forward, -Asteroids.TURN_SPEED * time);

            if (yUp) //yawing
                rotation *= Matrix.RotationAxis(up, Asteroids.TURN_SPEED * time);
            if (yDown)
                rotation *= Matrix.RotationAxis(up, -Asteroids.TURN_SPEED * time);

            if (zUp) //rolling
                rotation *= Matrix.RotationAxis(right, Asteroids.TURN_SPEED * time);
            if (zDown)
                rotation *= Matrix.RotationAxis(right, -Asteroids.TURN_SPEED * time);

            forward = Vector3.Transform(-Vector3.UnitX, (Matrix3x3)rotation);
            right = Vector3.Transform(Vector3.UnitZ, (Matrix3x3)rotation);
            up = Vector3.Transform(Vector3.UnitY, (Matrix3x3)rotation);
        }

        public Vector3 Update( float time )
        {
            return velocity * time;

            //wrap-around toroidal physics
            /*
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
                position.Z = Asteroids.RANGE;*/
        }
    }
}