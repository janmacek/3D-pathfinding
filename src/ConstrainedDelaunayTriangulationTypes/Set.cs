/************************************************************************************************************
 * Author: Jan Macek                                                                                        *
 * Project: ConstrainedDelaunayTriangulationTypes.csproj                                                    *
 * File: Set.cs                                                                                             *
 *      - reprezentuje usporiadanu mnozinu s pomocnymi funkciami na ulahcenie prace s nou                   *
 * Date: 29.4.2013                                                                                          *
 * Mail: xmacek18@fit.vutbr.cz                                                                              *
 *                                                                                                          *
 * Copyright © Jan Macek 2013                                                                               *
 * *********************************************************************************************************/
using System;
using System.Collections.Generic;

namespace PathfindingApp.DelaunayTriangulationTypes {
    /// <summary>
    /// reprezentuje sorted mnozinu
    /// </summary>
    /// <typeparam name="T">typ kluca</typeparam>
    public class Set<T> : IEnumerable<T> {
        /// <summary>
        /// list ktory uchovava prvky triedy
        /// </summary>
        private SortedList<T, int> list;

        /// <summary>
        /// inicializuje triedu
        /// </summary>
        public Set() {
            list = new SortedList<T, int>();
        }

        /// <summary>
        /// prida prvok pokial neexistuje
        /// </summary>
        /// <param name="k">pridavany prvok</param>
        public void Add(T k) {
            if (!list.ContainsKey(k))
                list.Add(k, 0);
        }

        /// <summary>
        /// vracia pocet prvkov mnoziny
        /// </summary>
        public int Count {
            get { return list.Count; }
        }

        /// <summary>
        /// skopiruje inu mnozinu do aktualnej
        /// </summary>
        /// <param name="other">kopirovana mnozina</param>
        public void DeepCopy(Set<T> other) {
            list.Clear();
            foreach (T k in other.list.Keys)
                Add(k);
        }

        /// <summary>
        /// vracia enumerator mnoziny
        /// </summary>
        /// <returns>enumerator</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return list.Keys.GetEnumerator();
        }

        /// <summary>
        /// vycisti mnozinu
        /// </summary>
        public void Clear() {
            list.Clear();
        }


        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }

        #endregion
    }
}
