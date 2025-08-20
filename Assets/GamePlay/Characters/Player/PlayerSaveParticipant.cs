using UnityEngine;
using UnityEngine.AI;
using PathOfFaith.Save;
using PathOfFaith.Fondation.Core;

[DisallowMultipleComponent]
public class PlayerSaveParticipant : MonoBehaviour, ISaveParticipant
{
    void OnEnable()  { ServiceLocator.Get<SaveManager>().Register(this); }
    void OnDisable() { if (ServiceLocator.TryGet<SaveManager>(out var m)) m.Unregister(this); }

    public void Capture(GameSave save)
    {
        if (save.player == null) save.player = new PlayerSave();
        var t = transform;
        save.player.x = t.position.x; save.player.y = t.position.y; save.player.z = t.position.z;
        var q = t.rotation; save.player.rx = q.x; save.player.ry = q.y; save.player.rz = q.z; save.player.rw = q.w;
    }

    public void Apply(GameSave save)
    {
        if (save.player == null) return;
        var pos = new Vector3(save.player.x, save.player.y, save.player.z);
        var rot = new Quaternion(save.player.rx, save.player.ry, save.player.rz, save.player.rw);

        var agent = GetComponent<NavMeshAgent>();
        if (agent && agent.enabled) agent.Warp(pos); else transform.position = pos;
        transform.rotation = rot;
    }
}
