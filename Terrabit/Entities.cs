namespace Terrabit
{
    public abstract class Entities
    {
        public double x { get; set; }
        public double y { get; set; }
        public ConsoleColor EntityColor { get; set; }
        int[] xy = { -123 };
        public int yVelocity = 0;
        public int Id;
        public int Type;
        public int health;
        public int Damage;
        public int airTime;
        public char sprite;
        public double maxhealth; //Why is this a decimal value? So the player healthbar can work correctly with the stupid integer division
        public Entities(int X, int Y, ConsoleColor Colour, int id)
        {
            this.x = X;
            this.y = Y;
            this.EntityColor = Colour;
            this.Id = id;
            this.sprite = 'Y';
        }
        public void DrawEntityOnScreen()
        {
            if (xy[0] != -123 && ((xy[0] > -1 && xy[0] < Screen.screenWidth) && (xy[1] > -1 && xy[1] < Screen.screenHeight))) //Clear last position on screen
            {
                Console.SetCursorPosition(xy[0], xy[1] + Screen.topBuffer);
                int oldData = World.worldDATA[Screen.GetWorldPosition(xy[0], xy[1])];

                Screen.BackGroundColor(oldData, Screen.GetWorldPosition(xy[0], xy[1]));
                Console.ForegroundColor = Program.colorInfo[oldData];
                Program.Write(oldData, Screen.GetWorldPosition(xy[0], xy[1]));
            }

            xy = Screen.GetPosition((int)x, (int)y); //just discovered you can do (int)"number"

            if ((xy[0] > -1 && xy[0] < Screen.screenWidth) && (xy[1] > -1 && xy[1] < Screen.screenHeight))
            {
                Console.SetCursorPosition(xy[0], xy[1] + Screen.topBuffer);

                if (!Inventory.entityOutlines)
                    Screen.BackGroundColor(World.worldDATA[Screen.GetWorldPosition(xy[0], xy[1])], World.GetPosition((int)x, (int)y));
                else //Pretty self explanatory, entity outlines
                {
                    switch (Type)
                    {
                        case 0: //Player
                            {
                                Console.BackgroundColor = ConsoleColor.Blue;
                                break;
                            }
                        case 1: //Enemies
                            {
                                Console.BackgroundColor = ConsoleColor.DarkRed;
                                break;
                            }
                    }
                }
                Console.ForegroundColor = EntityColor;
                Console.Write(sprite);
            }
            else if (Id == 0)
            {
                Screen.cameraX = (int)x;
                Screen.cameraY = (int)y;
                Screen.Draw(); //This is Draw() not AddDraw("0") because that doesnt work here even after a bunch of attempts to get it working in this context/thread/place
            }
        }
        public void Gravity()
        {
            if (YValueIsEmpty(1) || yVelocity > 0)
            {
                if (yVelocity != 0)
                    airTime++;
                if (yVelocity > -10)///Terminal velocity
                    yVelocity--;
                for (int i = 0; i < Math.Abs(yVelocity); i++)
                {
                    if (yVelocity > 0 && YValueIsEmpty(-1))
                    {
                        y -= 0.25;
                        if (!YValueIsEmpty(0))
                            y += 0.25;
                    }
                    else if (yVelocity < 0 && YValueIsEmpty(1))
                    {
                        y += 0.25;
                        if (!YValueIsEmpty(0))
                            y -= 0.25;
                    }
                }
            }
            else if (!YValueIsEmpty(1))
            {
                if (yVelocity < -8 && Type == 0)
                {
                    health -= (int)(airTime * 1.5);
                    Screen.AddDraw("1");
                }
                yVelocity = 0;
            }
        }
        public bool YValueIsEmpty(int value)
        {
            return World.worldDATA[World.GetPosition((int)x, (int)y + value)] == 0;
        }
        public bool XValueIsEmpty(int value)
        {
            return World.worldDATA[World.GetPosition((int)x + value, (int)y)] == 0;
        }
        public void Move(int direction, double amount)
        {
            switch (direction)
            {
                case 1:
                    {
                        if (YValueIsEmpty(-1))
                            y -= amount;
                        break;
                    }
                case 2:
                    {
                        if (YValueIsEmpty(1))
                            y += amount;
                        break;
                    }
                case 3:
                    {
                        if (XValueIsEmpty(-1))
                            x -= amount;
                        break;
                    }
                case 4:
                    {
                        if (XValueIsEmpty(1))
                            x += amount;
                        break;
                    }
            }
        }
        public virtual void Control() { }
        public virtual void Ai() { }
        public virtual void Hit(int damage) { }
    }
    class Player : Entities
    {
        int action = -1;
        int Iframes = 0;
        int usetimeDelay = 0;
        int inventoryslot;
        Item item;

        public Player(int X, int Y, ConsoleColor Colour, int id) : base(X, Y, Colour, id)
        {
            x = X;
            y = Y;
            EntityColor = Colour;
            Id = id;
            Type = 0;

            health = 100;
            airTime = 0;
            maxhealth = 100;
        }
        public override void Control()
        {
            while (true)
            {
                var input = Console.ReadKey(true);
                if (!Screen.pause)
                {
                    switch (input.Key)
                    {
                        case ConsoleKey.W:
                            {
                                if (!YValueIsEmpty(1))
                                    yVelocity = 3;
                                break;
                            }
                        case ConsoleKey.S: //Unused for now
                            {
                                break;
                            }
                        case ConsoleKey.A:
                            {
                                Move(3, 0.5);
                                break;
                            }
                        case ConsoleKey.D:
                            {
                                Move(4, 0.5);
                                break;
                            }
                        case ConsoleKey.C:
                            {
                                Screen.cameraX = (int)x;
                                Screen.cameraY = (int)y;
                                Screen.AddDraw("0");
                                break;
                            }
                        case ConsoleKey.UpArrow:
                            {
                                Action(1, action);
                                break;
                            }
                        case ConsoleKey.DownArrow:
                            {
                                Action(2, action);
                                break;
                            }
                        case ConsoleKey.LeftArrow:
                            {
                                Action(3, action);
                                break;
                            }
                        case ConsoleKey.RightArrow:
                            {
                                Action(4, action);
                                break;
                            }
                        case ConsoleKey.D1 or ConsoleKey.D2 or ConsoleKey.D3 or ConsoleKey.D4 or ConsoleKey.D5 or ConsoleKey.D6 or ConsoleKey.D7 or ConsoleKey.D8 or ConsoleKey.D9:
                            {
#pragma warning disable CS8604 // Possible null reference argument.
                                char number = Convert.ToString(input.Key).Last();
                                inventoryslot = (Convert.ToInt32(Convert.ToString(number))) - 1;
                                Screen.selectedItem = inventoryslot;
                                Screen.AddDraw("1");

                                if (Inventory.inventory[inventoryslot] != null)
                                {
                                    action = Inventory.inventory[inventoryslot].type;
                                    item = Inventory.inventory[inventoryslot];
                                }
                                else
                                    action = -1;
                                break;
                            }
                    }
                }
            }
        }
        public int Directed(int direction)
        {
            int X = 0;
            int Y = 0;
            switch (direction)
            {
                case 1:
                    {
                        Y--;
                        break;
                    }
                case 2:
                    {
                        Y++;
                        break;
                    }
                case 3:
                    {
                        X--;
                        break;
                    }
                case 4:
                    {
                        X++;
                        break;
                    }
            }
            return World.GetPosition((int)x + X, (int)y + Y);
        }
        public override void Hit(int damage)
        {
            if (Iframes == 0)
            {
                Iframes = 5;
                health -= damage;
            }

        }
        public void Action(int direction, int index)
        {
            //Directions 
            /*      1
             *    3   4
             *      2
             */
            if (usetimeDelay == 0)
            {
                switch (index)
                {
                    case 0: //Potions and such
                        {
                            Thread BuffT = new(() => { Buff(direction, item.buffId); });
                            usetimeDelay += item.Usetime;
                            BuffT.Start();
                            break;
                        }
                    case 1: //Breaking blocks
                        {
                            usetimeDelay += item.Usetime;
                            Mine(direction, item.power, 1);  //Power and type come from held item
                            break;
                        }
                    case 2: //Placing blocks
                        {
                            Build(direction, item.tileId, 0); //tile comes from held item //Power is used for blockreplacing
                            break;
                        }
                    case 3: //Att+áck§§ // ? what happened here
                        {
                            usetimeDelay += item.Usetime;
                            break;
                        }
                    case 4: //Spawning projectiles
                        {
                            usetimeDelay += item.Usetime;
                            break;
                        }
                }
            }
        }
        public void Buff(int direction, int id)
        {
            switch (id)
            {
                case 0:
                    {
                        if (health != maxhealth)
                        {
                            health += 50;
                            if (health > maxhealth)
                                health = (int)maxhealth;
                            UseItem();
                            Effect(direction, 100);
                        }
                        break;
                    }
                case 1:
                    {
                        if (health != maxhealth)
                        {
                            health += 100;
                            if (health > maxhealth)
                                health = (int)maxhealth;
                            UseItem();
                            Effect(direction, 100);
                        }
                        break;
                    }
            }
        }
        public void Mine(int direction, int power, int type)
        {
            if (type == 1) //Pickaxe
            {
                if (World.worldDATA[Directed(direction)] == 6 || World.worldDATA[(Directed(direction)) - World.worldWidth] == 6)
                {
                    int i;
                    if (World.worldDATA[Directed(direction)] == 6)
                        i = Directed(direction);
                    else
                        i = (Directed(direction)) - World.worldWidth;
                    while (World.worldDATA[i] == 6)
                    {
                        World.worldDATA[i] = 0;
                        Screen.AddDraw($"4,{i}");
                        i -= World.worldWidth;
                    }
                }
                World.worldDATA[Directed(direction)] = 0;
                Screen.AddDraw($"4,{Directed(direction)}"); // Why is this code so buggy?... I assume just problems with the console cursor being thrown around //Fixed... I assume...
            }
            else //Axe
            {

            }
        }
        public void Build(int direction, int tile, int power)
        {
            int targetTile = World.worldDATA[Directed(direction)];
            bool CanBuild = true;
            foreach (var Entity in Program.listOfEntities)
            {
                if (World.GetPosition((int)Entity.x, (int)Entity.y) == Directed(direction))
                {
                    CanBuild = false;
                    break;
                }
            }

            if (CanBuild && (targetTile == 0))
            {
                usetimeDelay += item.Usetime;
                World.worldDATA[Directed(direction)] = tile;
                Screen.AddDraw($"4,{Directed(direction)}");
                UseItem();
            }
            else if (Inventory.blockReplacing)
            {
                Mine(direction, power, 1);
                targetTile = World.worldDATA[Directed(direction)];

                if (CanBuild && targetTile == 0) //If block was succesfully mined within that strike, then build the thing
                {
                    usetimeDelay += item.Usetime;
                    World.worldDATA[Directed(direction)] = tile;
                    Screen.AddDraw($"4,{Directed(direction)}");
                    UseItem();
                }
            }
        }
        public override void Ai() //The player doesn't have AI, but since it gets called every tick, might as well use it for something
        {
            if (Iframes > 0)
                Iframes--;
            if (usetimeDelay > 0)
                usetimeDelay--;
            Gravity();
        }
        public void UseItem()
        {
            if (!(item.Consume()))
            {
                inventoryslot = 0;
                action = 0;
            }
        }
        public void Effect(int direction, int MSduration) //Quick flash of color next to player, simulates potion drinking animations and swords(?)
        {
            int tile = Directed(direction);
            int[] xy = World.GetXY(tile);
            Screen.AddDraw($"3,{xy[0]},{xy[1]},{item.color},A");

            Thread.Sleep(MSduration);

            Screen.AddDraw("4," + tile);
        }
    }
    class Enemy : Entities
    {
        readonly int AiStyle; //Not sure why this is readonly but it works so might as well be
        public Enemy(int X, int Y, ConsoleColor Colour, int id) : base(X, Y, Colour, id)
        {
            x = X;
            y = Y;
            EntityColor = Colour;
            Id = id;
            Type = 1;
            Damage = 10;
            health = 10;

            AiStyle = id;
        }
        public override void Ai()
        {
            switch (AiStyle)
            {
                case 1: //Simple warrior Ai// Move towards player, no pathfinding
                    {
                        if (Program.listOfEntities[0].x < x)
                        {
                            if (!XValueIsEmpty(-1) && !YValueIsEmpty(1))
                                yVelocity = 3;
                            Move(3, 0.25);
                        }
                        else
                        {
                            if (!XValueIsEmpty(1) && !YValueIsEmpty(1))
                                yVelocity = 3;
                            Move(4, 0.25);
                        }

                        Gravity();
                        break;
                    }
                case 2: //Simple flying AI//
                    {
                        if (Program.listOfEntities[0].x < x)
                            Move(3, 0.25);
                        else
                            Move(4, 0.25);

                        if (Program.listOfEntities[0].y < y)
                            Move(1, 0.25);
                        else
                            Move(2, 0.25);

                        break;
                    }
            }
        }

    }
    class Projectile : Entities
    {
        public Projectile(int X, int Y, ConsoleColor Colour, int id) : base(X, Y, Colour, id)
        {
            x = X;
            y = Y;
            EntityColor = Colour;
            Id = id;
            Type = 2;
        }
    }
    class ItemEntity : Entities
    {
        public ItemEntity(int X, int Y, ConsoleColor Colour, int id) : base(X, Y, Colour, id)
        {
            x = X;
            y = Y;
            Type = 3;

            Item item = new();
            item.SetStats(id);
            EntityColor = item.color;
            sprite = item.sprite;
        }
        public override void Ai()
        {
            Gravity();

            if ((Math.Abs(Program.Player.x - Math.Round(this.x)) + Math.Abs(Program.Player.y - Math.Round(this.y))) < 2) //If close enough to player, get picked up
            {
                Item.PickupItem(this.Id); //"this." isnt necessay but it does help with readability and doesnt really matter since the compiler doesnt care //Fine...  I guess
                Program.DeleteEntity(this);
                Screen.AddDraw("1");
            }

        }
    }
}