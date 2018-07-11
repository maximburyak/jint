﻿using System;
using Jint.Native;
using Jint.Native.Argument;

namespace Jint.Runtime.Environments
{
    /// <summary>
    /// Represents a declarative environment record
    /// http://www.ecma-international.org/ecma-262/5.1/#sec-10.2.1.1
    /// </summary>
    public sealed class DeclarativeEnvironmentRecord : EnvironmentRecord
    {
        private const string BindingNameArguments = "arguments";
        private Binding _argumentsBinding;

        private MruPropertyCache2<Binding> _bindings;

        public DeclarativeEnvironmentRecord(Engine engine) : base(engine)
        {
        }

        public override bool HasBinding(string name)
        {
            if (_argumentsBinding != null && name.Length == 9 && name == BindingNameArguments)
            {
                return true;
            }

            return _bindings?.ContainsKey(name) == true;
        }

        public override void CreateMutableBinding(string name, JsValue value, bool canBeDeleted = false)
        {
            var binding = new Binding
            {
                Value = value,
                CanBeDeleted =  canBeDeleted,
                Mutable = true
            };

            if (name == BindingNameArguments)
            {
                _argumentsBinding = binding;
            }
            else
            {
                if (_bindings == null)
                {
                    _bindings = new MruPropertyCache2<Binding>();
                }
                _bindings.Add(name, binding);
            }
        }

        public override void SetMutableBinding(string name, JsValue value, bool strict)
        {
            if (_bindings == null)
            {
                _bindings = new MruPropertyCache2<Binding>();
            }

            var binding = name == BindingNameArguments ? _argumentsBinding : _bindings[name];
            if (binding.Mutable)
            {
                binding.Value = value;
            }
            else
            {
                if (strict)
                {
                    ExceptionHelper.ThrowTypeError(_engine, "Can't update the value of an immutable binding.");
                }
            }
        }

        public override JsValue GetBindingValue(string name, bool strict)
        {
            var binding = name == BindingNameArguments ? _argumentsBinding : _bindings[name];

            if (!binding.Mutable && binding.Value.IsUndefined())
            {
                if (strict)
                {
                    ExceptionHelper.ThrowReferenceError(_engine, "Can't access an uninitialized immutable binding.");
                }

                return Undefined;
            }

            return binding.Value;
        }

        public override bool DeleteBinding(string name)
        {
            Binding binding = null;
            if (name == BindingNameArguments)
            {
                binding = _argumentsBinding;
            }
            else
            {
                _bindings?.TryGetValue(name, out binding);
            }

            if (binding == null)
            {
                return true;
            }

            if (!binding.CanBeDeleted)
            {
                return false;
            }

            if (name == BindingNameArguments)
            {
                _argumentsBinding = null;
            }
            else
            {
                _bindings.Remove(name);
            }

            return true;
        }

        public override JsValue ImplicitThisValue()
        {
            return Undefined;
        }

        /// <summary>
        /// Creates a new immutable binding in an environment record.
        /// </summary>
        /// <param name="name">The identifier of the binding.</param>
        /// <param name="value">The value  of the binding.</param>
        public void CreateImmutableBinding(string name, JsValue value)
        {
            var binding = new Binding
            {
                Value = value,
                Mutable = false,
                CanBeDeleted = false
            };

            if (name == BindingNameArguments)
            {
                _argumentsBinding = binding;
            }
            else
            {
                if (_bindings == null)
                {
                    _bindings = new MruPropertyCache2<Binding>();
                }
                _bindings.Add(name, binding);
            }
        }

        /// <summary>
        /// Returns an array of all the defined binding names
        /// </summary>
        /// <returns>The array of all defined bindings</returns>
        public override string[] GetAllBindingNames()
        {
            return _bindings?.GetKeys() ?? Array.Empty<string>();
        }

        internal void ReleaseArguments()
        {
            if (_argumentsBinding?.Value is ArgumentsInstance instance)
            {
                _engine.ArgumentsInstancePool.Return(instance);
                _argumentsBinding = null;
            }
        }
    }
}
