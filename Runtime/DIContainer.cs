using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RPGFramework.Core
{
    public class DIContainer
    {
        private readonly Dictionary<Type, Func<object>> m_Bindings;

        public DIContainer()
        {
            m_Bindings = new Dictionary<Type, Func<object>>();
        }

        public void BindTransient<TInterface, TConcrete>()
        {
            m_Bindings[typeof(TInterface)] = () => CreateInstance<TInterface, TConcrete>();
        }

        public void BindSingleton<TInterface, TConcrete>()
        {
            Lazy<object> lazy = new Lazy<object>(() => CreateInstance<TInterface, TConcrete>());
            m_Bindings[typeof(TInterface)] = () => lazy.Value;
        }

        public void BindSingletonFromInstance<TInterface, TConcrete>(TConcrete instance) where TConcrete : TInterface
        {
            m_Bindings[typeof(TInterface)] = () => instance;
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {
            Func<object> creator = m_Bindings[type];

            return creator();
        }

        private TInterface CreateInstance<TInterface, TConcrete>()
        {
            ConstructorInfo constructor = typeof(TConcrete)
                                         .GetConstructors()
                                         .OrderByDescending(c => c.GetParameters().Length)
                                         .First();

            object[] parameters = constructor
                                 .GetParameters()
                                 .Select(p => Resolve(p.ParameterType))
                                 .ToArray();

            return (TInterface)Activator.CreateInstance(typeof(TConcrete), parameters);
        }
    }
}