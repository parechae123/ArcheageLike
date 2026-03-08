using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcheageLike.Core
{
    /// <summary>
    /// Simple event bus for decoupled communication between systems.
    /// Usage:
    ///   EventBus.Subscribe<DamageEvent>(OnDamage);
    ///   EventBus.Publish(new DamageEvent { Target = enemy, Amount = 50 });
    ///   EventBus.Unsubscribe<DamageEvent>(OnDamage);
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();

        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (!_subscribers.ContainsKey(type))
                _subscribers[type] = new List<Delegate>();

            _subscribers[type].Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (_subscribers.ContainsKey(type))
                _subscribers[type].Remove(handler);
        }

        public static void Publish<T>(T eventData) where T : struct
        {
            var type = typeof(T);
            if (!_subscribers.ContainsKey(type)) return;

            // Iterate copy to allow modifications during iteration
            var handlers = new List<Delegate>(_subscribers[type]);
            foreach (var handler in handlers)
            {
                try
                {
                    ((Action<T>)handler)?.Invoke(eventData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EventBus] Error in handler for {type.Name}: {e}");
                }
            }
        }

        public static void Clear()
        {
            _subscribers.Clear();
        }
    }

    // ===== Game Events =====

    public struct DamageEvent
    {
        public GameObject Source;
        public GameObject Target;
        public float Amount;
        public DamageType Type;
    }

    public struct EntityDeathEvent
    {
        public GameObject Entity;
        public GameObject Killer;
    }

    public struct PlayerBoardShipEvent
    {
        public GameObject Player;
        public GameObject Ship;
    }

    public struct PlayerExitShipEvent
    {
        public GameObject Player;
        public GameObject Ship;
    }

    public struct BuildingPlacedEvent
    {
        public GameObject Building;
        public Vector3 Position;
    }

    public struct SkillUsedEvent
    {
        public GameObject Caster;
        public int SkillId;
    }

    public struct ItemPickedUpEvent
    {
        public int ItemId;
        public int Amount;
    }

    public enum DamageType
    {
        Physical,
        Magical,
        True
    }
}
