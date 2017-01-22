/************************************************************************************************************
 * Author: Jan Macek                                                                                        *
 * Project: AStarTypes.csproj                                                                               *
 * File: Astar3D.cs                                                                                         *
 *      - vyhlada cestu pomocou A* algoritmu napriec budovou (pouziva door-to-door algoritmus)              *
 * Date: 29.4.2013                                                                                          *
 * Mail: xmacek18@fit.vutbr.cz                                                                              *
 *                                                                                                          *
 * Copyright © Jan Macek 2013                                                                               *
 * *********************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapObjectTypes;
using PathfindingApp.DelaunayTriangulationTypes;

namespace PathfindingApp.AStarTypes {
    /// <summary>
    /// vyhlada cestu pomocou A* algoritmu napriec budovou
    /// </summary>
    public class Astar3D {
        /// <summary>
        /// vsetky prechody v budove
        /// </summary>
        private List<Transit> transits = new List<Transit>();

        /// <summary>
        /// cesty medzi prechodmi budovy
        /// </summary>
        private List<TransitRoute> transitRoutes = new List<TransitRoute>();
        
        /// <summary>
        /// zvacsi bariery pre potrebu vyhladania trasy
        /// </summary>
        /// <param name="barriers">mnozina barier</param>
        /// <returns>vracia mnozinu zvacsenych barier</returns>
        public List<ObjectOfBuilding> EnlargeBarriers(List<ObjectOfBuilding> barriers) {
            List<ObjectOfBuilding> enlargeBarriers = new List<ObjectOfBuilding>();
            foreach (ObjectOfBuilding barrier in barriers) {
                enlargeBarriers.Add(new Map().EnlargeBarrier(barrier));
            }
            return enlargeBarriers;
        }

        /// <summary>
        /// najde index prechodu
        /// </summary>
        /// <param name="transit1">index startovacieho bodu</param>
        /// <param name="transit2">index cieloveho bodu</param>
        /// <returns>vracia index prechodu pokial existuje, inak -1</returns>
        private int TransitRoutesIndex(int transit1, int transit2) {
            for (int i = 0; i < this.transitRoutes.Count; i++) {
                if ((this.transitRoutes[i].From == transit1 && this.transitRoutes[i].To == transit2)
                    || (this.transitRoutes[i].From == transit2 && this.transitRoutes[i].To == transit1)) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// najde index prechodu
        /// </summary>
        /// <returns>vracia index prechodu pokial existuje, inak -1</returns>
        private int TransitIndex(Transit actualTransit) {
            for (int i = 0; i < this.transits.Count; i++) {
                if (this.transits[i].Equals(actualTransit)) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// hlavna funkcia ktoru vykonava A*
        /// </summary>
        /// <param name="map">mapa budovy</param>
        /// <returns>vracia cestu v budove zo startu do ciela</returns>
        public List<MapPoint> Process(Map map, System.Windows.Controls.TabControl tabs, bool showGrid) {

            TriangulationAStar triAStar = new TriangulationAStar();
            TriangulationGenerator triangulator = new TriangulationGenerator();

            //nacitam vsetky prechody v budove
            foreach (Floor floor in map.Floors) {
                foreach (Room room in floor.Rooms) {
                    foreach (var transit in room.Transits) {
                        transits.Add(transit);
                    }
                }
            }

            //pridame startovaci
            Transit startTransit = new Transit(0, "start", Transit.transitType.nonSet);
            startTransit.Corners.Add(map.Start);
            transits.Add(startTransit);

            //pridame finalny bod
            Transit finalTransit = new Transit(0, "final", Transit.transitType.nonSet);
            finalTransit.Corners.Add(map.Final);
            transits.Add(finalTransit);

            //pre kazdu miestnost a jej prechody + final/start najdem optimalnu cestu
            int startIndex, finalIndex;
            for (int h = 0; h < map.Floors.Count; h++) {
                Floor floor = map.Floors[h];
                for (int p = 0; p < floor.Rooms.Count; p++) {
                    Room room = floor.Rooms[p];
                    //ak je final a start v rovnakej miestnosti
                    if (room.InRoom(map.Final) && room.InRoom(map.Start)) {
                        startIndex = TransitIndex(startTransit);
                        finalIndex = TransitIndex(finalTransit);
                        //nacitam body pre triangulaciu
                        List<MapPoint> points = new List<MapPoint>();
                        foreach (MapPoint corner in room.Corners) {
                            points.Add(corner);
                        }
                        //nacitam bariery a ich body
                        List<ObjectOfBuilding> barriers = new List<ObjectOfBuilding>();
                        foreach (ObjectOfBuilding barrier in room.Barriers) {
                            ObjectOfBuilding newBarrier = new Map().EnlargeBarrier(barrier);
                            barriers.Add(newBarrier);
                            foreach (MapPoint corner in newBarrier.Corners) {
                                points.Add(corner);
                            }
                        }

                        //pridam startovaci bod
                        points.Add(map.Start);
                        //pridam cielovy bod
                        points.Add(map.Final);
                        //nacitam body prechodov ako bariery
                        foreach (Transit transit in room.TransitsNotDoor()) {
                            if (!transit.Equals(transits[startIndex]) && !transit.Equals(finalIndex)) {
                                ObjectOfBuilding newTransit = new Map().EnlargeBarrier(transit);
                                barriers.Add(newTransit);
                                foreach (MapPoint corner in newTransit.Corners) {
                                    points.Add(corner);
                                }
                            }
                        }
                        List<Triangle> triangles = triangulator.Triangulation(points, barriers);
                        if (showGrid) {
                            DrawTriangulationGrid(triangles, tabs, points); 
                        }
                        return triAStar.Process(triangles, points, map.Start, map.Final, barriers, room.BarriersNotDoor());
                    }

                    for (int i = 0; i < room.Transits.Count; i++) {
                        if (room.InRoom(map.Start)) {

                            //do prechodov pridam startovaci bod mapy
                            startIndex = TransitIndex(startTransit);
                            for (int j = 0; j < room.Transits.Count; j++) {
                                finalIndex = TransitIndex(room.Transits[j]);
                                if (TransitRoutesIndex(finalIndex, startIndex) == -1 && !TransitAloneInRoom(room.Transits[j], map) && !TransitAloneInRoom(room.Transits[i], map)) {

                                    //nacitam body pre triangulaciu
                                    List<MapPoint> points = new List<MapPoint>();
                                    foreach (MapPoint corner in room.Corners) {
                                        points.Add(corner);
                                    }

                                    //nacitam bariery a ich body
                                    List<ObjectOfBuilding> barriers = new List<ObjectOfBuilding>();
                                    foreach (ObjectOfBuilding barrier in room.Barriers) {
                                        ObjectOfBuilding newBarrier = new Map().EnlargeBarrier(barrier);
                                        barriers.Add(newBarrier);
                                        foreach (MapPoint corner in newBarrier.Corners) {
                                            points.Add(corner);
                                        }
                                    }

                                    //pridam startovaci bod
                                    points.Add(map.Start);

                                    //pridam cielovy bod
                                    int x2 = 0;
                                    int y2 = 0;
                                    foreach (MapPoint corner in room.Transits[j].Corners) {
                                        x2 += corner.X;
                                        y2 += corner.Y;
                                    }
                                    points.Add(new MapPoint(x2 / room.Transits[j].Corners.Count, y2 / room.Transits[j].Corners.Count, room.Transits[j].Corners[0].Z));

                                    //nacitam body prechodov ako bariery
                                    foreach (Transit transit in room.TransitsNotDoor()) {
                                        if (!transit.Equals(transits[startIndex]) && !transit.Equals(room.Transits[j])
                                            //&& !OverlappedTransit(transit, room.Transits[j])
                                            ) {
                                            ObjectOfBuilding newTransit = new Map().EnlargeBarrier(transit);
                                            barriers.Add(newTransit);
                                            foreach (MapPoint corner in newTransit.Corners) {
                                                points.Add(corner);
                                            }
                                        }
                                    }
                                    List<Triangle> triangles = triangulator.Triangulation(points, barriers);
                                    if (showGrid) {
                                        DrawTriangulationGrid(triangles, tabs, points);
                                    }
                                    transitRoutes.Add(new TransitRoute(TransitIndex(startTransit), TransitIndex(room.Transits[j]), triAStar.Process(triangles, points, map.Start, new MapPoint(x2 / room.Transits[j].Corners.Count, y2 / room.Transits[j].Corners.Count, room.Transits[j].Corners[0].Z), barriers, room.BarriersNotDoor())));
                                }
                            }
                        }

                        //do prechodov pridam cielovy bod mapy
                        if (room.InRoom(map.Final)) {
                            startIndex = TransitIndex(finalTransit);
                            for (int j = 0; j < room.Transits.Count; j++) {
                                finalIndex = TransitIndex(room.Transits[j]);
                                if (TransitRoutesIndex(finalIndex, startIndex) == -1 && !TransitAloneInRoom(room.Transits[j], map) && !TransitAloneInRoom(room.Transits[i], map)) {

                                    //nacitam body pre triangulaciu
                                    List<MapPoint> points = new List<MapPoint>();
                                    foreach (MapPoint corner in room.Corners) {
                                        points.Add(corner);
                                    }

                                    //nacitam bariery a ich body
                                    List<ObjectOfBuilding> barriers = new List<ObjectOfBuilding>();
                                    foreach (ObjectOfBuilding barrier in room.Barriers) {
                                        ObjectOfBuilding newBarrier = new Map().EnlargeBarrier(barrier);
                                        barriers.Add(newBarrier);
                                        foreach (MapPoint corner in newBarrier.Corners) {
                                            points.Add(corner);
                                        }
                                    }

                                    //pridam startovaci bod
                                    points.Add(map.Final);

                                    //pridam cielovy bod
                                    int x2 = 0;
                                    int y2 = 0;
                                    foreach (MapPoint corner in room.Transits[j].Corners) {
                                        x2 += corner.X;
                                        y2 += corner.Y;
                                    }
                                    points.Add(new MapPoint(x2 / room.Transits[j].Corners.Count, y2 / room.Transits[j].Corners.Count, room.Transits[j].Corners[0].Z));

                                    //nacitam body prechodov ako bariery
                                    foreach (Transit transit in room.TransitsNotDoor()) {
                                        if (!transit.Equals(transits[startIndex]) && !transit.Equals(room.Transits[j]) 
                                           // && !OverlappedTransit(transit, room.Transits[j])
                                            ) {
                                            ObjectOfBuilding newTransit = new Map().EnlargeBarrier(transit);
                                            barriers.Add(newTransit);
                                            foreach (MapPoint corner in newTransit.Corners) {
                                                points.Add(corner);
                                            }
                                        }
                                    }

                                    List<Triangle> triangles = triangulator.Triangulation(points, barriers);
                                    if (showGrid) {
                                        DrawTriangulationGrid(triangles, tabs, points);
                                    }
                                    transitRoutes.Add(new TransitRoute(TransitIndex(finalTransit), TransitIndex(room.Transits[j]), triAStar.Process(triangles, points, map.Final, new MapPoint(x2 / room.Transits[j].Corners.Count, y2 / room.Transits[j].Corners.Count, room.Transits[j].Corners[0].Z), barriers, room.BarriersNotDoor())));
                                }
                            }
                        }

                        int finalTransitIndex = TransitIndex(room.Transits[i]);
                        for (int j = 0; j < room.Transits.Count; j++) {
                            int startTransitIndex = TransitIndex(room.Transits[j]);
                            if (TransitRoutesIndex(finalTransitIndex, startTransitIndex) == -1 && finalTransitIndex != startTransitIndex && !TransitAloneInRoom(room.Transits[j], map) && !TransitAloneInRoom(room.Transits[i], map)) {

                                //nacitam body pre triangulaciu
                                List<MapPoint> points = new List<MapPoint>();
                                foreach (MapPoint corner in room.Corners) {
                                    points.Add(corner);
                                }

                                //nacitam bariery a ich body
                                List<ObjectOfBuilding> barriers = new List<ObjectOfBuilding>();
                                foreach (ObjectOfBuilding barrier in room.Barriers) {
                                    ObjectOfBuilding newBarrier = new Map().EnlargeBarrier(barrier);
                                    barriers.Add(newBarrier);
                                    foreach (MapPoint corner in newBarrier.Corners) {
                                        points.Add(corner);
                                    }
                                }

                                //pridam startovaci bod
                                int x1 = 0;
                                int y1 = 0;
                                foreach (MapPoint corner in room.Transits[i].Corners) {
                                    x1 += corner.X;
                                    y1 += corner.Y;
                                }
                                points.Add(new MapPoint(x1 / room.Transits[i].Corners.Count, y1 / room.Transits[i].Corners.Count, room.Transits[i].Corners[0].Z));

                                //pridam cielovy bod
                                int x2 = 0;
                                int y2 = 0;
                                foreach (MapPoint corner in room.Transits[j].Corners) {
                                    x2 += corner.X;
                                    y2 += corner.Y;
                                }
                                points.Add(new MapPoint(x2 / room.Transits[j].Corners.Count, y2 / room.Transits[j].Corners.Count, room.Transits[j].Corners[0].Z));

                                //nacitam body prechodov ako bariery
                                foreach (Transit transit in room.TransitsNotDoor()) {
                                    if (!transit.Equals(room.Transits[i]) && !transit.Equals(room.Transits[j]) 
                                       // && !OverlappedTransit(transit, room.Transits[j])
                                        ) {
                                        ObjectOfBuilding newTransit = new Map().EnlargeBarrier(transit);
                                        barriers.Add(newTransit);
                                        foreach (MapPoint corner in newTransit.Corners) {
                                            points.Add(corner);
                                        }
                                    }
                                }
                                List<Triangle> triangles = triangulator.Triangulation(points, barriers);
                                if (showGrid) {
                                    DrawTriangulationGrid(triangles, tabs, points);
                                }
                                transitRoutes.Add(new TransitRoute(TransitIndex(room.Transits[i]), TransitIndex(room.Transits[j]), triAStar.Process(triangles, points, new MapPoint(x1 / room.Transits[i].Corners.Count, y1 / room.Transits[i].Corners.Count, room.Transits[i].Corners[0].Z), new MapPoint(x2 / room.Transits[j].Corners.Count, y2 / room.Transits[j].Corners.Count, room.Transits[j].Corners[0].Z), barriers, room.BarriersNotDoor())));
                            }
                        }
                    }
                }
            }

            //pridame cesty medzi samotnymi prechodmi
            for (int i = 0; i < transits.Count; i++) {
                for (int j = 0; j < transits.Count; j++) {
                    if (transits[i].Id == transits[j].Id && TransitRoutesIndex(i, j) == -1 && i != j && !TransitAloneInRoom(transits[i], map) && !TransitAloneInRoom(transits[j], map)) {
                        transitRoutes.Add(new TransitRoute(i, j, transits[i].Delay));
                    }
                }
            }

            //vyhladame najkratsiu cestu napriec prechodmi
            List<int> close = new List<int>();
            List<int> open = new List<int>();
            List<int> comeFrom = new List<int>();
            open.Add(TransitIndex(startTransit));
            List<double> openG = new List<double>();
            List<double> openH = new List<double>();
            List<double> openF = new List<double>();
            for (int i = 0; i < this.transits.Count; i++) {
                openG.Add(0);
                openH.Add(Transit.Middle(this.transits[i]).Distance(map.Final));
                openF.Add(openH[i]);
                comeFrom.Add(0);
            }
            int actualTransit = TransitIndex(startTransit);
            while (open.Count != 0) {
                int lower = 0;
                for (int i = 0; i < open.Count; i++) {
                    if (openF[actualTransit] > openF[open[i]] && openF[open[lower]] > openF[open[i]]) {
                        lower = i;
                    }
                }
                actualTransit = TransitIndex(transits[open[lower]]);
                if (transits[open[lower]].Equals(finalTransit) || map.Final.Equals(Transit.Middle(transits[open[lower]]))) {
                    close.Add(open[lower]);
                    return Reconstruct(comeFrom, TransitIndex(finalTransit), TransitIndex(startTransit), transitRoutes);
                }
                foreach (TransitRoute transitRoute in transitRoutes) {
                    double actualLength = 0;
                    bool better = false;
                    if (transitRoute.From == open[lower]) {
                        if (InTransitsListIndex(close, transitRoute.To) == -1) {
                            actualLength = openG[TransitIndex(transits[transitRoute.From])] + transitRoute.Length;
                            if (InTransitsListIndex(open, transitRoute.To) == -1) {
                                open.Add(transitRoute.To);
                                better = true;
                            } else if (actualLength < openG[TransitIndex(transits[transitRoute.To])]) {
                                better = true;
                            } else {
                                better = false;
                            }
                            if (better) {
                                comeFrom[transitRoute.To] = open[lower];
                                openG[TransitIndex(transits[transitRoute.To])] = actualLength;
                                openF[TransitIndex(transits[transitRoute.To])] = Transit.Middle(transits[transitRoute.To]).Distance(map.Final);
                                openH[TransitIndex(transits[transitRoute.To])] = openG[TransitIndex(transits[transitRoute.To])] + openF[TransitIndex(transits[transitRoute.To])];
                            }
                        }
                    } else if (transitRoute.To == open[lower]) {
                        if (InTransitsListIndex(close, transitRoute.From) == -1) {
                            actualLength = openG[TransitIndex(transits[transitRoute.To])] + transitRoute.Length;
                            if (InTransitsListIndex(open, transitRoute.From) == -1) {
                                open.Add(transitRoute.From);
                                better = true;
                            } else if (actualLength < openG[TransitIndex(transits[transitRoute.From])]) {
                                better = true;
                            } else {
                                better = false;
                            }
                            if (better) {
                                comeFrom[transitRoute.From] = open[lower];
                                openG[TransitIndex(transits[transitRoute.From])] = actualLength;
                                openF[TransitIndex(transits[transitRoute.From])] = Transit.Middle(transits[transitRoute.From]).Distance(map.Final);
                                openH[TransitIndex(transits[transitRoute.From])] = openG[TransitIndex(transits[transitRoute.From])] + openF[TransitIndex(transits[transitRoute.From])];
                            }
                        }
                    }
                }
                close.Add(open[lower]);
                open.RemoveAt(lower);
            }
            List<MapPoint> trace = new List<MapPoint>();
            return trace;
        }

        /// <summary>
        /// vykresli mnozinu trojuholnikov
        /// </summary>
        /// <param name="triangles">mnozina trojuholnikov</param>
        /// <param name="tabs">mnozina tab okien</param>
        /// <param name="points">mnozina bodov na ktore sa odkazuju trojuholniky</param>
        private void DrawTriangulationGrid(List<Triangle> triangles, System.Windows.Controls.TabControl tabs, List<MapPoint> points) {
            tabs.Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate {
                System.Windows.Media.SolidColorBrush blueBrush = new System.Windows.Media.SolidColorBrush();
                blueBrush.Color = System.Windows.Media.Colors.CornflowerBlue;
                foreach (Triangle triangle in triangles) {
                    System.Windows.Shapes.Polygon poligon = new System.Windows.Shapes.Polygon();
                    System.Windows.Shapes.Line line1 = new System.Windows.Shapes.Line();
                    System.Windows.Shapes.Line line2 = new System.Windows.Shapes.Line();
                    System.Windows.Shapes.Line line3 = new System.Windows.Shapes.Line();
                    poligon.StrokeThickness = 1;
                    poligon.Stroke = blueBrush;
                    System.Windows.Media.PointCollection polygonPoints = new System.Windows.Media.PointCollection();
                    line1.X1 = line2.X1 = points[triangle.a].X;
                    line1.Y1 = line2.Y1 = points[triangle.a].Y;
                    line1.X2 = line3.X1 = points[triangle.b].X;
                    line1.Y2 = line3.Y1 = points[triangle.b].Y;
                    line3.X2 = line2.X2 = points[triangle.c].X;
                    line3.Y2 = line2.Y2 = points[triangle.c].Y;
                    line1.StrokeThickness = line2.StrokeThickness = line3.StrokeThickness = 1;
                    line1.Stroke = line2.Stroke = line3.Stroke = blueBrush;
                    poligon.Points = polygonPoints;
                    (((tabs as System.Windows.Controls.TabControl).Items[points[triangle.a].Z] as System.Windows.Controls.TabItem).Content as System.Windows.Controls.Canvas).Children.Add(line1);
                    (((tabs as System.Windows.Controls.TabControl).Items[points[triangle.a].Z] as System.Windows.Controls.TabItem).Content as System.Windows.Controls.Canvas).Children.Add(line2);
                    (((tabs as System.Windows.Controls.TabControl).Items[points[triangle.a].Z] as System.Windows.Controls.TabItem).Content as System.Windows.Controls.Canvas).Children.Add(line3);
                }
            });
        }

        /// <summary>
        /// zisti ci je prechod v miestnosti jedinym prechodom
        /// </summary>
        /// <param name="testTransit">testovany prechod</param>
        /// <param name="map">mapa v ktorej sa prechod nachadza</param>
        /// <returns>vracia true/false ci je rpechod sam v miestnosti alebo nie</returns>
        private bool TransitAloneInRoom(Transit testTransit, Map map) {
            foreach (Floor floor in map.Floors) {
                foreach (Room room in floor.Rooms) {
                    foreach (Transit transit in room.Transits) {
                        if (!transit.Equals(testTransit) && transit.Id == testTransit.Id) {
                            if (room.Transits.Count > 1 || room.InObject(map.Start) || room.InObject(map.Final)) {
                                return false;
                            } else {
                                return true;
                            }
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// zisti ci sa dva prechody neprekryvaju
        /// </summary>
        /// <param name="transit1">prvy prechod</param>
        /// <param name="transit2">druhy prechod</param>
        /// <returns>vracia true/false ci sa dva prechody prekryvaju</returns>
        private bool OverlappedTransit(Transit transit1, Transit transit2) {
            if (transit1.Type == transit2.Type) {
                foreach (MapPoint corner in transit2.Corners) {
                    bool isThere = false;
                    foreach (MapPoint corner2 in transit1.Corners) {
                        if (corner.Equals(corner2)) {
                            isThere = true;
                            break;
                        }
                    }
                    if (!isThere) {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// vytvori trasu
        /// </summary>
        /// <param name="comeFrom">mnozina bodov ktore urcuju odkial sme prisli</param>
        /// <param name="transitRoutes">mnozina prechodov</param>
        /// <param name="actual">index aktualneho bodu</param>
        /// <param name="start">index pociatocneho bodu</param>
        /// <returns>vracia trasu</returns>
        private List<MapPoint> Reconstruct(List<int> comeFrom, int actual, int start, List<TransitRoute> transitRoutes) {
            List<MapPoint> trace = new List<MapPoint>();
            List<int> transitTrace = new List<int>();
            while (actual != start) {
                transitTrace.Add(actual);
                actual = comeFrom[actual];
            }
            transitTrace.Add(start);
            for (int i = 0; i < transitTrace.Count - 1; i++) {
                foreach (TransitRoute route in transitRoutes) {
                    
                        if (route.To == transitTrace[i] && route.From == transitTrace[i + 1]) {
                            if (route.Trace != null) {
                                for (int j = 0; j < route.Trace.Count; j++) {
                                    trace.Add(route.Trace[j]);
                                }
                                break;
                            }
                        } else if (route.From == transitTrace[i] && route.To == transitTrace[i + 1]) {
                            if (route.Trace != null) {
                                for (int j = route.Trace.Count - 1; j >= 0; j--) {
                                    trace.Add(route.Trace[j]);
                                }
                                break;
                            } else { 
                            
                            }
                        }
                    
                }
            }
            return trace;
        }

        /// <summary>
        /// najde poziciu prechodu v poli prechodov 
        /// </summary>
        /// <returns>vracia index prechodu alebo -1 pokial sa v poli nenachadza</returns>
        private int InTransitsListIndex(List<int> list, int transit) {
            for (int i = 0; i < list.Count; i++) {
                if (list[i] == transit) {
                    return i;
                }
            }
            return -1;
        }
    }
}
