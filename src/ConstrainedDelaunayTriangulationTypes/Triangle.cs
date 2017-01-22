/************************************************************************************************************
 * Author: Jan Macek                                                                                        *
 * Project: ConstrainedDelaunayTriangulationTypes.csproj                                                    *
 * File: Triangle.cs                                                                                        *
 *      - trieda trojuholniku obsahujuca informacie o hranach a vrcholoch trojuholnika                      *
 * Date: 29.4.2013                                                                                          *
 * Mail: xmacek18@fit.vutbr.cz                                                                              *
 *                                                                                                          *
 * Copyright © Jan Macek 2013                                                                               *
 * *********************************************************************************************************/
using MapObjectTypes;
using System.Collections.Generic;


namespace PathfindingApp.DelaunayTriangulationTypes {
    /// <summary>
    /// trieda trojuholniku obsahujuca informacie o hranach a vrcholoch trojuholnika
    /// </summary>
    public class Triangle {
        /// <summary>
        /// vrcholy trojuholnika
        /// </summary>
        public int a, b, c;
        public int A { get { return this.a; } set { this.a = value; } }
        public int B { get { return this.b; } set { this.b = value; } }
        public int C { get { return this.c; } set { this.c = value; } }

        /// <summary>
        ///doplnujuce body stran do trojuholnikov 
        /// </summary>
        public int ab, bc, ac;
        public int AB { get { return this.ab; } set { this.ab = value; } }
        public int BC { get { return this.bc; } set { this.bc = value; } }
        public int AC { get { return this.ac; } set { this.ac = value; } }

        /// <summary>
        /// okolia trojuholnika
        /// </summary>
        public float radius, circleX, circleY;
        public float Radius { get { return this.radius; } set { this.radius = value; } }
        public float CircleX { get { return this.circleX; } set { this.circleX = value; } }
        public float CircleY { get { return this.circleY; } set { this.circleY = value; } }

        /// <summary>
        /// konstruktor trojuholnika nastavi vrcholy podla parametrov
        /// </summary>
        public Triangle(int x, int y, int z) {
            this.a = x;
            this.b = y;
            this.c = z;
            this.ab = -1;
            this.bc = -1;
            this.ac = -1;
            this.radius = -1;
        }

        /// <summary>
        /// inicializacia trojuholnika
        /// </summary>
        /// <param name="x">1. bod trojuholnika</param>
        /// <param name="y">2. bod trojuholnika</param>
        /// <param name="z">3. bod trojuholnika</param>
        /// <param name="ab">doplnujuci bod do susedneho trojuholnika pre stranu xy</param>
        /// <param name="bc">doplnujuci bod do susedneho trojuholnika pre stranu yz</param>
        /// <param name="ac">doplnujuci bod do susedneho trojuholnika pre stranu zx</param>
        /// <param name="points"></param>
        public void Init(int x, int y, int z, int ab, int bc, int ac, List<MapPoint> points) {
            this.a = x;
            this.b = y;
            this.c = z;
            this.ab = ab;
            this.bc = bc;
            this.ac = ac;
            GenerateCircle(points);
        }

        /// <summary>
        /// vygeneruje kruhy susednosti okolo kazdeho vrcholu
        /// </summary>
        /// <param name="points">body pre ktore pocitame kruhy susednosti</param>
        public bool GenerateCircle(List<MapPoint> points) {
            // pouziva koordinaty relativne k bodu a
            MapPoint pa = points[a], pb = points[b], pc = points[c];

            double xba = pb.X - pa.X;
            double yba = pb.Y - pa.Y;
            double xca = pc.X - pa.X;
            double yca = pc.Y - pa.Y;

            // obdlzniky dlzok vrcholov patriace vrcholu a
            double balength = xba * xba + yba * yba;
            double calength = xca * xca + yca * yca;
            double D = xba * yca - yba * xca;
            if (D == 0) {
                this.circleX = 0;
                this.circleY = 0;
                this.radius = -1;
                return false;
            }
            double denom = 0.5 / D;
            double xC = (yca * balength - yba * calength) * denom;
            double yC = (xba * calength - xca * balength) * denom;
            double radius2 = xC * xC + yC * yC;
            if ((radius2 > 1e10 * balength || radius2 > 1e10 * calength)) {
                this.circleX = 0;
                this.circleY = 0;
                this.radius = -1;
                return false;
            }
            this.radius = (float)radius2;
            this.circleX = (float)(pa.X + xC);
            this.circleY = (float)(pa.Y + yC);
            return true;
        }

        /// <summary>
        /// zisti ci sa bod nachadza vo vnurtri trojuholnika
        /// </summary>
        /// <param name="point">testovany bod</param>
        /// <returns>pravdivostna hodnota vysledku</returns>
        public bool InsideTriangleCircles(MapPoint point) {
            float dx = this.circleX - point.X;
            float dy = this.circleY - point.Y;
            float radius = dx * dx + dy * dy;
            return radius < this.radius;
        }

        /// <summary>
        /// zmeni oznacenie vrcholu trojuholnika
        /// </summary>
        /// <param name="fromIndex">povodne oznacenie</param>
        /// <param name="toIndex">zmenene oznacenie</param>
        public void ChangeIndex(int fromIndex, int toIndex) {
            if (this.ab == fromIndex)
                this.ab = toIndex;
            else if (this.bc == fromIndex)
                this.bc = toIndex;
            else if (this.ac == fromIndex)
                this.ac = toIndex;
        }

        /// <summary>
        /// nastavi/vrati susedny trojuholnik pre stranu trojuholnika
        /// </summary>
        /// <param name="vertexIndex">index vrcholu trojuholnika</param>
        /// <param name="triangleIndex">index trojuholnika</param>
        /// <param name="indexOpposite">index doplnujuceho vrcholu trojuholnika ku prijatej strane</param>
        /// <param name="indexLeft">lavy vrchol trojuholnika</param>
        /// <param name="indexRight">pravy vrchol trojuholnika</param>
        public void FindNeighbour(int vertexIndex, int triangleIndex, out int indexOpposite, out int indexLeft, out int indexRight) {
            if (this.ab == triangleIndex) {
                indexOpposite = this.c;
                if (vertexIndex == this.a) {
                    indexLeft = this.ac;
                    indexRight = this.bc;
                } else {
                    indexLeft = this.bc;
                    indexRight = this.ac;
                }
            } else if (this.ac == triangleIndex) {
                indexOpposite = this.b;

                if (vertexIndex == this.a) {
                    indexLeft = this.ab;
                    indexRight = this.bc;
                } else {
                    indexLeft = this.bc;
                    indexRight = this.ab;
                }
            } else if (this.bc == triangleIndex) {
                indexOpposite = this.a;

                if (vertexIndex == this.b) {
                    indexLeft = this.ab;
                    indexRight = this.ac;
                } else {
                    indexLeft = this.ac;
                    indexRight = this.ab;
                }
            } else {
                indexOpposite = indexLeft = indexRight = 0;
            }
        }

        /// <summary>
        /// nastavi spravnu orientaciu trojuholnika
        /// </summary>
        /// <param name="points">body ktore urcuju orientaciu</param>
        public void Orientation(List<MapPoint> points) {
            float cX = (points[a].X + points[b].X + points[c].X) / 3.0f;
            float cY = (points[a].Y + points[b].Y + points[c].Y) / 3.0f;
            float dr = points[a].X - cX, dc = points[a].Y - cY;
            float dx = points[b].X - points[a].X, dy = points[b].Y - points[a].Y;
            float df = -dx * dc + dy * dr;
            if (df > 0) {
                // treba prehodit vrchol b na c a hrany ab a bc
                int pom = b;
                b = c;
                c = pom;
                pom = ab;
                ab = ac;
                ac = pom;
            }
        }
    }
}
