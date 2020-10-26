using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FFXIVAPP.Common.Core;

namespace FFXIVAPP.Plugin.TeastParse
{
    /// <summary>
    /// Because it is too much hazle to use an real ioc I created this small class
    /// like an ioc-thingy
    /// </summary>
    public class Ioc
    {
        private List<LazyLoader> _objects;

        private LazyLoader Contains(Type type)
        {
            foreach (var t in _objects)
            {
                if (t.InnerType == type)
                    return t;
            }

            return null;
        }

        public T Get<T>()
        {
            foreach (var t in _objects)
            {
                if (t is LazyLoader<T> x)
                    return x.Get();
            }

            return default;
        }

        public T Instantiate<T>()
        {
            Type ClassType = typeof(T);
            ConstructorInfo[] Constructors = ClassType.GetConstructors();
            var missing = new List<string>();
            foreach (ConstructorInfo constInfo in Constructors)
            {
                if (constInfo.GetParameters().Count() == 0)
                    return Activator.CreateInstance<T>();

                var pramas = new List<LazyLoader>();
                var found = true;
                foreach (ParameterInfo parameters in constInfo.GetParameters())
                {
                    var lazy = Contains(parameters.ParameterType);
                    if (lazy == null)
                    {
                        found = false;
                        missing.Add(parameters.ParameterType.Name);
                    }

                    pramas.Add(lazy);
                }

                if (!found)
                    throw new InvalidProgramException($"Could not create {typeof(T).Name} missing: {string.Join(", ", missing)}");
                return (T)Activator.CreateInstance(typeof(T), pramas.Select(l => l.GetObject()).ToArray());
            }

            throw new InvalidProgramException($"Could not create {typeof(T).Name} missing a constructor");
        }

        public Ioc()
        {
            _objects = new List<LazyLoader>();
        }

        public void Singelton<T>(Func<T> action) => AddSingleLoader(new LazyLoader<T>(action));
        public void Singelton<T>() => AddSingleLoader(new LazyLoader<T>(() => Instantiate<T>()));
        public void Singelton<I, T>() where T : I => AddSingleLoader(new LazyLoader<I>(() => Instantiate<T>()));

        /// <summary>
        /// Make sure only one loader for type T exists in <see ref="_objects" />.
        /// </summary>
        private void AddSingleLoader<T>(LazyLoader<T> loader)
        {
            var exists = Contains(typeof(T));
            if (exists != null)
                _objects.Remove(exists);

            _objects.Add(loader);
        }

        private abstract class LazyLoader
        {
            public abstract Type InnerType { get; }

            public abstract object GetObject();
        }

        private class LazyLoader<T> : LazyLoader
        {
            private T _object;
            private bool _executed = false;
            private Func<T> _executer;

            public override Type InnerType => typeof(T);

            public LazyLoader(Func<T> executer)
            {
                _executer = executer;
                _object = default;
            }

            public override object GetObject() => this.Get();

            public T Get()
            {
                if (!_executed)
                {
                    _object = _executer();
                    _executed = true;
                }

                return _object;
            }
        }
    }

}