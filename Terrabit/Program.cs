using System.Drawing;
using System.Security.Cryptography.X509Certificates; //???

namespace Terrabit //Placeholder name for the first week-ish of development was "Terramir" -Why, that doesn't make any sense? -Yeah well that's why I changed it
{
    class Program
    {
        public static List<Entities> listOfEntities = new();
        public static List<Entities> lineOfDeletion = new();
        public static ConsoleColor savedBackgroundColor;
        public static int ScreenRefresh = 1;
        public static Entities? Player;

        public static ConsoleColor[] colorInfo ={ //There are only 16 available colors, which I need to use carefully //deez nuts

            ConsoleColor.Black, //this is 0, remindes because I keep forgetting... AIR
            ConsoleColor.DarkYellow, //1 DIRT
            ConsoleColor.Gray, //2 AAA STONE
            ConsoleColor.Green, //3 GRASS
            ConsoleColor.Yellow, //4 GOLD
            ConsoleColor.Blue, //5 WATER
            ConsoleColor.DarkYellow, //6 WOOD // this is the first duplicate color, you'd think this is stupid but I can't think of a more efficient way        one that I'm bothered to code atleast
            ConsoleColor.DarkGreen, //Bush??? Leaves????????
        };
        public static void CreatePlayer(int x, int y, ConsoleColor Color)
        {
            listOfEntities.Add(new Player(x, y, Color, 0));
            Player = listOfEntities[0];
        }
        public static void CreateEnemy(int x, int y, ConsoleColor Color, int id)
        {
            listOfEntities.Add(new Enemy(x, y, Color, id));
        }
        public static void Main() //MAIN -----------------------------------------------------------------------------------------------------
        {
            /*TODO 
              * = to do, % = done, ? = ???
             
             //Basic things
             * Enemies, zombie and eye, maybe EYC //Began //EYC work began
             ? Mining, building, and potions //Began // Most work done? //Only pickaxe ticks
             * Combat
             * Inventory //Begin // Mostly done I guess? //Finished??? Just need more items // of course I still need the FULL ui...
             * Crafting
             * More Worldgeneration stuff //Began
             * Lights/lighting
             % day and night cycle //Partially implemented /// Finished I guess? // Yes, thought there are some problems, which will be fixed by implementing light
             * Projectiles
             * Pathfinding
             * MAKE MORE COMMENTS AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
             * Screen doesn't properly update...

             //Fixes
             % Revamp drawing system to a list where every draw request is placed and then drawn at the end of a tick so stuff doesnt draw on top of eachother, should probably also be in a seperate thread
             ? Maybe reduce number of threads used // technically have done some of that
             * Improve the collision detection for items
            */
            Console.CursorVisible = false;
            // Create Threads
            Thread Audio1 = new(() => {
                while (true)
                {
                    if (World.Time < 3000)
                        Music.Play(Music.Read(1), 4);
                    else
                        Music.Play(Music.Read(0), 4);
                    Thread.Sleep(1000);
                }
            });
            Thread Tick = new(() =>
            {
                while (true)
                {
                    if (!Screen.pause)
                    {
                        TickOnce();
                        Screen.DrawAll(); //Drawing was going to be a seperate thread but thread safety and all that
                        //Finished
                        Thread.Sleep(100); //Tickrate
                    }
                }
            });
            Thread Player = new(() => { listOfEntities[0].Control(); });

            CreatePlayer(50, 20, ConsoleColor.Magenta); //Magenta is the placeholder color for everything // is it though?
            CreateEnemy(100, 20, ConsoleColor.Red, 2);
            listOfEntities.Add(new ItemEntity(55, 15, ConsoleColor.Red, 5));

            Item.PickupItem(-1);
            Item.PickupItem(2);
            for (int i = 0; i < 10; i++)
                Item.PickupItem(1);
            Item.PickupItem(3);
            Item.PickupItem(4);

            if (OperatingSystem.IsWindows())
            {
                try
                {
                    Console.WindowWidth = Screen.screenWidth; // World.worldWidth;
                    Console.WindowHeight = Screen.screenHeight + Screen.topBuffer; // World.worldHeight;
                }
                catch (ArgumentOutOfRangeException) { }
            }
            World.WorldGeneration();
            Console.Clear();

            //READY:
            Audio1.Start();
            Tick.Start();
            Player.Start();

            Screen.AddDraw("0");
            Screen.AddDraw($"5, 50, 20, 45");

            //while (true)
            /*{
                
                Screen.Draw();
                //Screen.NoclipCamera();
            }*/

        }
        public static void RenderWorld() // Doesn't work anymore without a lot of configuring :( //The configuring has been done, which was, in case you're curious, adding the pause bool to the screen class
        {
            for (int i = 0; i < World.worldHeight; i++)
            {
                for (int j = 0; j < World.worldWidth; j++)
                {
                    Console.ForegroundColor = colorInfo[World.worldDATA[World.GetPosition(j, i)]];
                    Program.Write(World.worldDATA[World.GetPosition(j, i)], World.GetPosition(j, i));
                }
                Console.WriteLine();
            }
            Console.SetCursorPosition(0, 0);
        }
        public static void TickOnce() //MAIN LOGIC
        {
            foreach (var Entity in lineOfDeletion.ToList())
            {
                lineOfDeletion.Remove(Entity);
                listOfEntities.Remove(Entity);

                Screen.AddDraw($"2,{(int)Entity.x},{(int)Entity.y}");
            }

            double playerx = -123123;
            double playery = -123213;
            foreach (var Entity in listOfEntities) //Why does this still draw the enity after deletion? //Nvm figured it out
            {
                if (Entity.Id == 0)
                {
                    playerx = Entity.x;
                    playery = Entity.y;
                }
                else
                {
                    if ((int)Entity.x == (int)playerx && (int)Entity.y == (int)playery)
                    {
                        listOfEntities[0].Hit(Entity.Damage);
                        Screen.AddDraw("0");
                    }
                }

                Entity.Ai(); //Player entity also uses this method
                //finish entity
                Screen.AddDraw("6");
            }

            World.Time += 10; //+= 10 is 10X speed //Time goes up by one every 100ms // if every day and night cycle is 10 min : A full cycle is 600 000ms
            if (World.Time > 6000) //Remove 2 zeros due to the code updating every 100ms //Was originally 600000
                World.Time = 0;

            if (savedBackgroundColor != Screen.savedBackgroundColor && ScreenRefresh != 0) //Update background color
            {
                savedBackgroundColor = Screen.savedBackgroundColor;
                Screen.AddDraw("0");
            }

            if (ScreenRefresh == 0) //Periodically refresh the screen to fix artifacts //Helps somewhat
            {
                savedBackgroundColor = Screen.savedBackgroundColor;
                ScreenRefresh = 100;
                Screen.AddDraw("0");
            }
            else
                ScreenRefresh--;
        }
        public static void DeleteEntity(Entities entity)
        {
            lineOfDeletion.Add(entity);
        }
        public static void Write(int tile, int position)
        {
            char[] Levels = { '@', '#', 'H' , '3', 'Y', '+', '<', ':', '.'};
            Random random = new();

            switch (tile)
            {
                default:
                    {
                        //Console.Write(World.worldLevels[position]);
                        //Console.Write(Levels[8-World.worldLevels[position]]);  ///Lighting
                        Console.Write(Levels[random.Next(0, 8)]);
                        break;
                    }
                case 6:
                    {
                        Console.Write("|");
                        break;
                    }
            }
            /*
            if (tile != 0)
                Console.Write("@");
            else
                Console.Write(" ");
            */
        }
    }
}