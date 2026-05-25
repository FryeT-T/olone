using System.Collections.Generic;
using game.Items;
using game.Items.Guns;

namespace game.core
{
    public class Inventory
    {
        public Item[] MainSlots = new Item[15];
        public Weapon[] WeaponSlots = new Weapon[2];
        public int CurrentWeaponIndex = 0;

        public Inventory()
        {
        }
        public int GetFirstEmptyMainSlot()
        {
            for (int i = 0; i < MainSlots.Length; i++)
                if (MainSlots[i] == null) return i;
            return -1;
        }

        public int GetFirstEmptyWeaponSlot()
        {
            for (int i = 0; i < WeaponSlots.Length; i++)
                if (WeaponSlots[i] == null) return i;
            return -1;
        }
        public bool TryAddItem(Item newItem)
        {
            if (newItem is Weapon w)
            {
                int wSlot = GetFirstEmptyWeaponSlot();
                if (wSlot != -1)
                {
                    WeaponSlots[wSlot] = w;
                    return true;
                }
            }

            int mainSlot = GetFirstEmptyMainSlot();
            if (mainSlot != -1)
            {
                MainSlots[mainSlot] = newItem;
                return true;
            }

            return false;
        }

        public void DeleteItemFromCurrentSlot()
        {
            WeaponSlots[CurrentWeaponIndex] = null;
        }
        public void DeleteItemFromMainSlots(int slotNumber)
        {
            MainSlots[slotNumber] = null;
        }
        public void DeleteItemFromWeaponSlots(int slotNumber)
        {
            WeaponSlots[slotNumber] = null;
        }

        public Item TakeItemFromWeaponSlots(int slotNumber)
        {
            var item = WeaponSlots[slotNumber];
            DeleteItemFromWeaponSlots(slotNumber);
            return item;
        }
        public Item TakeItemFromMainSlots(int slotNumber)
        {
            var item = MainSlots[slotNumber];
            DeleteItemFromMainSlots(slotNumber);
            return item;
        }

        public Weapon GetCurrentWeapon() => WeaponSlots[CurrentWeaponIndex];
    }
}