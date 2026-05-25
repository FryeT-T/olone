using game.Items;
using game.Weapons;
using System.Drawing;
using System.Windows.Forms;

namespace game.core
{
    public partial class GameWindow
    {
        private void UpdateInventoryLogic()
        {
            invStartX = WindowWidth / 2 - 245;
            invStartY = WindowHeight / 2 - 150;
            wpnStartX = invStartX;
            wpnStartY = invStartY + 3 * (invCellSize + invPad) + 30;

            int mx = InputManager.MouseX;
            int my = InputManager.MouseY;

            if (InputManager.IsMouseLeftPressed && draggedItem == null)
                CheckSlotInteraction(mx, my, true);
            else if (!InputManager.IsMouseLeftPressed && draggedItem != null)
                DropDraggedItem(mx, my);

            if (InputManager.IsMouseRightPressed)
            {
                CheckSlotInteraction(mx, my, false);
                System.Threading.Thread.Sleep(150);
            }
        }

        private void CheckSlotInteraction(int mx, int my, bool isLeft)
        {
            for (int i = 0; i < 15; i++)
            {
                Rectangle r = new Rectangle(invStartX + (i % 5) * (invCellSize + invPad), invStartY + (i / 5) * (invCellSize + invPad), invCellSize, invCellSize);
                if (r.Contains(mx, my)) { if (isLeft) StartDragging(i, false); else QuickMove(i, false); return; }
            }
            for (int i = 0; i < 2; i++)
            {
                Rectangle r = new Rectangle(wpnStartX + i * (invCellSize + invPad), wpnStartY, invCellSize, invCellSize);
                if (r.Contains(mx, my)) { if (isLeft) StartDragging(i, true); else QuickMove(i, true); return; }
            }
        }

        private void StartDragging(int idx, bool isWpn)
        {
            Item item = isWpn ? player.Inventory.WeaponSlots[idx] : player.Inventory.MainSlots[idx];
            if (item == null) return;
            draggedItem = item; sourceSlotIndex = idx; isFromWeaponSlot = isWpn;
            if (isWpn) player.Inventory.WeaponSlots[idx] = null; else player.Inventory.MainSlots[idx] = null;
        }

        private void DropDraggedItem(int mx, int my)
        {
            bool placed = false;
            for (int i = 0; i < 15; i++)
            {
                Rectangle r = new Rectangle(invStartX + (i % 5) * (invCellSize + invPad), invStartY + (i / 5) * (invCellSize + invPad), invCellSize, invCellSize);
                if (r.Contains(mx, my))
                {
                    Item target = player.Inventory.MainSlots[i];
                    player.Inventory.MainSlots[i] = draggedItem;
                    if (target != null) { if (isFromWeaponSlot && target is Weapon w) player.Inventory.WeaponSlots[sourceSlotIndex] = w; else player.Inventory.MainSlots[sourceSlotIndex] = target; }
                    placed = true; break;
                }
            }
            if (!placed)
                for (int i = 0; i < 2; i++)
                {
                    Rectangle r = new Rectangle(wpnStartX + i * (invCellSize + invPad), wpnStartY, invCellSize, invCellSize);
                    if (r.Contains(mx, my) && draggedItem is Weapon w)
                    {
                        Item target = player.Inventory.WeaponSlots[i];
                        player.Inventory.WeaponSlots[i] = w;
                        if (target != null) { if (isFromWeaponSlot) player.Inventory.WeaponSlots[sourceSlotIndex] = (Weapon)target; else player.Inventory.MainSlots[sourceSlotIndex] = target; }
                        placed = true; break;
                    }
                }
            if (!placed) ReturnDraggedItem();
            draggedItem = null;
        }

        private void ReturnDraggedItem()
        {
            if (draggedItem == null) return;
            if (isFromWeaponSlot) player.Inventory.WeaponSlots[sourceSlotIndex] = (Weapon)draggedItem;
            else player.Inventory.MainSlots[sourceSlotIndex] = draggedItem;
            draggedItem = null;
        }

        private void QuickMove(int idx, bool isWpn)
        {
            if (isWpn)
            {
                Weapon w = player.Inventory.WeaponSlots[idx]; if (w == null) return;
                int free = player.Inventory.GetFirstEmptyMainSlot();
                if (free != -1) { player.Inventory.MainSlots[free] = w; player.Inventory.WeaponSlots[idx] = null; }
            }
            else
            {
                Item item = player.Inventory.MainSlots[idx];
                if (item is Weapon w)
                {
                    int free = player.Inventory.GetFirstEmptyWeaponSlot();
                    if (free != -1) { player.Inventory.WeaponSlots[free] = w; player.Inventory.MainSlots[idx] = null; }
                }
            }
        }
    }
}