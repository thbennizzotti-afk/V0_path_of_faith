using UnityEngine;
using PathOfFaith.Fondation.Core;
using PathOfFaith.Save;

[DisallowMultipleComponent]
public class PlayerSaveParticipant : MonoBehaviour, ISaveParticipant
{
    [Tooltip("Transform racine du joueur (position/rotation sauvegardées).")]
    public Transform playerRoot;

    void Awake()
    {
        // On utilise l'instance concrète pour s'enregistrer
        var mgr = ServiceLocator.Get<SaveManager>();
        mgr.Register(this);
    }

    void OnDestroy()
    {
        if (ServiceLocator.Get<SaveManager>() is SaveManager mgr) mgr.Unregister(this);
    }

    public void Capture(GameSave save)
    {
        if (!playerRoot) return;
        save.player ??= new PlayerSave();
        var t = playerRoot; var r = t.rotation;

        save.player.x = t.position.x;
        save.player.y = t.position.y;
        save.player.z = t.position.z;
        save.player.rx = r.x; save.player.ry = r.y; save.player.rz = r.z; save.player.rw = r.w;
    }

    public void Apply(GameSave save)
    {
        if (!playerRoot || save?.player == null) return;
        var p = save.player;
        playerRoot.SetPositionAndRotation(
            new Vector3(p.x, p.y, p.z),
            new Quaternion(p.rx, p.ry, p.rz, p.rw));
    }
}
