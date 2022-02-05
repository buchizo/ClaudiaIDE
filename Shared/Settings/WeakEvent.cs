using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ClaudiaIDE.Settings
{
    // https://gist.github.com/wonderful-panda/3b68a9a6040d89df907b

    public class WeakHandler<TEventArgs> : IEquatable<EventHandler<TEventArgs>>
        where TEventArgs : EventArgs
    {
        readonly WeakReference _targetRef;
        readonly MethodInfo _method;
        readonly Action<object, object, TEventArgs> _action;

        public WeakHandler(EventHandler<TEventArgs> handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");
            _targetRef = handler.Target != null ? new WeakReference(handler.Target) : null;
            _method = handler.Method;
            _action = CreateOpenMethod(handler);
        }

        /// <summary>
        /// 指定されたdelegateのTargetとMethodを元に、以下のようなActionを生成して返す
        /// instanceメソッドの場合
        ///   (object target, object sender, TArg e) => ((TargetType)target).Method(sender, e) 
        /// staticメソッドの場合
        ///   (object target, object sender, TArg e) => TargetType.Method(sender, e) 
        /// </summary>
        Action<object, object, TEventArgs> CreateOpenMethod(EventHandler<TEventArgs> h)
        {
            var target = Expression.Parameter(typeof(object), "target");
            var sender = Expression.Parameter(typeof(object), "sender");
            var e = Expression.Parameter(typeof(TEventArgs), "e");

            // null or (TargetType)target
            var instance = (h.Target == null ? null : Expression.Convert(target, h.Target.GetType()));

            var expression =
                // (target, sender, e) => ***
                Expression.Lambda<Action<object, object, TEventArgs>>(
                    // ***.Method(sender, e)
                    Expression.Call(instance, h.Method, sender, e)
                , target, sender, e);

            return expression.Compile();
        }

        /// <summary>
        /// 元になったイベントハンドラがstaticメソッドかどうか
        /// </summary>
        public bool IsStatic
        {
            get { return _targetRef == null; }
        }

        /// <summary>
        /// イベントリスナがまだ生きているかどうか
        /// </summary>
        public bool IsAlive
        {
            get { return this.IsStatic || _targetRef.IsAlive; }
        }

        /// <summary>
        /// イベントハンドラを呼び出す
        /// </summary>
        public void Invoke(object sender, TEventArgs e)
        {
            if (this.IsStatic)
            {
                _action(null, sender, e);
            }
            else
            {
                var target = _targetRef.Target;
                if (target != null)
                    _action(target, sender, e);
            }
        }

        public bool Equals(EventHandler<TEventArgs> other)
        {
            if (other.Target == null)
                return this._targetRef == null && this._method == other.Method;
            else
                return this._targetRef != null && this._targetRef.Target == other.Target && this._method == other.Method;
        }
    }

    public class WeakEvent<TEventArgs> where TEventArgs : EventArgs
    {
        readonly List<WeakHandler<TEventArgs>> _handlers = new List<WeakHandler<TEventArgs>>();

        public void AddEventHandler(EventHandler<TEventArgs> handler)
        {
            lock (_handlers)
            {
                _handlers.RemoveAll(w => !w.IsAlive);
                _handlers.Add(new WeakHandler<TEventArgs>(handler));
            }
        }

        public void RemoveEventHandler(EventHandler<TEventArgs> handler)
        {
            lock (_handlers)
            {
                _handlers.RemoveAll(w => !w.IsAlive || w.Equals(handler));
            }
        }

        public void RaiseEvent(object sender, TEventArgs e)
        {
            WeakHandler<TEventArgs>[] handlers;
            lock (_handlers)
            {
                _handlers.RemoveAll(w => !w.IsAlive);
                handlers = _handlers.ToArray();
            }
            foreach (var h in handlers)
                h.Invoke(sender, e);
        }
    }
}
