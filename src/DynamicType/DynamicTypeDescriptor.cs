using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using Scm = System.ComponentModel;

namespace LittleReviewer.DynamicType
{
    public enum SortOrder
    {
        // no custom sorting
        None,

        // sort asscending using the property name or category name
        ByNameAscending,

        // sort descending using the property name or category name
        ByNameDescending,

        // sort asscending using property id or categor id
        ByIdAscending,

        // sort descending using property id or categor id
        ByIdDescending
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ExclusiveStandardValuesAttribute : Attribute
    {
        public ExclusiveStandardValuesAttribute(bool exclusive)
        {
            Exclusive = exclusive;
        }

        public bool Exclusive
        {
            get;
            set;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Property)]
    public class ResourceAttribute : Attribute
    {
        public ResourceAttribute() { }

        public ResourceAttribute(string baseString)
        {
            BaseName = baseString;
        }

        public ResourceAttribute(string baseString, string keyPrefix)
        {
            BaseName = baseString;
            KeyPrefix = keyPrefix;
        }

        public string BaseName { get; set; }

        public string KeyPrefix { get; set; }

        public string AssemblyFullName { get; set; }

        // Use the hash code of the string objects and xor them together.
        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return (BaseName.GetHashCode() ^ KeyPrefix.GetHashCode()) ^ AssemblyFullName.GetHashCode();
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ResourceAttribute)) { return false; }
            var other = (ResourceAttribute)obj;

            return string.Compare(BaseName, other.BaseName, StringComparison.OrdinalIgnoreCase) == 0 &&
                   string.Compare(AssemblyFullName, other.AssemblyFullName, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public override bool Match(object obj)
        {
            if (Equals(obj, this)) return true;

            switch (obj)
            {
                case null:
                    return false;
                case ResourceAttribute attribute:
                    return attribute.GetHashCode() == GetHashCode();
                default:
                    return false;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SortIDAttribute : Attribute
    {
        public SortIDAttribute()
        {
            PropertyOrder = 0;
            CategoryOrder = 0;
        }

        public SortIDAttribute(int propertyId, int categoryId)
        {
            PropertyOrder = propertyId;
            CategoryOrder = categoryId;
        }

        public int PropertyOrder { get; set; }

        public int CategoryOrder { get; set; }
    }

    public static class AttributeCollectionExtension
    {
        public static void Add(this Scm.AttributeCollection ac, Attribute attribute)
        {
            var fi = ac.GetType().GetField("_attributes", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fi == null) return;

            var arrAttr = (Attribute[])fi.GetValue(ac);
            var listAttr = new List<Attribute>();
            if (arrAttr != null)
            {
                listAttr.AddRange(arrAttr);
            }
            listAttr.Add(attribute);
            fi.SetValue(ac, listAttr.ToArray());
        }

        public static void AddRange(this Scm.AttributeCollection ac, Attribute[] attributes)
        {
            var fi = ac.GetType().GetField("_attributes", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fi == null) return;

            var arrAttr = (Attribute[])fi.GetValue(ac);
            var listAttr = new List<Attribute>();
            if (arrAttr != null)
            {
                listAttr.AddRange(arrAttr);
            }
            listAttr.AddRange(attributes);
            fi.SetValue(ac, listAttr.ToArray());
        }

        public static void Add(this Scm.AttributeCollection ac, Attribute attribute, bool removeBeforeAdd)
        {
            var fi = ac.GetType().GetField("_attributes", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fi == null) return;

            var arrAttr = (Attribute[])fi.GetValue(ac);
            var listAttr = new List<Attribute>();
            if (arrAttr != null)
            {
                listAttr.AddRange(arrAttr);
            }
            if (removeBeforeAdd)
            {
                listAttr.RemoveAll(a => a.Match(attribute));
            }
            listAttr.Add(attribute);
            fi.SetValue(ac, listAttr.ToArray());
        }

        public static void Add(this Scm.AttributeCollection ac, Attribute attribute, Type typeToRemoveBeforeAdd)
        {
            var fi = ac.GetType().GetField("_attributes", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fi == null) return;

            var arrAttr = (Attribute[])fi.GetValue(ac);
            var listAttr = new List<Attribute>();
            if (arrAttr != null)
            {
                listAttr.AddRange(arrAttr);
            }
            if (typeToRemoveBeforeAdd != null)
            {
                listAttr.RemoveAll(a => a.GetType() == typeToRemoveBeforeAdd || a.GetType().IsSubclassOf(typeToRemoveBeforeAdd));
            }
            listAttr.Add(attribute);
            fi.SetValue(ac, listAttr.ToArray());
        }

        public static void Clear(this Scm.AttributeCollection ac)
        {
            var fi = ac.GetType().GetField("_attributes", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fi != null) fi.SetValue(ac, null);
        }

        public static void Remove(this Scm.AttributeCollection ac, Attribute attribute)
        {
            var fi = ac.GetType().GetField("_attributes", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fi == null) return;

            var arrAttr = (Attribute[])fi.GetValue(ac);
            var listAttr = new List<Attribute>();
            if (arrAttr != null)
            {
                listAttr.AddRange(arrAttr);
            }
            listAttr.RemoveAll(a => a.Match(attribute));
            fi.SetValue(ac, listAttr.ToArray());
        }

        public static void Remove(this Scm.AttributeCollection ac, Type type)
        {
            var fi = ac.GetType().GetField("_attributes", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fi == null) return;

            var arrAttr = (Attribute[])fi.GetValue(ac);
            var listAttr = new List<Attribute>();
            if (arrAttr != null)
            {
                listAttr.AddRange(arrAttr);
            }
            listAttr.RemoveAll(a => a.GetType() == type);
            fi.SetValue(ac, listAttr.ToArray());
        }

        public static Attribute Get(this Scm.AttributeCollection ac, Attribute attribute)
        {
            var fi = ac.GetType().GetField("_attributes", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fi == null) { return null; }

            var arrAttr = (Attribute[]) fi.GetValue(ac);
            if (arrAttr == null)
            {
                return null;
            }
            var attrFound = arrAttr.FirstOrDefault(a => a.Match(attribute));
            return attrFound;
        }

        public static List<Attribute> Get(this Scm.AttributeCollection ac, params Attribute[] attributes)
        {
            var fi = ac.GetType().GetField("_attributes", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fi == null) return new List<Attribute>();

            var arrAttr = (Attribute[])fi.GetValue(ac);

            if (arrAttr == null)
            {
                return null;
            }
            var listAttr = new List<Attribute>();
            listAttr.AddRange(arrAttr);
            var ac2 = new Scm.AttributeCollection(attributes);
            var listAttrFound = listAttr.FindAll(a => ac2.Matches(a));
            return listAttrFound;
        }

        public static Attribute Get(this Scm.AttributeCollection ac, Type attributeType)
        {
            var fi = ac.GetType().GetField("_attributes", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fi == null) { return null; }
            var arrAttr = (Attribute[]) fi.GetValue(ac);
            var attrFound = arrAttr.FirstOrDefault(a => a.GetType() == attributeType);
            return attrFound;
        }

        public static Attribute Get(this Scm.AttributeCollection ac, Type attributeType, bool derivedType)
        {
            var fi = ac.GetType().GetField("_attributes", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fi == null) { return null; }
            var arrAttr = (Attribute[]) fi.GetValue(ac);
            Attribute attrFound;
            if (!derivedType)
            {
                attrFound = arrAttr.FirstOrDefault(a => a.GetType() == attributeType);
            }
            else
            {
                attrFound = arrAttr.FirstOrDefault(a => a.GetType() == attributeType || a.GetType().IsSubclassOf(attributeType));
            }
            return attrFound;
        }

        public static List<Attribute> Get(this Scm.AttributeCollection ac, params Type[] attributeTypes)
        {
            var fi = ac.GetType().GetField("_attributes", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fi == null) { return new List<Attribute>(); }
            var arrAttr = (Attribute[]) fi.GetValue(ac);

            if (arrAttr == null)
            {
                return null;
            }
            var listAttr = new List<Attribute>();
            listAttr.AddRange(arrAttr);
            // ReSharper disable once PossibleMistakenCallToGetType.2
            var listAttrFound = listAttr.FindAll(a => a.GetType() == attributeTypes.FirstOrDefault(b => b.GetType() == a.GetType()));

            return listAttrFound;
        }

        public static Attribute[] ToArray(this Scm.AttributeCollection ac)
        {
            var fi = ac.GetType().GetField("_attributes", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fi == null) { return null; }
            var arrAttr = (Attribute[]) fi.GetValue(ac);
            return arrAttr;
        }
    }

    public class DynStandardValue
    {
        public DynStandardValue(object value)
        {
            Value = value;
            Enabled = true;
            Visible = true;
        }

        public DynStandardValue(object value, string displayName)
        {
            DisplayName = displayName;
            Value = value;
            Enabled = true;
            Visible = true;
        }

        public DynStandardValue(object value, string displayName, string description)
        {
            Value = value;
            DisplayName = displayName;
            Description = description;
            Enabled = true;
            Visible = true;
        }

        public string DisplayName { get; set; }

        public bool Visible { get; set; }

        public bool Enabled { get; set; }

        public string Description { get; set; }

        public object Value { get; }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(DisplayName) && (Value != null))
            {
                return Value.ToString();
            }
            return DisplayName;
        }
    }

    public class DynStandardValueConverter : Scm.TypeConverter
    {
        public override bool CanConvertFrom(Scm.ITypeDescriptorContext context, Type sourceType)
        {
            if (context != null &&
                context.PropertyDescriptor != null &&
                context.PropertyDescriptor is DynPropertyDescriptor &&
                sourceType == typeof(string))
            {
                return true;
            }

            bool bOk = base.CanConvertFrom(context, sourceType);
            return bOk;
        }

        public override bool CanConvertTo(Scm.ITypeDescriptorContext context, Type destinationType)
        {
            if (context != null &&
                context.PropertyDescriptor != null &&
                context.PropertyDescriptor is DynPropertyDescriptor &&
                (destinationType == typeof(string) || destinationType == typeof(DynStandardValue)))
            {
                return true;
            }

            bool bOk = base.CanConvertTo(context, destinationType);
            return bOk;
        }

        public override object ConvertFrom(Scm.ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            object retObj;
            if (context.PropertyDescriptor == null || !(context.PropertyDescriptor is DynPropertyDescriptor) || value == null)
            {
                retObj = base.ConvertFrom(context, culture, value);
                return retObj;
            }

            var pd = (DynPropertyDescriptor) context.PropertyDescriptor;

            if (value is string)
            {
                foreach (var sv in pd.StandardValues)
                {
                    if (string.Compare(value.ToString(), sv.DisplayName, true, culture) == 0 ||
                        string.Compare(value.ToString(), sv.Value.ToString(), true, culture) == 0)
                    {
                        return sv.Value;
                    }
                }
            }
            else if (value is DynStandardValue)
            {
                return ((DynStandardValue) value).Value;
            }

            // try the native converter of the value.
            Scm.TypeConverter tc = Scm.TypeDescriptor.GetConverter(context.PropertyDescriptor.PropertyType);
            Debug.Assert(tc != null);
            retObj = tc.ConvertFrom(context, culture, value);
            return retObj;
        }

        public override object ConvertTo(Scm.ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (context == null || context.PropertyDescriptor == null || !(context.PropertyDescriptor is DynPropertyDescriptor) || value == null)
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            var pd = (DynPropertyDescriptor)context.PropertyDescriptor;

            if (value is string)
            {
                if (destinationType == typeof(string))
                {
                    return value;
                }
                if (destinationType == pd.PropertyType)
                {
                    return ConvertFrom(context, culture, value);
                }
                if (destinationType == typeof(DynStandardValue))
                {
                    foreach (DynStandardValue sv in pd.StandardValues)
                    {
                        if (String.Compare(value.ToString(), sv.DisplayName, true, culture) == 0 ||
                            String.Compare(value.ToString(), sv.Value.ToString(), true, culture) == 0)
                        {
                            return sv;
                        }
                    }
                }
            }
            else if (value.GetType() == pd.PropertyType)
            {
                if (destinationType == typeof(string))
                {
                    foreach (DynStandardValue sv in pd.StandardValues)
                    {
                        if (sv.Value.Equals(value))
                        {
                            return sv.DisplayName;
                        }
                    }
                }
                else if (destinationType == typeof(DynStandardValue))
                {
                    foreach (DynStandardValue sv in pd.StandardValues)
                    {
                        if (sv.Value.Equals(value))
                        {
                            return sv;
                        }
                    }
                }
            }

            // try the native converter of the value.
            Scm.TypeConverter tc = Scm.TypeDescriptor.GetConverter(context.PropertyDescriptor.PropertyType);
            Debug.Assert(tc != null);
            return tc.ConvertTo(context, culture, value, destinationType);
        }

        public override bool GetStandardValuesSupported(Scm.ITypeDescriptorContext context)
        {
            if (context.PropertyDescriptor != null && context.PropertyDescriptor is DynPropertyDescriptor)
            {
                return ((DynPropertyDescriptor)context.PropertyDescriptor).StandardValues.Count > 0;
            }
            return base.GetStandardValuesSupported(context);
        }

        public override bool GetStandardValuesExclusive(Scm.ITypeDescriptorContext context)
        {
            if (context != null && context.PropertyDescriptor != null)
            {
                ExclusiveStandardValuesAttribute psfa = (ExclusiveStandardValuesAttribute)context.PropertyDescriptor.Attributes.Get(typeof(ExclusiveStandardValuesAttribute), true);
                if (psfa != null)
                {
                    return psfa.Exclusive;
                }
            }
            return base.GetStandardValuesExclusive(context);
        }

        public override StandardValuesCollection GetStandardValues(Scm.ITypeDescriptorContext context)
        {
            if (context.PropertyDescriptor == null || !(context.PropertyDescriptor is DynPropertyDescriptor))
            {
                return base.GetStandardValues(context);
            }
            var pd = (DynPropertyDescriptor)context.PropertyDescriptor;
            var list = new List<object>();
            foreach (DynStandardValue sv in pd.StandardValues)
            {
                list.Add(sv.Value);
            }
            var svc = new StandardValuesCollection(list);

            return svc;
        }
    }

    public class PropertySorter : IComparer
    {
        public int Compare(object x, object y)
        {
            DynPropertyDescriptor xCpd = x as DynPropertyDescriptor;
            DynPropertyDescriptor yCpd = y as DynPropertyDescriptor;

            if (xCpd == null || yCpd == null)
            {
                return 0;
            }
            xCpd.AppendCount = 0;
            yCpd.AppendCount = 0;
            int nCompResult = 0;
            switch (m_CategorySortOrder)
            {
                case SortOrder.None:
                    nCompResult = 0;
                    break;

                case SortOrder.ByIdAscending:
                    nCompResult = xCpd.CategoryId.CompareTo(yCpd.CategoryId);
                    break;

                case SortOrder.ByIdDescending:
                    nCompResult = xCpd.CategoryId.CompareTo(yCpd.CategoryId) * -1;
                    break;

                case SortOrder.ByNameAscending:
                    nCompResult = string.Compare(xCpd.Category, yCpd.Category, StringComparison.Ordinal);
                    break;

                case SortOrder.ByNameDescending:
                    nCompResult = string.Compare(xCpd.Category, yCpd.Category, StringComparison.Ordinal) * -1;
                    break;
            }
            if (nCompResult == 0)
            {
                nCompResult = CompareProperty(xCpd, yCpd);
            }
            return nCompResult;
        }

        private int CompareProperty(DynPropertyDescriptor xCpd, DynPropertyDescriptor yCpd)
        {
            int nCompResult = 0;

            switch (m_PropertySortOrder)
            {
                case SortOrder.None:
                    nCompResult = xCpd._ID.CompareTo(yCpd._ID);
                    break;

                case SortOrder.ByIdAscending:
                    nCompResult = xCpd.PropertyId.CompareTo(yCpd.PropertyId);
                    break;

                case SortOrder.ByIdDescending:
                    nCompResult = xCpd.PropertyId.CompareTo(yCpd.PropertyId) * -1;
                    break;

                case SortOrder.ByNameAscending:
                    nCompResult = string.Compare(xCpd.DisplayName, yCpd.DisplayName, StringComparison.Ordinal);
                    break;

                case SortOrder.ByNameDescending:
                    nCompResult = string.Compare(xCpd.DisplayName, yCpd.DisplayName, StringComparison.Ordinal) * -1;
                    break;
            }
            return nCompResult;
        }

        private SortOrder m_PropertySortOrder = SortOrder.ByNameAscending;

        public SortOrder PropertySortOrder
        {
            get
            {
                return m_PropertySortOrder;
            }
            set
            {
                m_PropertySortOrder = value;
            }
        }

        private SortOrder m_CategorySortOrder = SortOrder.ByNameAscending;

        public SortOrder CategorySortOrder
        {
            get
            {
                return m_CategorySortOrder;
            }
            set
            {
                m_CategorySortOrder = value;
            }
        }
    }

    public class DynTypeDescriptor : Scm.CustomTypeDescriptor
    {
        private readonly Scm.PropertyDescriptorCollection m_pdc = new Scm.PropertyDescriptorCollection(null, false);
        private readonly object m_instance;

        public DynTypeDescriptor(Scm.ICustomTypeDescriptor ctd, object instance) : base(ctd)
        {
            m_instance = instance;
        }

        public override Scm.PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            if (m_pdc.Count == 0)
            {
                GetProperties();
            }

            Scm.PropertyDescriptorCollection pdcFilterd = new Scm.PropertyDescriptorCollection(null);
            foreach (Scm.PropertyDescriptor pd in m_pdc)
            {
                if (pd.Attributes.Contains(attributes))
                {
                    pdcFilterd.Add(pd);
                }
            }

            PreProcess(pdcFilterd);
            return pdcFilterd;
        }

        public override Scm.PropertyDescriptorCollection GetProperties()
        {
            if (m_pdc.Count == 0)
            {
                var pdc = base.GetProperties();
                foreach (Scm.PropertyDescriptor pd in pdc)
                {
                    if (!(pd is DynPropertyDescriptor))
                    {
                        DynPropertyDescriptor dynPd;
                        if (pd.PropertyType.IsEnum)
                        {
                            dynPd = new EnumPropertyDescriptor(pd);
                        }
                        else if (pd.PropertyType == typeof(bool))
                        {
                            dynPd = new BooleanPropertyDescriptor(pd);
                        }
                        else
                        {
                            dynPd = new DynPropertyDescriptor(pd);
                        }
                        m_pdc.Add(dynPd);
                    }
                    else
                    {
                        m_pdc.Add(pd);
                    }
                }
            }
            return m_pdc;
        }

        private void PreProcess(Scm.PropertyDescriptorCollection pdc)
        {
            if (pdc.Count <= 0) return;
            UpdateStringFromResource(pdc);

            var propSorter = new PropertySorter
            {
                CategorySortOrder = CategorySortOrder,
                PropertySortOrder = PropertySortOrder
            };
            var pdcSorted = pdc.Sort(propSorter);

            UpdateAppendCount(pdcSorted);

            pdc.Clear();
            foreach (Scm.PropertyDescriptor pd in pdcSorted) { pdc.Add(pd); }
        }

        private void UpdateAppendCount(Scm.PropertyDescriptorCollection pdc)
        {
            if (CategorySortOrder == SortOrder.None)
            {
                return;
            }
            int nTabCount = 0;
            if (CategorySortOrder == SortOrder.ByNameAscending || CategorySortOrder == SortOrder.ByNameDescending)
            {
                string sCatName = null;

                // iterate from last to first
                for (int i = pdc.Count - 1; i >= 0; i--)
                {
                    var pd = pdc[i] as DynPropertyDescriptor;
                    if (sCatName == null )
                    {
                        if (pd != null)
                        {
                            sCatName = pd.Category;
                            pd.AppendCount = nTabCount;
                        }
                    }
                    else if (pd != null && string.Compare(pd.Category, sCatName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        pd.AppendCount = nTabCount;
                    }
                    else
                    {
                        nTabCount++;
                        sCatName = pdc[i].Category;
                        if (pd != null) pd.AppendCount = nTabCount;
                    }
                }
            }
            else
            {
                int? nCatID = null;

                // iterate from last to first
                for (int i = pdc.Count - 1; i >= 0; i--)
                {
                    var pd = pdc[i] as DynPropertyDescriptor;
                    if (nCatID == null)
                    {
                        if (pd != null)
                        {
                            nCatID = pd.CategoryId;
                            pd.AppendCount = nTabCount;
                        }
                    }
                    if (pd != null && pd.CategoryId == nCatID)
                    {
                        pd.AppendCount = nTabCount;
                    }
                    else
                    {
                        nTabCount++;
                        if (pd != null)
                        {
                            nCatID = pd.CategoryId;
                            pd.AppendCount = nTabCount;
                        }
                    }
                }
            }
        }

        public SortOrder PropertySortOrder { get; set; } = SortOrder.ByIdAscending;

        public SortOrder CategorySortOrder { get; set; } = SortOrder.ByIdAscending;

        private void UpdateStringFromResource(Scm.PropertyDescriptorCollection pdc)
        {
            ResourceAttribute ra = (ResourceAttribute)GetAttributes().Get(typeof(ResourceAttribute), true);
            ResourceManager rm;
            if (ra == null)
            {
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(ra.BaseName) == false && String.IsNullOrEmpty(ra.AssemblyFullName) == false)
                {
                    rm = new ResourceManager(ra.BaseName, Assembly.ReflectionOnlyLoad(ra.AssemblyFullName));
                }
                else if (string.IsNullOrEmpty(ra.BaseName) == false)
                {
                    rm = new ResourceManager(ra.BaseName, m_instance.GetType().Assembly);
                }
                else
                {
                    rm = new ResourceManager(m_instance.GetType());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            string sKeyPrefix = (ra.KeyPrefix);
            foreach (DynPropertyDescriptor pd in pdc)
            {
                Scm.LocalizableAttribute la = (Scm.LocalizableAttribute)pd.Attributes.Get(typeof(Scm.LocalizableAttribute), true);
                if (la != null && !pd.IsLocalizable)
                {
                    continue;
                }
                if (pd.LCID == CultureInfo.CurrentUICulture.LCID)
                {
                    continue;
                }

                //al = pd.AttributeList;
                string sKey;
                string sResult;

                // first category
                if (!string.IsNullOrEmpty(pd.CategoryResourceKey))
                {
                    sKey = sKeyPrefix + pd.CategoryResourceKey;

                    try
                    {
                        sResult = rm.GetString(sKey);
                        if (!string.IsNullOrEmpty(sResult))
                        {
                            pd.Attributes.Add(new Scm.CategoryAttribute(sResult), true);
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Key '{0}' does not exist in the resource.", sKey);
                    }
                }

                // now display name
                sKey = sKeyPrefix + pd.Name + "_Name";
                try
                {
                    sResult = rm.GetString(sKey);
                    if (!string.IsNullOrEmpty(sResult))
                    {
                        pd.Attributes.Add(new Scm.DisplayNameAttribute(sResult), typeof(Scm.DisplayNameAttribute));
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Key '{0}' does not exist in the resource.", sKey);
                }

                // and now description
                sKey = sKeyPrefix + pd.Name + "_Desc";
                try
                {
                    sResult = rm.GetString(sKey);
                    if (!string.IsNullOrEmpty(sResult))
                    {
                        pd.Attributes.Add(new Scm.DescriptionAttribute(sResult), true);
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Key '{0}' does not exist in the resource.", sKey);
                }
            }
        }

        public void ResetProperties()
        {
            m_pdc.Clear();
            GetProperties();
        }

        private static readonly Hashtable TypeDescriptorTable = new Hashtable();

        public static DynTypeDescriptor GetTypeDescriptor(object instance)
        {
            CleanUpRef();
            return (from DictionaryEntry de in TypeDescriptorTable let wr = de.Key as WeakReference where wr != null && (wr.IsAlive && instance.Equals(wr.Target)) select de.Value as DynTypeDescriptor).FirstOrDefault();
        }

        public static bool InstallTypeDescriptor(object instance)
        {
            CleanUpRef();
            if ((from DictionaryEntry de in TypeDescriptorTable select de.Key as WeakReference).Any(wr => wr != null && (wr.IsAlive && instance.Equals(wr.Target))))
            {
                return false; // because already installed
            }

            // will have to install the provider and create a new entry in the hash table
            var parentProvider = Scm.TypeDescriptor.GetProvider(instance);
            var parentCtd = parentProvider.GetTypeDescriptor(instance);
            var ourCtd = new DynTypeDescriptor(parentCtd, instance);
            var ourProvider = new TypeDescriptionProvider(parentProvider, ourCtd);
            Scm.TypeDescriptor.AddProvider(ourProvider, instance);
            var weakRef = new WeakReference(instance, true);
            TypeDescriptorTable.Add(weakRef, ourCtd);
            return true;
        }

        private static void CleanUpRef()
        {
            var deadList = TypeDescriptorTable.Keys.Cast<WeakReference>().Where(wr => !wr.IsAlive).ToList();
            foreach (var wr in deadList)
            {
                TypeDescriptorTable.Remove(wr);
            }
        }
    }


    public class TypeDescriptionProvider : Scm.TypeDescriptionProvider
    {
        private readonly Scm.ICustomTypeDescriptor m_ctd;

        public TypeDescriptionProvider() { } 
        public TypeDescriptionProvider(Scm.TypeDescriptionProvider parent) : base(parent) { }

        public TypeDescriptionProvider(Scm.TypeDescriptionProvider parent, Scm.ICustomTypeDescriptor ctd) : base(parent) { m_ctd = ctd; }

        public override Scm.ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance) { return m_ctd; }
    }

    public class DynPropertyDescriptor : Scm.PropertyDescriptor
    {
        private readonly Type m_compType;
        private readonly Type m_PropType;
        private readonly Scm.PropertyDescriptor m_pd;
        private readonly List<PropertyValueUIItem> m_colUIItem = new List<PropertyValueUIItem>();
        private static readonly char m_HiddenChar = '\t';
        private static ulong m_COUNT = 1;
        internal readonly ulong _ID;

        public DynPropertyDescriptor(Type componentType, string sName, Type propType, object value, params Attribute[] attributes) : base(sName, attributes)
        {
            _ID = m_COUNT++;
            m_compType = componentType;
            m_value = value;
            m_PropType = propType;
        }

        public DynPropertyDescriptor(Scm.PropertyDescriptor pd) : base(pd)
        {
            _ID = m_COUNT++;
            m_pd = pd;
        }

        public override Type ComponentType { get { return m_pd != null ? m_pd.ComponentType : m_compType; } }

        public override Type PropertyType { get { return m_pd != null ? m_pd.PropertyType : m_PropType; } }

        public override bool IsReadOnly
        {
            get
            {
                Scm.ReadOnlyAttribute attr = (Scm.ReadOnlyAttribute)Attributes.Get(typeof(Scm.ReadOnlyAttribute), true);
                return attr != null && attr.IsReadOnly;
            }
        }

        public override string Category
        {
            get
            {
                string sOut = base.Category;

                Scm.CategoryAttribute attr = (Scm.CategoryAttribute)Attributes.Get(typeof(Scm.CategoryAttribute), true);
                if (attr != null && attr.Category != null)
                {
                    sOut = attr.Category;
                }
                if (sOut == null) return null;
                sOut = sOut.PadLeft(sOut.Length + AppendCount, m_HiddenChar);
                return sOut;
            }
        }

        internal object DefaultValue
        {
            get
            {
                Scm.DefaultValueAttribute attr = (Scm.DefaultValueAttribute)Attributes.Get(typeof(Scm.DefaultValueAttribute), true);
                if (attr != null)
                {
                    return attr.Value;
                }
                return null;
            }
            set
            {
                Attributes.Add(new Scm.DefaultValueAttribute(value), true);
            }
        }

        internal int PropertyId
        {
            get
            {
                SortIDAttribute rsa = (SortIDAttribute)Attributes.Get(typeof(SortIDAttribute), true);
                if (rsa != null)
                {
                    return rsa.PropertyOrder;
                }
                return 0;
            }
            set
            {
                SortIDAttribute rsa = (SortIDAttribute)Attributes.Get(typeof(SortIDAttribute), true);
                if (rsa == null)
                {
                    rsa = new SortIDAttribute();
                    Attributes.Add(rsa);
                }
                rsa.PropertyOrder = value;
            }
        }

        internal int CategoryId
        {
            get
            {
                SortIDAttribute rsa = (SortIDAttribute)Attributes.Get(typeof(SortIDAttribute), true);

                if (rsa != null)
                {
                    return rsa.CategoryOrder;
                }
                return 0;
            }
            set
            {
                SortIDAttribute rsa = (SortIDAttribute)Attributes.Get(typeof(SortIDAttribute), true);
                if (rsa == null)
                {
                    rsa = new SortIDAttribute();
                    Attributes.Add(rsa);
                }
                rsa.CategoryOrder = value;
            }
        }

        internal string CategoryResourceKey
        {
            get
            {
                CategoryResourceKeyAttribute rsa = (CategoryResourceKeyAttribute)Attributes.Get(typeof(CategoryResourceKeyAttribute), true);
                if (rsa != null)
                {
                    return rsa.ResourceKey;
                }
                return String.Empty;
            }
            set
            {
                CategoryResourceKeyAttribute rsa = (CategoryResourceKeyAttribute)Attributes.Get(typeof(CategoryResourceKeyAttribute), true);
                if (rsa == null)
                {
                    rsa = new CategoryResourceKeyAttribute();
                    Attributes.Add(rsa);
                }
                rsa.ResourceKey = value;
            }
        }

        internal int AppendCount { get; set; }

        public override bool DesignTimeOnly { get { return false; } }

        private object m_value;

        public override object GetValue(object component)
        {
            Debug.Assert(component != null);
            Debug.Assert(component.GetType() == ComponentType);

            return m_pd != null ? m_pd.GetValue(component) : m_value;
        }

        public override void SetValue(object component, object value)
        {
            Debug.Assert(component != null);
            Debug.Assert(component.GetType() == ComponentType);

            Debug.Assert(value != null);
            Debug.Assert(value.GetType() == PropertyType);

            m_value = value;

            if (m_pd != null)
            {
                m_pd.SetValue(component, m_value);
            }
            OnValueChanged(component, new EventArgs());
        }

        /// <summary>
        /// Abstract base members
        /// </summary>
        public override void ResetValue(object component)
        {
            Debug.Assert(component != null);
            Debug.Assert(component.GetType() == ComponentType);

            if (m_pd != null)
            {
                m_pd.ResetValue(component);
            }
            else
            {
                SetValue(component, DefaultValue);
            }
        }

        public override bool CanResetValue(object component)
        {
            Debug.Assert(component != null);
            Debug.Assert(component.GetType() == ComponentType);

            if (DefaultValue == null) { return false; }

            var value = GetValue(component);
            return value != null && !value.Equals(DefaultValue);
        }

        public override bool ShouldSerializeValue(object component)
        {
            Debug.Assert(component != null);
            Debug.Assert(component.GetType() == ComponentType);

            return DefaultValue == null || CanResetValue(component);
        }

        public ICollection<PropertyValueUIItem> StateItems { get { return m_colUIItem; } }

        protected List<DynStandardValue> m_StandardValues = new List<DynStandardValue>();

        public virtual IList<DynStandardValue> StandardValues { get { return m_StandardValues; } }

        public Image ValueImage { get; set; } = null;

        internal int LCID { get; set; }
    }

    public class EnumPropertyDescriptor : DynPropertyDescriptor
    {
        public EnumPropertyDescriptor(Scm.PropertyDescriptor pd) : base(pd)
        {
            Debug.Assert(pd.PropertyType.IsEnum);

            m_StandardValues.Clear();
            var svaArr = EnumUtil.GetStandardValues(PropertyType);
            m_StandardValues.AddRange(svaArr);
        }

        public override IList<DynStandardValue> StandardValues { get { return m_StandardValues.AsReadOnly(); } }
    }

    public class EnumUtil
    {
        public static bool IsBitsOn(object enumInstance, object bits)
        {
            Debug.Assert(enumInstance != null);
            Debug.Assert(enumInstance.GetType().IsEnum);
            Debug.Assert(bits != null);
            Debug.Assert(bits.GetType().IsEnum);
            Debug.Assert(enumInstance.GetType() == bits.GetType());

            if (!IsFlag(enumInstance.GetType()))
            {
                return (enumInstance.Equals(bits));
            }

            if (IsZeroDefined(enumInstance.GetType()))  // special case
            {
                return (IsZero(enumInstance) && IsZero(bits));
            }

            // otherwise (!valueIsZero && !bitsIsZero)
            var enumDataType = Enum.GetUnderlyingType(enumInstance.GetType());
            if (enumDataType == typeof(Int16))
            {
                Int16 _value = Convert.ToInt16(enumInstance);
                Int16 _bits = Convert.ToInt16(bits);
                return ((_value & _bits) == _bits);
            }
            if (enumDataType == typeof(UInt16))
            {
                UInt16 _value = Convert.ToUInt16(enumInstance);
                UInt16 _bits = Convert.ToUInt16(bits);
                return ((_value & _bits) == _bits);
            }
            if (enumDataType == typeof(Int32))
            {
                Int32 _value = Convert.ToInt32(enumInstance);
                Int32 _bits = Convert.ToInt32(bits);
                return ((_value & _bits) == _bits);
            }
            if (enumDataType == typeof(UInt32))
            {
                UInt32 _value = Convert.ToUInt32(enumInstance);
                UInt32 _bits = Convert.ToUInt32(bits);
                return ((_value & _bits) == _bits);
            }
            if (enumDataType == typeof(Int64))
            {
                Int64 _value = Convert.ToInt64(enumInstance);
                Int64 _bits = Convert.ToInt64(bits);
                return ((_value & _bits) == _bits);
            }
            if (enumDataType == typeof(UInt64))
            {
                UInt64 _value = Convert.ToUInt64(enumInstance);
                UInt64 _bits = Convert.ToUInt64(bits);
                return ((_value & _bits) == _bits);
            }
            if (enumDataType == typeof(SByte))
            {
                SByte _value = Convert.ToSByte(enumInstance);
                SByte _bits = Convert.ToSByte(bits);
                return ((_value & _bits) == _bits);
            }
            if (enumDataType == typeof(Byte))
            {
                Byte _value = Convert.ToByte(enumInstance);
                Byte _bits = Convert.ToByte(bits);
                return ((_value & _bits) == _bits);
            }
            return false;
        }

        public static bool TurnOffBits(ref object enumInstance, object bits)
        {
            Debug.Assert(enumInstance != null);
            Debug.Assert(enumInstance.GetType().IsEnum);
            Debug.Assert(bits != null);
            Debug.Assert(bits.GetType().IsEnum);
            Debug.Assert(enumInstance.GetType() == bits.GetType());

            if (!IsFlag(enumInstance.GetType()))
            {
                return false;
            }

            if (!IsBitsOn(enumInstance, bits)) // already turned off
            {
                return false;
            }
            if (IsZeroDefined(enumInstance.GetType()))  // special case
            {
                return false;
            }
            Type enumType = enumInstance.GetType();
            Type enumDataType = Enum.GetUnderlyingType(enumInstance.GetType());

            if (enumDataType == typeof(Int16))
            {
                Int32 _value = Convert.ToInt32(enumInstance);
                Int32 _bits = Convert.ToInt32(bits);
                _value &= ~(_bits);
                enumInstance = _value;
            }
            else if (enumDataType == typeof(UInt16))
            {
                UInt32 _value = Convert.ToUInt32(enumInstance);
                UInt32 _bits = Convert.ToUInt32(bits);
                _value &= ~(_bits);
                enumInstance = _value;
            }
            else if (enumDataType == typeof(Int32))
            {
                Int32 _value = Convert.ToInt32(enumInstance);
                Int32 _bits = Convert.ToInt32(bits);
                _value &= ~(_bits);
                enumInstance = _value;
            }
            else if (enumDataType == typeof(UInt32))
            {
                UInt32 _value = Convert.ToUInt32(enumInstance);
                UInt32 _bits = Convert.ToUInt32(bits);
                _value &= ~(_bits);
                enumInstance = _value;
            }
            else if (enumDataType == typeof(Int64))
            {
                Int64 _value = Convert.ToInt64(enumInstance);
                Int64 _bits = Convert.ToInt64(bits);
                _value &= ~(_bits);
                enumInstance = _value;
            }
            else if (enumDataType == typeof(UInt64))
            {
                UInt64 _value = Convert.ToUInt64(enumInstance);
                UInt64 _bits = Convert.ToUInt64(bits);
                _value &= ~(_bits);
                enumInstance = _value;
            }
            else if (enumDataType == typeof(SByte))
            {
                Int32 _value = Convert.ToInt32(enumInstance);
                Int32 _bits = Convert.ToInt32(bits);
                _value &= ~(_bits);
                enumInstance = _value;
            }
            else if (enumDataType == typeof(Byte))
            {
                Int32 _value = Convert.ToInt32(enumInstance);
                Int32 _bits = Convert.ToInt32(bits);
                _value &= ~(_bits);
                enumInstance = _value;
            }

            enumInstance = Enum.ToObject(enumType, enumInstance);

            return true;
        }

        public static bool TurnOnBits(ref object enumInstance, object bits)
        {
            Debug.Assert(enumInstance != null);
            Debug.Assert(enumInstance.GetType().IsEnum);
            Debug.Assert(bits != null);
            Debug.Assert(bits.GetType().IsEnum);
            Debug.Assert(enumInstance.GetType() == bits.GetType());

            if (!IsFlag(enumInstance.GetType()))
            {
                if (enumInstance.Equals(bits)) return false;
                enumInstance = bits;
                return true;
            }

            if (IsBitsOn(enumInstance, bits)) // already turned on
            {
                return false;
            }

            if (IsZeroDefined(enumInstance.GetType()))  // special case
            {
                return !(IsZero(enumInstance) && IsZero(bits));
            }

            Type enumType = enumInstance.GetType();
            Type enumDataType = Enum.GetUnderlyingType(enumInstance.GetType());

            if (enumDataType == typeof(Int16))
            {
                Int32 _value = Convert.ToInt32(enumInstance);
                Int32 _bits = Convert.ToInt32(bits);
                _value |= _bits;
                enumInstance = _value;
            }
            else if (enumDataType == typeof(UInt16))
            {
                UInt32 _value = Convert.ToUInt32(enumInstance);
                UInt32 _bits = Convert.ToUInt32(bits);
                _value |= _bits;
                enumInstance = _value;
            }
            else if (enumDataType == typeof(Int32))
            {
                Int32 _value = Convert.ToInt32(enumInstance);
                Int32 _bits = Convert.ToInt32(bits);
                _value |= _bits;
                enumInstance = _value;
            }
            else if (enumDataType == typeof(UInt32))
            {
                UInt32 _value = Convert.ToUInt32(enumInstance);
                UInt32 _bits = Convert.ToUInt32(bits);
                _value |= _bits;
                enumInstance = _value;
            }
            else if (enumDataType == typeof(Int64))
            {
                Int64 _value = Convert.ToInt64(enumInstance);
                Int64 _bits = Convert.ToInt64(bits);
                _value |= _bits;
                enumInstance = _value;
            }
            else if (enumDataType == typeof(UInt64))
            {
                UInt64 _value = Convert.ToUInt64(enumInstance);
                UInt64 _bits = Convert.ToUInt64(bits);
                _value |= _bits;
                enumInstance = _value;
            }
            else if (enumDataType == typeof(SByte))
            {
                Int32 _value = Convert.ToInt32(enumInstance);
                Int32 _bits = Convert.ToInt32(bits);
                _value |= _bits;
                enumInstance = _value;
            }
            else if (enumDataType == typeof(Byte))
            {
                Int32 _value = Convert.ToInt32(enumInstance);
                Int32 _bits = Convert.ToInt32(bits);
                _value |= _bits;
                enumInstance = _value;
            }
            enumInstance = Enum.ToObject(enumType, enumInstance);
            return true;
        }

        public static bool IsZeroDefined(Type enumType)
        {
            Debug.Assert(enumType != null);
            Debug.Assert(enumType.IsEnum);
            Debug.Assert(IsFlag(enumType));

            Type enumDataType = Enum.GetUnderlyingType(enumType);

            if (enumDataType == typeof(Int16))
            {
                Int16 zero = 0;
                return Enum.IsDefined(enumType, zero);
            }
            if (enumDataType == typeof(UInt16))
            {
                UInt16 zero = 0;
                return Enum.IsDefined(enumType, zero);
            }
            if (enumDataType == typeof(Int32))
            {
                Int32 zero = 0;
                return Enum.IsDefined(enumType, zero);
            }
            if (enumDataType == typeof(UInt32))
            {
                UInt32 zero = 0;
                return Enum.IsDefined(enumType, zero);
            }
            if (enumDataType == typeof(Int64))
            {
                Int64 zero = 0;
                return Enum.IsDefined(enumType, zero);
            }
            if (enumDataType == typeof(UInt64))
            {
                UInt64 zero = 0;
                return Enum.IsDefined(enumType, zero);
            }
            if (enumDataType == typeof(SByte))
            {
                SByte zero = 0;
                return Enum.IsDefined(enumType, zero);
            }
            if (enumDataType == typeof(Byte))
            {
                Byte zero = 0;
                return Enum.IsDefined(enumType, zero);
            }

            return false;
        }

        public static bool IsZero(object enumInstance)
        {
            Debug.Assert(enumInstance != null);
            Debug.Assert(enumInstance.GetType().IsEnum);

            if (!IsZeroDefined(enumInstance.GetType()))
            {
                return false;
            }

            Type enumDataType = Enum.GetUnderlyingType(enumInstance.GetType());

            if (enumDataType == typeof(Int16))
            {
                Int16 zero = 0;
                object objZero = Enum.ToObject(enumInstance.GetType(), zero);
                return objZero.Equals(enumInstance);
            }
            if (enumDataType == typeof(UInt16))
            {
                UInt16 zero = 0;
                object objZero = Enum.ToObject(enumInstance.GetType(), zero);
                return objZero.Equals(enumInstance);
            }
            if (enumDataType == typeof(Int32))
            {
                Int32 zero = 0;
                object objZero = Enum.ToObject(enumInstance.GetType(), zero);
                return objZero.Equals(enumInstance);
            }
            if (enumDataType == typeof(UInt32))
            {
                UInt32 zero = 0;
                object objZero = Enum.ToObject(enumInstance.GetType(), zero);
                return objZero.Equals(enumInstance);
            }
            if (enumDataType == typeof(Int64))
            {
                Int64 zero = 0;
                object objZero = Enum.ToObject(enumInstance.GetType(), zero);
                return objZero.Equals(enumInstance);
            }
            if (enumDataType == typeof(UInt64))
            {
                UInt64 zero = 0;
                object objZero = Enum.ToObject(enumInstance.GetType(), zero);
                return objZero.Equals(enumInstance);
            }
            if (enumDataType == typeof(SByte))
            {
                SByte zero = 0;
                object objZero = Enum.ToObject(enumInstance.GetType(), zero);
                return objZero.Equals(enumInstance);
            }
            if (enumDataType == typeof(Byte))
            {
                Byte zero = 0;
                object objZero = Enum.ToObject(enumInstance.GetType(), zero);
                return objZero.Equals(enumInstance);
            }

            return false;
        }

        public static bool IsFlag(Type enumType)
        {
            Debug.Assert(enumType != null);
            Debug.Assert(enumType.IsEnum);
            return (enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0);
        }

        public static DynStandardValue[] GetStandardValues(object enumInstance)
        {
            Debug.Assert(enumInstance != null);
            Debug.Assert(enumInstance.GetType().IsEnum);
            return GetStandardValues(enumInstance.GetType(), BindingFlags.Public | BindingFlags.Instance);
        }

        public static DynStandardValue[] GetStandardValues(Type enumType)
        {
            Debug.Assert(enumType != null);
            Debug.Assert(enumType.IsEnum);

            return GetStandardValues(enumType, BindingFlags.Public | BindingFlags.Static);
        }

        private static DynStandardValue[] GetStandardValues(Type enumType, BindingFlags flags)
        {
            var arrAttr = new ArrayList();
            var fields = enumType.GetFields(flags);

            foreach (var fi in fields)
            {
                var sv = new DynStandardValue(Enum.ToObject(enumType, fi.GetValue(null)));
                sv.DisplayName = Enum.GetName(enumType, sv.Value); // by default

                if (fi.GetCustomAttributes(typeof(Scm.DescriptionAttribute), false) is Scm.DescriptionAttribute[] da && da.Length > 0)
                {
                    sv.Description = da[0].Description;
                }

                if (fi.GetCustomAttributes(typeof(Scm.BrowsableAttribute), false) is Scm.BrowsableAttribute[] ba && ba.Length > 0)
                {
                    sv.Visible = ba[0].Browsable;
                }

                if (fi.GetCustomAttributes(typeof(Scm.ReadOnlyAttribute), false) is Scm.ReadOnlyAttribute[] roa && roa.Length > 0)
                {
                    sv.Enabled = !roa[0].IsReadOnly;
                }
                arrAttr.Add(sv);
            }
            var retAttr = arrAttr.ToArray(typeof(DynStandardValue)) as DynStandardValue[];
            return retAttr;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    internal class CategoryResourceKeyAttribute : Attribute
    {
        public CategoryResourceKeyAttribute() { }
        public CategoryResourceKeyAttribute(string resourceKey) { ResourceKey = resourceKey; }
        public string ResourceKey { get; set; }
    }


    internal class BooleanPropertyDescriptor : DynPropertyDescriptor
    {
        public BooleanPropertyDescriptor(Scm.PropertyDescriptor pd) : base(pd)
        {
            Debug.Assert(pd.PropertyType == typeof(bool));

            m_StandardValues.Clear();
            m_StandardValues.Add(new DynStandardValue(true));
            m_StandardValues.Add(new DynStandardValue(false));
        }

        public BooleanPropertyDescriptor(Type componentType, string sName, bool value, params Attribute[] attributes)
          : base(componentType, sName, typeof(bool), value, attributes)
        {
            m_StandardValues.Clear();
            m_StandardValues.Add(new DynStandardValue(true));
            m_StandardValues.Add(new DynStandardValue(false));
        }

        public override IList<DynStandardValue> StandardValues
        {
            get
            {
                return m_StandardValues.AsReadOnly();
            }
        }
    }
}