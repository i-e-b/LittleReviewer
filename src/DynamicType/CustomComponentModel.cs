using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Linq;
using Scm = System.ComponentModel;

namespace LittleReviewer.DynamicType
{
    internal class PropertyValueUIService : IPropertyValueUIService
    {
        private PropertyValueUIHandler m_ValueUIHandler;
        private EventHandler m_NotifyHandler;

        /// <summary>
        /// Adds or removes an event handler that will be invoked
        /// when the global list of PropertyValueUIItems is modified.
        /// </summary>
        event EventHandler IPropertyValueUIService.PropertyUIValueItemsChanged
        {
            add
            {
                lock (this) {
                    if (m_NotifyHandler.GetInvocationList().Contains(value)) return;
                    m_NotifyHandler += value;
                }
            }
            remove
            {
                // ReSharper disable once DelegateSubtraction
                lock (this) { m_NotifyHandler -= value; }
            }
        }

        /// <summary>
        /// Tell the IPropertyValueUIService implementation that the global list of PropertyValueUIItems has been modified.
        /// </summary>
        void IPropertyValueUIService.NotifyPropertyValueUIItemsChanged()
        {
            m_NotifyHandler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Adds a PropertyValueUIHandler to this service.  When GetPropertyUIValueItems is
        /// called, each handler added to this service will be called and given the opportunity
        /// to add an icon to the specified property.
        /// </summary>
        void IPropertyValueUIService.AddPropertyValueUIHandler(PropertyValueUIHandler newHandler)
        {
            if (newHandler == null)
            {
                throw new ArgumentNullException("newHandler");
            }
            lock (this)
                m_ValueUIHandler = (PropertyValueUIHandler)Delegate.Combine(m_ValueUIHandler, newHandler);
        }

        /// <summary>
        /// Removes a PropertyValueUIHandler to this service.  When GetPropertyUIValueItems is
        /// called, each handler added to this service will be called and given the opportunity
        /// to add an icon to the specified property.
        /// </summary>
        void IPropertyValueUIService.RemovePropertyValueUIHandler(PropertyValueUIHandler newHandler)
        {
            if (newHandler == null)
            {
                throw new ArgumentNullException("newHandler");
            }

            m_ValueUIHandler = (PropertyValueUIHandler)Delegate.Remove(m_ValueUIHandler, newHandler);
        }

        /// <summary>
        /// Gets all the PropertyValueUIItems that should be displayed on the given property.
        /// For each item returned, a glyph icon will be aded to the property.
        /// </summary>
        PropertyValueUIItem[] IPropertyValueUIService.GetPropertyUIValueItems(Scm.ITypeDescriptorContext context, Scm.PropertyDescriptor propDesc)
        {

            if (propDesc == null)
            {
                throw new ArgumentNullException("propDesc");
            }

            if (m_ValueUIHandler == null)
            {
                return new PropertyValueUIItem[0];
            }


            lock (this)
            {
                ArrayList result = new ArrayList();

                m_ValueUIHandler(context, propDesc, result);

                return (PropertyValueUIItem[])result.ToArray(typeof(PropertyValueUIItem));
            }

        }
    }


    internal sealed class SimpleSite : Scm.ISite
    {
        public Scm.IComponent Component
        {
            get;
            set;
        }

        Scm.IContainer Scm.ISite.Container { get; } = new Scm.Container();

        public bool DesignMode
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
        private Dictionary<Type, object> services;
        public void AddService<T>(T service) where T : class
        {
            if (services == null)
                services = new Dictionary<Type, object>();
            services[typeof(T)] = service;
        }
        public void RemoveService<T>() where T : class
        {
            if (services != null)
                services.Remove(typeof(T));
        }
        object IServiceProvider.GetService(Type serviceType)
        {
            object service;
            if (services != null && services.TryGetValue(serviceType, out service))
            {
                return service;
            }
            return null;
        }
    }





}
