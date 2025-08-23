// Assets/GamePlay/Characters/Player/PlayerSaveParticipant.cs
using UnityEngine;
using UnityEngine.AI;
using PathOfFaith.Fondation.Core;
using PathOfFaith.Save;

namespace PathOfFaith.Gameplay.Characters.Player
{
    [DisallowMultipleComponent]
    public class PlayerSaveParticipant : MonoBehaviour, ISaveParticipant
    {
        [SerializeField] Transform target;
        [SerializeField] bool useNavMeshAgentWarp = true;

        NavMeshAgent _agent;
        SaveManager  _mgr;

        void Awake()
        {
            if (!target) target = transform;
            _agent = target.GetComponent<NavMeshAgent>();
        }

        void OnEnable()
        {
            _mgr = ServiceLocator.Get<SaveManager>();
            if (_mgr != null) _mgr.Register(this);
        }

        void OnDisable()
        {
            if (_mgr != null) _mgr.Unregister(this);
        }

        public void Capture(GameSave s)
        {
            if (s.player == null) s.player = new PlayerSave();
            var p = target.position; var r = target.rotation;
            s.player.x=p.x; s.player.y=p.y; s.player.z=p.z;
            s.player.rx=r.x; s.player.ry=r.y; s.player.rz=r.z; s.player.rw=r.w;
        }

        public void Apply(GameSave s)
        {
            if (s?.player == null) return;

            var pos = new Vector3(s.player.x,s.player.y,s.player.z);
            var rot = new Quaternion(s.player.rx,s.player.ry,s.player.rz,s.player.rw);

            if (useNavMeshAgentWarp && _agent && _agent.isOnNavMesh)
            {
                _agent.Warp(pos);
                target.rotation = rot;
            }
            else
            {
                target.SetPositionAndRotation(pos, rot);
            }
        }
    }
}
