/************************************************************************************************************
 * Author: Jan Macek                                                                                        *
 * Project: AStarTypes.csproj                                                                               *
 * File: TransitRoute.cs                                                                                    *
 *      - reprezentuje triedu prechodu ktory uchovava informacie odkial/kam smeruje, trasu, vzdialenost     *
 * Date: 29.4.2013                                                                                          *
 * Mail: xmacek18@fit.vutbr.cz                                                                              *
 *                                                                                                          *
 * Copyright © Jan Macek 2013                                                                               *
 * *********************************************************************************************************/
using MapObjectTypes;
using System;
using System.Collections.Generic;

namespace PathfindingApp.AStarTypes {
    /// <summary>
    /// reprezentuje triedu prechodu
    /// </summary>
    public class TransitRoute {
        /// <summary>
        /// pociaticny prechod
        /// </summary>
        private int from;
        public int From { get { return this.from; } set { this.from = value; } }
        /// <summary>
        /// cielovy prechod
        /// </summary>
        private int to;
        public int To { get { return this.to; } set { this.to = value; } }

        /// <summary>
        /// podrobna cesta medzi prechodmi
        /// </summary>
        private List<MapPoint> trace;
        public List<MapPoint> Trace { get { return this.trace; } set { this.trace = value; } }

        /// <summary>
        /// dlzka prechodu
        /// </summary>
        private float length;
        public float Length { get { return this.length; } set { this.length = value; } }

        /// <summary>
        /// vypocita dlzku cesty
        /// </summary>
        /// <param name="route">trasa napriec bodmi trasy</param>
        /// <returns>vracia dlzku cesty</returns>
        public float TraceLength(List<MapPoint> route) {
            float newLength = 0;
            if (route != null) {
                for (int i = 1; i < route.Count; i++) {
                    newLength += (float)Math.Sqrt((double)((route[i].X - route[i - 1].X) * (route[i].X - route[i - 1].X)) + ((route[i].Y - route[i - 1].Y) * (route[i].Y - route[i - 1].Y)) + ((route[i].Z - route[i - 1].Z) * (route[i].Z - route[i - 1].Z)));
                }
            }
            return newLength;
        }

        /// <summary>
        /// novy objekt s inicializovanymi vlastnostami
        /// </summary>
        /// <param name="transit1">startovaci prechod</param>
        /// <param name="transit2">cielovy prechod</param>
        /// <param name="newTrace">trasa medzi prechodmi</param>
        public TransitRoute(int transit1, int transit2, List<MapPoint> newTrace) {
            this.from = transit1;
            this.to = transit2;
            this.trace = newTrace;
            this.length = TraceLength(newTrace);
        }

        /// <summary>
        /// novy objekt s inicializovanymi vlastnostami
        /// </summary>
        /// <param name="transit1">startovaci prechod</param>
        /// <param name="transit2">cielovy prechod</param>
        /// <param name="length">dlzka prechodu</param>
        public TransitRoute(int transit1, int transit2, float length) {
            this.from = transit1;
            this.to = transit2;
            this.trace = new List<MapPoint>();
            this.length = length;
        }
    }
}
