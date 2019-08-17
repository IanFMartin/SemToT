using UnityEngine;
using System.Collections;

public class PoolObject<T>
{
    private bool _isActive; //Si el objeto esta activo o no
    private T _obj; //El objeto que esta almacenando
    public delegate void PoolCallback(T obj); //Delegado que se va a usar para los métodos de activar y desactivar
    private PoolCallback _initializationCallback; //Método para inicilizar
    private PoolCallback _finalizationCallback; //Método para finalizar

    public PoolObject(T obj, PoolCallback initialization, PoolCallback finalization)
    {        
        _obj = obj; //Guardo el objeto que va a contener este PoolObject
        _initializationCallback = initialization; //Guardo el método para inicilizar
        _finalizationCallback = finalization; //Guardo el método para finalizar
        isActive = false; //Pongo que esta desactivado (por el setter me aseguro que se va a desactivar)
    }

    public T GetObj
    {
        get
        {
            return _obj; //Me devuelve el objeto desde un get. Hago sólo el get porque no deberían poder setearlo
        }
    }

    public bool isActive
    {
        get
        {
            return _isActive; //Devuelve si el objeto esta activo
        }
        set
        {
            _isActive = value; //Asigna el valor pedido
            if (_isActive) //Si se activo
            {
                if (_initializationCallback != null) //Y el método para inicializar es distinto a nulo
                    _initializationCallback(_obj); //Llama al método para inicializar
            }
            else //Si esta desactivado
            {
                if (_finalizationCallback != null) //Y el método para finalizar es distinto a nulo
                    _finalizationCallback(_obj); //Llama al método para desactivar
            }
        }
    }
}