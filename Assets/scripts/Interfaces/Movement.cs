using UnityEngine;

namespace Assets.scripts
{
    public interface Movement
    {
        public Vector2 direction { get; set; }
        public bool isRunning { get; set; }
        public void setDirection(Vector2 position);
    }
}
