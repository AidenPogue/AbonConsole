using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terasievert.AbonConsole
{
    [Serializable]
    public class SerializedKeyValuePair<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;

        public static implicit operator KeyValuePair<TKey, TValue>(SerializedKeyValuePair<TKey, TValue> pair) => KeyValuePair.Create(pair.Key, pair.Value);
    }
}
