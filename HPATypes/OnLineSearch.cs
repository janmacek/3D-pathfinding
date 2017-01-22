/************************************************************************************************************
 * Author: Jan Macek                                                                                        *
 * Project: HPATypes.csproj                                                                                 *
 * File: OnLineSearch.cs                                                                                    *
 *      - trieda vyhladava trasu v priestore rozdelenom na clustre                                          *
 * Date: 29.4.2013                                                                                          *
 * Mail: xmacek18@fit.vutbr.cz                                                                              *
 *                                                                                                          *
 * Copyright © Jan Macek 2013                                                                               *
 * *********************************************************************************************************/
using MapObjectTypes;
using System.Collections.Generic;

namespace PathfindingApp.HPATypes {
    /// <summary>
    /// vyhladava trasu v priestore rozdelenom na clustre
    /// </summary>
    public static class OnLineSearch {

        /// <summary>
        /// zisti id bodu v mnozine
        /// </summary>
        /// <param name="transitList">mnozina bodov</param>
        /// <param name="transit">hladany bod</param>
        /// <returns>vracia id bodu inak -1</returns>
        private static int IndexInList(List<MapPoint> open, MapPoint point) {
            for (int i = 0; i < open.Count; i++) {
                if (open[i].Z == point.Z && open[i].X == point.X && open[i].Y == point.Y) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// vytvori trasu
        /// </summary>
        /// <param name="closeComeFrom">mnozina bodov ktore urcuju odkial sme prisli</param>
        /// <param name="close">mnozina bodov</param>
        /// <param name="actualIndex">aktualny index</param>
        /// <param name="start">pociatocny bod</param>
        /// <param name="end">koncovy bod</param>
        /// <returns>vracia trasu</returns>
        private static List<MapPoint> Reconstruct(List<MapPoint> closeComeFrom, List<MapPoint> close, int actualIndex, MapPoint start, MapPoint end, List<HPAEdge> edges) {
            List<MapPoint> trace = new List<MapPoint>();
            MapPoint actual = end;
            while (!actual.Equals(start)) {
                actualIndex = IndexInList(close, closeComeFrom[actualIndex]);
                HPAEdge edge = edges[IndexInList(edges, actual, close[actualIndex])];
                if (edge != null) {
                    if (edge.Trace.Count > 0 && !edge.Trace[0].Equals(actual)) {
                        edge.Trace.Reverse();
                    } 
                    edge.Trace.ForEach(point => trace.Add(point));
                }
                actual = close[actualIndex];
            }
            return trace;
        }

        /// <summary>
        /// zmensi barieru pre potrebu vypoctu trasy
        /// </summary>
        /// <param name="barrier">bariera</param>
        /// <returns>vracia upravenu barieru</returns>
        public static ObjectOfBuilding ReduceBarrier(ObjectOfBuilding barrier) {
            int maxX = int.MinValue;
            int maxY = int.MinValue;
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            foreach (MapPoint item in barrier.Corners) {
                if (item.X > maxX) {
                    maxX = item.X;
                }
                if (item.Y > maxY) {
                    maxY = item.Y;
                }
                if (item.X < minX) {
                    minX = item.X;
                }
                if (item.Y < minY) {
                    minY = item.Y;
                }
            }
            ObjectOfBuilding enlargeBarrier = new ObjectOfBuilding();
            enlargeBarrier.Corners.Add(new MapPoint(minX + 1, minY + 1, barrier.Corners[0].Z));
            enlargeBarrier.Corners.Add(new MapPoint(minX + 1, maxY - 1, barrier.Corners[0].Z));
            enlargeBarrier.Corners.Add(new MapPoint(maxX - 1, maxY - 1, barrier.Corners[0].Z));
            enlargeBarrier.Corners.Add(new MapPoint(maxX - 1, minY + 1, barrier.Corners[0].Z));
            return enlargeBarrier;
        }

        /// <summary>
        /// zmensi barieruy pre potrebu vypoctu trasy v budove
        /// </summary>
        /// <param name="barriers">mnozina barier</param>
        /// <returns>vracia upravene bariery</returns>
        public static List<ObjectOfBuilding> ReduceBarriers(List<ObjectOfBuilding> barriers) {
            List<ObjectOfBuilding> enlargeBarriers = new List<ObjectOfBuilding>();
            foreach (ObjectOfBuilding barrier in barriers) {
                enlargeBarriers.Add(ReduceBarrier(barrier));
            }
            return enlargeBarriers;
        }

        /// <summary>
        /// zisti ci sa dve priamky nepretinaju
        /// </summary>
        /// <param name="a">zaciatocny bod prvej priamky</param>
        /// <param name="b">konecny bod prvej priamky</param>
        /// <param name="c">zaciatocny bod druhej priamky</param>
        /// <param name="d">konecny bod druhej priamky</param>
        /// <returns>vracia hodnotu ci sa priamky pretinaju</returns>
        private static bool IsIntersecting(MapPoint a, MapPoint b, MapPoint c, MapPoint d) {
            float denominator = ((b.X - a.X) * (d.Y - c.Y)) - ((b.Y - a.Y) * (d.X - c.X));
            float numerator1 = ((a.Y - c.Y) * (d.X - c.X)) - ((a.X - c.X) * (d.Y - c.Y));
            float numerator2 = ((a.Y - c.Y) * (b.X - a.X)) - ((a.X - c.X) * (b.Y - a.Y));

            if (denominator == 0) return numerator1 == 0 && numerator2 == 0;

            float r = numerator1 / denominator;
            float s = numerator2 / denominator;

            bool result = (r > 0 && r < 1) && (s > 0 && s < 1);
            return result;
        }
    
        /// <summary>
        /// optimalizuje trasu
        /// </summary>
        /// <param name="trace">cesta</param>
        /// <param name="barriers">mnozina prekazok</param>
        /// <returns>vracia optimalizovanu trasu</returns>
        private static List<MapPoint> TraceOptimalization(List<MapPoint> trace, List<ObjectOfBuilding> barriers) {
            barriers = ReduceBarriers(barriers);
            bool changed = true;
            List<MapPoint> newTrace = new List<MapPoint>();
            while (changed) {
                changed = false;
                int i;
                for (i = 0; i < trace.Count - 2; i++) {
                    bool intersect = false;
                    foreach (ObjectOfBuilding barrier in barriers) {
                        for (int j = 0; j < barrier.Corners.Count && intersect == false; j++) {
                            if (IsIntersecting(trace[i], trace[i + 2], barrier.Corners[j], barrier.Corners[(j + 1) % barrier.Corners.Count]) ) {
                                intersect = true;
                            }
                        }
                    }
                    newTrace.Add(trace[i]);
                    if (!intersect) {
                        i++;
                        changed = true;
                    }
                }
                for (; i < trace.Count; i++) {
                    newTrace.Add(trace[i]);
                }
                trace = new List<MapPoint>(newTrace);
                newTrace.Clear();
            }
            return trace;
        }

        private static int IndexInList(List<HPAEdge> edges, MapPoint a, MapPoint b) {
            for (int i = 0; i < edges.Count; i++) {
                if ((edges[i].A.Equals(a) && edges[i].B.Equals(b)) || (edges[i].A.Equals(b) && edges[i].B.Equals(a))) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// vyhladava trasu v priestore rozdelenom na clustre
        /// </summary>
        /// <param name="edges">prechody medzi clustrami</param>
        /// <param name="start">startovaci bod</param>
        /// <param name="end">finalny bod</param>
        /// <param name="barriers">mnozina prechodov</param>
        /// <returns></returns>
        public static List<MapPoint> Process(List<HPAEdge> edges, MapPoint start, MapPoint end, List<ObjectOfBuilding> barriers) {
            List<MapPoint> trace = new List<MapPoint>();
            List<MapPoint> close = new List<MapPoint>();
            List<MapPoint> open = new List<MapPoint>();
            List<MapPoint> openComeFrom = new List<MapPoint>();
            List<MapPoint> closeComeFrom = new List<MapPoint>();
            List<double> openG = new List<double>();
            List<double> openH = new List<double>();
            List<double> openF = new List<double>();
            open.Add(start);
            openG.Add(0);
            openH.Add(start.Distance(end));
            openF.Add(openH[IndexInList(open, start)]);
            openComeFrom.Add(start);
            MapPoint actual = start;
            while (open.Count != 0) {
                int lower = 0;
                for (int i = 0; i < open.Count; i++) {
                    if ((double)actual.Distance(end) > (double)open[i].Distance(end) && (double)open[lower].Distance(end) > (double)open[i].Distance(end) && IndexInList(edges, actual, open[i]) != -1) {
                        lower = i;
                    }
                }
                actual = open[lower];
                if (open[lower].Equals(end)) {
                    close.Add(open[lower]);
                    closeComeFrom.Add(openComeFrom[lower]);
                    return TraceOptimalization(Reconstruct(closeComeFrom, close, IndexInList(close, open[lower]), start, end, edges), barriers);
                }
                MapPoint newPoint = new MapPoint();
                foreach (HPAEdge edge in edges) {
                        if (edge.A.Equals(actual)) {
                            newPoint = edge.B;
                            if (IndexInList(close, newPoint) == -1) {
                                double newG = newPoint.Distance(end) + openF[lower];
                                if (IndexInList(open, newPoint) == -1) {
                                    open.Add(newPoint);
                                    openG.Add(newG);
                                    openH.Add(newPoint.Distance(end));
                                    openF.Add(newG + newPoint.Distance(end));
                                    openComeFrom.Add(open[lower]);
                                }
                            }
                        } else if (edge.B.Equals(actual)) {
                            newPoint = edge.A;
                            if (IndexInList(close, newPoint) == -1) {
                                double newG = newPoint.Distance(end) + openF[lower];
                                if (IndexInList(open, newPoint) == -1) {
                                    open.Add(newPoint);
                                    openG.Add(newG);
                                    openH.Add(newPoint.Distance(end));
                                    openF.Add(newG + newPoint.Distance(end));
                                    openComeFrom.Add(open[lower]);
                                }
                            }
                        }
                }
                close.Add(open[lower]);
                closeComeFrom.Add(openComeFrom[lower]);
                open.RemoveAt(lower);
                openComeFrom.RemoveAt(lower);
                openF.RemoveAt(lower);
                openG.RemoveAt(lower);
                openH.RemoveAt(lower);
            }
            return trace;
        }       
    }
}
