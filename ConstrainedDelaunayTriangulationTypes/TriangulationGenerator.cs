/************************************************************************************************************
 * Author: Jan Macek                                                                                        *
 * Project: ConstrainedDelaunayTriangulationTypes.csproj                                                    *
 * File: TriangulationGenerator.cs                                                                          *
 *      - trieda obsahujuca metody na generovanie delaunayovej trianguacie                                  *
 * Date: 29.4.2013                                                                                          *
 * Mail: xmacek18@fit.vutbr.cz                                                                              *
 *                                                                                                          *
 * Copyright © Jan Macek 2013                                                                               *
 * *********************************************************************************************************/
using MapObjectTypes;
using System;
using System.Collections.Generic;

namespace PathfindingApp.DelaunayTriangulationTypes {

    /// <summary>
    /// trieda obsahujuca metody na generovanie delaunayovej trianguacie
    /// </summary>
    public class TriangulationGenerator {

        /// <summary>
        /// body pre ktore sa triangulacia pocita 
        /// </summary>
        private List<MapPoint> points;

        /// <summary>
        /// povolena odchylka vypoctu
        /// </summary>
        public const float fraction = 0.3f;

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

            if (denominator == 0) return numerator1 == 0 && numerator2 == 0;

            float r = numerator1 / denominator;
            float s = numerator2 / denominator;

            bool result = (r > 0 && r < 1) && (s > 0 && s < 1);
            return result;
        }

        /// <summary>
        /// vytvori delaunayovu triangulaciu
        /// </summary>
        /// <param name="points">vrcholy mapy</param>
        /// <param name="barriers">prekazky na mape</param>
        /// <returns>vracia delaunayovu triangulaciu</returns>
        public List<Triangle> Triangulation(List<MapPoint> points, List<ObjectOfBuilding> barriers) {
            List<Triangle> triangles = new List<Triangle>();
            Convex convex = new Convex();
            this.Preprocess(points, convex, triangles, false);
            int numt = triangles.Count;
            bool[] idsA = new bool[numt];
            bool[] idsB = new bool[numt];
            int flipped = FlipTriangles(triangles, idsA);
            int iterations = 1;
            while (flipped > (int)(fraction * (float)numt) && iterations < 1000) {
                if ((iterations & 1) == 1)
                    flipped = FlipTriangles(triangles, idsA, idsB);
                else
                    flipped = FlipTriangles(triangles, idsB, idsA);

                iterations++;
            }
            Set<int> idSetA = new Set<int>(), idSetB = new Set<int>();
            flipped = FlipTriangles(triangles, ((iterations & 1) == 1) ? idsA : idsB, idSetA);
            iterations = 1;
            while (flipped > 0 && iterations < 2000) {
                if ((iterations & 1) == 1) {
                    flipped = FlipTriangles(triangles, idSetA, idSetB);
                } else {
                    flipped = FlipTriangles(triangles, idSetB, idSetA);
                }
                iterations++;
            }

            //odstrani trojuholniky ktore su v kolizii s barierami
            List<Triangle> toRemove = new List<Triangle>();
            foreach (Triangle triangle in triangles) {
                foreach (ObjectOfBuilding barrier in barriers) {
                    int samePoint = 0;
                    foreach (MapPoint barrierPoint in barrier.Corners) {
                        if (barrierPoint.X == points[triangle.a].X && barrierPoint.Y == points[triangle.a].Y) {
                            samePoint++;
                        } else if (barrierPoint.X == points[triangle.b].X && barrierPoint.Y == points[triangle.b].Y) {
                            samePoint++;
                        } else if (barrierPoint.X == points[triangle.c].X && barrierPoint.Y == points[triangle.c].Y) {
                            samePoint++;
                        }
                    }
                    if (samePoint >= 3) {
                        toRemove.Add(triangle);
                    }
                }
            }

            foreach (Triangle triangle in toRemove) {
                triangles.Remove(triangle);
            }

            toRemove.Clear();
            foreach (Triangle triangle in triangles) {
                bool intersect = false;
                foreach (ObjectOfBuilding barrier in barriers) {
                    for (int j = 0; j < barrier.Corners.Count - 1 && intersect == false; j++) {
                        if ((IsIntersecting(points[triangle.a], points[triangle.b], barrier.Corners[j], barrier.Corners[(j + 1) % barrier.Corners.Count])
                                && (!points[triangle.a].Equals(barrier.Corners[j]) && !points[triangle.b].Equals(barrier.Corners[j + 1]))
                                && (!points[triangle.b].Equals(barrier.Corners[j]) && !points[triangle.a].Equals(barrier.Corners[j + 1]))
                            ) ||
                            (IsIntersecting(points[triangle.b], points[triangle.c], barrier.Corners[j], barrier.Corners[(j + 1) % barrier.Corners.Count])
                                && (!points[triangle.b].Equals(barrier.Corners[j]) && !points[triangle.c].Equals(barrier.Corners[j + 1]))
                                && (!points[triangle.c].Equals(barrier.Corners[j]) && !points[triangle.b].Equals(barrier.Corners[j + 1]))
                            ) ||
                            (IsIntersecting(points[triangle.c], points[triangle.a], barrier.Corners[j], barrier.Corners[(j + 1) % barrier.Corners.Count])
                                && (!points[triangle.c].Equals(barrier.Corners[j]) && !points[triangle.a].Equals(barrier.Corners[j + 1]))
                                && (!points[triangle.a].Equals(barrier.Corners[j]) && !points[triangle.c].Equals(barrier.Corners[j + 1]))
                            )) {
                            intersect = true;
                        }
                    }
                }
                if (intersect) {
                    toRemove.Add(triangle);
                }
            }
            foreach (Triangle triangle in toRemove) {
                triangles.Remove(triangle);
            }
            return triangles;
        }

        /// <summary>
        /// predspracuje triangulaciu priestoru
        /// </summary>
        /// <param name="suppliedPoints">body triangulacie</param>
        /// <param name="convex">konvexny bod priestoru</param>
        /// <param name="triangles">triangulacia</param>
        private void Preprocess(List<MapPoint> suppliedPoints, Convex convex, List<Triangle> triangles, bool hullOnly = false) {
            //na trojuholnik je potreba minimalne 3 body
            if (suppliedPoints.Count < 3) {
                throw new ArgumentException("Number of points supplied must be >= 3");
            }
            this.points = suppliedPoints;
            int pointsCount = points.Count;
            float[] distanceTo = new float[pointsCount];
            int[] pointsPositions = new int[pointsCount];

            // vybereme prvy bod
            for (int k = 0; k < pointsCount; k++) {
                distanceTo[k] = points[0].Distance(points[k]);
                pointsPositions[k] = k;
            }

            // usporiadame podla vzdialenosti
            Array.Sort(distanceTo, pointsPositions);

            //odstranenie duplikatnych bodov
            for (int k = pointsCount - 2; k >= 0; k--) {
                if ((points[pointsPositions[k]].X == points[pointsPositions[k + 1]].X) &&
                    (points[pointsPositions[k]].Y == points[pointsPositions[k + 1]].Y)) {
                    Array.Copy(pointsPositions, k + 2, pointsPositions, k + 1, pointsCount - k - 2);
                    Array.Copy(distanceTo, k + 2, distanceTo, k + 1, pointsCount - k - 2);
                    pointsCount--;
                }
            }

            //na trojuholnik je potreba minimalne 3 body
            if (pointsCount < 3) {
                throw new ArgumentException("Points count should be more or equal to 3");
            }
            int middle = -1;
            float radiusMin = float.MaxValue, circumCentreX = 0, circumCentreY = 0;

            // najdeme trojuholnik ktory tvori s dvomi susedmi najmensie kruhy susednosti
            Triangle triangle = new Triangle(pointsPositions[0], pointsPositions[1], 2);
            for (int kc = 2; kc < pointsCount; kc++) {
                triangle.c = pointsPositions[kc];
                if (triangle.GenerateCircle(points) && triangle.radius < radiusMin) {
                    middle = kc;
                    radiusMin = triangle.radius;
                    circumCentreX = triangle.circleX;
                    circumCentreY = triangle.circleY;
                } else if (radiusMin * 4 < distanceTo[kc])
                    break;
            }
            if (middle != 2) {
                int indexMiddle = pointsPositions[middle];
                float distance2Middle = distanceTo[middle];

                Array.Copy(pointsPositions, 2, pointsPositions, 3, middle - 2);
                Array.Copy(distanceTo, 2, distanceTo, 3, middle - 2);
                pointsPositions[2] = indexMiddle;
                distanceTo[2] = distance2Middle;
            }
            triangle.c = pointsPositions[2];
            triangle.Orientation(points);
            triangle.GenerateCircle(points);
            triangles.Add(triangle);
            convex.Add(new ConvexPoint(points, triangle.a));
            convex.Add(new ConvexPoint(points, triangle.b));
            convex.Add(new ConvexPoint(points, triangle.c));
            MapPoint centre = new MapPoint((int)circumCentreX, (int)circumCentreY, 0);
            for (int k = 3; k < pointsCount; k++) {
                distanceTo[k] = points[pointsPositions[k]].Distance(centre);
            }
            Array.Sort(distanceTo, pointsPositions, 3, pointsCount - 3);
            int numt = 0;
            for (int k = 3; k < pointsCount; k++) {
                int pointsIndex = pointsPositions[k];
                ConvexPoint ptx = new ConvexPoint(points, pointsIndex);
                float dx = ptx.X - convex[0].X, dy = ptx.Y - convex[0].Y;
                int numh = convex.Count, numh_old = numh;
                List<int> pidx = new List<int>(), tridx = new List<int>();
                int hidx;
                if (convex.EdgeVisibleFrom(0, dx, dy)) {
                    int e2 = numh;
                    hidx = 0;
                    if (convex.EdgeVisibleFrom(numh - 1, dx, dy)) {
                        pidx.Add(convex[numh - 1].pointsIndex);
                        tridx.Add(convex[numh - 1].triadIndex);
                        for (int h = 0; h < numh - 1; h++) {
                            pidx.Add(convex[h].pointsIndex);
                            tridx.Add(convex[h].triadIndex);
                            if (convex.EdgeVisibleFrom(h, ptx)) {
                                convex.RemoveAt(h);
                                h--;
                                numh--;
                            } else {
                                convex.Insert(0, ptx);
                                numh++;
                                break;
                            }
                        }
                        for (int h = numh - 2; h > 0; h--) {
                            if (convex.EdgeVisibleFrom(h, ptx)) {
                                pidx.Insert(0, convex[h].pointsIndex);
                                tridx.Insert(0, convex[h].triadIndex);
                                convex.RemoveAt(h + 1);
                            } else {
                                break;
                            }
                        }
                    } else {
                        hidx = 1;
                        tridx.Add(convex[0].triadIndex);
                        pidx.Add(convex[0].pointsIndex);
                        for (int h = 1; h < numh; h++) {
                            pidx.Add(convex[h].pointsIndex);
                            tridx.Add(convex[h].triadIndex);
                            if (convex.EdgeVisibleFrom(h, ptx)) {
                                convex.RemoveAt(h);
                                h--;
                                numh--;
                            } else {
                                convex.Insert(h, ptx);
                                break;
                            }
                        }
                    }
                } else {
                    int e1 = -1, e2 = numh;
                    for (int h = 1; h < numh; h++) {
                        if (convex.EdgeVisibleFrom(h, ptx)) {
                            if (e1 < 0)
                                e1 = h;
                        } else {
                            if (e1 > 0) {
                                e2 = h;
                                break;
                            }
                        }
                    }
                    if (e2 < numh) {
                        for (int e = e1; e <= e2; e++) {
                            pidx.Add(convex[e].pointsIndex);
                            tridx.Add(convex[e].triadIndex);
                        }
                    } else if (e1 >= 0) {
                        for (int e = e1; e < e2; e++) {
                            pidx.Add(convex[e].pointsIndex);
                            tridx.Add(convex[e].triadIndex);
                        }
                        pidx.Add(convex[0].pointsIndex);
                    }
                    if (e1 < e2 - 1) {
                        convex.RemoveRange(e1 + 1, e2 - e1 - 1);
                    }
                    convex.Insert(e1 + 1, ptx);
                    hidx = e1 + 1;
                }
                if (hullOnly) {
                    continue;
                }
                int a = pointsIndex, T0;
                int npx = pidx.Count - 1;
                numt = triangles.Count;
                T0 = numt;
                for (int p = 0; p < npx; p++) {
                    Triangle triangleX = new Triangle(a, pidx[p], pidx[p + 1]);
                    triangleX.GenerateCircle(points);
                    triangleX.bc = tridx[p];
                    if (p > 0) {
                        triangleX.ab = numt - 1;
                    }
                    triangleX.ac = numt + 1;
                    Triangle txx = triangles[tridx[p]];
                    if ((triangleX.b == txx.a && triangleX.c == txx.b) | (triangleX.b == txx.b && triangleX.c == txx.a)) {
                        txx.ab = numt;
                    } else if ((triangleX.b == txx.a && triangleX.c == txx.c) | (triangleX.b == txx.c && triangleX.c == txx.a)) {
                        txx.ac = numt;
                    } else if ((triangleX.b == txx.b && triangleX.c == txx.c) | (triangleX.b == txx.c && triangleX.c == txx.b)) {
                        txx.bc = numt;
                    }
                    triangles.Add(triangleX);
                    numt++;
                }
                triangles[numt - 1].ac = -1;
                convex[hidx].triadIndex = numt - 1;
                if (hidx > 0) {
                    convex[hidx - 1].triadIndex = T0;
                } else {
                    numh = convex.Count;
                    convex[numh - 1].triadIndex = T0;
                }
            }
        }

        /// <summary>
        /// Prejde trojuholnik a jeho susedov, pokial najde nejaky trojuholnik v kruznicovom okoli tak ho zrusi
        /// </summary>
        /// <param name="triangles">mnozina trojuholnikov</param>
        /// <param name="triadIndexToTest">index testovaneho trojuholnika</param>
        /// <param name="triadIndexFlipped">index trojuholnika ktory ma byt vyhodeny</param>
        /// <returns>vracia ci bol nejaky vyhodeny</returns>
        bool FlipTriangle(List<Triangle> triangles, int triadIndexToTest, out int triadIndexFlipped) {
            int oppositeVertex = 0, edge1, edge2, edge3 = 0, edge4 = 0;
            triadIndexFlipped = 0;
            Triangle triangle = triangles[triadIndexToTest];
            if (triangle.bc >= 0) {
                triadIndexFlipped = triangle.bc;
                Triangle t2 = triangles[triadIndexFlipped];
                t2.FindNeighbour(triangle.b, triadIndexToTest, out oppositeVertex, out edge3, out edge4);
                if (triangle.InsideTriangleCircles(points[oppositeVertex])) {
                    edge1 = triangle.ab;
                    edge2 = triangle.ac;
                    if (edge1 != edge3 && edge2 != edge4) {
                        int tria = triangle.a, trib = triangle.b, tric = triangle.c;
                        triangle.Init(tria, trib, oppositeVertex, edge1, edge3, triadIndexFlipped, points);
                        t2.Init(tria, tric, oppositeVertex, edge2, edge4, triadIndexToTest, points);
                        if (edge3 >= 0) {
                            triangles[edge3].ChangeIndex(triadIndexFlipped, triadIndexToTest);
                        }
                        if (edge2 >= 0) {
                            triangles[edge2].ChangeIndex(triadIndexToTest, triadIndexFlipped);
                        }
                        return true;
                    }
                }
            }
            if (triangle.ab >= 0) {
                triadIndexFlipped = triangle.ab;
                Triangle t2 = triangles[triadIndexFlipped];

                t2.FindNeighbour(triangle.a, triadIndexToTest, out oppositeVertex, out edge3, out edge4);
                if (triangle.InsideTriangleCircles(points[oppositeVertex])) {
                    edge1 = triangle.ac;
                    edge2 = triangle.bc;
                    if (edge1 != edge3 && edge2 != edge4) {
                        int tria = triangle.a, trib = triangle.b, tric = triangle.c;
                        triangle.Init(tric, tria, oppositeVertex, edge1, edge3, triadIndexFlipped, points);
                        t2.Init(tric, trib, oppositeVertex, edge2, edge4, triadIndexToTest, points);
                        if (edge3 >= 0) {
                            triangles[edge3].ChangeIndex(triadIndexFlipped, triadIndexToTest);
                        }
                        if (edge2 >= 0) {
                            triangles[edge2].ChangeIndex(triadIndexToTest, triadIndexFlipped);
                        }
                        return true;
                    }
                }
            }
            if (triangle.ac >= 0) {
                triadIndexFlipped = triangle.ac;
                Triangle t2 = triangles[triadIndexFlipped];
                t2.FindNeighbour(triangle.a, triadIndexToTest, out oppositeVertex, out edge3, out edge4);
                if (triangle.InsideTriangleCircles(points[oppositeVertex])) {
                    edge1 = triangle.ab;
                    edge2 = triangle.bc;
                    if (edge1 != edge3 && edge2 != edge4) {
                        int tria = triangle.a, trib = triangle.b, tric = triangle.c;
                        triangle.Init(trib, tria, oppositeVertex, edge1, edge3, triadIndexFlipped, points);
                        t2.Init(trib, tric, oppositeVertex, edge2, edge4, triadIndexToTest, points);
                        if (edge3 >= 0) {
                            triangles[edge3].ChangeIndex(triadIndexFlipped, triadIndexToTest);
                        }
                        if (edge2 >= 0) {
                            triangles[edge2].ChangeIndex(triadIndexToTest, triadIndexFlipped);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Vyhodi trojuholniky ktore nesplnaju delaunayovu triangulaciu
        /// </summary>
        /// <param name="triads">mnozina trojuholnikov</param>
        /// <returns>pozicia vyradovaneho trojuholnika</returns>
        private int FlipTriangles(List<Triangle> triads, bool[] idsFlipped) {
            int numt = (int)triads.Count;
            Array.Clear(idsFlipped, 0, numt);
            int flipped = 0;
            for (int t = 0; t < numt; t++) {
                int t2;
                if (FlipTriangle(triads, t, out t2)) {
                    flipped += 2;
                    idsFlipped[t] = true;
                    idsFlipped[t2] = true;

                }
            }
            return flipped;
        }

        /// <summary>
        /// Vyhodi trojuholniky ktore nesplnaju delaunayovu triangulaciu
        /// </summary>
        /// <param name="triads">mnozina trojuholnikov</param>
        /// <param name="idsToTest">mnozina ktora oznacuje kt trojuholnik maju byt testovane</param>
        /// <param name="idsFlipped">oznacuje ktore trojuholniky maju byt vyhodene</param>
        /// <returns>ak bol pridany trojuholnik na vyhodenie</returns>
        private int FlipTriangles(List<Triangle> triads, bool[] idsToTest, bool[] idsFlipped) {
            int numt = (int)triads.Count;
            Array.Clear(idsFlipped, 0, numt);
            int flipped = 0;
            for (int t = 0; t < numt; t++) {
                if (idsToTest[t]) {
                    int t2;
                    if (FlipTriangle(triads, t, out t2)) {
                        flipped += 2;
                        idsFlipped[t] = true;
                        idsFlipped[t2] = true;
                    }
                }
            }
            return flipped;
        }

        /// <summary>
        /// Vyhodi trojuholniky ktore nesplnaju delaunayovu triangulaciu
        /// </summary>
        /// <param name="triads">mnozina trojuholnikov</param>
        /// <param name="idsToTest">mnozina ktora oznacuje kt trojuholnik maju byt testovane</param>
        /// <param name="idsFlipped">oznacuje ktore trojuholniky maju byt vyhodene</param>
        /// <returns>pocet trojuholnikov pridanych na vyhodenie</returns>
        private int FlipTriangles(List<Triangle> triads, bool[] idsToTest, Set<int> idsFlipped) {
            int numt = (int)triads.Count;
            idsFlipped.Clear();
            int flipped = 0;
            for (int t = 0; t < numt; t++) {
                if (idsToTest[t]) {
                    int t2;
                    if (FlipTriangle(triads, t, out t2)) {
                        flipped += 2;
                        idsFlipped.Add(t);
                        idsFlipped.Add(t2);
                    }
                }
            }
            return flipped;
        }

        /// <summary>
        /// Prejde trojuholniky a ich susedov, pokial najde nejaky trojuholnik v kruznicovom okoli tak ho zrusi
        /// </summary>
        /// <param name="triangles">mnozina trojuholnikov</param>
        /// <param name="triadIndexToTest">indexy testovaneho trojuholnika</param>
        /// <param name="triadIndexFlipped">indexy trojuholnikov ktory ma byt vyhodeny</param>
        /// <returns>vracia pocet vyhodenych trojuholnikov</returns>
        private int FlipTriangles(List<Triangle> triads, Set<int> idsToTest, Set<int> idsFlipped) {
            int flipped = 0;
            idsFlipped.Clear();
            foreach (int t in idsToTest) {
                int t2;
                if (FlipTriangle(triads, t, out t2)) {
                    flipped += 2;
                    idsFlipped.Add(t);
                    idsFlipped.Add(t2);
                }
            }
            return flipped;
        }
    }
}
