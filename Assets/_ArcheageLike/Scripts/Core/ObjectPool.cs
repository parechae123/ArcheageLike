using System.Collections.Generic;
using UnityEngine;

namespace ArcheageLike.Core
{
    /// <summary>
    /// Generic object pool for reusing GameObjects (projectiles, VFX, etc.)
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        [System.Serializable]
        public class PoolEntry
        {
            public string tag;
            public GameObject prefab;
            public int initialSize = 10;
        }

        [SerializeField] private List<PoolEntry> _pools = new List<PoolEntry>();

        private Dictionary<string, Queue<GameObject>> _poolDict = new Dictionary<string, Queue<GameObject>>();
        private Dictionary<string, PoolEntry> _entryDict = new Dictionary<string, PoolEntry>();

        private void Start()
        {
            foreach (var entry in _pools)
            {
                var queue = new Queue<GameObject>();
                _entryDict[entry.tag] = entry;

                for (int i = 0; i < entry.initialSize; i++)
                {
                    var obj = CreateNewObject(entry);
                    queue.Enqueue(obj);
                }

                _poolDict[entry.tag] = queue;
            }
        }

        private GameObject CreateNewObject(PoolEntry entry)
        {
            var obj = Instantiate(entry.prefab, transform);
            obj.SetActive(false);
            return obj;
        }

        public GameObject Spawn(string tag, Vector3 position, Quaternion rotation)
        {
            if (!_poolDict.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPool] Pool with tag '{tag}' not found.");
                return null;
            }

            GameObject obj;
            if (_poolDict[tag].Count > 0)
            {
                obj = _poolDict[tag].Dequeue();
            }
            else
            {
                obj = CreateNewObject(_entryDict[tag]);
            }

            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            return obj;
        }

        public void Despawn(string tag, GameObject obj, float delay = 0f)
        {
            if (delay > 0f)
            {
                StartCoroutine(DespawnDelayed(tag, obj, delay));
                return;
            }

            obj.SetActive(false);
            obj.transform.SetParent(transform);

            if (_poolDict.ContainsKey(tag))
                _poolDict[tag].Enqueue(obj);
        }

        private System.Collections.IEnumerator DespawnDelayed(string tag, GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            Despawn(tag, obj);
        }
    }
}
