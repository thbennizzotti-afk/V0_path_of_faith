using System;
using UnityEngine;

namespace PathOfFaith.Fondation.Core
{
    // Types d'événements
    public static class Events
    {
        public readonly struct CamModeChanged
        {
            public enum Mode { Follow, Free }
            public readonly Mode NewMode;
            public CamModeChanged(Mode mode) { NewMode = mode; }
        }

        public readonly struct NavPointSelected
        {
            public readonly Vector3 Position;
            public NavPointSelected(Vector3 pos) { Position = pos; }
        }

        public readonly struct PlayerMoveStarted
        {
            public readonly Vector3 Destination;
            public PlayerMoveStarted(Vector3 dest) { Destination = dest; }
        }

        // >>> AJOUT POUR TON CAS
        public readonly struct PlayerMoveEnded
        {
            public readonly Vector3 Position;
            public PlayerMoveEnded(Vector3 position) { Position = position; }
        }
    }

    public static class EventBus
    {
        public static event Action<Events.CamModeChanged>     OnCamModeChanged;
        public static event Action<Events.NavPointSelected>   OnNavPointSelected;
        public static event Action<Events.PlayerMoveStarted>  OnPlayerMoveStarted;
        public static event Action<Events.PlayerMoveEnded>    OnPlayerMoveEnded;  // <<< AJOUT

        public static void Raise(Events.CamModeChanged e)    => OnCamModeChanged?.Invoke(e);
        public static void Raise(Events.NavPointSelected e)  => OnNavPointSelected?.Invoke(e);
        public static void Raise(Events.PlayerMoveStarted e) => OnPlayerMoveStarted?.Invoke(e);
        public static void Raise(Events.PlayerMoveEnded e)   => OnPlayerMoveEnded?.Invoke(e); // <<< AJOUT
    }
}
