using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Common
{
    /// <summary>
    /// 自定义属性对象
    /// </summary>
    public class PropertyAttr
    {
        public PropertyAttr()
        {
            IsReadOnly = true;
        }
        public PropertyAttr(string key, object value)
        {
            this.key = key;
            if (value == null)
            {
                return;
            }
            this.value = value;
            this.DefaultValue = value;
        }
        public PropertyAttr(string key, object value, Type byClassT) : this(key, value)
        {
            PropertyInfo[] peroperties = (byClassT).GetProperties();
            foreach (PropertyInfo property in peroperties)
            {
                if (property.Name == key)
                {
                    object[] objs = property.GetCustomAttributes(typeof(object), true);
                    if (objs.Length > 0)
                    {
                        //填充注解
                        foreach (var 注解 in objs)
                        {
                            if (注解 is CategoryAttribute category)
                            {
                                Category = category.Category;
                                continue;
                            }
                            if (注解 is DisplayNameAttribute displayName)
                            {
                                DisplayName = displayName.DisplayName;
                                continue;
                            }
                            if (注解 is DescriptionAttribute description)
                            {
                                Description = description.Description;
                                continue;
                            }
                            if (注解 is BrowsableAttribute browsable)
                            {
                                IsBrowsable = browsable.Browsable;
                                continue;
                            }
                            if (注解 is ReadOnlyAttribute readOnly)
                            {
                                IsReadOnly = readOnly.IsReadOnly;
                                continue;
                            }
                        }
                    }
                    break;
                }
            }
        }
        public PropertyAttr(string key, object value, Type byClassT, Action<object> DataCallBack) : this(key, value, byClassT)
        {
            this.DataCallBack = DataCallBack;
        }
        public Action<object> DataCallBack;
        private string key = string.Empty;
        private string displayName = string.Empty;

        public string Key
        {
            get { return key; }
            set {
                key = value;
                if (string.IsNullOrWhiteSpace(displayName)) displayName = value;
            }
        }
        private object value = null;
        public object DefaultValue = null;

        public object Value
        {
            get { return this.value; }
            set { 
                this.value = value;
                if(DataCallBack != null)
                    DataCallBack(value);
            }
        }

        private string description = string.Empty;

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public bool IsReadOnly { get; set; } = false;
        public string Category { get; set; }
        public string DisplayName
        {
            get { return displayName; }
            set { displayName = value; }
        }
        public bool IsBrowsable { get; set; } = true;

        public override string ToString()
        {
            return string.Format("{0}:{1}", key.ToString(), value?.ToString());
        }
    }

    /// <summary>
    /// 自定义性质描述类
    /// </summary>
    public class PropertyDesc : PropertyDescriptor
    {
        private PropertyAttr myattr = null;
        public PropertyDesc(PropertyAttr myattr, Attribute[] attrs) : base(myattr.Key, attrs)
        {
            this.myattr = myattr;
        }
        public override bool CanResetValue(object component)
        {
            return true;
        }

        public override Type ComponentType
        {
            get
            {
                return this.GetType();
            }
        }

        public override object GetValue(object component)
        {
            return myattr.Value;
        }

        public override bool IsReadOnly
        {
            get
            {
                return myattr.IsReadOnly;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return myattr.Value.GetType();
            }
        }

        public override void ResetValue(object component)
        {
            //重置
            myattr.Value = myattr.DefaultValue;
        }

        public override void SetValue(object component, object value)
        {
            myattr.Value = value;
        }
        /// <summary>
        /// 是否应该持久化保存
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
        public override string Category { get=> myattr.Category; }
        public override string DisplayName { get => myattr.DisplayName; }
        public override bool IsBrowsable { get => myattr.IsBrowsable; }
        /// <summary>
        /// 属性说明
        /// </summary>
        public override string Description
        {
            get
            {
                return myattr.Description;
            }
        }
    }

    /// <summary>
    /// 实现自定义的特殊属性对象必须继承ICustomTypeDescriptor,并实现Dictionary
    /// </summary>
    public class PropertyList : Dictionary<String, PropertyAttr>, ICustomTypeDescriptor
    {
        /// <summary>
        /// 重写Add方法
        /// </summary>
        /// <param name="attr"></param>
        public void Add(PropertyAttr attr)
        {
            if (!this.ContainsKey(attr.Key))
            {
                base.Add(attr.Key, attr);
            }
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public string GetClassName()
        {
            //return TypeDescriptor.GetClassName(this, true);
            return "测试";
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            int count = this.Values.Count;
            PropertyDescriptor[] pds = new PropertyDescriptor[count];
            int index = 0;
            foreach (PropertyAttr item in this.Values)
            {
                pds[index] = new PropertyDesc(item, attributes);
                index++;
            }
            return new PropertyDescriptorCollection(pds);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            //return TypeDescriptor.GetProperties(this, true);
            return this.GetProperties(new Attribute[] { });
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }
    }
}
