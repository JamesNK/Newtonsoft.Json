using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;


namespace Newtonsoft.Json.Tests.TestObjects
{
    /// <summary>
    /// A singleton to represent an undefined value from a JSON object.
    /// </summary>

    public class Undefined
    {
        static Undefined()
        {
            Value = new Undefined();
        }
        public static readonly Undefined Value;
    };

    /// <summary>
    /// A custom dynamic object implementation that returns a special value for undefined properties
    /// instead of throwing an exception.
    /// </summary>

    public class CustomDynamicObject: DynamicObject, IDictionary<string,object>
    {


        public CustomDynamicObject()
        {
            AllowReadingUndefinedMembers = true;
        }

        static CustomDynamicObject() {
           
        }
        public  static Undefined Undefined 
        {
            get {
                return Undefined.Value;
                }
        }

       

        private IDictionary<string, object> InnerDictionary = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets a value indicating whether we allow reading of undefined members.
        /// </summary>

        public bool AllowReadingUndefinedMembers
        {
            get;
            set;
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryGetMember(binder.Name, out result);
        }
        private bool TryGetMember(string name, out object result)
        {
            if (InnerDictionary.TryGetValue(name, out result))
            {
                return true;
            }
            else
            {
                result = Undefined;
                return AllowReadingUndefinedMembers;
            }
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return TrySetMember(binder.Name, value);
        }
        protected bool TrySetMember(string name, object value)
        {
            try
            {
                if (String.IsNullOrEmpty(name))
                {
                    return false;
                }

                if (value is IDictionary<string, object> && !(value is CustomDynamicObject))
                {
                    InnerDictionary[name] = Wrap((IDictionary<string, object>)value);
                }
                else
                {
                    InnerDictionary[name] = value;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        protected CustomDynamicObject Wrap(IDictionary<string, object> value)
        {
            var obj = new CustomDynamicObject();
            IDictionary<string, object> dict = obj;
            foreach (KeyValuePair<string, object> kvp in value)
            {
                dict[kvp.Key] = kvp.Value;
            }
            return obj;
        }

        #region interface members

        public object this[string name]
        {
            get
            {
                object result;
                TryGetMember(name,out result);
                return result;
            }
            set
            {
                TrySetMember(name, value);
            }
        }

        void IDictionary<string, object>.Add(string key, object value)
        {
            TrySetMember(key, value);
        }

        bool IDictionary<string, object>.ContainsKey(string key)
        {
            return InnerDictionary.ContainsKey(key);
        }

        ICollection<string> IDictionary<string, object>.Keys
        {
            get { return InnerDictionary.Keys; }
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            return InnerDictionary.Remove(key);
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            if (InnerDictionary.ContainsKey(key))
            {
                return TryGetMember(key, out value);
            }
            else
            {
                value = null;
                return false;
            }
        }

        ICollection<object> IDictionary<string, object>.Values
        {
            get { return InnerDictionary.Values; }
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            TrySetMember(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            InnerDictionary.Clear();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return InnerDictionary.Contains(item);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            InnerDictionary.CopyTo(array, arrayIndex);
        }

        int ICollection<KeyValuePair<string, object>>.Count
        {
            get { return InnerDictionary.Count; }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return InnerDictionary.Remove(item);
        }


        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return InnerDictionary.GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion


       
    }
}
