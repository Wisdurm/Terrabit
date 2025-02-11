namespace Terrabit
{
    public class World
    {
        public static int worldWidth = 200;
        public static int worldHeight = 100;
        public static int[] worldDATA = new int[worldHeight * worldWidth];
        public static int[] worldLevels = new int[worldWidth * worldHeight];
        public static String[] metaDATA = new String[worldHeight * worldWidth];

        public static int dirtHeight = 2;
        public static int Time = 3750;
        public static int SeededCount;
        public static void WorldGeneration()
        {
            SeededCount = 2;
            int counter;

            for (int i = 0; i < worldDATA.Length; i++) //create dirt and stone
            {
                if (i < GetLayer(dirtHeight))
                {
                    worldDATA[i] = 1;
                }
                else
                    worldDATA[i] = 2;

                metaDATA[i] = "00";
            }

            CreatePockets(worldWidth / 8, 1, (worldHeight / dirtHeight) - 5, worldHeight, 1); //creates pockets of dirt in stone
            CreatePockets(worldWidth / 10, 2, 1, worldHeight / dirtHeight, 2); //creates pockets of stone in dirt

            CreatePockets(worldWidth / 7, 4, (worldHeight / dirtHeight) - 10, worldHeight - 5, 3); //creates pockets of ore

            CreatePockets(worldWidth / 10, 0, (worldHeight / dirtHeight), worldHeight - 5, 4); //create caves
            CreatePockets(1, 0, (worldHeight / 5), worldHeight / 4, 5); //create large main cave

            for (int i = 0; i < worldDATA.Length; i++) //creates air
            {
                if (i < GetLayer(5))
                    worldDATA[i] = 0;
            }

            Random rand = new();
            int offset = rand.Next(0, 50);
            for (int i = 0; i < worldWidth; i += 6) //hills
            {
                DrawCircle(i, Convert.ToInt32(worldHeight / 5 + (Math.Sin(i + offset) + 0.9 * Math.Sin(i * 0.8))), 0, 4);
            }

            //structures here

            for (int i = 0; i < worldWidth; i++) //grass floats down from the heavens
            {
                counter = 0;
                while (worldDATA[GetPosition(i, counter)] == 0)
                {
                    counter++;
                    if (worldDATA[GetPosition(i, counter)] == 1)
                        worldDATA[GetPosition(i, counter)] = 3;
                    if (counter > worldDATA.Length)
                        break;
                }
            }

            for (int i = 0; i < 5; i++) //This is slow but since it's only done once on generation so it's alright for what it is
            {
                EmptyDataFill(); //Grass
                Grass();
            }

            for (int i = 2; i < worldWidth; i += rand.Next(3, 15)) //Trees
            { // ^ this is X
                counter = 0; //This is y
                while (worldDATA[GetPosition(i, counter)] == 0)
                {
                    counter++;
                    if (worldDATA[GetPosition(i, counter)] == 3)
                    {
                        for (int i2 = counter - 1; i2 > counter - rand.Next(4, 10); i2--)
                            worldDATA[GetPosition(i, i2)] = 6;
                    }
                    if (counter > worldDATA.Length)
                        break;
                }
            }

            //GenerateFullLighting();
        }
        public static int GetLayer(int divider)
        {
            int value = Convert.ToInt32((Math.Floor(Convert.ToDouble(worldHeight) / divider) * worldWidth) + worldWidth);
            return value;
        }
        public static int GetPosition(int x, int y) //returns value in world list
        {
            int value = (y * worldWidth) + x;
            if (value < worldDATA.Length)
                return value;
            else
                return worldDATA.Length - 1;
        }
        public static void DrawCircle(int X, int Y, int TileID, int size) //drawSquare, getLayer, getPosition and insideCircle methods are reused from an earlier game I made, fun fact
        {
            int pointer = GetPosition(X, Y);

            if (size <= 1)
            {
                DrawTile(pointer, TileID);
            }
            else
            {
                for (int y = 0; y < worldHeight; y++)
                {
                    for (int x = 0; x < worldWidth; x++)
                        if (InsideCircle(X, Y, x, y, size))
                            DrawTile(GetPosition(x, y), TileID);
                }
            }

        }
        public static bool InsideCircle(int Xcenter, int Ycenter, int Xpoint, int Ypoint, int radius)
        {
            double dx = Xcenter - Xpoint;
            double dy = Ycenter - Ypoint;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            return distance < radius;
        }
        public static void CreatePockets(int Amount, int Material, int minHeight, int maxHeight, int Style)
        {
            Random random = new(); //replace with seeded random method/class later
            int x;
            int y;
            int size;
            int max;

            for (int i = 0; i < Amount; i++)
            {
                x = random.Next(0, worldWidth + 1);
                y = random.Next(minHeight, maxHeight);

                switch (Style)
                {
                    case 1: //dirt pockets
                        {
                            size = random.Next(3, 5); //3-4 NOT 3-5
                            max = 6;

                            while (size > 1 || max > 0)
                            {
                                DrawCircle(x, y, Material, size);
                                if (random.Next(1, 3) == 1) // if 1 in 2 is 1
                                    size--;
                                max--; //ensure pockets don't continue on forever
                                y++;
                                x += random.Next(-2, 3);
                            }

                            break;
                        }
                    case 2: //stone pockets
                        {
                            size = 4 - (((worldHeight / 3) + 3) - y) / 5; //size starts smaller the higher up you are
                            max = 5;

                            while (size > 1 || max > 0)
                            {
                                DrawCircle(x, y, Material, size);
                                if (random.Next(1, 3) == 1) // if 1 in 2 is 1
                                    size--;
                                max--;
                                y++;
                                x += random.Next(-2, 3);
                            }

                            break;
                        }
                    case 3: //ore
                        {
                            size = random.Next(2, 4); //2-3 NOT 2-4
                            max = random.Next(1, 4);

                            while (max > 0)
                            {
                                DrawCircle(x, y, Material, size);
                                max--;
                                y++;
                                x += random.Next(-1, 2);
                            }

                            break;
                        }
                    case 4: //caves
                        {
                            size = random.Next(3, 5); //3-4 NOT 3-5
                            max = 7;

                            while (size > 1 || max > 0)
                            {
                                DrawCircle(x, y, Material, size);
                                if (random.Next(1, 4) == 1) // if 1 in 3 is 1
                                    size--;
                                max--;
                                y += random.Next(1, 3);
                                x += random.Next(-3, 4);
                            }

                            break;
                        }
                    case 5: //Main tunnel
                        {
                            size = random.Next(4, 6); //4-5 NOT 4-6
                            max = worldHeight / 4;

                            while (size > 1 || max > 0)
                            {
                                DrawCircle(x, y, Material, size);
                                if (random.Next(1, 4) == 1 && max < worldHeight / 8) // if 1 in 3 is 1
                                    size--;
                                max--;
                                y += random.Next(2, 4);
                                x += random.Next(-7, 8);
                            }

                            break;
                        }
                }
            }
        }
        public static void DrawTile(int gridValue, int material)
        {
            try
            {
                worldDATA[gridValue] = material;
            }
            catch (IndexOutOfRangeException) { }
        }
        public static void EmptyDataFill()
        {
            for (int i = 0; i < worldDATA.Length; i++)
            {
                if (worldDATA[i] == 1 || worldDATA[i] == 3) //MASSIVE optimization since this code is currently only used for grass, try changing the one to a zero
                {
                    if (Touching(i, 0))// I promise I tried to find better alternatives first // I have since improved the code alot, used to be alot of "if"
                    {
                        char[] data = metaDATA[i].ToCharArray();
                        data[0] = '1';
#pragma warning disable CS8601 // Possible null reference assignment.
                        metaDATA[i] = data.ToString();
                    }
                }
            }
        }
        public static void Grass()
        {
            for (int i = 0; i < worldDATA.Length; i++)
            {
                if (worldDATA[i] == 1 && metaDATA[i].StartsWith("1") && Touching(i, 3))
                    worldDATA[i] = 3;
            }
        }
        public static bool Touching(int tileid, int material) //Unoptimized yet still somewhat optimized due to being a method
        {
            int[] xy = GetXY(tileid);
            int x = xy[0];
            int y = xy[1];

            for (int X = x - 1; X < x + 2; X++)
            {
                for (int Y = y - 1; Y < y + 2; Y++)
                {
                    try
                    {
                        if (worldDATA[GetPosition(X, Y)] == material && !(X == x && Y == y))
                        {
                            return true;
                        }
                    }
                    catch (Exception) { }
                }
            }
            return false;
        }
        public static int[] GetXY(int tileid)
        {
            int[] xy = new int[2];
            //We know the width and height of the world, so we can use that to obtain the X and Y of a grid position
            xy[0] = tileid % worldWidth; //X position
            //Then the Y
            xy[1] = tileid / worldWidth; //Y position
            // While I may have figured out GetPosition, this code was figured out thanks to Youtube

            return xy;
        }
        public static void GenerateFullLighting()
        {
            for (int i = 0; i < worldDATA.Length; i++) //creates lighting from the sun, in regards to how all this will be cleared at night time, no idea...
            {
                worldLevels[i] = 8;
            }

            for (int i = 0; i < worldDATA.Length; i++) //spreads lighting
            {
                Light(i);
            }
        }
        public static void Light(int position) //Why does this use position instead of X Y? Because if it used X Y it would convert it back and forth a few times which would be stupid
        {
            int x = GetXY(position)[0];
            int y = GetXY(position)[1];
            int position2;

            for (int X = x - 1; X < x + 2; X++)
            {
                for (int Y = y - 1; Y < y + 2; Y++)
                {
                    if (!(X == x) || !(Y == y))
                    {
                        position2 = GetPosition(X, Y);
                        try
                        {
                            if (worldDATA[position2] == 0) //if air
                                break;
                            else //if solid
                                break;

                        }
                        catch (Exception) { }
                    }
                }
            }
        }
    }
}