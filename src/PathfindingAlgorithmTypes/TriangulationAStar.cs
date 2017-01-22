/************************************************************************************************************
 * Author: Jan Macek                                                                                        *
 * Project: AStarTypes.csproj                                                                               *
 * File: TriangulationAStar.cs                                                                              *
 *      - vyhlada trasu napriec triangulaciou pomocou metody A*                                             *
 * Date: 29.4.2013                                                                                          *
 * Mail: xmacek18@fit.vutbr.cz                                                                              *
 *                                                                                                          *
 * Copyright © Jan Macek 2013                                                                               *
 * *********************************************************************************************************/
using PathfindingApp.DelaunayTriangulationTypes;
using MapObjectTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathfindingApp.AStarTypes {
    /// <summary>
    /// vyhlada trasu napriec triangulaciou pomocou metody A*
    /// </summary>
    public class TriangulationAStar {

        /// <summary>
        /// zisti index v mnozine bodov
        /// </summary>
        /// <param name="points">mnozina bodov</param>
        /// <param name="point"></param>
        /// <returns></returns>
        private int PointIndex(List<MapPoint> points, MapPoint point) {
            for (int i = 0; i < points.Count; i++) {
                if (points[i].Equals(point)) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// ziska najlepsie ohodnoteny bod v mnozine
        /// </summary>
        /// <param name="points">mnozina bodov</param>
        /// <param name="end">koncovy bod</param>
        /// <returns>vracia index najlepsie ohodnoteneho prvku</returns>
        public int GetLower(List<MapPoint> points, MapPoint end) {
            int lower = 0;
            for (int i = 0; i < points.Count; i++) {
                if (points[i].Distance(end) < points[lower].Distance(end)) {
                    lower = i;
                }
            }
            return lower;
        }

        /// <summary>
        /// zisti ci sa bod nenachadza v mnozine
        /// </summary>
        /// <param name="open">mnozina bodov</param>
        /// <param name="points">vsetky body</param>
        /// <param name="a">hladany bod</param>
        /// <returns>vracia ci sa bod nenachadza v mnozine</returns>
        public bool NotIn(List<MapPoint> open, List<MapPoint> points, MapPoint a) {
            foreach (MapPoint point in open) {
                if (point.Equals(a)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// vytvori trasu
        /// </summary>
        /// <param name="closeComeFrom">mnozina bodov ktore urcuju odkial sme prisli</param>
        /// <param name="points">mnozina bodov</param>
        /// <param name="actual">aktualny index</param>
        /// <param name="start">pociatocny bod</param>
        /// <returns>vracia trasu</returns>
        public List<MapPoint> Reconstruct(List<MapPoint> points, List<int> comeFrom, MapPoint actual, MapPoint start) {
            List<MapPoint> trace = new List<MapPoint>();
            while (!actual.Equals(start)) {
                trace.Add(actual);
                actual = points[comeFrom[PointIndex(points, actual)]];
            }
            trace.Add(start);
            return trace;
        }

        /// <summary>
        /// vyhlada trasu napriec triangulaciou pomocou metody A*
        /// </summary>
        /// <param name="triangles">mnozina trojuholnikov tvoriacich triangulaciu</param>
        /// <param name="points">mnozina bodov priestoru</param>
        /// <param name="start">startovaci bod</param>
        /// <param name="end">cielovy bod</param>
        /// <param name="barriers">mnozina prekazok</param>
        /// <param name="barriersNotDoor">mnozina prekazok bez dvier</param>
        /// <returns></returns>
        public List<MapPoint> Process(List<Triangle> triangles, List<MapPoint> points, MapPoint start, MapPoint end, List<ObjectOfBuilding> barriers, List<ObjectOfBuilding> barriersNotDoor) {
            List<MapPoint> close = new List<MapPoint>();
            List<MapPoint> open = new List<MapPoint>();
            List<int> comeFrom = new List<int>();
            open.Add(start);
            List<double> openG = new List<double>();
            List<double> openH = new List<double>();
            List<double> openF = new List<double>();
            for (int i = 0; i < points.Count; i++) {
                openG.Add(0);
                openH.Add(points[i].Distance(end));
                openF.Add(openH[i]);
                comeFrom.Add(0);
            }
            MapPoint actual = new MapPoint(start.X, start.Y, start.Z);
            while (open.Count != 0) {
                int lower = GetLower(open, end);
                actual = points[PointIndex(points, open[lower])];
                if (actual.Equals(end)) {
                    List<MapPoint> trace = Reconstruct(points, comeFrom, end, start);
                    trace = TraceOptimalization(trace, barriersNotDoor);
                    return trace;
                }

                //vyhladame vsetky susedne trojuholniky k tomu aktualnemu
                foreach (Triangle triangle in triangles) {
                    if (triangle.a == PointIndex(points, open[lower])) {
                        if (NotIn(close, points, points[triangle.b])) {
                            double actualG = openG[PointIndex(points, open[lower])] + points[PointIndex(points, open[lower])].Distance(points[triangle.b]);
                            bool better = false;
                            if (NotIn(open, points, points[triangle.b])) {
                                open.Add(points[triangle.b]);
                                better = true;
                            } else if (actualG < openG[triangle.b]) {
                                better = true;
                            }
                            if (better) {
                                comeFrom[triangle.b] = PointIndex(points, open[lower]);
                                openG[triangle.b] = actualG;
                                openF[triangle.b] = openG[triangle.b] + openH[triangle.b];
                            }
                        }
                        if (NotIn(close, points, points[triangle.c])) {
                            double actualG = openG[PointIndex(points, open[lower])] + points[PointIndex(points, open[lower])].Distance(points[triangle.c]);
                            bool better = false;
                            if (NotIn(open, points, points[triangle.c])) {
                                open.Add(points[triangle.c]);
                                better = true;
                            } else if (actualG < openG[triangle.c]) {
                                better = true;
                            }
                            if (better) {
                                comeFrom[triangle.c] = PointIndex(points, open[lower]);
                                openG[triangle.c] = actualG;
                                openF[triangle.c] = openG[triangle.c] + openH[triangle.c];
                            }
                        }
                    } else if (triangle.b == PointIndex(points, open[lower])) {
                        if (NotIn(close, points, points[triangle.a])) {
                            double actualG = openG[PointIndex(points, open[lower])] + points[PointIndex(points, open[lower])].Distance(points[triangle.a]);
                            bool better = false;
                            if (NotIn(open, points, points[triangle.a])) {
                                open.Add(points[triangle.a]);
                                better = true;
                            } else if (actualG < openG[triangle.a]) {
                                better = true;
                            }
                            if (better) {
                                comeFrom[triangle.a] = PointIndex(points, open[lower]);
                                openG[triangle.a] = actualG;
                                openF[triangle.a] = openG[triangle.a] + openH[triangle.a];
                            }
                        }
                        if (NotIn(close, points, points[triangle.c])) {
                            double actualG = openG[PointIndex(points, open[lower])] + points[PointIndex(points, open[lower])].Distance(points[triangle.c]);
                            bool better = false;
                            if (NotIn(open, points, points[triangle.c])) {
                                open.Add(points[triangle.c]);
                                better = true;
                            } else if (actualG < openG[triangle.c]) {
                                better = true;
                            }
                            if (better) {
                                comeFrom[triangle.c] = PointIndex(points, open[lower]);
                                openG[triangle.c] = actualG;
                                openF[triangle.c] = openG[triangle.c] + openH[triangle.c];
                            }
                        }
                    } else if (triangle.c == PointIndex(points, open[lower])) {
                        if (NotIn(close, points, points[triangle.a])) {
                            double actualG = openG[PointIndex(points, open[lower])] + points[PointIndex(points, open[lower])].Distance(points[triangle.a]);
                            bool better = false;
                            if (NotIn(open, points, points[triangle.a])) {
                                open.Add(points[triangle.a]);
                                better = true;
                            } else if (actualG < openG[triangle.a]) {
                                better = true;
                            }
                            if (better) {
                                comeFrom[triangle.a] = PointIndex(points, open[lower]);
                                openG[triangle.a] = actualG;
                                openF[triangle.a] = openG[triangle.a] + openH[triangle.a];
                            }
                        }
                        if (NotIn(close, points, points[triangle.b])) {
                            double actualG = openG[PointIndex(points, open[lower])] + points[PointIndex(points, open[lower])].Distance(points[triangle.b]);
                            bool better = false;
                            if (NotIn(open, points, points[triangle.b])) {
                                open.Add(points[triangle.b]);
                                better = true;
                            } else if (actualG < openG[triangle.b]) {
                                better = true;
                            }
                            if (better) {
                                comeFrom[triangle.b] = PointIndex(points, open[lower]);
                                openG[triangle.b] = actualG;
                                openF[triangle.b] = openG[triangle.b] + openH[triangle.b];
                            }
                        }
                    }
                }
                close.Add(points[PointIndex(points, open[lower])]);
                open.RemoveAt(lower);
            }
            return null;
        }

        /// <summary>
        /// zisti ci sa dve priamky nepretinaju
        /// </summary>
        /// <param name="a">zaciatocny bod prvej priamky</param>
        /// <param name="b">konecny bod prvej priamky</param>
        /// <param name="c">zaciatocny bod druhej priamky</param>
        /// <param name="d">konecny bod druhej priamky</param>
        /// <returns>vracia hodnotu ci sa priamky pretinaju</returns>
        bool IsIntersecting(MapPoint a, MapPoint b, MapPoint c, MapPoint d) {
            float denominator = ((b.X - a.X) * (d.Y - c.Y)) - ((b.Y - a.Y) * (d.X - c.X));
            float numerator1 = ((a.Y - c.Y) * (d.X - c.X)) - ((a.X - c.X) * (d.Y - c.Y));
            float numerator2 = ((a.Y - c.Y) * (b.X - a.X)) - ((a.X - c.X) * (b.Y - a.Y));

            // Detect coincident lines (has a problem, read below)
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
        private List<MapPoint> TraceOptimalization(List<MapPoint> trace, List<ObjectOfBuilding> barriers) {
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
    }
}
