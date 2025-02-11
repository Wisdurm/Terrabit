namespace Terrabit
{
    class Screen
    {
        public static int screenWidth = 60;
        public static int screenHeight = 25;
        public static int cameraX = 50;
        public static int cameraY = 20;
        public static int topBuffer = 3;
        public static int selectedItem;

        public static bool pause = false;

        public static ConsoleColor savedBackgroundColor;
        public static Entities Player = Program.listOfEntities[0];
        public static List<string> drawWaitingLine = new(); // me when line systems :D

        public static ConsoleColor[] consoleColors =
        {
            ConsoleColor.Black, //0
            ConsoleColor.Blue, //1
            ConsoleColor.Cyan, //2
            ConsoleColor.DarkBlue, //3
            ConsoleColor.DarkCyan, //4
            ConsoleColor.DarkGray, //5
            ConsoleColor.DarkGreen, //6
            ConsoleColor.DarkMagenta, //7
            ConsoleColor.DarkRed, //8
            ConsoleColor.DarkYellow, //9
            ConsoleColor.Gray, //A
            ConsoleColor.Green, //B
            ConsoleColor.Magenta, //C
            ConsoleColor.Red, //D
            ConsoleColor.White, //E
            ConsoleColor.Yellow, //F
            //G is empty I guess
            //?? idk hexadecimal is pretty good for what I'm doing // Though it's technically base17 because of course it fuckign is, maybe I should just actually learn file reading
        };
        public static void AddDraw(string draw)
        {
            drawWaitingLine.Add(draw);
        }
        public static void DrawAll() //All entitys and worlds and what not request to draw something by adding it to a line, which is cleared every frame(???) by this code
        { //Which is ran constanly in a seperate thread, maybe, writing this before implementation and maybe in the future I'll reduce the amount of threads used
            List<string> Line = drawWaitingLine.ToList();

            if (Line.Count > 0)
            {
                foreach (string instruction in Line)
                { //Instructions are formatted in DrawID(what draw operation to do) and then all the parameters used
                    var parameters = new object[4]; //Why four? Because its the most parameters used for anything
                    int DrawID;
                    if (instruction.Contains(','))
                    {
                        DrawID = Convert.ToInt32(instruction.Split(',')[0]); // if instruction contains parameters we know the first one is the DrawID
                        for (int i = 0; i < instruction.Split(',').Length - 1; i++)
                        {
                            parameters[i] = instruction.Split(',')[i + 1]; // +1 because instruction.Split(',')[0] is the DrawID or whatever you want to call it, and NOT a parameter
                        }
                    }
                    else
                    {
                        DrawID = Convert.ToInt32(instruction); //if instruction doesn't contain parameters we know all it is IS the DrawID
                    }

                    switch (DrawID)
                    {
                        case 0: // pretty self explanatory if you read the other comments, maybe, idk at least I think I'll be able to decipher it assuming it works 
                            {
                                Draw();
                                break;
                            }
                        case 1:
                            {
                                DrawUi();
                                break;
                            }
                        case 2:
                            {
                                Wipe(Convert.ToInt32(parameters[0]), Convert.ToInt32(parameters[1])); // Im not sure if these NEED to be Convert.ToInt32 but since that fixed code in one place I'm just gonna assume it's better everywhere
                                break;
                            }
                        case 3:
                            {
                                DrawTile(Convert.ToInt32(parameters[0]), Convert.ToInt32(parameters[1]), (ConsoleColor) Enum.Parse(typeof(ConsoleColor),(string)(parameters[2])), (string)parameters[3]);
                                break;
                            }
                        case 4:
                            {
                                UpdateTile(Convert.ToInt32(parameters[0]));
                                break;
                            }
                        case 5:
                            {
                                DrawSprite(Convert.ToInt32(parameters[0]), Convert.ToInt32(parameters[1]), Convert.ToInt32(parameters[2]));
                                break;
                            }
                        case 6:
                            {
                                DrawEntities();
                                break;
                            }
                    }
                }
                drawWaitingLine.Clear();
            }
        }
        public static void Draw()
        {
            if (WithinBounds())
            {
                int tile = World.GetPosition(cameraX - (screenWidth / 2), (cameraY - (screenHeight / 2))); //Drawing starts from this position
                int tileData;

                for (int i = 0; i < screenHeight; i++)
                {
                    for (int j = 0; j < screenWidth; j++)
                    {
                        Console.SetCursorPosition(j, i + topBuffer);
                        try
                        {
                            tileData = World.worldDATA[tile];
                        }
                        catch (IndexOutOfRangeException) { tileData = 0; }

                        BackGroundColor(tileData, tile);

                        Console.ForegroundColor = Program.colorInfo[tileData];
                        Program.Write(tileData, tile);

                        tile++;
                    }
                    tile += World.worldWidth - screenWidth;
                }
                DrawEntities();
                DrawUi();
            }
        }
        public static void DrawUi()
        {
            //Clear everything
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Black;
            string clear = string.Empty;
            for (int j = 0; j < topBuffer; j++)
            {
                for (int i = 0; i < screenWidth; i++)
                {
                    clear += "0"; //You never see this ingame :D
                }
                clear += "\n";
            }
            Console.SetCursorPosition(0, 0);
            Console.Write(clear);

            //BEING UI //GAMMER // *grammar

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            for (int j = 0; j < 4; j += 2)
            {
                for (int i = 0; i < (10 * 2) + 1; i++)
                {
                    Console.SetCursorPosition(i, j);
                    Console.Write("=");
                }
            }
            for (int i = 0; i < (10 * 2) + 1; i += 2)
            {
                Console.SetCursorPosition(i, 1);
                Console.Write("|");
            }
            //Draw items
            for (int i = 0; i < 10; i++)
            {
                if (Inventory.inventory[i] != null)
                {
                    Console.SetCursorPosition(1 + (i * 2), 1);
                    Console.ForegroundColor = Inventory.inventory[i].color;

                    if (i == selectedItem)
                        Console.BackgroundColor = ConsoleColor.DarkCyan;
                    else
                        Console.BackgroundColor = ConsoleColor.Black;

                    if (Inventory.inventory[i].stackable)
                    {
                        if (Inventory.inventory[i].amount <= 9)
                            Console.Write(Inventory.inventory[i].amount);
                        else
                            Console.Write(Convert.ToString(Inventory.inventory[i].amount)[0]);
                    }
                    else
                        Console.Write(Inventory.inventory[i].sprite);
                }
            }

            Console.BackgroundColor = ConsoleColor.Black;
            for (int i = 0; i < (Player.health / Player.maxhealth) * 21; i++) //Draw Healh Bar
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.SetCursorPosition(screenWidth-25 + i, 1);
                Console.Write("0");
            }
        }
        public static void BackGroundColor(int tileData, int tile)
        {
            if (tileData == 0) //Change to -1 to disable this completely
            {
                if (tile < (World.GetLayer(World.dirtHeight)))
                {
                    switch (World.Time) //300 000 is mid-day, 0 and 600 000 are midnight
                    { //6000 is a full day
                        case (>= 5250):
                            {
                                Console.BackgroundColor = ConsoleColor.DarkBlue;
                                savedBackgroundColor = Console.BackgroundColor;
                                break;
                            }
                        case (>= 3750):
                            {
                                Console.BackgroundColor = ConsoleColor.Blue;
                                savedBackgroundColor = Console.BackgroundColor;
                                break;
                            }
                        case (>= 3000):
                            {
                                Console.BackgroundColor = ConsoleColor.DarkBlue;
                                savedBackgroundColor = Console.BackgroundColor;
                                break;
                            }
                        default:
                            {
                                Console.BackgroundColor = ConsoleColor.Black;
                                savedBackgroundColor = Console.BackgroundColor;
                                break;
                            }
                    }
                }
                else
                    Console.BackgroundColor = ConsoleColor.DarkGray;
            }
            else
                Console.BackgroundColor = ConsoleColor.Black;
        }
        public static void NoclipCamera()
        {
            var input = Console.ReadKey(false);
            switch (input.Key)
            {
                case ConsoleKey.UpArrow:
                    {
                        if (cameraY > 0 + (screenHeight / 2) + 1)
                            cameraY--;
                        break;
                    }
                case ConsoleKey.DownArrow:
                    {
                        if (cameraY < World.worldHeight - (screenHeight / 2) - 1)
                            cameraY++;
                        break;
                    }
                case ConsoleKey.LeftArrow:
                    {
                        cameraX -= 2;
                        break;
                    }
                case ConsoleKey.RightArrow:
                    {
                        cameraX += 2;
                        break;
                    }
            }
        }
        public static int[] GetPosition(int x, int y)
        {
            int[] position = new int[2];

            position[0] = (screenWidth / 2) + (x - cameraX); //Find position relative to camera
            position[1] = (screenHeight / 2) + (y - cameraY);

            return position;
        }
        public static void DrawEntities()
        {
            foreach (var Entity in Program.listOfEntities)
            {
                Entity.DrawEntityOnScreen();
            }
        }
        public static int GetWorldPosition(int x, int y)
        {
            return World.GetPosition((cameraX - (screenWidth / 2)) + x, (cameraY - (screenHeight / 2)) + y);
        }
        public static void Wipe(int x, int y)
        {
            int tile = World.worldDATA[World.GetPosition(x, y)]; // Using both these methods together?
            int[] position = GetPosition(x, y); // Wow...
            Console.SetCursorPosition(position[0], position[1]);

            BackGroundColor(tile, World.GetPosition(x, y));
            Console.ForegroundColor = Program.colorInfo[tile];
            Program.Write(tile, World.GetPosition(x, y));
        }
        public static bool WithinBounds()
        {
            return ((cameraY > 0 + (screenHeight / 2) + 1) && (cameraY < World.worldHeight - (screenHeight / 2) - 1)) && ((cameraX > 0 + (screenWidth / 2) + 1) && (cameraX < World.worldWidth - (screenWidth / 2) - 1));
            //*grumble grumble*
        }
        public static void DrawTile(int x, int y, ConsoleColor Color, string material)
        {
            //while (Drawing) { } //Simple changes can have huge benefits //What the fuck is this code????? //nvm figured it out

            int[] xy = GetPosition((int)x, (int)y); //just discovered you can do (int)"number"

            if (Onscreen(x, y))
            {
                Console.SetCursorPosition(xy[0], xy[1] + topBuffer);
                BackGroundColor(World.worldDATA[GetWorldPosition(xy[0], xy[1])], World.GetPosition(x, y));
                Console.ForegroundColor = Color;
                Console.Write(material);
            }
        }
        public static void UpdateTile(int tile)
        {
            int[] xy = World.GetXY(tile);
            DrawTile(xy[0], xy[1], Program.colorInfo[Convert.ToInt32(World.worldDATA[tile])], "&");  // Why '&'
        }
        public static bool Onscreen(int x, int y)
        {
            int[] xy;
            xy = GetPosition((int)x, (int)y);

            return ((xy[0] > -1 && xy[0] < screenWidth) && (xy[1] > -1 && xy[1] < screenHeight));
        }
        public static void DrawSprite(int startx, int starty, int rotation) //Angles not radians
        {
            double radian = (Math.PI * rotation / 180); //Now we convert the angles to radians, also fun fact it's been like a week since I last worked on this as of writing this
            string[] image = File.ReadAllLines("data/Eye.consoleimage");
            int x = GetPosition(startx, starty)[0] - (image[0].Length / 2); //Get left //Starting x
            int y = GetPosition(startx, starty)[1] - (image.Length / 2); //Get top //Starting y
            //We now have the top left position to start drawing on the screen, code will HAVE to be rewritten for rotations so everything doesn't rotate around the top left
            if (Onscreen(startx, starty))
            {
                for (int Y = 0; Y < image.Length; Y++)
                {
                    for (int X = 0; X < image[0].Length; X++)
                    {
                        try
                        {
                            Console.SetCursorPosition(x + (int)(X * Math.Cos(radian) + Y * Math.Sin(radian)), y + (int)(-X * Math.Sin(radian) + Y * Math.Cos(radian))); //oh boy rotation...
                            Console.ForegroundColor = consoleColors[GetIntFromHex(image[Y].ElementAt(X))]; // :)
                            Console.Write("#");
                        }
                        catch (IndexOutOfRangeException) { } //this isn't an error it's just an efficient way of doing what I want it to do
                        catch (ArgumentOutOfRangeException) { } //The code to do this myself would be alot longer, complex, and difficult to write
                    }
                }
            }
        }
        public static int GetIntFromHex(char hex)
        {
            char[] Hexes = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G' }; //G? Yes, read the text at the top of the class
            return Array.IndexOf(Hexes, hex);
        }
    }
}
