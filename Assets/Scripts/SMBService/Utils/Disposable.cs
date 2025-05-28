using System;

namespace NativePlugin.Utils {
	public static class Disposable {
		public static readonly IDisposable Empty = EmptyDisposable.Singleton;

		public static IDisposable Create(Action disposeAction) {
			return new AnonymousDisposable(disposeAction);
		}

		public static IDisposable CreateWithState<TState>(TState state, Action<TState> disposeAction) {
			return new AnonymousDisposable<TState>(state, disposeAction);
		}

		class EmptyDisposable : IDisposable {
			public static EmptyDisposable Singleton = new ();

			private EmptyDisposable() { }

			public void Dispose() { }
		}

		class AnonymousDisposable : IDisposable {
			bool _isDisposed = false;
			readonly Action _dispose;

			public AnonymousDisposable(Action dispose) {
				this._dispose = dispose ?? throw new ArgumentNullException(nameof(dispose));
			}

			public void Dispose() {
				if (!_isDisposed) {
					_isDisposed = true;
					_dispose();
				}
			}
		}

		class AnonymousDisposable<T> : IDisposable {
			bool _isDisposed = false;
			readonly T _state;
			readonly Action<T> _dispose;

			public AnonymousDisposable(T state, Action<T> dispose) {
				this._state = state;
				this._dispose = dispose;
			}

			public void Dispose() {
				if (!_isDisposed) {
					_isDisposed = true;
					_dispose(_state);
				}
			}
		}
	}
}
