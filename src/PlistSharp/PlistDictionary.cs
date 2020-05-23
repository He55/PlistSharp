using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PlistSharp
{
    public class PlistDictionary : PlistStructure, IDictionary<string, PlistNode>
    {
        private readonly IDictionary<string, PlistNode> _map = new Dictionary<string, PlistNode>();

        public PlistDictionary(PlistStructure? parent = null)
        {
            CreatePlistNode(plist_type.PLIST_DICT, parent);
        }

        public PlistDictionary(plist_t node, PlistStructure? parent = null)
        {
            _node = node;
            _parent = parent;
            dictionary_fill(_node);
        }

        /// <inheritdoc />
        public ICollection<string> Keys => _map.Keys;

        /// <inheritdoc />
        public ICollection<PlistNode> Values => _map.Values;

        /// <inheritdoc />
        public int Count => _map.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public PlistNode this[string key]
        {
            get => _map[key];
            set
            {
                PlistNode clone = value.Clone();
                UpdateNodeParent(clone);
                LibPlist.plist_dict_set_item(_node, key, clone._node);
                _map[key] = clone;
            }
        }

        /// <inheritdoc />
        public void Add(string key, PlistNode value)
        {
            PlistNode clone = value.Clone();
            UpdateNodeParent(clone);
            LibPlist.plist_dict_set_item(_node, key, clone._node);
            _map.Add(key, clone);
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return _map.ContainsKey(key);
        }

        /// <inheritdoc />
        public bool Remove(string key)
        {
            LibPlist.plist_dict_remove_item(_node, key);
            return _map.Remove(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out PlistNode value)
        {
            return _map.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<string, PlistNode> item)
        {
            PlistNode clone = item.Value.Clone();
            UpdateNodeParent(clone);
            LibPlist.plist_dict_set_item(_node, item.Key, clone._node);
            _map.Add(KeyValuePair.Create(item.Key, clone));
        }

        /// <inheritdoc />
        public void Clear()
        {
            foreach (string key in _map.Keys)
            {
                LibPlist.plist_dict_remove_item(_node, key);
            }
            _map.Clear();
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, PlistNode> item)
        {
            return _map.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, PlistNode>[] array, int arrayIndex)
        {
            KeyValuePair<string, PlistNode>[] clones = new KeyValuePair<string, PlistNode>[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                PlistNode clone = array[i].Value.Clone();
                UpdateNodeParent(clone);
                LibPlist.plist_dict_set_item(_node, array[i].Key, clone._node);
                clones[i] = KeyValuePair.Create(array[i].Key, clone);
            }
            _map.CopyTo(clones, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, PlistNode> item)
        {
            LibPlist.plist_dict_remove_item(_node, item.Key);
            return _map.Remove(item);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, PlistNode>> GetEnumerator()
        {
            return _map.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _map.GetEnumerator();
        }

        public override PlistNode Clone()
        {
            PlistDictionary plistDictionaries = new PlistDictionary();
            plistDictionaries._node = LibPlist.plist_copy(_node);
            plistDictionaries.dictionary_fill(plistDictionaries._node);

            return new PlistDictionary(this);
        }

        public override void Remove(PlistNode node)
        {
            LibPlist.plist_dict_get_item_key(node._node, out IntPtr key);
            LibPlist.plist_dict_remove_item(_node, key);

            string dicKey = Marshal.PtrToStringUTF8(key);
            Marshal.FreeHGlobal(key);

            _map.Remove(dicKey);
        }

        private void dictionary_fill(plist_t node)
        {
            LibPlist.plist_dict_new_iter(node, out plist_dict_iter it);

            while (true)
            {
                LibPlist.plist_dict_next_item(node, it, out IntPtr key, out plist_t subnode);
                if (key == IntPtr.Zero || subnode == IntPtr.Zero)
                {
                    break;
                }

                string dicKey = Marshal.PtrToStringUTF8(key);
                _map[dicKey] = FromPlist(subnode, this)!;

                Marshal.FreeHGlobal(key);
            }

            Marshal.FreeHGlobal(it);
        }
    }
}
