using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Mathematics;

#nullable enable

namespace F3DZEX.Render
{
    public class MatrixStack
    {
        public class TopMatrixChangedEventArgs : EventArgs
        {
            public Matrix4 newTop;

            public TopMatrixChangedEventArgs(Matrix4 top) => newTop = top;
        }

        public event EventHandler<TopMatrixChangedEventArgs>? OnTopMatrixChanged;

        private Stack<Matrix4> _stack;

        public Matrix4 Top()
        {
            return _stack.Peek();
        }

        public void Push()
        {
            _stack.Push(_stack.Peek());
        }

        public void Push(Matrix4 mtx)
        {
            _stack.Push(mtx);

            OnTopMatrixChanged?.Invoke(this, new TopMatrixChangedEventArgs(Top()));
        }

        public Matrix4 Pop()
        {
            var ret = _stack.Pop();
            OnTopMatrixChanged?.Invoke(this, new TopMatrixChangedEventArgs(Top()));
            return ret;
        }

        public void Load(Matrix4 mtx)
        {
            _stack.Pop();
            Push(mtx);

            OnTopMatrixChanged?.Invoke(this, new TopMatrixChangedEventArgs(Top()));
        }

        public void Clear()
        {
            _stack.Clear();
            _stack.Push(Matrix4.Identity);

            OnTopMatrixChanged?.Invoke(this, new TopMatrixChangedEventArgs(Top()));
        }

        public MatrixStack()
        {
            _stack = new Stack<Matrix4>();
            Clear();
        }
    }
}
