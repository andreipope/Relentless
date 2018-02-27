using System;
using System.Collections.Generic;

namespace GrandDevs.CZB
{
    public abstract class ServiceLocatorBase : IServiceLocator
    {
        protected IDictionary<Type, IService> _services;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceLocatorBase"/> class.
        /// </summary>
        internal ServiceLocatorBase()
        {
            _services = new Dictionary<Type, IService>();
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="System.Exception">Service  + typeof(T) +  is not registered!</exception>
        public T GetService<T>()
        {
            try
            {
                return (T)_services[typeof(T)];
            }
            catch (KeyNotFoundException)
            {
                throw new Exception("Service " + typeof(T) + " is not registered!");
            }
        }

        /// <summary>
        /// Adds the service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        protected void AddService<T>(IService service)
        {
            if (service is T)
            {
                _services.Add(typeof(T), service);

            }
            else
            {
                throw new Exception("Service " + service.ToString() + " have not implemented interface: " + typeof(T));
            }
        }

        /// <summary>
        /// Initializes the services.
        /// </summary>
        /// <exception cref="System.Exception">Service don't have Init() method!</exception>
        public void InitServices()
        {
            foreach (IService service in _services.Values)
                service.Init();
        }

        /// <summary>
        /// Calls Update to each service.
        /// </summary>
        public void Update()
        {
            foreach (IService service in _services.Values)
                service.Update();
        }

        /// <summary>
        /// Dispose the services
        /// </summary>
        public void Dispose()
        {
            foreach (IService service in _services.Values)
                service.Dispose();
        }
    }
}