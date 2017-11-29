using System.Collections.Generic;
using System.Linq;

namespace LittleReviewer
{
    /// <summary>
    /// Dictionary of key to list of values.
    /// Adding and reading is safe when keys are not already present
    /// </summary>
    public class Map<TKey, TValue>
    {
        private readonly Dictionary<TKey,List<TValue>> storage;
        public Map() {
            storage = new Dictionary<TKey, List<TValue>>();
        }

        public ICollection<TKey> Keys() {
            return storage.Keys.ToList();
        }

        public ICollection<TValue> Get(TKey key){
            if ( ! storage.ContainsKey(key)) return new List<TValue>();
            return storage[key];
        }

        public void Add(TKey key, TValue value) {
            if ( ! storage.ContainsKey(key)) storage.Add(key, new List<TValue>());
            storage[key].Add(value);
        }
    }
}