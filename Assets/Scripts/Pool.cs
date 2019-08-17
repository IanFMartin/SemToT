using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pool<T>
{
    private int count = 0;

    public delegate T Factory();
    public delegate void ObjectHandler(T obj);

    private List<T> availables;

    private Factory factory;
    private ObjectHandler onRelease;
    private ObjectHandler onGet;

    public Pool(int initialCount, Factory factory, ObjectHandler onRelease, ObjectHandler onGet)
    {
        this.factory = factory;
        this.onGet = onGet;
        this.onRelease = onRelease;

        availables = new List<T>();

        Populate(initialCount);
    }

    public T GetObject()
    {
        if (availables.Count <= 0)
        {
            Populate(count);
        }

        var current = availables[availables.Count - 1];
        availables.RemoveAt(availables.Count - 1);

        onGet(current);
        return current;
    }

    public void ReleaseObject(T obj)
    {
        onRelease(obj);
        availables.Add(obj);
    }

    private void Populate(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            var obj = factory();
            availables.Add(obj);
            //onRelease(obj);
        }
        count = count + amount;
    }

}
