using System;
using System.Collections.Generic;
using RPGFramework.Core.SharedTypes;
using RPGFramework.DI;
using UnityEngine;

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
        private readonly IDisposable              m_GlobalDisposable;
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
            m_GlobalDisposable    = container;
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

        INonLazyBinding IDIContainer.BindSingleton<TInterface, TConcrete>()
        {
            if (typeof(IModule).IsAssignableFrom(typeof(TInterface)))
            {
                m_ModuleNames[typeof(TInterface)] = typeof(TConcrete).Name;
            }

            return m_GlobalContainer.BindSingleton<TInterface, TConcrete>();
        }

        void IDIContainer.BindSingletonFromInstance<TInterface>(TInterface instance)
        {
            m_GlobalContainer.BindSingletonFromInstance<TInterface>(instance);
        }

        void IDIContainer.BindTransientIfNotRegistered<TInterface, TConcrete>()
        {
            m_GlobalContainer.BindTransientIfNotRegistered<TInterface, TConcrete>();
        }

        INonLazyBinding IDIContainer.BindSingletonIfNotRegistered<TInterface, TConcrete>()
        {
            if (typeof(IModule).IsAssignableFrom(typeof(TInterface)))
            {
                m_ModuleNames[typeof(TInterface)] = typeof(TConcrete).Name;
            }

            return m_GlobalContainer.BindSingletonIfNotRegistered<TInterface, TConcrete>();
        }

        void IDIContainer.BindSingletonFromInstanceIfNotRegistered<TInterface>(TInterface instance)
        {
            m_GlobalContainer.BindSingletonFromInstanceIfNotRegistered<TInterface>(instance);
        }

        void IDIContainer.ForceBindTransient<TInterface, TConcrete>()
        {
            m_GlobalContainer.ForceBindTransient<TInterface, TConcrete>();
        }

        INonLazyBinding IDIContainer.ForceBindSingleton<TInterface, TConcrete>()
        {
            if (typeof(IModule).IsAssignableFrom(typeof(TInterface)))
            {
                m_ModuleNames[typeof(TInterface)] = typeof(TConcrete).Name;
            }

            return m_GlobalContainer.ForceBindSingleton<TInterface, TConcrete>();
        }

        void IDIContainer.ForceBindSingletonFromInstance<TInterface>(TInterface instance)
        {
            m_GlobalContainer.ForceBindSingletonFromInstance<TInterface>(instance);
        }

        INonLazyBinding IDIContainer.BindInterfacesToSelfSingleton<TConcrete>()
        {
            return m_GlobalContainer.BindInterfacesToSelfSingleton<TConcrete>();
        }

        INonLazyBinding IDIContainer.BindInterfacesAndConcreteToSelfSingleton<TConcrete>()
        {
            return m_GlobalContainer.BindInterfacesAndConcreteToSelfSingleton<TConcrete>();
        }

        INonLazyBinding IDIContainer.BindInterfacesToSelfSingletonIfNotRegistered<TConcrete>()
        {
            return m_GlobalContainer.BindInterfacesToSelfSingletonIfNotRegistered<TConcrete>();
        }

        INonLazyBinding IDIContainer.BindInterfacesToAndConcreteSelfSingletonIfNotRegistered<TConcrete>()
        {
            return m_GlobalContainer.BindInterfacesToAndConcreteSelfSingletonIfNotRegistered<TConcrete>();
        }

        INonLazyBinding IDIContainer.ForceBindInterfacesToSelfSingleton<TConcrete>()
        {
            return m_GlobalContainer.ForceBindInterfacesToSelfSingleton<TConcrete>();
        }

        INonLazyBinding IDIContainer.ForceBindInterfacesAndConcreteToSelfSingleton<TConcrete>()
        {
            return m_GlobalContainer.ForceBindInterfacesAndConcreteToSelfSingleton<TConcrete>();
        }

        void IDIContainer.BindPrefab<TInterface, TConcrete>(TConcrete prefab)
        {
            m_GlobalContainer.BindPrefab<TInterface, TConcrete>(prefab);
        }

        void IDIContainer.BindPrefabIfNotRegistered<TInterface, TConcrete>(TConcrete prefab)
        {
            m_GlobalContainer.BindPrefabIfNotRegistered<TInterface, TConcrete>(prefab);
        }

        void IDIContainer.ForceBindPrefab<TInterface, TConcrete>(TConcrete prefab)
        {
            m_GlobalContainer.ForceBindPrefab<TInterface, TConcrete>(prefab);
        }

        T IDIResolver.Resolve<T>()
        {
            return m_GlobalResolver.Resolve<T>();
        }

        object IDIResolver.Resolve(Type type)
        {
            return m_GlobalResolver.Resolve(type);
        }

        TInterface IDIResolver.ResolvePrefab<TInterface>(Transform parent)
        {
            return m_GlobalResolver.ResolvePrefab<TInterface>(parent);
        }

        void IDIResolver.InjectInto(object instance)
        {
            m_GlobalResolver.InjectInto(instance);
        }

        T IDIResolver.InstantiateAndInject<T>(T prefab, Transform parent)
        {
            return m_GlobalResolver.InstantiateAndInject<T>(prefab, parent);
        }

        void IDisposable.Dispose()
        {
            m_GlobalDisposable.Dispose();
        }
    }
}