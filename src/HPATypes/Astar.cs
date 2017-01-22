/************************************************************************************************************
 * Author: Jan Macek                                                                                        *
 * Project: HPATypes.csproj                                                                                 *
 * File: Astar.cs                                                                                           *
 *      - trieda obsahujuca metody pre vyhladavanie cesty napriec dvojrozmernym priestorom metodou A*       *
 * Date: 29.4.2013                                                                                          *
 * Mail: xmacek18@fit.vutbr.cz                                                                              *
 *                                                                                                          *
 * Copyright © Jan Macek 2013                                                                               *
 * *********************************************************************************************************/
using System.Collections.Generic;
using MapObjectTypes;

namespace PathfindingApp.AStarTypes {
    /// <summary>
    /// vyhladava cestu napriec dvojrozmerneho priestoru metodou A*
    /// </summary>
    public static class Astar {

        /// <summary>
        /// najde index prvku v mnozine
        /// </summary>
        /// <param name="open">mnozina bodov</param>
        /// <param name="point">hladany bod</param>
        /// <returns>index hladaneho bodu v mnozine</returns>
        private static int IndexInList(List<MapPoint> open, MapPoint point) {
            for (int i = 0; i < open.Count; i++ ) {
                if (open[i].Z == point.Z && open[i].X == point.X && open[i].Y == point.Y) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// vyhladava cestu napriec dvojrozmerneho priestoru metodou A*
        /// </summary>
        /// <param name="barriers">mnozina prekazok</param>
        /// <param name="start">startovaci bod</param>
        /// <param name="end">cielovy bod</param>
        /// <returns>vracia cestu zo startu do ciela</returns>
        public static List<MapPoint> Process(List<ObjectOfBuilding> barriers, MapPoint start, MapPoint end, List<Transit> transits, Room room, Transit RoomFinalTransit, Transit RoomStartTransit) {
            List<MapPoint> trace = new List<MapPoint>();
            List<MapPoint> close = new List<MapPoint>();
            List<MapPoint> open = new List<MapPoint>();
            List<MapPoint> openComeFrom = new List<MapPoint>();
            List<MapPoint> closeComeFrom = new List<MapPoint>();    
            List<double> openG = new List<double>();
            List<double> openH = new List<double>();
            List<double> openF = new List<double>();
            int actualPoints = 0;
            open.Add(start);
            openG.Add(0);
            openH.Add(start.Distance(end));
            openF.Add(openH[IndexInList(open, start)]);
            openComeFrom.Add(start);
            MapPoint actual = start;
            while (open.Count != 0) {
                actualPoints++;
                int lower = 0;
                for (int i = 0; i < open.Count; i++) {
                    if ((double)actual.Distance(end) > (double)open[i].Distance(end) && (double)open[lower].Distance(end) > (double)open[i].Distance(end)) {
                        lower = i;
                    }
	            }
                if (open[lower].Equals(end)) {
                    close.Add(open[lower]);
                    closeComeFrom.Add(openComeFrom[lower]);
                    
                    return Reconstruct(closeComeFrom, close, IndexInList(close, open[lower]), start, end);
                }
                MapPoint newPoint = new MapPoint(open[lower].X + 1, open[lower].Y, open[lower].Z);
                if ((!InBarrier(barriers, newPoint, RoomFinalTransit, RoomStartTransit) || newPoint.Equals(start) || newPoint.Equals(end) || InActualTransit(newPoint, end, start, transits)))
                    if (IndexInList(close, newPoint) == -1)
                        if ((room.InObject(newPoint) || newPoint.Equals(start) || newPoint.Equals(end) || InActualTransit(newPoint, end, start, transits))) {
                            double newG = newPoint.Distance(end) + openF[lower];
                            if (IndexInList(open, newPoint) == -1) {
                                open.Add(newPoint);
                                openG.Add(newG);
                                openH.Add(newPoint.Distance(end));
                                openF.Add(newG + newPoint.Distance(end));
                                openComeFrom.Add(open[lower]);
                            }
                }

                newPoint = new MapPoint(open[lower].X + 1, open[lower].Y+1, open[lower].Z);
                if ((!InBarrier(barriers, newPoint, RoomFinalTransit, RoomStartTransit) || newPoint.Equals(start) || newPoint.Equals(end) || InActualTransit(newPoint, end, start, transits)))
                    if (IndexInList(close, newPoint) == -1)
                        if ((room.InObject(newPoint) || newPoint.Equals(start) || newPoint.Equals(end) || InActualTransit(newPoint, end, start, transits))) {
                            double newG = newPoint.Distance(end) + openF[lower];
                            if (IndexInList(open, newPoint) == -1) {
                                open.Add(newPoint);
                                openG.Add(newG);
                                openH.Add(newPoint.Distance(end));
                                openF.Add(newG + newPoint.Distance(end));
                                openComeFrom.Add(open[lower]);
                            }
                        }

                newPoint = new MapPoint(open[lower].X + 1, open[lower].Y-1, open[lower].Z);
                if ((!InBarrier(barriers, newPoint, RoomFinalTransit, RoomStartTransit) || newPoint.Equals(start) || newPoint.Equals(end) || InActualTransit(newPoint, end, start, transits)))
                    if (IndexInList(close, newPoint) == -1)
                        if ((room.InObject(newPoint) || newPoint.Equals(start) || newPoint.Equals(end) || InActualTransit(newPoint, end, start, transits))) {
                            double newG = newPoint.Distance(end) + openF[lower];
                            if (IndexInList(open, newPoint) == -1) {
                                open.Add(newPoint);
                                openG.Add(newG);
                                openH.Add(newPoint.Distance(end));
                                openF.Add(newG + newPoint.Distance(end));
                                openComeFrom.Add(open[lower]);
                            }
                        }

                newPoint = new MapPoint(open[lower].X - 1, open[lower].Y, open[lower].Z);
                if ((!InBarrier(barriers, newPoint, RoomFinalTransit, RoomStartTransit) || newPoint.Equals(start) || newPoint.Equals(end) || InActualTransit(newPoint, end, start, transits)))
                    if (IndexInList(close, newPoint) == -1)
                        if ((room.InObject(newPoint) || newPoint.Equals(start) || newPoint.Equals(end) || InActualTransit(newPoint, end, start, transits))) {
                            double newG = newPoint.Distance(end) + openF[lower];
                            if (IndexInList(open, newPoint) == -1) {
                                open.Add(newPoint);
                                openG.Add(newG);
                                openH.Add(newPoint.Distance(end));
                                openF.Add(newG + newPoint.Distance(end));
                                openComeFrom.Add(open[lower]);
                            }
                        }

                newPoint = new MapPoint(open[lower].X - 1, open[lower].Y+1, open[lower].Z);
                if ((!InBarrier(barriers, newPoint, RoomFinalTransit, RoomStartTransit) || newPoint.Equals(start) || newPoint.Equals(end) || InActualTransit(newPoint, end, start, transits)))
                    if (IndexInList(close, newPoint) == -1)
                        if ((room.InObject(newPoint) || newPoint.Equals(start) || newPoint.Equals(end) || InActualTransit(newPoint, end, start, transits))) {
                            double newG = newPoint.Distance(end) + openF[lower];
                            if (IndexInList(open, newPoint) == -1) {
                                open.Add(newPoint);
                                openG.Add(newG);
                                openH.Add(newPoint.Distance(end));
                                openF.Add(newG + newPoint.Distance(end));
                                openComeFrom.Add(open[lower]);
                            }
                        }

                newPoint = new MapPoint(open[lower].X - 1, open[lower].Y-1, open[lower].Z);
                if ((!InBarrier(barriers, newPoint, RoomFinalTransit, RoomStartTransit) || newPoint.Equals(start) || newPoint.Equals(end) || InActualTransit(newPoint, end, start, transits)))
                    if (IndexInList(close, newPoint) == -1)
                        if ((room.InObject(newPoint) || newPoint.Equals(start) || newPoint.Equals(end) || InActualTransit(newPoint, end, start, transits))) {
                            double newG = newPoint.Distance(end) + openF[lower];
                            if (IndexInList(open, newPoint) == -1) {
                                open.Add(newPoint);
                                openG.Add(newG);
                                openH.Add(newPoint.Distance(end));
                                openF.Add(newG + newPoint.Distance(end));
                                openComeFrom.Add(open[lower]);
                            }
                        }

                newPoint = new MapPoint(open[lower].X, open[lower].Y+1, open[lower].Z);
                if (room.InObject(newPoint) && IndexInList(close, newPoint) == -1)
                    if ((!InBarrier(barriers, newPoint, RoomFinalTransit, RoomStartTransit) || newPoint.Equals(start) || newPoint.Equals(end) || InActualTransit(newPoint, end, start, transits))) {
                        double newG = newPoint.Distance(end) + openF[lower];
                        if (IndexInList(open, newPoint) == -1) {
                            open.Add(newPoint);
                            openG.Add(newG);
                            openH.Add(newPoint.Distance(end));
                            openF.Add(newG + newPoint.Distance(end));
                            openComeFrom.Add(open[lower]);
                        }
                    }

                newPoint = new MapPoint(open[lower].X, open[lower].Y-1, open[lower].Z);
                if ((!InBarrier(barriers, newPoint, RoomFinalTransit, RoomStartTransit) || newPoint.Equals(start) || newPoint.Equals(end) || InActualTransit(newPoint, end, start, transits)))
                    if (IndexInList(close, newPoint) == -1)
                        if ((room.InObject(newPoint) || newPoint.Equals(start) || newPoint.Equals(end) || InActualTransit(newPoint, end, start, transits))) {
                            double newG = newPoint.Distance(end) + openF[lower];
                            if (IndexInList(open, newPoint) == -1) {
                                open.Add(newPoint);
                                openG.Add(newG);
                                openH.Add(newPoint.Distance(end));
                                openF.Add(newG + newPoint.Distance(end));
                                openComeFrom.Add(open[lower]);
                            }
                        }
                actual = open[lower];
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

        /// <summary>
        /// zisti ci sa nachadza bod vo finalnom/cielovom prechode
        /// </summary>
        /// <param name="newPoint">hladany bod prechodu</param>
        /// <param name="end">finalny bod</param>
        /// <param name="start">cielovy bod</param>
        /// <param name="transits">mnozina prechodov</param>
        private static bool InActualTransit(MapPoint newPoint, MapPoint end, MapPoint start, List<Transit> transits) {
            foreach (Transit transit in transits) {
                if ((Transit.Middle(transit).Equals(end) || Transit.Middle(transit).Equals(start)) && transit.InObject(newPoint)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// zisti ci sa bod nachadza v prekazke
        /// </summary>
        /// <param name="barriers">mnozina prekazok</param>
        /// <param name="newPoint">testovany bod</param>
        /// <returns>vracia ci sa bod nachadza v prekazke</returns>
        public static bool InBarrier(List<ObjectOfBuilding> barriers, MapPoint newPoint, Transit finaltransit, Transit startTransit) {
            

            foreach (ObjectOfBuilding barrier in barriers) {
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
                if (newPoint.X > minX - 1 && newPoint.X < maxX + 1 && newPoint.Y > minY - 1 && newPoint.Y < maxY + 1) {
                    if (barrier.Equals(finaltransit) || barrier.Equals(startTransit)) {
                        return false;
                    } else {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// optimalizuje trasu
        /// </summary>
        /// <param name="trace">cesta</param>
        /// <param name="barriers">mnozina prechodov</param>
        /// <returns>vracia optimalizovanu trasu</returns>
        private static List<MapPoint> TraceOptimalization(List<MapPoint> trace, List<ObjectOfBuilding> barriers) {
            bool changed = true;
            List<MapPoint> newTrace = new List<MapPoint>();
            while (changed) {
                changed = false;
                int i;
                for (i = 0; i < trace.Count - 2; i++) {
                    bool intersect = false;
                    foreach (ObjectOfBuilding barrier in barriers) {
                        for (int j = 0; j < barrier.Corners.Count && intersect == false; j++) {
                            if (IsIntersecting(trace[i], trace[i + 2], barrier.Corners[j], barrier.Corners[(j + 1) % barrier.Corners.Count])) {
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

        /// <summary>
        /// vytvori trasu
        /// </summary>
        /// <param name="closeComeFrom">mnozina bodov ktore urcuju odkial sme prisli</param>
        /// <param name="close">mnozina bodov</param>
        /// <param name="actualIndex">aktualny index</param>
        /// <param name="start">pociatocny bod</param>
        /// <param name="end">koncovy bod</param>
        /// <returns>vracia trasu</returns>
        private static List<MapPoint> Reconstruct(List<MapPoint> closeComeFrom, List<MapPoint> close, int actualIndex, MapPoint start, MapPoint end) {
            List<MapPoint> trace = new List<MapPoint>();
            while (!closeComeFrom[actualIndex].Equals(start)) {
                trace.Add(close[actualIndex]);
                actualIndex = IndexInList(close, closeComeFrom[actualIndex]);
            }
            trace.Add(close[actualIndex]);
            return trace;
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
    }
}
