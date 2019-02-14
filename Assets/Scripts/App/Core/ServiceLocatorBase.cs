using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Loom.ZombieBattleground
{
    public abstract class ServiceLocatorBase : IServiceLocator
    {
        protected IDictionary<Type, IService> Services;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceLocatorBase" /> class.
        /// </summary>
        internal ServiceLocatorBase()
        {
            Services = new Dictionary<Type, IService>();
        }

        /// <summary>
        ///     Gets the service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception">Service  + typeof(T) +  is not registered!</exception>
        public T GetService<T>()
        {
            try
            {
                return (T) Services[typeof(T)];
            }
            catch (KeyNotFoundException)
            {
                throw new Exception("Service " + typeof(T) + " is not registered!");
            }
        }

        /// <summary>
        ///     Calls Update to each service.
        /// </summary>
        public virtual void Update()
        {
            foreach (IService service in Services.Values)
            {
                service.Update();
            }
        }

        /// <summary>
        ///     Initializes the services.
        /// </summary>
        /// <exception cref="Exception">Service don't have Init() method!</exception>
        public virtual void InitServices()
        {
            Stopwatch stopwatch = new Stopwatch();
            StringBuilder logMessageBuilder = new StringBuilder(1024);

            foreach (IService service in Services.Values)
            {
                stopwatch.Restart();
                service.Init();
                stopwatch.Stop();
                logMessageBuilder.AppendFormat(
                    "Service {0}.Init() took {1} ms\n",
                    service.GetType().Name,
                    stopwatch.ElapsedMilliseconds
                );
            }

            UnityEngine.Debug.Log(logMessageBuilder.ToString());
        }

        /// <summary>
        ///     Dispose the services
        /// </summary>
        public void Dispose()
        {
            foreach (IService service in Services.Values)
            {
                service.Dispose();
            }
        }

        /// <summary>
        ///     Adds the service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        protected void AddService<T>(IService service)
        {
            if (service is T)
            {
                Services.Add(typeof(T), service);
            }
            else
            {
                throw new Exception("Service " + service + " have not implemented interface: " + typeof(T));
            }
        }
    }
}
