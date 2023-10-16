using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularStack<T>: ICollection, ICloneable {
    protected T[] array;
    protected int index;
    protected int count;
    
    public int Count {
        get {
            return count;
        }
        protected set {
            count = value;
        }
    }
    public bool IsSynchronized => throw new NotImplementedException();
    public object SyncRoot => throw new NotImplementedException();

    public CircularStack(int size) {
        array = new T[size];
        index = 0;
        count = 0;
    }

    public bool IsEmpty() {
        return Count <= 0;
    }

    public void Push(T item) {
        index = (index + 1) % array.Length;
        array[index] = item;
        Count = Math.Min(Count+1, array.Length);
    }

    public T Pop() {
        T item = default(T);

        if (IsEmpty()) {
            throw new InvalidOperationException(this.GetType().Name + " is empty");
        }
        item = array[index];
        index = MathUtils.NonNegativeMod(index - 1, array.Length); // C#'s % operator is remainder. -1%10 will return -1 and throw IndexOutOfBounds
        Count--;

        return item;
    }

    public T Peek() {
        if (IsEmpty()) {
            throw new InvalidOperationException(this.GetType().Name + " is empty");
        }
        return array[index];
    }

    public T Peek(int depth) {
        if (IsEmpty()) {
            throw new InvalidOperationException(this.GetType().Name + " is empty");
        }

        if(depth > Count) {
            throw new InvalidOperationException(this.GetType().Name + " only has " + Count  +
                                                " elements and you have tried to peek at element " + depth);
        }
        return array[MathUtils.NonNegativeMod(index - depth, array.Length)];
    }

    public void CopyTo(Array array, int index) {
        this.array.CopyTo(array, index);
    }

    public void Clear() {
        int size = array.Length;
        array = new T[size];
        index = 0;
        count = 0;
    }

    public IEnumerator GetEnumerator() {
        return array.GetEnumerator();
    }

    public object Clone() {
        CircularStack<T> clone = new CircularStack<T>(array.Length);
        array.CopyTo(clone.array, 0);
        return clone;
    }

    public T this[int index] {
        get => array[index];
    }

    public int Size() {
        return array.Length;
    }

}

