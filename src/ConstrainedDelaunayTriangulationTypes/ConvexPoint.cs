/************************************************************************************************************
 * Author: Jan Macek                                                                                        *
 * Project: ConstrainedDelaunayTriangulationTypes.csproj                                                    *
 * File: ConvexPoint.cs                                                                                     *
 *      - obsahuje internu triedu pre konvexny bod trojuholnika a triedu rozsirujuca vlastnosti tohoto bodu *
 * Date: 29.4.2013                                                                                          *
 * Mail: xmacek18@fit.vutbr.cz                                                                              *
 *                                                                                                          *
 * Copyright © Jan Macek 2013                                                                               *
 * *********************************************************************************************************/
using MapObjectTypes;
using System.Collections.Generic;

namespace PathfindingApp.DelaunayTriangulationTypes {
    /// <summary>
    /// interna trieda pre konvexny bod trojuholnika
    /// </summary>
    internal class ConvexPoint : MapPoint {
        /// <summary>
        /// index konvexneho bodu
        /// </summary>
        public int pointsIndex;
        public int PointsIndex { set { this.pointsIndex = value; } get { return this.pointsIndex; } }

        /// <summary>
        /// index trojuholnika obsahujuceho bod
        /// </summary>
        public int triadIndex;
        public int TriadIndex { set { this.triadIndex = value; } get { return this.triadIndex; } }

        /// <summary>
        /// konstruktor konvexneho bodu
        /// </summary>
        /// <param name="points">body triangulacie</param>
        /// <param name="pointIndex">index bodu</param>
        public ConvexPoint(List<MapPoint> points, int pointIndex)
        {
            this.X = points[pointIndex].X;
            this.Y = points[pointIndex].Y;
            pointsIndex = pointIndex;
            triadIndex = 0;
        }
    }

    /// <summary>
    /// trieda rozsirujuca konvexny bod trojuholnika
    /// </summary>
    class Convex : List<ConvexPoint> {
        private int NextIndex(int index) {
            if (index == Count - 1)
                return 0;
            else
                return index + 1;
        }

        /// <summary>
        /// vrati vektor z konvexneho bodu do indexu nasledujuceho bodu
        /// </summary>
        public void VectorToNext(int index, out float dx, out float dy) {
            MapPoint et = this[index], en = this[NextIndex(index)];

            dx = en.X - et.X;
            dy = en.Y - et.Y;
        }

        /// <summary>
        /// vracia ci je bod viditelny z indexu nasledujuceho bodu
        /// </summary>
        public bool EdgeVisibleFrom(int index, float dx, float dy) {
            float idx, idy;
            VectorToNext(index, out idx, out idy);

            float crossProduct = -dy * idx + dx * idy;
            return crossProduct < 0;
        }

        /// <summary>
        /// vracia ci je bod viditelny z indexu nasledujuceho bodu
        /// </summary>
        public bool EdgeVisibleFrom(int index, MapPoint point) {
            float idx, idy;
            VectorToNext(index, out idx, out idy);

            float dx = point.X - this[index].X;
            float dy = point.Y - this[index].Y;

            float crossProduct = -dy * idx + dx * idy;
            return crossProduct < 0;
        }
    }
}
