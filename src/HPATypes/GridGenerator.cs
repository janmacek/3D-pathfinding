/************************************************************************************************************
 * Author: Jan Macek                                                                                        *
 * Project: HPATypes.csproj                                                                                 *
 * File: GridGenerator.cs                                                                                   *
 *      - trieda obsahujuca generator ktory vytvori Grid z mapy budovy                                      *
 * Date: 29.4.2013                                                                                          *
 * Mail: xmacek18@fit.vutbr.cz                                                                              *
 *                                                                                                          *
 * Copyright © Jan Macek 2013                                                                               *
 * *********************************************************************************************************/
using PathfindingApp.AStarTypes;
using MapObjectTypes;
using System.Collections.Generic;

namespace PathfindingApp.HPATypes {
    /// <summary>
    /// reprezentuje generator kt vytvori Grid z mapy budovy
    /// </summary>
    public class GridGenerator {

        /// <summary>
        /// reprezentuje velkost generovaneho clusteru
        /// </summary>
        private int clusterSize;

        private MapPoint startPoint;

        private MapPoint finalPoint;

        private Transit startTransit;

        private Transit finalTransit;

        /// <summary>
        /// zisti ci sa bod nachadza v miestnosti
        /// </summary>
        /// <param name="point">trstovany bod</param>
        /// <returns>vracia ci sa bod nachadza v miestnosti</returns>
        private bool InRoom(MapPoint point, Room room) {
            if (point.Z == room.Corners[0].Z) {
                //skontrolujeme ci sa nenachadza v bariere
                int maxX = 0;
                int minX = int.MaxValue;
                int maxY = 0;
                int minY = int.MaxValue;
                foreach (ObjectOfBuilding barrier in room.Barriers) {
                    if (barrier.InObject(point)) {
                        return false;
                    }
                    foreach (MapPoint corner in barrier.Corners) {
                        if (corner.X > maxX) {
                            maxX = corner.X;
                        }
                        if (corner.X < minX) {
                            minX = corner.X;
                        }
                        if (corner.Y > maxY) {
                            maxY = corner.Y;
                        }
                        if (corner.Y < minY) {
                            minY = corner.Y;
                        }
                    }
                    if (point.X <= maxX && point.X >= minX && point.Y >= minY && point.Y <= maxY) {
                        return false;
                    }
                }
                //skontrolujeme ci sa nenachadza v prechode
                foreach (Transit transit in room.Transits) {
                    if (transit.InObject(point) && !transit.Equals(finalTransit) && !transit.Equals(startTransit)) {
                        return false;
                    }
                }
                //skontrolujeme ci sa nachadza v samotnej izbe
                maxX = 0;
                minX = int.MaxValue;
                maxY = 0;
                minY = int.MaxValue;
                foreach (MapPoint corner in room.Corners) {
                    if (corner.X > maxX) {
                        maxX = corner.X;
                    }
                    if (corner.X < minX) {
                        minX = corner.X;
                    }
                    if (corner.Y > maxY) {
                        maxY = corner.Y;
                    }
                    if (corner.Y < minY) {
                        minY = corner.Y;
                    }
                }
                if (point.X <= maxX && point.X >= minX && point.Y >= minY && point.Y <= maxY) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// vygeneruje hierarchicky grid pre miestnost
        /// </summary>
        /// <param name="room">miestnost pre ktoru sa grid generuje</param>
        /// <param name="start">startovaci bod v gride</param>
        /// <param name="final">cielovy bod v gride</param>
        /// <returns>vracia grid miestnosti</returns>
        public List<HPAEdge> Process(Room room, MapPoint start, Transit startTransit, MapPoint final, Transit finalTransit, int generatorClusterSize) {
            this.clusterSize = generatorClusterSize;
            this.startPoint = start;
            this.finalPoint = final;
            this.finalTransit = finalTransit;
            this.startTransit = startTransit;
            List<HPAEdge> grid = new List<HPAEdge>();
            //nacitame vnutorne prechody clusterov
            List<List<List<MapPoint>>> intraEdges = new List<List<List<MapPoint>>>();
            int maxX = 0;
            int minX = int.MaxValue;
            int maxY = 0;
            int minY = int.MaxValue;
            foreach (MapPoint corner in room.Corners) {
                if (corner.X > maxX) {
                    maxX = corner.X;
                }
                if (corner.X < minX) {
                    minX = corner.X;
                }
                if (corner.Y > maxY) {
                    maxY = corner.Y;
                }
                if (corner.Y < minY) {
                    minY = corner.Y;
                }
            }
            for (int j = 0; j <= ((maxY - minY) / clusterSize); j++) {
                List<List<MapPoint>> intraEdgesX = new List<List<MapPoint>>();
                for (int k = 0; k <= ((maxX - minX) / clusterSize); k++) {
                    intraEdgesX.Add(new List<MapPoint>());
                }
                intraEdges.Add(intraEdgesX);
            }
            MapPoint leftTopClusterCorner = new MapPoint(minX, minY, room.Corners[0].Z);
            int yCluster = -1;
            int xCluster = -1;
            while (leftTopClusterCorner.Y <= maxY) {
                yCluster++;
                xCluster = -1;
                while (leftTopClusterCorner.X <= maxX) {
                    xCluster++;
                    bool edge = false;
                    int edgeSize = 0;
                    HPAEdge edgeStart = null;

                    //vyhladame krajne inter prepojenia v Y
                    for (int i = 0; i <= clusterSize; i++) {
                        if ((InRoom(new MapPoint(leftTopClusterCorner.X + clusterSize, leftTopClusterCorner.Y + i, room.Corners[0].Z), room)) &&
                            (InRoom(new MapPoint(leftTopClusterCorner.X + clusterSize + 1, leftTopClusterCorner.Y + i, room.Corners[0].Z), room))) {
                            if (!edge) {
                                edgeStart = new HPAEdge(new MapPoint(leftTopClusterCorner.X + clusterSize, leftTopClusterCorner.Y + i, room.Corners[0].Z),
                                                        new MapPoint(leftTopClusterCorner.X + clusterSize + 1, leftTopClusterCorner.Y + i, room.Corners[0].Z),
                                                        1, HPAEdge.HPAEdgeType.inter);
                                edge = true;
                                edgeSize = 1;
                            } else {
                                edgeSize++;
                            }
                        } else if (edge && edgeSize > 0) {
                            if (edgeSize <= 3) {
                                edgeStart.A.Y += 1;
                                edgeStart.B.Y += 1;
                                grid.Add(edgeStart);
                                intraEdges[yCluster][xCluster].Add(edgeStart.A);
                                intraEdges[yCluster][xCluster + 1].Add(edgeStart.B);
                            } else {
                                grid.Add(edgeStart);
                                intraEdges[yCluster][xCluster].Add(edgeStart.A);
                                intraEdges[yCluster][xCluster + 1].Add(edgeStart.B);
                                grid.Add(new HPAEdge(new MapPoint(edgeStart.A.X, edgeStart.A.Y + edgeSize - 1, room.Corners[0].Z),
                                         new MapPoint(edgeStart.B.X, edgeStart.B.Y + edgeSize - 1, room.Corners[0].Z),
                                         1, HPAEdge.HPAEdgeType.inter));
                                intraEdges[yCluster][xCluster].Add(new MapPoint(edgeStart.A.X, edgeStart.A.Y + edgeSize - 1, room.Corners[0].Z));
                                intraEdges[yCluster][xCluster + 1].Add(new MapPoint(edgeStart.B.X, edgeStart.B.Y + edgeSize - 1, room.Corners[0].Z));
                            }
                            edge = false;
                            edgeSize = 0;
                            edgeStart = null;
                        }
                    }
                    if (edge && edgeSize > 0) {
                        if (edgeSize <= 3) {
                            edgeStart.A.Y += 1;
                            edgeStart.B.Y += 1;
                            grid.Add(edgeStart);
                            intraEdges[yCluster][xCluster].Add(edgeStart.A);
                            intraEdges[yCluster][xCluster + 1].Add(edgeStart.B);
                        } else {
                            grid.Add(edgeStart);
                            intraEdges[yCluster][xCluster].Add(edgeStart.A);
                            intraEdges[yCluster][xCluster + 1].Add(edgeStart.B);
                            grid.Add(new HPAEdge(new MapPoint(edgeStart.A.X, edgeStart.A.Y + edgeSize - 1, room.Corners[0].Z),
                                     new MapPoint(edgeStart.B.X, edgeStart.B.Y + edgeSize - 1, room.Corners[0].Z),
                                     1, HPAEdge.HPAEdgeType.inter));
                            intraEdges[yCluster][xCluster].Add(new MapPoint(edgeStart.A.X, edgeStart.A.Y + edgeSize - 1, room.Corners[0].Z));
                            intraEdges[yCluster][xCluster + 1].Add(new MapPoint(edgeStart.B.X, edgeStart.B.Y + edgeSize - 1, room.Corners[0].Z));
                        }
                        edge = false;
                        edgeSize = 0;
                        edgeStart = null;
                    }
                    edge = false;
                    edgeSize = 0;
                    edgeStart = null;

                    //vyhladame krajne inter prepojenia v X
                    for (int i = 0; i <= clusterSize; i++) {
                        if ((InRoom(new MapPoint(leftTopClusterCorner.X + i, leftTopClusterCorner.Y + clusterSize, room.Corners[0].Z), room)) &&
                            (InRoom(new MapPoint(leftTopClusterCorner.X + i, leftTopClusterCorner.Y + clusterSize + 1, room.Corners[0].Z), room)
                            )) {
                            if (!edge) {
                                edgeStart = new HPAEdge(new MapPoint(leftTopClusterCorner.X + i, leftTopClusterCorner.Y + clusterSize, room.Corners[0].Z),
                                            new MapPoint(leftTopClusterCorner.X + i, leftTopClusterCorner.Y + clusterSize + 1, room.Corners[0].Z),
                                            1, HPAEdge.HPAEdgeType.inter);
                                edge = true;
                                edgeSize = 1;
                            } else {
                                edgeSize++;
                            }
                        } else if (edge && edgeSize > 0) {
                            if (edgeSize <= 3) {
                                edgeStart.A.Y += 1;
                                edgeStart.B.Y += 1;
                                grid.Add(edgeStart);
                                intraEdges[yCluster][xCluster].Add(edgeStart.A);
                                intraEdges[yCluster + 1][xCluster].Add(edgeStart.B);
                            } else {
                                grid.Add(edgeStart);
                                intraEdges[yCluster][xCluster].Add(edgeStart.A);
                                intraEdges[yCluster + 1][xCluster].Add(edgeStart.B);
                                grid.Add(new HPAEdge(new MapPoint(edgeStart.A.X + edgeSize - 1, edgeStart.A.Y, room.Corners[0].Z),
                                         new MapPoint(edgeStart.B.X + edgeSize - 1, edgeStart.B.Y, room.Corners[0].Z),
                                         1, HPAEdge.HPAEdgeType.inter));
                                intraEdges[yCluster][xCluster].Add(new MapPoint(edgeStart.A.X + edgeSize - 1, edgeStart.A.Y, room.Corners[0].Z));
                                intraEdges[yCluster + 1][xCluster].Add(new MapPoint(edgeStart.B.X + edgeSize - 1, edgeStart.B.Y, room.Corners[0].Z));
                            }
                            edge = false;
                            edgeSize = 0;
                            edgeStart = null;
                        }
                    }
                    if (edge && edgeSize > 0) {
                        if (edgeSize <= 3) {
                            edgeStart.A.Y += 1;
                            edgeStart.B.Y += 1;
                            grid.Add(edgeStart);
                            intraEdges[yCluster][xCluster].Add(edgeStart.A);
                            intraEdges[yCluster + 1][xCluster].Add(edgeStart.B);
                        } else {
                            grid.Add(edgeStart);
                            intraEdges[yCluster][xCluster].Add(edgeStart.A);
                            intraEdges[yCluster + 1][xCluster].Add(edgeStart.B);
                            grid.Add(new HPAEdge(new MapPoint(edgeStart.A.X + edgeSize - 1, edgeStart.A.Y, room.Corners[0].Z),
                                     new MapPoint(edgeStart.B.X + edgeSize - 1, edgeStart.B.Y, room.Corners[0].Z),
                                     1, HPAEdge.HPAEdgeType.inter));
                            intraEdges[yCluster][xCluster].Add(new MapPoint(edgeStart.A.X + edgeSize - 1, edgeStart.A.Y, room.Corners[0].Z));
                            intraEdges[yCluster + 1][xCluster].Add(new MapPoint(edgeStart.B.X + edgeSize - 1, edgeStart.B.Y, room.Corners[0].Z));
                        }
                        edge = false;
                        edgeSize = 0;
                        edgeStart = null;
                    }

                    ///pridame body prechodov
                    foreach (Transit transit in room.Transits) {
                        for (int i = 0; i < transit.Corners.Count; i++) {
                            MapPoint corner = transit.Corners[i];
                            if (corner.Z == leftTopClusterCorner.Z && corner.X >= leftTopClusterCorner.X && corner.X <= leftTopClusterCorner.X + clusterSize && corner.Y >= leftTopClusterCorner.Y && corner.Y <= leftTopClusterCorner.Y + clusterSize) {
                                intraEdges[yCluster][xCluster].Add(corner);
                            }
                        }
                        MapPoint middle = Transit.Middle(transit);
                        if ((transit.InObject(start) || transit.InObject(final)) && middle.Z == leftTopClusterCorner.Z && middle.X >= leftTopClusterCorner.X && middle.X <= leftTopClusterCorner.X + clusterSize && middle.Y >= leftTopClusterCorner.Y && middle.Y <= leftTopClusterCorner.Y + clusterSize) {
                            intraEdges[yCluster][xCluster].Add(middle);
                        }
                    }

                    leftTopClusterCorner.X += clusterSize + 1;
                }
                leftTopClusterCorner.X = minX;
                leftTopClusterCorner.Y += clusterSize + 1;
            }

            //vyhladame intra prechody v clusteroch
            for (int i = 0; i < intraEdges.Count; i++) {
                List<MapPoint> points = new List<MapPoint>();
                //pridame startovaci a finalny bod
                for (int j = 0; j < intraEdges[i].Count; j++) {
                    points.Clear();
                    int maxXcluster = 0;
                    int minXcluster = int.MaxValue;
                    int maxYcluster = 0;
                    int minYcluster = int.MaxValue;
                    for (int k = 0; k < intraEdges[i][j].Count; k++) {
                        if (!InList(intraEdges[i][j][k], points)) {
                            if (intraEdges[i][j][k].Z == room.Corners[0].Z) {
                                points.Add(intraEdges[i][j][k]);
                                if (intraEdges[i][j][k].X > maxXcluster) {
                                    maxXcluster = intraEdges[i][j][k].X;
                                }
                                if (intraEdges[i][j][k].X < minXcluster) {
                                    minXcluster = intraEdges[i][j][k].X;
                                }
                                if (intraEdges[i][j][k].Y > maxYcluster) {
                                    maxYcluster = intraEdges[i][j][k].Y;
                                }
                                if (intraEdges[i][j][k].Y < minYcluster) {
                                    minYcluster = intraEdges[i][j][k].Y;
                                }
                            }
                        }
                    }
                    if (start.X >= minXcluster && start.X <= maxXcluster && start.Y >= minYcluster && start.Y <= maxYcluster && start.Z == room.Corners[0].Z) {
                        points.Add(start);
                    }

                    if (final.X >= minXcluster && final.X <= maxXcluster && final.Y >= minYcluster && final.Y <= maxYcluster && final.Z == room.Corners[0].Z) {
                        points.Add(final);
                    }

                    //vyhladame prepojenie medzi vnutornymi bodmi clusteru
                    List<ObjectOfBuilding> barriers = room.GetBarriers();
                    List<ObjectOfBuilding> toRemove = new List<ObjectOfBuilding>();
                    foreach (ObjectOfBuilding barrier in barriers) {
                        if (barrier.InObject(start) || barrier.InObject(final)) {
                            toRemove.Remove(barrier);
                        }
                    }

                    foreach (ObjectOfBuilding barrier in toRemove) {
                        barriers.Remove(barrier);
                    }
                    for (int k = 0; k < points.Count - 1; k++) {
                        for (int l = k + 1; l < points.Count; l++) {
                            List<MapPoint> traceIntra = Astar.Process(barriers, points[k], points[l], room.Transits, room, finalTransit, startTransit);
                            grid.Add(new HPAEdge(points[k], points[l], traceWidth(traceIntra), HPAEdge.HPAEdgeType.intra, traceIntra));

                        }
                    }
                }

            }
            return grid;
        }

        /// <summary>
        /// zisti ci sa hodnota nachadza v mnozine
        /// </summary>
        /// <param name="list">mnozina retazcov</param>
        /// <param name="value">hladana hodnota</param>
        /// <returns>vracia ci sa nachadza odnota v mnozine</returns>
        public bool InList(List<string> list, string value) {
            foreach (string item in list) {
                if (item == value) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// zisti ci sa hodnota nachadza v mnozine hran
        /// </summary>
        /// <param name="list">mnozina hran</param>
        /// <param name="a">prvy bod hrany</param>
        /// <param name="b">prvy bod hrany</param>
        /// <returns>vracia ci sa nachadza odnota v mnozine</returns>
        private bool InList(MapPoint a, MapPoint b, List<HPAEdge> list) {
            foreach (HPAEdge edge in list) {
                if ((edge.A.Equals(a) && edge.B.Equals(b)) || (edge.A.Equals(b) && edge.B.Equals(a))) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// zisti ci sa hodnota nachadza v mnozine
        /// </summary>
        /// <param name="list">mnozina retazcov</param>
        /// <param name="a">hladana hodnota</param>
        /// <returns>vracia ci sa nachadza odnota v mnozine</returns>
        private bool InList(MapPoint a, List<MapPoint> list) {
            foreach (MapPoint edge in list) {
                if (edge.Equals(a)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// zisti id prechodu v mnozine
        /// </summary>
        /// <param name="transitList">mnozina prechodov</param>
        /// <param name="transit">hladany prechod</param>
        /// <returns>vracia id prechodu inak -1</returns>
        public int FindIdTransit(List<Transit> transitList, Transit transit) {
            for (int i = 0; i < transitList.Count; i++) {
                if (transitList[i].Id == transit.Id && !transitList[i].Equals(transit)) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// zisti ci sa dve priamky nepretinaju
        /// </summary>
        /// <param name="a">zaciatocny bod prvej priamky</param>
        /// <param name="b">konecny bod prvej priamky</param>
        /// <param name="c">zaciatocny bod druhej priamky</param>
        /// <param name="d">konecny bod druhej priamky</param>
        /// <returns>vracia hodnotu ci sa priamky pretinaju</returns>
        public bool IsIntersecting(MapPoint a, MapPoint b, MapPoint c, MapPoint d) {
            float denominator = ((b.X - a.X) * (d.Y - c.Y)) - ((b.Y - a.Y) * (d.X - c.X));
            float numerator1 = ((a.Y - c.Y) * (d.X - c.X)) - ((a.X - c.X) * (d.Y - c.Y));
            float numerator2 = ((a.Y - c.Y) * (b.X - a.X)) - ((a.X - c.X) * (b.Y - a.Y));
            if (denominator == 0) {
                return numerator1 == 0 && numerator2 == 0;
            }
            float r = numerator1 / denominator;
            float s = numerator2 / denominator;
            bool result = (r > 0 && r < 1) && (s > 0 && s < 1);
            return result;
        }



        /// <summary>
        /// zisti dlzku trasy medzi startom a cielom
        /// </summary>
        /// <param name="list">mnozina bodov reprezentujuca trasu</param>
        /// <returns>dlzka trasy</returns>
        private double traceWidth(List<MapPoint> list) {
            if (list != null && list.Count > 1) {
                double width = 0;
                for (int i = 0; i < list.Count - 1; i++) {
                    width += list[i].Distance(list[i + 1]);
                }
                return width;
            }
            return 0;
        }   
    }
}
