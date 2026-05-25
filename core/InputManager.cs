using System.Collections.Generic;
using System.Windows.Forms;

namespace game.core
{
    public static class InputManager
    {
        private static HashSet<Keys> _pressedKeys = new HashSet<Keys>();
        public static int MouseX { get; private set; }
        public static int MouseY { get; private set; }
        public static bool IsMouseLeftPressed { get; private set; }
        public static bool IsMouseRightPressed { get; private set; }
        public static bool MouseLeftJustPressed { get; private set; }
        public static bool MouseRightJustPressed { get; private set; }
        private static bool _lastLeft;
        private static bool _lastRight;
        public static void PostUpdate()
        {
            MouseLeftJustPressed = IsMouseLeftPressed && !_lastLeft;
            MouseRightJustPressed = IsMouseRightPressed && !_lastRight;
            _lastLeft = IsMouseLeftPressed;
            _lastRight = IsMouseRightPressed;
        }

        public static void Update(Form form)
        {
            var mousePos = form.PointToClient(Cursor.Position);
            MouseX = mousePos.X;
            MouseY = mousePos.Y;
        }

        public static bool IsKeyDown(Keys key) => _pressedKeys.Contains(key);

        public static void KeyDown(Keys key) => _pressedKeys.Add(key);
        public static void KeyUp(Keys key) => _pressedKeys.Remove(key);

        public static void MouseDown(MouseButtons button)
        {
            if (button == MouseButtons.Left) IsMouseLeftPressed = true;
            if (button == MouseButtons.Right) IsMouseRightPressed = true; 
        }

        public static void MouseUp(MouseButtons button)
        {
            if (button == MouseButtons.Left) IsMouseLeftPressed = false;
            if (button == MouseButtons.Right) IsMouseRightPressed = false;
        }
    }
}