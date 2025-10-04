using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyRisk.Core
{
    // Lista enlazada personalizada (requerida por las especificaciones)
    public class Lista<T>
    {
        private Nodo<T> cabeza;
        private int contador;

        private class Nodo<TData>
        {
            public TData Dato { get; set; }
            public Nodo<TData> Siguiente { get; set; }

            public Nodo(TData dato)
            {
                Dato = dato;
                Siguiente = null;
            }
        }

        public int Contar => contador;

        public void Agregar(T elemento)
        {
            Nodo<T> nuevoNodo = new Nodo<T>(elemento);

            if (cabeza == null)
            {
                cabeza = nuevoNodo;
            }
            else
            {
                Nodo<T> actual = cabeza;
                while (actual.Siguiente != null)
                {
                    actual = actual.Siguiente;
                }
                actual.Siguiente = nuevoNodo;
            }
            contador++;
        }

        public bool Remover(T elemento)
        {
            if (cabeza == null) return false;

            if (cabeza.Dato.Equals(elemento))
            {
                cabeza = cabeza.Siguiente;
                contador--;
                return true;
            }

            Nodo<T> actual = cabeza;
            while (actual.Siguiente != null)
            {
                if (actual.Siguiente.Dato.Equals(elemento))
                {
                    actual.Siguiente = actual.Siguiente.Siguiente;
                    contador--;
                    return true;
                }
                actual = actual.Siguiente;
            }
            return false;
        }

        public T[] ObtenerTodos()
        {
            T[] elementos = new T[contador];
            Nodo<T> actual = cabeza;
            int indice = 0;

            while (actual != null)
            {
                elementos[indice] = actual.Dato;
                actual = actual.Siguiente;
                indice++;
            }
            return elementos;
        }

        public bool Contiene(T elemento)
        {
            Nodo<T> actual = cabeza;
            while (actual != null)
            {
                if (actual.Dato.Equals(elemento))
                    return true;
                actual = actual.Siguiente;
            }
            return false;
        }
    }

    // Pila personalizada
    public class Pila<T>
    {
        private Lista<T> elementos;

        public Pila()
        {
            elementos = new Lista<T>();
        }

        public void Apilar(T elemento)
        {
            elementos.Agregar(elemento);
        }

        public T Desapilar()
        {
            T[] todosElementos = elementos.ObtenerTodos();
            if (todosElementos.Length == 0)
                throw new InvalidOperationException("La pila está vacía");

            T ultimo = todosElementos[todosElementos.Length - 1];
            elementos.Remover(ultimo);
            return ultimo;
        }

        public bool EstaVacia => elementos.Contar == 0;
    }

    // Cola personalizada
    public class Cola<T>
    {
        private Lista<T> elementos;

        public Cola()
        {
            elementos = new Lista<T>();
        }

        public void Encolar(T elemento)
        {
            elementos.Agregar(elemento);
        }

        public T Desencolar()
        {
            T[] todosElementos = elementos.ObtenerTodos();
            if (todosElementos.Length == 0)
                throw new InvalidOperationException("La cola está vacía");

            T primero = todosElementos[0];
            elementos.Remover(primero);
            return primero;
        }

        public bool EstaVacia => elementos.Contar == 0;
    }
}