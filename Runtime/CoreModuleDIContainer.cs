using System;
using System.Collections.Generic;
using RPGFramework.Core.SharedTypes;
using RPGFramework.DI;

namespace RPGFramework.Core
{
    internal interface IModuleNameProvider
    {
        internal string GetModuleName<T>() where T : IModule;
    }

    internal class CoreModuleDIContainer : IDIContainer, IModuleNameProvider
    {
        private readonly IDIContainer             m_GlobalContainer;
        private readonly Dictionary<Type, string> m_ModuleNames;

        string IModuleNameProvider.GetModuleName<T>()
        {
            return m_ModuleNames[typeof(T)];
        }

        internal CoreModuleDIContainer()
        {
            m_GlobalContainer = new DIContainer();
            m_ModuleNames     = new Dictionary<Type, string>();
        }

        IDIContainer IDIContainer.GetFallback()
        {
            return m_GlobalContainer.GetFallback();
        }

        void IDIContainer.SetFallback(IDIContainer fallback)
        {
            m_GlobalContainer.SetFallback(fallback);
        }

        bool IDIContainer.TryGetBinding(Type type, out Func<object> creator)
        {
            return m_GlobalContainer.TryGetBinding(type, out creator);
        }

        void IDIContainer.BindTransient<TInterface, TConcrete>()
        {
            m_GlobalContainer.BindTransient<TInterface, TConcrete>();
        }

        void IDIContainer.BindSingleton<TInterface, TConcrete>()
        {
            if (typeof(IModule).IsAssignableFrom(typeof(TInterface)))
            {
                m_ModuleNames[typeof(TInterface)] = typeof(TConcrete).Name;
            }

            m_GlobalContainer.BindSingleton<TInterface, TConcrete>();
        }

        void IDIContainer.BindSingletonFromInstance<TInterface>(TInterface instance)
        {
            m_GlobalContainer.BindSingletonFromInstance<TInterface>(instance);
        }

        T IDIContainer.Resolve<T>()
        {
            return m_GlobalContainer.Resolve<T>();
        }

        object IDIContainer.Resolve(Type type)
        {
            return m_GlobalContainer.Resolve(type);
        }
    }
}