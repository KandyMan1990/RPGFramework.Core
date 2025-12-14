using System;
using System.Collections.Generic;
using RPGFramework.Core.SharedTypes;
using RPGFramework.DI;

namespace RPGFramework.Core
{
    internal interface IModuleNameProvider
    {
        internal string GetModuleName<T>() where T : IModule;
        internal string GetModuleName(Type type);
    }

    internal class CoreModuleDIContainer : IDIContainer, IDIResolver, IDIContainerNode, IModuleNameProvider
    {
        private readonly IDIContainer             m_GlobalContainer;
        private readonly IDIResolver              m_GlobalResolver;
        private readonly IDIContainerNode         m_GlobalContainerNode;
        private readonly Dictionary<Type, string> m_ModuleNames;
        private readonly IModuleNameProvider      m_ModuleNameProvider;

        string IModuleNameProvider.GetModuleName<T>()
        {
            return m_ModuleNameProvider.GetModuleName(typeof(T));
        }

        string IModuleNameProvider.GetModuleName(Type type)
        {
            return m_ModuleNames[type];
        }

        internal CoreModuleDIContainer()
        {
            DIContainer container = new DIContainer();

            m_GlobalContainer     = container;
            m_GlobalResolver      = container;
            m_GlobalContainerNode = container;
            m_ModuleNames         = new Dictionary<Type, string>();
            m_ModuleNameProvider  = this;
        }

        IDIContainerNode IDIContainerNode.GetFallback()
        {
            return m_GlobalContainerNode.GetFallback();
        }

        void IDIContainerNode.SetFallback(IDIContainerNode fallback)
        {
            m_GlobalContainerNode.SetFallback(fallback);
        }

        bool IDIContainerNode.TryGetBinding(Type type, out Func<object> creator)
        {
            return m_GlobalContainerNode.TryGetBinding(type, out creator);
        }

        void IDIContainer.BindTransient<TInterface, TConcrete>()
        {
            m_GlobalContainer.BindTransient<TInterface, TConcrete>();
        }

        void IDIContainer.BindSingletonLazy<TInterface, TConcrete>()
        {
            if (typeof(IModule).IsAssignableFrom(typeof(TInterface)))
            {
                m_ModuleNames[typeof(TInterface)] = typeof(TConcrete).Name;
            }

            m_GlobalContainer.BindSingletonLazy<TInterface, TConcrete>();
        }

        void IDIContainer.BindSingletonNonLazy<TInterface, TConcrete>()
        {
            if (typeof(IModule).IsAssignableFrom(typeof(TInterface)))
            {
                m_ModuleNames[typeof(TInterface)] = typeof(TConcrete).Name;
            }

            m_GlobalContainer.BindSingletonNonLazy<TInterface, TConcrete>();
        }

        void IDIContainer.BindSingletonFromInstance<TInterface>(TInterface instance)
        {
            m_GlobalContainer.BindSingletonFromInstance<TInterface>(instance);
        }

        void IDIContainer.BindTransientIfNotRegistered<TInterface, TConcrete>()
        {
            m_GlobalContainer.BindTransientIfNotRegistered<TInterface, TConcrete>();
        }

        void IDIContainer.BindSingletonIfNotRegisteredLazy<TInterface, TConcrete>()
        {
            if (typeof(IModule).IsAssignableFrom(typeof(TInterface)))
            {
                m_ModuleNames[typeof(TInterface)] = typeof(TConcrete).Name;
            }

            m_GlobalContainer.BindSingletonIfNotRegisteredLazy<TInterface, TConcrete>();
        }

        void IDIContainer.BindSingletonIfNotRegisteredNonLazy<TInterface, TConcrete>()
        {
            if (typeof(IModule).IsAssignableFrom(typeof(TInterface)))
            {
                m_ModuleNames[typeof(TInterface)] = typeof(TConcrete).Name;
            }

            m_GlobalContainer.BindSingletonIfNotRegisteredNonLazy<TInterface, TConcrete>();
        }

        void IDIContainer.BindSingletonFromInstanceIfNotRegistered<TInterface>(TInterface instance)
        {
            m_GlobalContainer.BindSingletonFromInstanceIfNotRegistered<TInterface>(instance);
        }

        void IDIContainer.ForceBindTransient<TInterface, TConcrete>()
        {
            m_GlobalContainer.ForceBindTransient<TInterface, TConcrete>();
        }

        void IDIContainer.ForceBindSingletonLazy<TInterface, TConcrete>()
        {
            if (typeof(IModule).IsAssignableFrom(typeof(TInterface)))
            {
                m_ModuleNames[typeof(TInterface)] = typeof(TConcrete).Name;
            }

            m_GlobalContainer.ForceBindSingletonLazy<TInterface, TConcrete>();
        }

        void IDIContainer.ForceBindSingletonNonLazy<TInterface, TConcrete>()
        {
            if (typeof(IModule).IsAssignableFrom(typeof(TInterface)))
            {
                m_ModuleNames[typeof(TInterface)] = typeof(TConcrete).Name;
            }

            m_GlobalContainer.ForceBindSingletonNonLazy<TInterface, TConcrete>();
        }

        void IDIContainer.ForceBindSingletonFromInstance<TInterface>(TInterface instance)
        {
            m_GlobalContainer.ForceBindSingletonFromInstance<TInterface>(instance);
        }

        T IDIResolver.Resolve<T>()
        {
            return m_GlobalResolver.Resolve<T>();
        }

        object IDIResolver.Resolve(Type type)
        {
            return m_GlobalResolver.Resolve(type);
        }
    }
}