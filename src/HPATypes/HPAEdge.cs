/********************************************************************************************************************
 * Author: Jan Macek                                                                                                *
 * Project: HPATypes.csproj                                                                                         *
 * File: HPAEdge.cs                                                                                                 *
 *      - trieda reprezentuje prepojenie dvoch bodov medzi sebou v jednom clustry/v hranicnej oblasti clustrov      *
 * Date: 29.4.2013                                                                                                  *
 * Mail: xmacek18@fit.vutbr.cz                                                                                      *
 *                                                                                                                  *
 * Copyright © Jan Macek 2013                                                                                       *
 * *****************************************************************************************************************/
using MapObjectTypes;
using System;
using System.Collections.Generic;

namespace PathfindingApp.HPATypes {
    /// <summary>
    /// reprezentuje prepojenie dvoch bodov medzi sebou v jednom clustry/v hranicnej oblasti clustrov
    /// </summary>
    public class HPAEdge {

        /// <summary>
        /// definuje typ prepojenia
        /// </summary>
        public enum HPAEdgeType { intra, inter };

        /// <summary>
        /// reprezentuje typ prechodu: intra = v clustry / inter = medzi clustrami
        /// </summary>
        private HPAEdgeType type;
        public HPAEdgeType Type { get { return this.type; } set { this.type = value; } }

        /// <summary>
        /// prvy bod prepojenia
        /// </summary>
        private MapPoint a;
        public MapPoint A { get { return this.a; } set { this.a = value; } }

        /// <summary>
        /// druhy bod prepojenia
        /// </summary>
        private MapPoint b;
        public MapPoint B { get { return this.b; } set { this.b = value; } }

        /// <summary>
        /// reprezentuje vzdialenost medzi dvomi bodmi
        /// </summary>
        private double distance;
        public double Distance { get { return this.distance; } set { this.distance = value; } }

        /// <summary>
        /// cesta medzi prechodmi
        /// </summary>
        private List<MapPoint> trace;
        public List<MapPoint> Trace { get { return this.trace; } set { this.trace = value; } }

        /// <summary>
        /// vytvori a inicializuje objekt HPAEdge
        /// </summary>
        /// <param name="a">prvy bod</param>
        /// <param name="b">druhy bod</param>
        public HPAEdge(MapPoint a, MapPoint b, HPAEdgeType type) {
            this.a = a;
            this.b = b;
            this.distance = Math.Sqrt((double)((b.X - a.X) * (b.X - a.X)) + ((b.Y - a.Y) * (b.Y - a.Y)) + ((b.Z - a.Z) * (b.Z - a.Z)));
            this.type = type;
            this.trace = new List<MapPoint>();
        }

        /// <summary>
        /// vytvori a inicializuje objekt HPAEdge
        /// </summary>
        /// <param name="a">prvy bod</param>
        /// <param name="b">druhy bod</param>
        /// <param name="distance">vzdialenost medzi bodmi</param>
        public HPAEdge(MapPoint a, MapPoint b, double distance, HPAEdgeType type, List<MapPoint> trace = null) {
            this.a = a;
            this.b = b;
            if (distance == -1) {
                if (trace != null) {
                    this.distance = 0;
                    for (int i = 0; i < trace.Count-1; i++) {
                        distance += Math.Sqrt((double)((b.X - a.X) * (b.X - a.X)) + ((b.Y - a.Y) * (b.Y - a.Y)) + ((b.Z - a.Z) * (b.Z - a.Z)));    
                    }
                }
            } else {
                this.distance = distance;
            }
            this.type = type;
            if (trace == null) {
                this.trace = new List<MapPoint>();
            } else {
                this.trace = trace;
            } 
        }
    }
}
