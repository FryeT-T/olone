using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace game.Entities
{
    public class AnimationController
    {
        private Dictionary<string, List<Bitmap>> animations = new Dictionary<string, List<Bitmap>>();
        private Dictionary<string, Bitmap> idleSprites = new Dictionary<string, Bitmap>();

        private string currentDirection = "down";
        private int currentFrame = 0;
        private float frameTimer = 0;
        private float frameDelay = 0.1f;
        private bool isMoving = false;

        private string[] directions = { "down", "down-right", "right", "up-right", "up", "up-left", "left", "down-left" };

        public AnimationController(float frameDelaySeconds = 0.1f)
        {
            frameDelay = frameDelaySeconds;
        }

        public void LoadAnimation(string basePath, string direction, int frameCount, string prefix = "walk_")
        {
            var frames = new List<Bitmap>();
            for (int i = 0; i < frameCount; i++)
            {
                string filePath = $"{basePath}/{direction}/{prefix}{i}.png";
                try
                {
                    if (File.Exists(filePath))
                        frames.Add(new Bitmap(filePath));
                }
                catch { }
            }
            if (frames.Count > 0)
                animations[direction] = frames;
        }

        public void LoadAllDirections(string basePath, int frameCount = 6, string prefix = "walk_")
        {
            foreach (string dir in directions)
            {
                LoadAnimation(basePath, dir, frameCount, prefix);
            }
        }

        public void LoadIdleSprites(string rotationsPath)
        {
            var mapping = new Dictionary<string, string>
            {
                { "down", "south.png" },
                { "down-right", "southeast.png" },
                { "right", "east.png" },
                { "up-right", "northeast.png" },
                { "up", "north.png" },
                { "up-left", "northwest.png" },
                { "left", "west.png" },
                { "down-left", "southwest.png" }
            };

            foreach (var kvp in mapping)
            {
                string filePath = Path.Combine(rotationsPath, kvp.Value);
                try
                {
                    if (File.Exists(filePath))
                        idleSprites[kvp.Key] = new Bitmap(filePath);
                }
                catch { }
            }
        }

        public void Update(float deltaTime, float moveX, float moveY)
        {
            isMoving = Math.Abs(moveX) > 0.01f || Math.Abs(moveY) > 0.01f;

            string newDirection = GetDirectionFromVector(moveX, moveY);
            if (newDirection != null && newDirection != currentDirection)
            {
                currentDirection = newDirection;
                currentFrame = 0;
                frameTimer = 0;
            }

            if (isMoving && animations.ContainsKey(currentDirection) && animations[currentDirection].Count > 0)
            {
                frameTimer += deltaTime;
                if (frameTimer >= frameDelay)
                {
                    frameTimer = 0;
                    currentFrame = (currentFrame + 1) % animations[currentDirection].Count;
                }
            }
            else
            {
                currentFrame = 0;
                frameTimer = 0;
            }
        }

        private string GetDirectionFromVector(float moveX, float moveY)
        {
            if (moveX == 0 && moveY == 0) return currentDirection;

            double angle = Math.Atan2(moveY, moveX) * 180 / Math.PI;
            if (angle < 0) angle += 360;

            if (angle >= 22.5 && angle < 67.5) return "down-right";
            if (angle >= 67.5 && angle < 112.5) return "down";
            if (angle >= 112.5 && angle < 157.5) return "down-left";
            if (angle >= 157.5 && angle < 202.5) return "left";
            if (angle >= 202.5 && angle < 247.5) return "up-left";
            if (angle >= 247.5 && angle < 292.5) return "up";
            if (angle >= 292.5 && angle < 337.5) return "up-right";
            return "right";
        }

        public Bitmap GetCurrentFrame()
        {
            if (!isMoving && idleSprites.ContainsKey(currentDirection))
                return idleSprites[currentDirection];

            if (animations.ContainsKey(currentDirection) && animations[currentDirection].Count > currentFrame)
                return animations[currentDirection][currentFrame];

            return null;
        }

        public void Dispose()
        {
            foreach (var kvp in animations)
                foreach (var bmp in kvp.Value)
                    bmp?.Dispose();
            animations.Clear();

            foreach (var kvp in idleSprites)
                kvp.Value?.Dispose();
            idleSprites.Clear();
        }
    }
}