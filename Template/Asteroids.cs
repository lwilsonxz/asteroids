using System;
using System.Text;
using SharpDX;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Forms;
using SharpDX.Toolkit.Content;
using SharpDX.Toolkit.Audio;

/**
 * @authors
 * 
 * Luke Wilson
 * 
 * 
 **/

namespace Project4
{
    // Use these namespaces here to override SharpDX.Direct3D11
    using SharpDX.Toolkit;
    using SharpDX.Toolkit.Graphics;
    using SharpDX.Toolkit.Input;

    /// <summary>
    /// Simple Template game using SharpDX.Toolkit.
    /// </summary>
    public class Asteroids : Game
    {
        //constant fields
        #region
        public const float RANGE = 750.0f;
        public const float RANGE_FRACTION = .1f;
        public const float SHIP_SIZE = 2.5f;
        public const float MAX_SHIP_SPEED = 500f;
        public const float SHIP_ACCELERATION = 8f;
        public const float TURN_SPEED = 4f;
        public const float BLAST_SPEED = 95f;
        public const float BLAST_SIZE = 1f;
        public const float ASTEROID_SPEED = 20f;
        public const float MINIMUM_ASTEROID = 8f;
        public const float DANGER_ZONE = 10 * SHIP_SIZE;
        public const float EXPLOSION_TIME = 3f;
        public const float THRUSTER_SIZE = .4f;
        public const float COOLDOWN = .25f;
        public const float SCALE_ASTEROID = .228f; //scale asteroids properly
        public const float SCALE_SHIP = .06f; //scales ship to unit size
        public const float LAUNCH_COOLDOWN = .75f; //missile launcher cooldown
        #endregion


        private GraphicsDeviceManager graphicsDeviceManager;
        private AudioManager audioDeviceManager;
        private SpriteBatch spriteBatch;
        private SpriteFont arial16Font;

        public bool firstPerson = true;
        public bool testCamera = false;

        private Matrix view;
        private Matrix projection;

        private BasicEffect basicEffect;
        private Effect skyEffect;
        private GeometricPrimitive cubePrimitive;
        private GeometricPrimitive ballPrimitive;
        private GeometricPrimitive planePrimitive;

        private Ship ship;
        private Model shipModel;
        private Texture2D shipTexture;
        private Texture2D laserTexture;
        private Texture2D missileTexture;
        private Texture2D deathRayTexture;
        private Texture2D hyperBeamTexture;

        private Asteroid rock;
        private Asteroid victorySphere;
        private Asteroid missileTarget;
        private Model rockModel;
        private Texture2D rockTexture;
        private List<Asteroid> rocks = new List<Asteroid>(100);
        private List<Blast> lasers = new List<Blast>(10);
        private List<Missile> missiles = new List<Missile>(5);
        private List<DeathRay> deathRays = new List<DeathRay>(5);
        private List<BigBlast> bigBlasts = new List<BigBlast>(1);

        private KeyboardManager keyboard;
        private KeyboardState keyBoardState;

        private MouseManager mouse;
        private MouseState mouseState;

        public bool gameRunning = false;
        bool lost = false;
        public int level = 0;
        public int lives = 3;
        public float invincibility = 0;
        public int asteroidsKilled = 0;

        //variables for changing view
        public Vector3 eyePosition = new Vector3(0, 0, 40); //starting camera location
        Vector3 up = new Vector3(0, 1, 0);
        Vector3 right = new Vector3(1, 0, 0);
        Vector3 forward = new Vector3(0, 0, -1);
        
        private float yawAngle = 0;
        private float pitchAngle = 0;
        private float rollAngle = 0;
        public float shotCooldown = 0;
        public float missileCooldown = 0;

        public Vector3 shipPos = Vector3.Zero;

        SoundEffect pew; //gun shooting
        SoundEffect pop; //asteroid popping
        SoundEffect boom; //ship dying
        SoundEffect go; //thruster blasting
        SoundEffect stop; //thruster stopping
        SoundEffect hurt; //sound of ship hitting asteroid but livin'
        SoundEffect launch; //missile launch
        SoundEffect deathRaySound; //destructobeam sound
        SoundEffect bigBlast; //hyper beam sound

        public int weaponNumber = 1;

        //these are the centers of the asteroid models, so translate them accordingly before using
        public Vector3 smallCenter = new Vector3(1.316769f, 51.23239f, -7484485f);
        public Vector3 mediumCenter = new Vector3(-5.080181f, 54.76867f, -5.37693f);
        public Vector3 largeCenter = new Vector3(-1.79172f, 54.06906f, -2.052012f);

        /// <summary>
        /// Initializes a new instance of the <see cref="Asteroids" /> class.
        /// </summary>
        public Asteroids()
        {
            // Creates a graphics manager. This is mandatory.
            graphicsDeviceManager = new GraphicsDeviceManager(this);
            audioDeviceManager = new AudioManager(this);

            // Initialize input keyboard system
            keyboard = new KeyboardManager(this);
            keyBoardState = new KeyboardState();

            // Initialize input mouse system
            mouse = new MouseManager(this);

            // Setup the relative directory to the executable directory
            // for loading contents with the ContentManager
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // Modify the title of the window
            Window.Title = "Asteroids: A Love Story";

            IsMouseVisible = true;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Instantiate a SpriteBatch
            spriteBatch = ToDisposeContent(new SpriteBatch(GraphicsDevice));

            // Loads a sprite font
            // The [Arial16.xml] file is defined with the build action [ToolkitFont] in the project
            arial16Font = Content.Load<SpriteFont>("Arial16");

            //load the models and textures for ships and asteroids
            #region
            shipModel = Content.Load<Model>("Ship");
            shipTexture = Content.Load<Texture2D>("shiptexture");
            rockModel = Content.Load<Model>("rock1");
            rockTexture = Content.Load<Texture2D>("rock1Texture");
            laserTexture = Content.Load<Texture2D>("blast");
            missileTexture = Content.Load<Texture2D>("missileTexture");
            deathRayTexture = Content.Load<Texture2D>("deathRayTexture");
            hyperBeamTexture = Content.Load<Texture2D>("hyperBeamTexture");
            #endregion

            //initialize actual ship and asteroid objects with their models
            #region
            ship = new Ship(shipModel, shipTexture);
            rock = new Asteroid(rockModel, rockTexture, new Random()); //only the first asteroid should be initialized with the random
            rock.RandomizePosition();
            victorySphere = new Asteroid( rockModel, new Vector3(-150, 0, 0), new Vector3(25, 0, 0));
            #endregion

            //create an array of asteroids
            #region
            for (int i = 0; i < rocks.Capacity; ++i)
                rocks.Add(new Asteroid(rockModel, rockTexture));
            #endregion

            // Creates a basic effect
            #region
            basicEffect = ToDisposeContent(new BasicEffect(GraphicsDevice));
            basicEffect.PreferPerPixelLighting = true;
            basicEffect.EnableDefaultLighting();
            basicEffect.TextureEnabled = true;
            #endregion

            //create the skybox effect
            #region
            skyEffect = Content.Load<Effect>("skybox");
            skyEffect.Parameters["SkyBoxTexture"].SetResource(Content.Load<TextureCube>("GreenSpacePlanet"));
            #endregion

            //initialize primitives
            #region
            cubePrimitive = ToDisposeContent(GeometricPrimitive.Cube.New(GraphicsDevice));
            ballPrimitive = ToDisposeContent(GeometricPrimitive.Sphere.New(GraphicsDevice));
            planePrimitive = ToDisposeContent(GeometricPrimitive.Plane.New(GraphicsDevice));
            #endregion

            //load sound effects
            #region
            pew = Content.Load<SoundEffect>("pew");
            pop = Content.Load<SoundEffect>("pop");
            boom = Content.Load<SoundEffect>("explosion");
            go = Content.Load<SoundEffect>("thruster");
            stop = Content.Load<SoundEffect>("stopThruster");
            hurt = Content.Load<SoundEffect>("hurt");
            launch = Content.Load<SoundEffect>("launch");
            deathRaySound = Content.Load<SoundEffect>("deathRaySound");
            bigBlast = Content.Load<SoundEffect>("bigBlast");
            #endregion

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Get the current state of the keyboard
            keyBoardState = keyboard.GetState();

            // Get the current state of the mouse
            mouseState = mouse.GetState();

            var time = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //count down the invicibility timer. Player is invincible for 10 seconds upon each death
            invincibility -= time;

            if (level == 0)
            {
                if (keyBoardState.IsKeyDown(Keys.Enter))
                    ++level;
                projection = Matrix.PerspectiveFovRH(0.9f, (float)GraphicsDevice.BackBuffer.Width / GraphicsDevice.BackBuffer.Height, .1f, 10000.0f);
                view = Matrix.LookAtRH(Vector3.Zero, -Vector3.UnitX, Vector3.UnitY);

                return;
            }

            else if (lost)
                return;//don't update nothin' if we lost

            if ( !gameRunning )
            {
                if (keyBoardState.IsKeyDown(Keys.Space))
                    gameRunning = true; //if the game isn't running, update nothing until the user stops reading the level info
                return;
            }

            //mouse aiming stuff
            Ray mouseRay = Ray.GetPickRay((int)(mouseState.X * GraphicsDevice.Viewport.Width + .5f), (int)(mouseState.Y * GraphicsDevice.Viewport.Height + .5f), GraphicsDevice.Viewport, view*projection);
            
            if (keyBoardState.IsKeyDown(Keys.W) && !testCamera && level != 1 && level != 8) //ship can't move during the first level
            {
                go.Play();
                ship.fireThrusters(level);
            }
            else if (keyBoardState.IsKeyDown(Keys.S) && !testCamera && level != 1 && level != 8)
            {
                stop.Play();
                ship.stopThrusters();
            }

            if (keyBoardState.IsKeyPressed(Keys.Enter))
                firstPerson = !firstPerson;
            if (keyBoardState.IsKeyPressed(Keys.Escape))
                testCamera = !testCamera;

            //weapon changing
            #region
            if (keyBoardState.IsKeyDown(Keys.NumPad1))
                weaponNumber = 1;
            else if (keyBoardState.IsKeyDown(Keys.NumPad2))
                weaponNumber = 2;
            else if (keyBoardState.IsKeyDown(Keys.NumPad3))
                weaponNumber = 3;
            #endregion

            if (testCamera)
                firstPerson = false; //always disable first person for test camera view

            if ( level == 1 )
            {
                victorySphere.Update(time, Vector3.Zero);
                if ( HitsShip(victorySphere)  )
                {
                    ++level;
                    gameRunning = false;
                    victorySphere.RandomizePosition();
                    victorySphere.velocity = Vector3.Zero;
                }
            }

            //allow the control of the ship
            #region
            if ( !testCamera ) //disable ship controls with the test camera on
                ship.RotateShip(keyBoardState.IsKeyDown(Keys.Down),
                    keyBoardState.IsKeyDown(Keys.Up),
                    keyBoardState.IsKeyDown(Keys.Left),
                    keyBoardState.IsKeyDown(Keys.Right),
                    keyBoardState.IsKeyDown(Keys.D),
                    keyBoardState.IsKeyDown(Keys.A),
                    time); //controls require both hands on the keyboard
            #endregion

            //update the ship's position
            shipPos = ship.Update(time);

            if ( level == 2 )
            {
                victorySphere.Update(time, shipPos);
                if ( HitsShip(victorySphere) )
                {
                    ++level;
                    gameRunning = false;
                }
            }

            if ( level == 3 )
                if ( asteroidsKilled >= 50 )
                {
                    ++level;
                    gameRunning = false;
                    lasers.Clear();
                }

            //missile launch code
            #region
            if (level > 3 && weaponNumber == 1 && level != 8)
            {
                //find the asteroid the missile is locked on
                int t = 0;
                bool shot = false;
                for (t = 0; t < rocks.Count && !shot; ++t)
                {
                    BoundingSphere bound = rocks[t].model.CalculateBounds();
                    bound.Radius *= rocks[t].size;//scale the sphere by the size of the asteroid
                    bound.Center = rocks[t].position; //move it around
                    if (mouseRay.Intersects(bound)) //if the mouseRay intersects an asteroid
                        missileTarget = rocks[t];
                }

                if ( mouseState.RightButton.Down && missileCooldown >= LAUNCH_COOLDOWN )
                {
                    missiles.Add(new Missile(missileTexture, missileTarget));
                    launch.Play();
                    missileCooldown = 0;
                }
            }
            for (int i = 0; i < missiles.Count; )
            {
                missiles[i].Update(time, shipPos);
                if (!missiles[i].isAlive)
                    missiles.RemoveAt(i);
                else
                    ++i;
            }
            #endregion

            //destruct-o-beam launch code
            #region
            if (level > 4 && weaponNumber == 2 && level != 8)
            {
                if (mouseState.RightButton.Down && missileCooldown >= LAUNCH_COOLDOWN)
                {
                    int t = 0;
                    bool shot = false;
                    for (t = 0; t < rocks.Count && !shot; ++t)
                    {
                        BoundingSphere bound = rocks[t].model.CalculateBounds();
                        bound.Radius *= rocks[t].size;//scale the sphere by the size of the asteroid
                        bound.Center = rocks[t].position; //move it around
                        if (mouseRay.Intersects(bound)) //if the mouseRay intersects an asteroid
                        {
                            rocks[t].targeted = true;
                            shot = true;
                            break; //stop on the first asteroid we see
                        }
                    }

                    if (shot)
                    {
                        BoundingSphere bound = rocks[t].model.CalculateBounds();
                        bound.Center = rocks[t].position;
                        bound.Radius *= rocks[t].size;
                        Vector3 colPoint;
                        mouseRay.Intersects(ref bound, out colPoint);
                        deathRays.Add(new DeathRay(Vector3.Zero, Vector3.Normalize(colPoint) * BLAST_SPEED + ship.velocity, deathRayTexture));
                        deathRaySound.Play();
                        missileCooldown = 0;
                    }
                }
            }
            for (int i = 0; i < deathRays.Count; )
            {
                deathRays[i].Update(time, shipPos);
                if (!deathRays[i].isAlive)
                    deathRays.RemoveAt(i);
                else
                    ++i;
            }
            #endregion

            //Hyper beam launch code
            #region
            if ( level > 6 && weaponNumber == 3 && level != 8 )
            {
                if (mouseState.RightButton.Down && missileCooldown >= LAUNCH_COOLDOWN / 5 && bigBlasts.Count <= 10) //had to limit big blasts for performance reasons
                {
                    bigBlast.Play();
                    bigBlasts.Add(new BigBlast(Vector3.Zero, ship.forward * BLAST_SPEED + ship.velocity, hyperBeamTexture));
                    missileCooldown = 0;
                }
            }
            for (int i = 0; i < bigBlasts.Count; )
            {
                bigBlasts[i].Update(time, shipPos);
                if (!bigBlasts[i].isAlive)
                    bigBlasts.RemoveAt(i);
                else
                    ++i;
            }
            #endregion

            if ( level == 4 )
                if (asteroidsKilled >= 100)
                {
                    ++level;
                    gameRunning = false;
                    for (int i = 0; i < 50; ++i)
                        rocks.Add(new Asteroid(rockModel, rockTexture)); //add some more asteroids
                    lasers.Clear();
                    missiles.Clear();
                }

            if ( level == 5 )
                if ( asteroidsKilled >= 200 )
                {
                    ++level;
                    gameRunning = false;
                    for (int i = 0; i < 200; ++i)
                        rocks.Add(new Asteroid(rockModel, rockTexture)); //add some more asteroids
                    lasers.Clear();
                    missiles.Clear();
                    deathRays.Clear();
                }

            if (level == 6)
                if (asteroidsKilled >= 250)
                {
                    ++level;
                    gameRunning = false;
                    for (int i = 0; i < 1100; ++i)
                        rocks.Add(new Asteroid(rockModel, rockTexture)); //add some more asteroids
                    lasers.Clear();
                    missiles.Clear();
                    deathRays.Clear();
                }

            if (level == 7)
            {
                if (asteroidsKilled >= 1250)
                {
                    ++level;
                    gameRunning = false;
                    lasers.Clear();
                    missiles.Clear();
                    deathRays.Clear();
                    bigBlasts.Clear();
                    rocks.Clear();
                    ship.velocity = Vector3.Zero;
                    for ( int i = 0 ; i < 15 ; ++i )
                    {
                        rocks.Add(new Asteroid(rockModel, rockTexture));
                        rocks[i].velocity = -Vector3.Normalize(rocks[i].position) * 10;
                    }
                }
            }

            if (level == 8)
                if (rocks.Count <= 0)
                {
                    ++level;//win!
                    gameRunning = false;
                    lasers.Clear();
                    missiles.Clear();
                    deathRays.Clear();
                }

            //I'm'a firin' mah lazor
            #region
            if (mouseState.LeftButton.Down && shotCooldown >= COOLDOWN && level > 2)
            {
                int t = 0;
                bool shot = false;
                for (t = 0; t < rocks.Count && !shot; ++t )
                {
                    BoundingSphere bound = rocks[t].model.CalculateBounds();
                    bound.Radius *= rocks[t].size;//scale the sphere by the size of the asteroid
                    bound.Center = rocks[t].position; //move it around
                    if (mouseRay.Intersects(bound)) //if the mouseRay intersects an asteroid
                    {
                        rocks[t].targeted = true;
                        shot = true;
                        break; //stop on the first asteroid we see
                    }
                }

                if (shot)
                {
                    //You can do it Luke! I believe in you! @author Stuart Foley
                    BoundingSphere bound = rocks[t].model.CalculateBounds();
                    bound.Center = rocks[t].position;
                    bound.Radius *= rocks[t].size;
                    Vector3 colPoint;
                    mouseRay.Intersects(ref bound, out colPoint);
                    lasers.Add(new Blast(Vector3.Zero, Vector3.Normalize(colPoint) * BLAST_SPEED + ship.velocity, laserTexture));
                    pew.Play();
                }

                //else
                    //lasers.Add(new Blast(Vector3.Zero, ship.forward * BLAST_SPEED + ship.velocity, laserTexture)); //shot straight with no target

                shotCooldown = 0;
            }

            for (int i = 0; i < lasers.Count;  )
            {
                lasers[i].Update(time, shipPos);
                if (!lasers[i].isAlive)
                    lasers.RemoveAt(i);
                else
                    ++i;
            }

            //cool down the weapons
            shotCooldown += time;
            missileCooldown += time;
            #endregion

            if (firstPerson) //standard first person camera
            {
                eyePosition = ship.position;
                view = Matrix.LookAtRH(eyePosition, eyePosition + ship.forward, ship.up);
            }

            #region
            else if ( testCamera ) //full control testing camera
            {
                if (keyBoardState.IsKeyDown(Keys.Up))
                    pitchAngle += .04f;
                if (keyBoardState.IsKeyDown(Keys.Down))
                    pitchAngle -= .04f;
                if (keyBoardState.IsKeyDown(Keys.Left))
                    yawAngle += .04f;
                if (keyBoardState.IsKeyDown(Keys.Right))
                    yawAngle -= .04f;

                Matrix3x3 rotation = (Matrix3x3)Matrix.RotationYawPitchRoll(yawAngle, pitchAngle, rollAngle);
                forward = Vector3.Transform(-Vector3.UnitZ, rotation);
                right = Vector3.Transform(Vector3.UnitX, rotation);
                up = Vector3.Transform(Vector3.UnitY, rotation);

                if (keyBoardState.IsKeyDown(Keys.W))
                    eyePosition += 1f * forward;
                if (keyBoardState.IsKeyDown(Keys.S))
                    eyePosition -= 1f * forward;
                if (keyBoardState.IsKeyDown(Keys.A))
                    eyePosition -= 1f * right;
                if (keyBoardState.IsKeyDown(Keys.D))
                    eyePosition += 1f * right;

                //easter-egg panning feature
                if (keyBoardState.IsKeyDown(Keys.Q))
                    eyePosition += .4f * up;
                if (keyBoardState.IsKeyDown(Keys.E))
                    eyePosition -= .4f * up;

                view = Matrix.LookAtRH(eyePosition, eyePosition + forward, up);
            }
            #endregion

            #region
            else //third person camera
                view = Matrix.LookAtRH(new Vector3( 1, 40, 0 ), Vector3.Zero, Vector3.UnitY);
            #endregion

            projection = Matrix.PerspectiveFovRH(0.9f, (float)GraphicsDevice.BackBuffer.Width / GraphicsDevice.BackBuffer.Height, .1f, 10000.0f);
            
            basicEffect.View = view;
            basicEffect.Projection = projection;

            //update all asteroids
            #region
            for (int i = 0; i < rocks.Count; )
            {
                if (rocks[i] != null)
                {
                    rocks[i].Update(time, shipPos);
                    if ( invincibility <= 0 && HitsShip(rocks[i]) && level > 2)//if we crash into an asteroid, kill us, unless we are invincible or haven't reached level 3
                    {
                        --lives;
                        invincibility = 4;
                        if (lives < 0)
                        {
                            gameRunning = false;
                            if (!lost)
                                boom.Play();
                            lost = true;
                        }
                        else
                            hurt.Play();
                    }

                    for (int j = 0; j < lasers.Count; )
                    {
                        if (lasers[j].HitsRock(rocks[i]))
                        {
                            Asteroid newAsteroid = rocks[i].Break(lasers[j].velocity);
                            pop.Play();
                            rocks[i].velocity = -Vector3.Normalize(rocks[i].position) * 10;
                            if (newAsteroid != null)
                            {
                                if (level == 8)
                                    newAsteroid.velocity = -Vector3.Normalize(newAsteroid.position) * 10;
                                rocks.Add(newAsteroid);
                            }
                            lasers.RemoveAt(j);
                            ++asteroidsKilled;
                        }
                        else
                            ++j;
                    }

                    for (int j = 0; j < missiles.Count; )
                    {
                        if (missiles[j].HitsRock(rocks[i]))
                        {
                            Asteroid newAsteroid = rocks[i].Break(missiles[j].velocity);
                            pop.Play();
                            if (newAsteroid != null)
                                rocks.Add(newAsteroid);
                            missiles.RemoveAt(j);
                            ++asteroidsKilled;
                        }
                        else
                            ++j;
                    }

                    for (int j = 0; j < deathRays.Count; )
                    {
                        if (deathRays[j].HitsRock(rocks[i]))
                        {
                            pop.Play();
                            deathRays.RemoveAt(j);
                            if (rocks[i].size == 1)//destruct-o-beam takes care of business
                                asteroidsKilled += 7;
                            else if (rocks[i].size == .5f)
                                asteroidsKilled += 3;
                            else
                                ++asteroidsKilled;
                            rocks[i].isAlive = false;
                        }
                        else
                            ++j;
                    }

                    for (int j = 0; j < bigBlasts.Count; )
                    {
                        if (rocks[i].isAlive && bigBlasts[j].HitsRock(rocks[i]))
                        {
                            pop.Play();
                            if (rocks[i].size == 1)//as does the hyper beam
                                asteroidsKilled += 7;
                            else if (rocks[i].size == .5f)
                                asteroidsKilled += 3;
                            else
                                ++asteroidsKilled;
                            rocks[i].isAlive = false;
                        }
                        else
                            ++j;
                    }
                    if (!rocks[i].isAlive)
                        rocks.RemoveAt(i);
                    else
                        ++i;
                }
            }
            #endregion
        }
        protected override void Draw(GameTime gameTime)
        {
            // Use time in seconds directly
            var time = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Clears the screen with the Color.CornflowerBlue
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.CullFront); //backwards culling for skybox

            //draw a skybox
            //always draw the skybox
            skyEffect.Parameters["World"].SetValue(Matrix.Scaling(10000) * Matrix.Translation(eyePosition));
            skyEffect.Parameters["Projection"].SetValue(projection);
            skyEffect.Parameters["View"].SetValue(view);
            skyEffect.Parameters["CameraPosition"].SetValue(eyePosition);
            cubePrimitive.Draw(skyEffect);

            GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.CullBack); //back to normal

            if ( level == 0 )
            {
                DrawIntro(time);
                return; //draw just the intro and then stop drawing since we don't need it
            }

            else if ( lost )
            {
                DrawLossScreen(time);
                return; //that's all folks
            }

            //all the level instruction screens
            #region
            if ( !gameRunning && level == 1 )
            {
                DrawLevelOneInstructions(time);
                return;
            }
            else if (!gameRunning && level == 2)
            {
                DrawLevelTwoInstructions(time);
                return;
            }
            else if (!gameRunning && level == 3)
            {
                DrawLevelThreeInstructions(time);
                return;
            }
            else if (!gameRunning && level == 4)
            {
                DrawLevelFourInstructions(time);
                return;
            }
            else if (!gameRunning && level == 5)
            {
                DrawLevelFiveInstructions(time);
                return;
            }
            else if (!gameRunning && level == 6)
            {
                DrawLevelSixInstructions(time);
                return;
            }
            else if (!gameRunning && level == 7)
            {
                DrawLevelSevenInstructions(time);
                return;
            }
            else if ( !gameRunning && level == 8 )
            {
                DrawLevelEightInstructions(time);
                return;
            }
            else if ( !gameRunning )
            {
                DrawCreditScreen(time);
                return;
            }
            #endregion

            Matrix world;

            #region
            if ( level == 1 || level == 2 )
            {
                basicEffect.LightingEnabled = false;
                world = Matrix.Scaling(50) * Matrix.Translation(victorySphere.position);
                basicEffect.World = world;
                basicEffect.View = view;
                basicEffect.Projection = projection;
                basicEffect.DiffuseColor = (Vector4)(Color.Green);
                ballPrimitive.Draw(basicEffect);
                basicEffect.DiffuseColor = (Vector4)(Color.White);
                basicEffect.LightingEnabled = true;
            }
            #endregion

            //draw the ship
            if (!firstPerson) //only draw the ship if not in first person mode, since it will never be visible in first persom
            {
                world = Matrix.Scaling(SCALE_SHIP * SHIP_SIZE) * ship.rotation * Matrix.Translation(Vector3.Zero);
                basicEffect.World = world;
                basicEffect.View = view;
                basicEffect.Projection = projection;
                basicEffect.Texture = shipTexture;
                ship.Draw(GraphicsDevice, world, view, projection, basicEffect);
            }

            //draw all asteroids
            basicEffect.View = view;
            basicEffect.Projection = projection;
            basicEffect.Texture = rockTexture;
            for (int i = 0; i < rocks.Count; ++i)
            {
                //BoundingSphere rockBound = rocks[i].model.CalculateBounds();
                //basicEffect.DiffuseColor = (Vector4)Color.White;
                if (rocks[i] != null)
                {
                    if (rocks[i].Equals(missileTarget) && weaponNumber == 1)
                        basicEffect.DiffuseColor = (Vector4)(Color.Red);

                    rocks[i].targeted = false; //detarget

                    if (rocks[i].size == 1f)//full sized asteroid
                        world = Matrix.Translation(-largeCenter) * Matrix.Scaling(rocks[i].size * SCALE_ASTEROID) * Matrix.RotationX(rocks[i].angularRotations.X)
                        * Matrix.RotationY(rocks[i].angularRotations.Y) * Matrix.RotationZ(rocks[i].angularRotations.Z)
                        * Matrix.Translation(rocks[i].position);

                    else if (rocks[i].size == .5f)//medium
                        world = Matrix.Translation(-mediumCenter) * Matrix.Scaling(rocks[i].size * SCALE_ASTEROID) * Matrix.RotationX(rocks[i].angularRotations.X)
                        * Matrix.RotationY(rocks[i].angularRotations.Y) * Matrix.RotationZ(rocks[i].angularRotations.Z)
                        * Matrix.Translation(rocks[i].position);

                    else//small
                    {
                        world = /*Matrix.Translation(smallCenter) */ Matrix.Scaling(rocks[i].size * SCALE_ASTEROID) * Matrix.RotationX(rocks[i].angularRotations.X)
                        * Matrix.RotationY(rocks[i].angularRotations.Y) * Matrix.RotationZ(rocks[i].angularRotations.Z)
                        * Matrix.Translation(rocks[i].position);
                        basicEffect.World = world;
                        ballPrimitive.Draw(basicEffect);
                    }

                    basicEffect.World = world;
                    rocks[i].Draw(GraphicsDevice, world, view, projection, basicEffect); //dead asteroids won't be drawn because they'll be removed from the list
                }

                basicEffect.DiffuseColor = (Vector4)(Color.White);

                //rockBound.Center = rocks[i].position;
                //basicEffect.DiffuseColor = (Vector4)Color.Red;
                //basicEffect.World = Matrix.Scaling(rockBound.Radius * rocks[i].size * 2) * Matrix.Translation(rockBound.Center); //need to multiply by two for radius
                //ballPrimitive.Draw(basicEffect);
            }

            //draw the lasers
            GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.CullNone);
            GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.DepthRead);
            GraphicsDevice.SetBlendState(GraphicsDevice.BlendStates.Additive);
            basicEffect.View = view;
            basicEffect.Projection = projection;
            basicEffect.LightingEnabled = false;
            foreach (Blast laser in lasers)
            {
                basicEffect.DiffuseColor = (Vector4)Color.White;
                basicEffect.Texture = laser.laserImage;
                Matrix billboard = Matrix.BillboardRH(laser.position, eyePosition, ship.up, ship.forward);
                basicEffect.World = Matrix.Scaling(1) * billboard;
                planePrimitive.Draw(basicEffect);
            }

            //draw the missiles
            if ( level > 3 )
                foreach ( Missile missile in missiles )
                {
                    basicEffect.DiffuseColor = (Vector4)Color.White;
                    basicEffect.Texture = missile.laserImage;
                    Matrix billboard = Matrix.BillboardRH(missile.position, eyePosition, ship.up, ship.forward);
                    basicEffect.World = Matrix.Scaling(1) * billboard;
                    planePrimitive.Draw(basicEffect);
                }

            //draw the death rays
            if (level > 4)
                foreach (DeathRay beam in deathRays)
                {
                    basicEffect.DiffuseColor = (Vector4)Color.White;
                    basicEffect.Texture = beam.laserImage;
                    Matrix billboard = Matrix.BillboardRH(beam.position, eyePosition, ship.up, ship.forward);
                    basicEffect.World = Matrix.Scaling(1) * billboard;
                    planePrimitive.Draw(basicEffect);
                }

            //draw the hyper beams
            if (level > 4)
                foreach (BigBlast beam in bigBlasts)
                {
                    basicEffect.DiffuseColor = (Vector4)Color.White;
                    basicEffect.Texture = beam.laserImage;
                    Matrix billboard = Matrix.BillboardRH(beam.position, eyePosition, ship.up, ship.forward);
                    basicEffect.World = Matrix.Scaling(50) * billboard;
                    planePrimitive.Draw(basicEffect);
                }

            GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.CullBack);
            GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.Default);
            GraphicsDevice.SetBlendState(GraphicsDevice.BlendStates.Default);
            basicEffect.LightingEnabled = true;

            // ------------------------------------------------------------------------
            // Draw the some 2d text
            // ------------------------------------------------------------------------
            spriteBatch.Begin();
            var text = new StringBuilder("Asteroid count: " + rocks.Count
                + "\nLives: " + lives
                + "\nLevel : " + level
                + "\n Asteroids destroyed: " + asteroidsKilled)
                .AppendLine();

            List<Keys> keys = new List<Keys>();

            //Display pressed keys
            keyBoardState.GetDownKeys(keys);
            text.Append("Key Pressed: [");
            foreach (var key in keys)
            {
                text.Append(key.ToString());
                text.Append(" ");
            }
            text.Append("]").AppendLine();
            spriteBatch.DrawString(arial16Font, text.ToString(), new Vector2(16, 16), Color.White);
            spriteBatch.End();

            // Setup states for posteffect
            //lol idk what that means
            GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.Default);
            GraphicsDevice.SetBlendState(GraphicsDevice.BlendStates.Default);
            GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.None);

            base.Draw(gameTime);
        }

        private void DrawCreditScreen(float time)
        {
            // ------------------------------------------------------------------------
            // Draw the some 2d text
            // ------------------------------------------------------------------------
            spriteBatch.Begin();
            var text = new StringBuilder("Congratulations!\nYou were able to DO IT YOURSELF!\nYou survived but your copilot is no where to be found.\nUnable to return to the academy,\nyou explore the galaxy alone.\nHopefully she has a nice life.\n\n\nCredits\nGame by Luke Wilson\nSpecial thanks:\nBarry Wittman \n \nShoutout to [people] that done it:\nOmar Mustardo\nTaylor Ryan\nTom Gorko\nDMX").AppendLine();

            spriteBatch.DrawString(arial16Font, text.ToString(), new Vector2(500, 200), Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.Default);
            GraphicsDevice.SetBlendState(GraphicsDevice.BlendStates.Default);
            GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.None);

            base.Draw(gameTime);
        }

        private void DrawLevelOneInstructions(float time)
        {
            // ------------------------------------------------------------------------
            // Draw the some 2d text
            // ------------------------------------------------------------------------
            spriteBatch.Begin();
            var text = new StringBuilder("Level 1\nJust wait for your ship to calibrate automatically.\nSPACE to continue.").AppendLine();

            spriteBatch.DrawString(arial16Font, text.ToString(), new Vector2(500, 400), Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.Default);
            GraphicsDevice.SetBlendState(GraphicsDevice.BlendStates.Default);
            GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.None);

            base.Draw(gameTime);
        }

        private void DrawLevelTwoInstructions(float time)
        {
            // ------------------------------------------------------------------------
            // Draw the some 2d text
            // ------------------------------------------------------------------------
            spriteBatch.Begin();
            var text = new StringBuilder("Level 2\nGood job! Now you have full control of your flight.\nSee if you can navigate to the green sphere.\nSPACE to continue.").AppendLine();

            spriteBatch.DrawString(arial16Font, text.ToString(), new Vector2(500, 400), Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.Default);
            GraphicsDevice.SetBlendState(GraphicsDevice.BlendStates.Default);
            GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.None);

            base.Draw(gameTime);
        }

        private void DrawLevelThreeInstructions(float time)
        {
            // ------------------------------------------------------------------------
            // Draw the some 2d text
            // ------------------------------------------------------------------------
            spriteBatch.Begin();
            var text = new StringBuilder("Level 3\nExcellent work!\nYour copilot has now joined.\nFind someone to operate her gun using the mouse.\nWatch out! Running into an asteroid will hurt you now.\nTo advance, destroy 50 asteroids.\nSPACE to continue.").AppendLine();

            spriteBatch.DrawString(arial16Font, text.ToString(), new Vector2(500, 400), Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.Default);
            GraphicsDevice.SetBlendState(GraphicsDevice.BlendStates.Default);
            GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.None);

            base.Draw(gameTime);
        }

        private void DrawLevelFourInstructions(float time)
        {
            // ------------------------------------------------------------------------
            // Draw the some 2d text
            // ------------------------------------------------------------------------
            spriteBatch.Begin();
            var text = new StringBuilder("Level 4\nLooks like you're getting the hang of working together.\nYou've learned to use the missile launcher!\nPlayer 2 can launch a homing missile by right clicking.\nAsteroids turn RED when you have a lock.\nDestroy 50 more asteroids to advance.\nSPACE to continue.").AppendLine();

            spriteBatch.DrawString(arial16Font, text.ToString(), new Vector2(500, 400), Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.Default);
            GraphicsDevice.SetBlendState(GraphicsDevice.BlendStates.Default);
            GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.None);

            base.Draw(gameTime);
        }

        private void DrawLevelFiveInstructions(float time)
        {
            // ------------------------------------------------------------------------
            // Draw the some 2d text
            // ------------------------------------------------------------------------
            spriteBatch.Begin();
            var text = new StringBuilder("Level 5\nWOW! Pretty sweet, huh?\nThings are going GREAT and\nyou're kind of HAPPY!\nYou've learned to use the DESTRUCT-O-BEAM\nThis weapon fires a SINGULARITY that destroys an ENTIRE ASTEROID!\nYour copilot can fire it with the right click.\nThe DESTRUC-O-BEAM destroys asteroids completely in one shot! Amazing!\nPress 1 on the numpad to select missiles\nand press 2 on the numpad to select DESTRUC-O-BEAM.\n\nWith your new weapon, it should be no trouble to destroy another 100 asteroids.\nSPACE to continue.").AppendLine();

            spriteBatch.DrawString(arial16Font, text.ToString(), new Vector2(500, 400), Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.Default);
            GraphicsDevice.SetBlendState(GraphicsDevice.BlendStates.Default);
            GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.None);

            base.Draw(gameTime);
        }

        private void DrawLevelSixInstructions(float time)
        {
            // ------------------------------------------------------------------------
            // Draw the some 2d text
            // ------------------------------------------------------------------------
            spriteBatch.Begin();
            var text = new StringBuilder("Level 6\nYour COPILOT really hates this level.\nShe's willing to help out, but don't expect\nany new weapons technology.\nBesides, COMMAND wants you to do things on your own a little more.\nFortunately you love this level.\nYour ship's SPEED has been DOUBLED!\n\nThe asteroid field has grown denser!\nMaybe if you guys clear enough out\nthings will be okay.\nDestroy 50 more asteroids to advance.\nSPACE to continue.").AppendLine();

            spriteBatch.DrawString(arial16Font, text.ToString(), new Vector2(500, 400), Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.Default);
            GraphicsDevice.SetBlendState(GraphicsDevice.BlendStates.Default);
            GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.None);

            base.Draw(gameTime);
        }

        private void DrawLevelSevenInstructions(float time)
        {
            // ------------------------------------------------------------------------
            // Draw the some 2d text
            // ------------------------------------------------------------------------
            spriteBatch.Begin();
            var text = new StringBuilder("Level 7\n\nWow! This is incredible!\nYou knew your copilot was good\nbut she's so much more capable than you imagined!\nShe has unlocked the HYPER BEAM,\na weapon powerful enough to destroy a planet!\nSelect the hyper beam by pressing 3 on the num pad.\n\nUse your new weapon to destroy 1,000 asteroids!\nSPACE to continue").AppendLine();

            spriteBatch.DrawString(arial16Font, text.ToString(), new Vector2(500, 400), Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.Default);
            GraphicsDevice.SetBlendState(GraphicsDevice.BlendStates.Default);
            GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.None);

            base.Draw(gameTime);
        }

        private void DrawLevelEightInstructions(float time)
        {
            // ------------------------------------------------------------------------
            // Draw the some 2d text
            // ------------------------------------------------------------------------
            spriteBatch.Begin();
            var text = new StringBuilder("Level 8\nYour copilot has disappeared! You must operate the gun alone.\nDestroy all incoming asteroids\nbefore they destroy you!\n\nYou can still TURN and FIRE, but can't escape.\nSPACE to continue.").AppendLine();

            spriteBatch.DrawString(arial16Font, text.ToString(), new Vector2(500, 400), Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.Default);
            GraphicsDevice.SetBlendState(GraphicsDevice.BlendStates.Default);
            GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.None);

            base.Draw(gameTime);
        }

        private void DrawLossScreen(float time)
        {
            // ------------------------------------------------------------------------
            // Draw the some 2d text
            // ------------------------------------------------------------------------
            spriteBatch.Begin();
            var text = new StringBuilder("Sorry! You CRASHED and BURNED!").AppendLine();

            if (level == 8)
                text.Append("\nYour COPILOT goes on without you.\n        No hard feelings.\n               She deserves better than you anyway.");

            spriteBatch.DrawString(arial16Font, text.ToString(), new Vector2(500, 400), Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.Default);
            GraphicsDevice.SetBlendState(GraphicsDevice.BlendStates.Default);
            GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.None);

            base.Draw(gameTime);
        }

        private void DrawIntro(float time)
        {
            // ------------------------------------------------------------------------
            // Draw the some 2d text
            // ------------------------------------------------------------------------
            spriteBatch.Begin();
            var text = new StringBuilder("YOU are a brave space pilot fresh from the academy.\nYou weren't the top of your class\nand had never flown anything before the academy. But you quickly established\n yourself as an able learner\nand as capable as some of the veterans.\nTall, aloof, and beautiful, you weren't always the best\nbut you usually packed enough arrogance for you and a copilot.\n\nYour COPILOT is a friend from the academy.\nFrom a small backwater planet, she never imagined she'd be flying spaceships\nbut now she's a better gunner than you ever were.\nShe tends to underestimate herself but you know she can do it.\nPragmatic and cool under pressure, she is ready to destroy some asteroids!" 
                + "\n\nControls:\nW: Thrust\nS: Brake\nUp and down: Pitch the ship\nLeft and right: turn.\nA and D: Roll\nMouse: Aim and fire!\n\nPress ENTER to continue!")
                .AppendLine();

            List<Keys> keys = new List<Keys>();

            //Display pressed keys
            keyBoardState.GetDownKeys(keys);
            text.Append("Key Pressed: [");
            foreach (var key in keys)
            {
                text.Append(key.ToString());
                text.Append(" ");
            }
            text.Append("]").AppendLine();
            spriteBatch.DrawString(arial16Font, text.ToString(), new Vector2(16, 16), Color.White);
            spriteBatch.End();

            // Setup states for posteffect
            //lol idk what that means
            GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.Default);
            GraphicsDevice.SetBlendState(GraphicsDevice.BlendStates.Default);
            GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.None);

            base.Draw(gameTime);
        }


        private bool HitsShip( Asteroid asteroid ) //these collisions appear to work fine
        {
            float distance = Vector3.Distance(ship.position, asteroid.position);
            BoundingSphere shipBound = ship.model.CalculateBounds();
            BoundingSphere rockBound = asteroid.model.CalculateBounds();
            rockBound.Radius *= asteroid.size; //scale
            rockBound.Radius /= 25; //make smaller to be more forgiving to ship
            if (distance < shipBound.Radius + rockBound.Radius)
                return true;
            return false;
        }
    }
}