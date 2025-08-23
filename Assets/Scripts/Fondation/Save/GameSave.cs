// Assets/Scripts/Fondation/Save/GameSave.cs
using System;

namespace PathOfFaith.Save
{
    [Serializable]
    public class GameSave
    {
        public int version = 1;
        public string scene;
        public long savedAtTicks;   // pour afficher l'heure
        public PlayerSave player;   // étends ici: stats, inventaire, etc.
    }

    [Serializable]
    public class PlayerSave
    {
        public float x, y, z;
        public float rx, ry, rz, rw;
    }
}
