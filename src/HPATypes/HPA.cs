/************************************************************************************************************
 * Author: Jan Macek                                                                                        *
 * Project: HPATypes.csproj                                                                                 *
 * File: HPA.cs                                                                                             *
 *      - trieda ktora vyhlada trasu medzi startovacim a cielovym bodom pomocou algoritmu HPA*              *
 * Date: 29.4.2013                                                                                          *
 * Mail: xmacek18@fit.vutbr.cz                                                                              *
 *                                                                                                          *
 * Copyright © Jan Macek 2013                                                                               *
 * *********************************************************************************************************/
using PathfindingApp.HPATypes;
using PathfindingApp.AStarTypes;
using MapObjectTypes;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Shapes;


namespace HPATypes {
    /// <summary>
    /// vyhlada trasu medzi startovacim a cielovym bodom
    /// </summary>
    public class HPA {
        
        /// <summary>
        /// mnozina prechodov v budove
        /// </summary>
        List<Transit> transits = new List<Transit>();

        /// <summary>
        /// generator hierarchickej reprezentacie priestoru
        /// </summary>
        GridGenerator gridGen = new GridGenerator();

        /// <summary>
        /// cesty medzi prechodmi budovy
        /// </summary>
        private List<TransitRoute> transitRoutes = new List<TransitRoute>();

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
                        if (route.Trace != null && route.Trace.Count > 0) {
                            for (int j = 0; j < route.Trace.Count; j++) {
                                trace.Add(route.Trace[j]);
                            }
                            break;
                        } else {
                            trace.Add(Transit.Middle(transits[route.To]));
                            trace.Add(Transit.Middle(transits[route.From]));
                        }
                    } else if (route.From == transitTrace[i] && route.To == transitTrace[i + 1]) {
                        if (route.Trace != null && route.Trace.Count > 0) {
                            for (int j = route.Trace.Count - 1; j >= 0; j--) {
                                trace.Add(route.Trace[j]);
                            }
                            break;
                        } else {
                            trace.Add(Transit.Middle(transits[route.From]));
                            trace.Add(Transit.Middle(transits[route.To]));
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

        /// <summary>
        /// zvacsi barieru pre potreby najdenia trasy
        /// </summary>
        /// <param name="barrier">bariera</param>
        /// <returns>vracia zvacsenu barieru</returns>
        public ObjectOfBuilding EnlargeBarrier(ObjectOfBuilding barrier) {
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
            enlargeBarrier.Corners.Add(new MapPoint(minX - 1, minY - 1, barrier.Corners[0].Z));
            enlargeBarrier.Corners.Add(new MapPoint(minX - 1, maxY + 1, barrier.Corners[0].Z));
            enlargeBarrier.Corners.Add(new MapPoint(maxX + 1, maxY + 1, barrier.Corners[0].Z));
            enlargeBarrier.Corners.Add(new MapPoint(maxX + 1, minY - 1, barrier.Corners[0].Z));
            return enlargeBarrier;
        }

        /// <summary>
        /// zvacsi bariery pre potreby najdenia trasy
        /// </summary>
        /// <param name="barrier">mnozina barier</param>
        /// <returns>vracia zvacsenu mnozinu barier</returns>
        public List<ObjectOfBuilding> EnlargeBarriers(List<ObjectOfBuilding> barriers) {
            List<ObjectOfBuilding> enlargeBarriers = new List<ObjectOfBuilding>();
            foreach (ObjectOfBuilding barrier in barriers) {
                enlargeBarriers.Add(EnlargeBarrier(barrier));
            }
            return enlargeBarriers;
        }

        /// <summary>
        /// najde stred prechodu
        /// </summary>
        /// <returns>vracia bod ktory sa nachadza v relativnom strede prechodu</returns>
        private MapPoint TransitCenter(Transit transit) {
            int y = 0;
            int x = 0;
            foreach (MapPoint corner in transit.Corners) {
                x += corner.X;
                y += corner.Y;
            }
            x /= transit.Corners.Count;
            y /= transit.Corners.Count;
            return new MapPoint(x, y, transit.Corners[0].Z);
        }

        /// <summary>
        /// vyhlada trasu medzi startovacim a cielovym bodom
        /// </summary>
        /// <param name="map">mapa priestoru v ktorom vyhladavame</param>
        /// <param name="tabs">tabControl do ktoreho zakreslujeme grid</param>
        /// <param name="showGrid">zobrazenie grid-u</param>
        /// <returns>vracia mnozinu bodov vyhladanej trasy</returns>
        public List<MapPoint> Process(Map map, System.Windows.Controls.TabControl tabs, bool showGrid, int HPAGridSize) {
            List<MapPoint> trace = new List<MapPoint>();
            transits = map.GetTransits();

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
                    bool isPainted = false;
                    //ak je start a ciel v jednej miestnosti
                    if (room.InRoom(map.Final) && room.InRoom(map.Start)) {
                        startIndex = TransitIndex(startTransit);
                        finalIndex = TransitIndex(finalTransit);

                        //nacitam bariery a ich body
                        List<ObjectOfBuilding> barriers = new List<ObjectOfBuilding>();
                        foreach (ObjectOfBuilding barrier in room.Barriers) {
                            barriers.Add(barrier);
                        }
                        //nacitam body prechodov ako bariery
                        foreach (Transit transit in room.Transits) {
                            if (!transit.Equals(transits[startIndex]) && !transit.Equals(finalIndex)) {
                                barriers.Add(transit);
                            }
                        }

                        List<HPAEdge> grid = gridGen.Process(room, map.Start, startTransit, map.Final, finalTransit, HPAGridSize);

                        if (showGrid && !isPainted) {
                            DrawHPAGrid(grid, tabs);
                            isPainted = true;
                        }
                        trace = OnLineSearch.Process(grid, map.Start, map.Final, EnlargeBarriers(room.GetBarriers()));
                        return trace;
                    }

                    for (int i = 0; i < room.Transits.Count; i++) {
                        //ak je v miestnosti startovaci bod
                        if (room.InRoom(map.Start)) {
                            //do prechodov pridam startovaci bod mapy
                            startIndex = TransitIndex(startTransit);
                            for (int j = 0; j < room.Transits.Count; j++) {
                                finalIndex = TransitIndex(room.Transits[j]);
                                if (TransitRoutesIndex(finalIndex, startIndex) == -1) {

                                    List<HPAEdge> grid = gridGen.Process(room, Transit.Middle(startTransit), startTransit, Transit.Middle(room.Transits[j]), room.Transits[j], HPAGridSize);
                                    if (showGrid && !isPainted) {
                                        DrawHPAGrid(grid, tabs);
                                        isPainted = true;
                                    }
                                    List<ObjectOfBuilding> barriers = room.BarriersNotDoor();
                                    barriers = EnlargeBarriers(barriers);
                                    transitRoutes.Add(new TransitRoute(TransitIndex(startTransit), TransitIndex(room.Transits[j]), OnLineSearch.Process(grid, map.Start, TransitCenter(room.Transits[j]), barriers)));
                                }
                            }
                        }

                        //ak je v miestnosti cielovy bod
                        if (room.InRoom(map.Final)) {
                            startIndex = TransitIndex(finalTransit);
                            for (int j = 0; j < room.Transits.Count; j++) {
                                finalIndex = TransitIndex(room.Transits[j]);
                                if (TransitRoutesIndex(finalIndex, startIndex) == -1) {

                                    //nacitam bariery a ich body
                                    List<ObjectOfBuilding> barriers = new List<ObjectOfBuilding>();
                                    foreach (ObjectOfBuilding barrier in room.Barriers) {
                                        barriers.Add(barrier);
                                    }

                                    //nacitam body prechodov ako bariery
                                    foreach (Transit transit in room.Transits) {
                                        if (!transit.Equals(transits[startIndex]) && !transit.Equals(room.Transits[j])) {
                                            barriers.Add(transit);
                                        }
                                    }

                                    List<HPAEdge> grid = gridGen.Process(room, map.Final, finalTransit, Transit.Middle(room.Transits[j]), room.Transits[j], HPAGridSize);
                                    if (showGrid && !isPainted) {
                                        DrawHPAGrid(grid, tabs);
                                        isPainted = true;
                                    }
                                    barriers = room.GetBarriers();
                                    barriers = EnlargeBarriers(barriers);
                                    List<MapPoint> actualTrace = OnLineSearch.Process(grid, map.Final, TransitCenter(room.Transits[j]), barriers);
                                    transitRoutes.Add(new TransitRoute(TransitIndex(finalTransit), TransitIndex(room.Transits[j]), actualTrace));
                                }
                            }
                        }

                        int finalTransitIndex = TransitIndex(room.Transits[i]);
                        for (int j = 0; j < room.Transits.Count; j++) {
                            int startTransitIndex = TransitIndex(room.Transits[j]);
                            if (TransitRoutesIndex(finalTransitIndex, startTransitIndex) == -1 && finalTransitIndex != startTransitIndex && !TransitAloneInRoom(room.Transits[j], map) && !TransitAloneInRoom(room.Transits[i], map)) {

                                //nacitam bariery a ich body
                                List<ObjectOfBuilding> barriers = new List<ObjectOfBuilding>();
                                foreach (ObjectOfBuilding barrier in room.Barriers) {
                                    barriers.Add(barrier);
                                }

                                //nacitam body prechodov ako bariery
                                foreach (Transit transit in room.Transits) {
                                    if (!transit.Equals(room.Transits[i]) && !transit.Equals(room.Transits[j]) && !OverlappedTransit(transit, room.Transits[j])) {
                                        barriers.Add(transit);
                                    }
                                }

                                List<HPAEdge> grid = gridGen.Process(room, Transit.Middle(room.Transits[i]), room.Transits[i], Transit.Middle(room.Transits[j]), room.Transits[j], HPAGridSize);
                                if (showGrid && !isPainted) {
                                    DrawHPAGrid(grid, tabs);
                                    isPainted = true;
                                }
                                barriers = room.GetBarriers();
                                barriers = EnlargeBarriers(barriers);
                                List<MapPoint> actualTrace = OnLineSearch.Process(grid, TransitCenter(room.Transits[i]), TransitCenter(room.Transits[j]), barriers);
                                transitRoutes.Add(new TransitRoute(TransitIndex(room.Transits[i]), TransitIndex(room.Transits[j]), actualTrace));

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
                openH.Add(TransitCenter(this.transits[i]).Distance(map.Final));
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
                if (transits[open[lower]].Equals(finalTransit) || map.Final.Equals(TransitCenter(transits[open[lower]]))) {
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
                                openF[TransitIndex(transits[transitRoute.To])] = TransitCenter(transits[transitRoute.To]).Distance(map.Final);
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
                                openF[TransitIndex(transits[transitRoute.From])] = TransitCenter(transits[transitRoute.From]).Distance(map.Final);
                                openH[TransitIndex(transits[transitRoute.From])] = openG[TransitIndex(transits[transitRoute.From])] + openF[TransitIndex(transits[transitRoute.From])];
                            }
                        }
                    }
                }
                close.Add(open[lower]);
                open.RemoveAt(lower);
            }
            return trace;
        }

        private void DrawHPAGrid(List<HPAEdge> grid, System.Windows.Controls.TabControl tabs) {
            tabs.Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate {
                SolidColorBrush blueBrush = new SolidColorBrush();
                blueBrush.Color = Colors.CornflowerBlue;
                for (int k = 0; k < grid.Count; k++) {
                    if (grid[k].Trace != null && grid[k].Trace.Count > 2) {
                        for (int m = 0; m < grid[k].Trace.Count - 1; m++) {
                            Line line1 = new Line();
                            line1.X1 = grid[k].Trace[m].X;
                            line1.Y1 = grid[k].Trace[m].Y;
                            line1.X2 = grid[k].Trace[m + 1].X;
                            line1.Y2 = grid[k].Trace[m + 1].Y;
                            line1.Stroke = blueBrush;
                            line1.StrokeThickness = 1;
                            (((tabs as System.Windows.Controls.TabControl).Items[grid[k].Trace[m].Z] as System.Windows.Controls.TabItem).Content as System.Windows.Controls.Canvas).Children.Add(line1);
                        }
                    } else {
                        Line line1 = new Line();
                        line1.X1 = grid[k].A.X;
                        line1.Y1 = grid[k].A.Y;
                        line1.X2 = grid[k].B.X;
                        line1.Y2 = grid[k].B.Y;
                        line1.Stroke = blueBrush;
                        line1.StrokeThickness = 1;
                        (((tabs as System.Windows.Controls.TabControl).Items[grid[k].A.Z] as System.Windows.Controls.TabItem).Content as System.Windows.Controls.Canvas).Children.Add(line1);
                    }
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
        private bool OverlappedTransit(ObjectOfBuilding transit1, ObjectOfBuilding transit2) {
            if (transit1.Corners.Count == transit2.Corners.Count) {
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
    }
}
