namespace Terrabit
{
    internal class Inventory //Why is this a separate class? Organization. Like I've ever been good at that...
    {
        ///CONFIG:
        public static bool autoJump;
        public static bool blockReplacing = false;
        public static bool entityOutlines = true;
        public static bool healthBar; //???
        //Inventory
        public static Item[] inventory = new Item[10];
        public static int[] inventoryIDs = new int[10];
    }
    class Item
    {
        public int id;
        public string name; //the name
        public string description; //the        description
        public int type; //The meat and potatoes. Potion, axe, sword, bow, ect.
        public ConsoleColor color; //Colour in inventory
        public int projectileId; //Projectile to shoot
        public int tileId; //Tile to be placed
        public int buffId; //Potion
        public int power; //Pickaxe power or damage // interchangeable because tools can't be used as weapons
        public int Usetime; //Cooldown
        //public int rarity; //idk man...
        public int[] recipe; //recipe
        public int defense; //if accesory
        public int enemyId; //boss summons

        public bool consumable; //Removed on usage
        public bool stackable;
        public char sprite = '#'; //Sprite "has" to have a default, since not including it would require extra code in the drawing of entities

        public int amount = 0; // Amount of said object in inventory slot

        public void SetStats(int Id) //Am I really going to do this...
        {
            id = Id;
            switch (Id) //If future me or someone else is reading this who is more experienced with C#, OOP, or just programming in general, I'm sorry.
            {
                default:
                    {
                        this.name = "missing item";
                        this.description = "unknown id";
                        this.sprite = '?';
                        this.color = ConsoleColor.Black;
                        break;
                    }
                case 0:
                    {
                        this.name = "broken";
                        break;
                    }
                case -1:
                    {
                        this.type = 3; //Melee Weapon
                        this.name = "Aleph";
                        this.description = "The first item";
                        this.power = 0; //Damage
                        this.Usetime = 0;
                        this.color = ConsoleColor.Gray;
                        this.sprite = 'I';
                        break;
                    }
                case 1:
                    {
                        this.type = 2; //Block
                        this.name = "Dirt";
                        this.description = "A dirt block";
                        this.tileId = 1;
                        this.color = ConsoleColor.DarkYellow;
                        this.stackable = true;
                        this.consumable = true;
                        this.Usetime = 2; // 10 usetime is 1 second

                        //this.sprite = ' '; Stackable items always display stack amount instead of sprite
                        break;
                    }
                case 2:
                    {
                        this.type = 1; //Pickaxe
                        this.name = "Gold PickAxe"; //...
                        this.description = "A golden axe"; //These are just temporary... I hope...
                        this.color = ConsoleColor.Yellow;
                        this.sprite = 'T';
                        this.Usetime = 5;
                        this.power = 1; //bruhhhh
                        /// Power scales
                        /// 0 = basic
                        /// 1 = Gold
                        /// idk I'll continue this
                        break;
                    }
                case 3:
                    {
                        this.type = 0; //Potion
                        this.name = "Lesser Healing Potion"; //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
                        this.color = ConsoleColor.Red;
                        this.consumable = true;
                        this.stackable = true;
                        this.buffId = 0;
                        // this.sprite = 'ϫ'; //Ϫ // Potions stack, this cool character goes unused :( // apparently the console doesn't (seemingly?) use UTF-8 so it wouldn't work anyway
                        break;
                    }
                case 4:
                    {
                        this.type = 0;
                        this.name = "Healing Potion";
                        this.color = ConsoleColor.Red;
                        this.consumable = true;
                        this.stackable = true;
                        this.buffId = 1;
                        break;
                    }
                case 5:
                    {
                        this.type = 2; //Block
                        this.name = "Stone";
                        this.description = "A stone block";
                        this.tileId = 2;
                        this.color = ConsoleColor.Gray;
                        this.stackable = true;
                        this.consumable = true;
                        this.Usetime = 2; // 10 usetime is 1 second
                        break;
                    }
            }
        }
        public static void PickupItem(int id)
        {
            Item item = new();
            item.SetStats(id);

            if (Array.IndexOf(Inventory.inventoryIDs, id) == -1 || !item.stackable)
            {
                int emptySlot = Array.IndexOf(Inventory.inventory, null);
                if (emptySlot != -1)
                {
                    Inventory.inventory[emptySlot] = item;
                    Inventory.inventory[emptySlot].amount++;
                    Inventory.inventoryIDs[emptySlot] = id;
                }
            }
            else
            {
                int slot = Array.IndexOf(Inventory.inventoryIDs, id);
                Inventory.inventory[slot].amount++;
            }
        }
        public bool DoIExist() //existential pog :O
        {
            if (this.amount < 1)
            {
                int index = Array.IndexOf(Inventory.inventory, this);
                Inventory.inventory[index] = null;
                Inventory.inventoryIDs[index] = -1;
                return false;
            }
            else
                return true;
        }
        public bool Consume()
        {
            if (this.consumable)
                this.amount--;
            bool DoI = DoIExist();
            if (!DoI || this.consumable)
                Screen.AddDraw("1");
            return DoI;
        }
    }
}
