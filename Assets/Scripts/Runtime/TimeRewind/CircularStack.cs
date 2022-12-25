using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularStack<T>: ICollection, ICloneable {
    private T[] array;
    private int index;
    private int count; 
    public int Count {
        get {
            return count;
        }
        private set {
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
        Count++;
    }

    public T Pop() {
        if (IsEmpty()) {
            throw new InvalidOperationException(this.GetType().Name + " is empty");
        }
        T item = array[index];
        index = (index - 1) % array.Length;
        Count--;
        
        return item;
    }

    public T Peek() {
        if (IsEmpty()) {
            throw new InvalidOperationException(this.GetType().Name + " is empty");
        }
        return array[index];
    }

    public void CopyTo(Array array, int index) {
        this.array.CopyTo(array, index);
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

}

