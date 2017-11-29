using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LittleReviewer.DynamicType
{
    public static class DynamicPropertyObject
    {
        /// <summary>
        /// Create a new instance of a wrapper object type, ready to be used with various extension methods
        /// </summary>
        public static PropertyTarget NewObject()
        {
            return new PropertyTarget();
        }

        public static void AddProperty<T>(this PropertyTarget target, string key, string displayName, string description, T initialValue, IEnumerable<T> standardValues = null)
        {
            DynTypeDescriptor.InstallTypeDescriptor(target);
            var td = DynTypeDescriptor.GetTypeDescriptor(target);
            if (td == null) throw new Exception("Could not load type descriptor");

            
            var pd = new DynPropertyDescriptor(target.GetType(), key, typeof(T), initialValue
                ,new BrowsableAttribute(true)
                ,new DisplayNameAttribute(displayName)
                ,new DescriptionAttribute(description)
            );

            if (standardValues != null){
                var allValues = standardValues.ToArray();
                if (allValues.Length > 0)
                {
                    pd.Attributes.Add(new TypeConverterAttribute(typeof(DynStandardValueConverter)), true);
                    foreach (var value in allValues)
                    {
                        var sv = new DynStandardValue(value);
                        sv.DisplayName = value.ToString();
                        pd.StandardValues.Add(sv);
                    }
                }
            }

            td.GetProperties().Add(pd);
        }
    }

    /// <summary>
    /// Proxy object which can store and return property values
    /// </summary>
    public class PropertyTarget
    {
        /// <summary>
        /// Retrieve a value by an added property's `key` name.
        /// </summary>
        public object this[string key]
        {
            get
            {
                var td = DynTypeDescriptor.GetTypeDescriptor(this);
                return td.GetProperties().Find(key,true)?.GetValue(this);
            }
        }

        /// <summary>
        /// Provide a list of added `key` names
        /// </summary>
        public IEnumerable<string> ListProperties(){
            var td = DynTypeDescriptor.GetTypeDescriptor(this);
            var properties = td.GetProperties();
            var list = new List<string>();
            for (int i = 0; i < properties.Count; i++)
            {
                list.Add(properties[i].Name);
            }
            return list;
        }
    }
}