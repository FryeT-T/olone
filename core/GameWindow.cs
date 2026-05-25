using game.Entities;
using game.Environment;
using game.Weapons;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace game.core
{
    public enum GameState { MainMenu, Lobby, Playing, Paused, GameOver }

    public partial class GameWindow : Form
    {
        public static int WindowWidth = 1920;
        public static int WindowHeight = 1080;
        public static bool isInventoryOpen = false;

        private GameState currentState = GameState.MainMenu;
        private Player player;
        private List<Enemy> enemies = new List<Enemy>();
        private List<Projectile> playerProjectiles = new List<Projectile>();
        private static List<Projectile> pendingProjectiles = new List<Projectile>();
        private List<Projectile> enemyProjectiles = new List<Projectile>();
        private static List<Projectile> pendingEnemyProjectiles = new List<Projectile>();
        private List<NPC> npcs = new List<NPC>();
        private Teleport lobbyTeleport;

        private TileMap tileMap;
        private Room currentRoom;
        private Dictionary<Point, Room> rooms = new Dictionary<Point, Room>();
        private LevelData currentLevel;
        private Point currentRoomPos;
        private int waveDelayTimer = 0;

        private Items.Item draggedItem = null;
        private int sourceSlotIndex = -1;
        private bool isFromWeaponSlot = false;
        private int invStartX, invStartY, invCellSize = 90, invPad = 10;
        private int wpnStartX, wpnStartY;

        private Timer gameTimer;

        public GameWindow()
        {
            Width = WindowWidth;
            Height = WindowHeight;
            Text = "OLONE";
            DoubleBuffered = true;
            BackColor = Color.Black;

            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            Paint += OnPaint;

            gameTimer = new Timer { Interval = 16 };
            gameTimer.Tick += (s, e) => GameLoop();
            gameTimer.Start();
        }

        public static void AddProjectile(Projectile p) => pendingProjectiles.Add(p);
        public static void AddEnemyProjectile(Projectile p) => pendingEnemyProjectiles.Add(p);
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            InputManager.KeyDown(e.KeyCode);

            if (e.KeyCode == Keys.F5)
            {
                if (currentState == GameState.Playing || currentState == GameState.Paused || currentState == GameState.Lobby)
                {
                    SaveGame();
                }
            }
        }
        
        private void OnKeyUp(object sender, KeyEventArgs e) => InputManager.KeyUp(e.KeyCode);
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            InputManager.MouseDown(e.Button);
            int centerX = WindowWidth / 2 - 250;

            if (currentState == GameState.MainMenu)
            {
                if (new Rectangle(centerX, 400, 500, 40).Contains(e.Location)) InitializeLobby();
                if (new Rectangle(centerX, 460, 500, 40).Contains(e.Location) && SaveManager.HasSave()) LoadGameProgress();
                if (new Rectangle(centerX, 520, 500, 40).Contains(e.Location)) Application.Exit();
            }

            if (currentState == GameState.Paused)
            {
                if (new Rectangle(centerX, 400, 500, 40).Contains(e.Location))
                    currentState = (lobbyTeleport != null) ? GameState.Lobby : GameState.Playing;

                if (new Rectangle(centerX, 460, 500, 40).Contains(e.Location))
                    SaveGame();

                if (new Rectangle(centerX, 520, 500, 40).Contains(e.Location))
                    currentState = GameState.MainMenu;
            }
        }
        private void OnMouseUp(object sender, MouseEventArgs e) => InputManager.MouseUp(e.Button);
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            player?.Dispose();
            base.OnFormClosed(e);
        }
    }
}