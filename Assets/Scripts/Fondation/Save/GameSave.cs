using System;

namespace PathOfFaith.Save
{
    [Serializable]
    public class GameSave
    {
        public int version = 1;
        public string scene;
        public long savedAtTicks;     // pour afficher la date des slots
        public PlayerSave player;     // v2: inventaire/quetes/monde...
    }

    [Serializable] public class PlayerSave { public float x,y,z; public float rx,ry,rz,rw; }
    // Tu pourras ajouter ici d’autres sections (HP, stats, inventaire, etc.)
}
