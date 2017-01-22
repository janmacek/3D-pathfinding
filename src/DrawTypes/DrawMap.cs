/************************************************************************************************************
 * Author: Jan Macek                                                                                        *
 * Project: DrawTypes.csproj                                                                                *
 * File: DrawMap.cs                                                                                         *
 *      - trieda obsahujuca metody pre vykreslenie mapy budovy                                              *
 * Date: 29.4.2013                                                                                          *
 * Mail: xmacek18@fit.vutbr.cz                                                                              *
 *                                                                                                          *
 * Copyright © Jan Macek 2013                                                                               *
 * *********************************************************************************************************/
using MapObjectTypes;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PathfindingApp.DrawTypes {
    /// <summary>
    /// metody pre vykreslenie mapy budovy
    /// </summary>
    public static class DrawMap {
        /// <summary>
        /// vykresli mapu bodovy
        /// </summary>
        /// <param name="map">mapa budovy</param>
        /// <param name="tabs">spravca tab okien kde sa budu vykreslovat poschodia bodovy</param>
        public static void Paint(Map map, System.Windows.Controls.TabControl tabs) {
            tabs.Items.Clear();
            List<string> drawDoor = new List<string>();
            foreach (Floor floor in map.Floors) {
                TabItem tab = new TabItem();
                tab.Header = string.Concat(floor.FloorNumber, ". floor");
                Canvas canvas = new Canvas();
                //vykreslovanie vsetkych miestnosti na poschodi
                foreach (Room room in floor.Rooms) {
                    SolidColorBrush whiteBrush = new SolidColorBrush();
                    whiteBrush.Color = Colors.White;
                    SolidColorBrush blackBrush = new SolidColorBrush();
                    blackBrush.Color = Colors.Black;
                    SolidColorBrush stairsBrush = new SolidColorBrush();
                    stairsBrush.Color = Colors.Coral;

                    //vykreslenie miestnosti
                    Polygon newPolygon = new Polygon();
                    newPolygon.Stroke = blackBrush;
                    newPolygon.Fill = whiteBrush;
                    newPolygon.StrokeThickness = 1;
                    PointCollection polygonPoints = new PointCollection();
                    foreach (MapPoint corner in room.Corners) {
                        System.Windows.Point Point = new System.Windows.Point(corner.X, corner.Y);
                        polygonPoints.Add(Point);
                    }
                    newPolygon.Points = polygonPoints;
                    canvas.Children.Add(newPolygon);
                }
                //prejdeme vsetky miestnosti a vykreslime prechody a prekazky
                foreach (Room room in floor.Rooms) {
                    SolidColorBrush whiteBrush = new SolidColorBrush();
                    whiteBrush.Color = Colors.White;
                    SolidColorBrush blackBrush = new SolidColorBrush();
                    blackBrush.Color = Colors.Black;
                    SolidColorBrush doorsBrush = new SolidColorBrush();
                    doorsBrush.Color = Colors.OrangeRed;
                    Brush barrierBrush = CreateGridBrush(new Rect(0, 0, 500, 500), new Size(10, 10), Transit.transitType.nonSet);

                    //vykreslenie vsetkych prekazok v miestnosti
                    foreach (ObjectOfBuilding barrier in room.Barriers) {
                        Polygon transitPolygon = new Polygon();
                        transitPolygon.Stroke = blackBrush;
                        transitPolygon.Fill = barrierBrush;
                        transitPolygon.StrokeThickness = 1;
                        PointCollection transitPoints = new PointCollection();
                        foreach (MapPoint corner in barrier.Corners) {
                            System.Windows.Point Point = new System.Windows.Point(corner.X, corner.Y);
                            transitPoints.Add(Point);
                        }
                        transitPolygon.Points = transitPoints;
                        canvas.Children.Add(transitPolygon);
                    }

                    //vykreslenie vsetkych prechodov v miestnosti
                    foreach (Transit transit in room.Transits) {
                        //ak sa jedna o dvere
                        if (transit.Type == Transit.transitType.door) {
                            bool notIn = true;
                            foreach (string doorId in drawDoor) {
                                if (doorId.Equals(transit.Id)) {
                                    notIn = false;
                                }
                            }
                            if (notIn) {
                                drawDoor.Add(transit.Id);
                                Image myImage = new Image();
                                if (transit.Corners[0].X == transit.Corners[1].X) {
                                    myImage.Width = ((transit.Corners[1].Y - transit.Corners[0].Y) > 0) ? (transit.Corners[1].Y - transit.Corners[0].Y) : -1 * (transit.Corners[1].Y - transit.Corners[0].Y);
                                    myImage.Height = 20;
                                    if (DoorOrientation(room, transit) == false) {
                                        Canvas.SetTop(myImage, transit.Corners[0].Y);
                                        Canvas.SetLeft(myImage, transit.Corners[0].X - myImage.Width + 2);
                                        BitmapImage myImageSource = new BitmapImage();
                                        myImageSource.BeginInit();
                                        myImageSource.UriSource = new Uri(@"/DrawTypes;component/Resources/door-left.png", UriKind.Relative);
                                        myImageSource.EndInit();
                                        myImage.Source = myImageSource;
                                    } else {
                                        Canvas.SetTop(myImage, transit.Corners[0].Y);
                                        Canvas.SetLeft(myImage, transit.Corners[0].X - 2);
                                        BitmapImage myImageSource = new BitmapImage();
                                        myImageSource.BeginInit();
                                        myImageSource.UriSource = new Uri(@"/DrawTypes;component/Resources/door-right.png", UriKind.Relative);
                                        myImageSource.EndInit();
                                        myImage.Source = myImageSource;
                                    }

                                } else if (transit.Corners[0].Y == transit.Corners[1].Y) {
                                    myImage.Height = ((transit.Corners[1].X - transit.Corners[0].X) > 0) ? (transit.Corners[1].X - transit.Corners[0].X) : -1 * (transit.Corners[1].X - transit.Corners[0].X);
                                    myImage.Width = 20;
                                    if (DoorOrientation(room, transit) == false) {
                                        Canvas.SetTop(myImage, transit.Corners[0].Y - myImage.Height + 2);
                                        Canvas.SetLeft(myImage, transit.Corners[0].X);
                                        BitmapImage myImageSource = new BitmapImage();
                                        myImageSource.BeginInit();
                                        myImageSource.UriSource = new Uri(@"/DrawTypes;component/Resources/door-top.png", UriKind.Relative);
                                        myImageSource.EndInit();
                                        myImage.Source = myImageSource;
                                    } else {
                                        Canvas.SetTop(myImage, transit.Corners[0].Y - 2);
                                        Canvas.SetLeft(myImage, transit.Corners[0].X);
                                        BitmapImage myImageSource = new BitmapImage();
                                        myImageSource.BeginInit();
                                        myImageSource.UriSource = new Uri(@"/DrawTypes;component/Resources/door-bottom.png", UriKind.Relative);
                                        myImageSource.EndInit();
                                        myImage.Source = myImageSource;
                                    }
                                }
                                canvas.Children.Add(myImage);
                            }
                            Line transitLined = new Line();
                            transitLined.Stroke = doorsBrush;
                            transitLined.StrokeThickness = 1;
                            transitLined.Stroke = whiteBrush;
                            transitLined.X1 = transit.Corners[0].X;
                            transitLined.X2 = transit.Corners[1].X;
                            transitLined.Y1 = transit.Corners[0].Y;
                            transitLined.Y2 = transit.Corners[1].Y;
                            canvas.Children.Add(transitLined);

                            //ak sa jedna o schody
                        } else if (transit.Type == Transit.transitType.stairs) {
                            Polygon transitPolygon = new Polygon();
                            SolidColorBrush stairsBorderBrush = new SolidColorBrush();
                            stairsBorderBrush.Color = Colors.DimGray;
                            transitPolygon.Stroke = stairsBorderBrush;
                            int maxY = transit.Corners[1].Y - transit.Corners[0].Y;
                            int maxX = transit.Corners[2].X - transit.Corners[1].X;
                            transitPolygon.Fill = CreateGridBrush(new Rect(0, 0, maxX, maxY), new Size(5, 5), Transit.transitType.stairs); ;
                            transitPolygon.StrokeThickness = 1;
                            PointCollection transitPoints = new PointCollection();
                            foreach (MapPoint corner in transit.Corners) {
                                System.Windows.Point Point = new System.Windows.Point(corner.X, corner.Y);
                                transitPoints.Add(Point);
                            }
                            transitPolygon.Points = transitPoints;
                            canvas.Children.Add(transitPolygon);
                            //ak sa jedna o vytah
                        } else if (transit.Type == Transit.transitType.elevator) {
                            Polygon transitPolygon = new Polygon();
                            SolidColorBrush eleBorderBrush = new SolidColorBrush();
                            eleBorderBrush.Color = Colors.DimGray;
                            transitPolygon.Stroke = eleBorderBrush;
                            int maxY = transit.Corners[1].Y - transit.Corners[0].Y;
                            int maxX = transit.Corners[2].X - transit.Corners[1].X;
                            transitPolygon.Fill = CreateGridBrush(new Rect(0, 0, maxX, maxY), new Size(maxX, maxY), Transit.transitType.elevator); ;
                            transitPolygon.StrokeThickness = 1;
                            PointCollection transitPoints = new PointCollection();
                            foreach (MapPoint corner in transit.Corners) {
                                System.Windows.Point Point = new System.Windows.Point(corner.X, corner.Y);
                                transitPoints.Add(Point);
                            }
                            transitPolygon.Points = transitPoints;
                            canvas.Children.Add(transitPolygon);
                        }
                    }
                }
                //vykresli startovaci bod
                if (map.Start != null && map.Start.Z == floor.FloorNumber) {
                    Line crossLine1 = new Line();
                    crossLine1.X1 = map.Start.X - 4;
                    crossLine1.X2 = map.Start.X + 4;
                    crossLine1.Y1 = map.Start.Y - 4;
                    crossLine1.Y2 = map.Start.Y + 4;
                    crossLine1.Stroke = new SolidColorBrush(Colors.Green);
                    crossLine1.StrokeThickness = 2;
                    canvas.Children.Add(crossLine1);
                    Line crossLine2 = new Line();
                    crossLine2.X1 = map.Start.X + 4;
                    crossLine2.X2 = map.Start.X - 4;
                    crossLine2.Y1 = map.Start.Y - 4;
                    crossLine2.Y2 = map.Start.Y + 4;
                    crossLine2.Stroke = new SolidColorBrush(Colors.Green);
                    crossLine2.StrokeThickness = 2;
                    canvas.Children.Add(crossLine2);
                }
                //vykresli cielovy bod
                if (map.Final != null && map.Final.Z == floor.FloorNumber) {
                    Line crossLine3 = new Line();
                    crossLine3.X1 = map.Final.X - 4;
                    crossLine3.X2 = map.Final.X + 4;
                    crossLine3.Y1 = map.Final.Y - 4;
                    crossLine3.Y2 = map.Final.Y + 4;
                    crossLine3.Stroke = new SolidColorBrush(Colors.Blue);
                    crossLine3.StrokeThickness = 2;
                    canvas.Children.Add(crossLine3);
                    Line crossLine4 = new Line();
                    crossLine4.X1 = map.Final.X + 4;
                    crossLine4.X2 = map.Final.X - 4;
                    crossLine4.Y1 = map.Final.Y - 4;
                    crossLine4.Y2 = map.Final.Y + 4;
                    crossLine4.Stroke = new SolidColorBrush(Colors.Blue);
                    crossLine4.StrokeThickness = 2;
                    canvas.Children.Add(crossLine4);
                }
                tab.Content = canvas;
                tabs.Items.Add(tab);
            }
            tabs.SelectedIndex = map.Final.Z;
        }

        /// <summary>
        /// zisti orientaciu dveri vzhladom k miestnosti v ktorej sa nachadzaju
        /// </summary>
        /// <param name="room">miestnost v ktorej sa dvere nachadzaju</param>
        /// <param name="transit">prechod reprezentujuci dvere</param>
        /// <returns>vracia true pokial su otocene do miestnosti / false pokial z miestnosti</returns>
        private static bool DoorOrientation(Room room, Transit transit) {
            int min = int.MaxValue;
            int max = int.MinValue;
            if (transit.Corners[0].X == transit.Corners[1].X) {
                foreach (MapPoint corner in room.Corners) {
                    if (corner.X > max) {
                        max = corner.X;
                    } else {
                        min = corner.X;
                    }
                }
                if (transit.Corners[0].X >= max && transit.Corners[0].X >= min) {
                    return false;
                } else {
                    return true;
                }
            } else {
                foreach (MapPoint corner in room.Corners) {
                    if (corner.Y > max) {
                        max = corner.Y;
                    } else {
                        min = corner.Y;
                    }
                }
                if (transit.Corners[0].Y >= max && transit.Corners[0].Y >= min) {
                    return false;
                } else {
                    return true;
                }
            }
        }

        /// <summary>
        /// generuje vzor pozadia objektov budovy
        /// </summary>
        /// <param name="bounds">obdlznik reprezentujuci prechod zacinajuci v bode [0,0]</param>
        /// <param name="tileSize">velkost detailu vzoru</param>
        /// <param name="type">typ prechodu</param>
        /// <returns>vracia vzor prechodu</returns>
        private static Brush CreateGridBrush(Rect bounds, Size tileSize, MapObjectTypes.Transit.transitType type) {
            var gridColor = Brushes.Black;
            var gridThickness = 1.0;
            var tileRect = new Rect(tileSize);
            DrawingBrush gridTile;
            //ak sa jedna o schody
            if (type == Transit.transitType.stairs) {
                GeometryCollection geomCol;
                //zistime ci su schody vodorovne alebo zvisle
                if (bounds.Height < bounds.Width) {
                    geomCol = new GeometryCollection {
                                new LineGeometry(tileRect.TopLeft, tileRect.BottomLeft),
                                new LineGeometry(tileRect.TopRight, tileRect.BottomRight)
                            };
                } else {
                    geomCol = new GeometryCollection {
                                new LineGeometry(tileRect.TopLeft, tileRect.TopRight),
                                new LineGeometry(tileRect.BottomLeft, tileRect.BottomRight)
                            };
                }
                gridTile = new DrawingBrush {
                    Stretch = Stretch.None,
                    TileMode = TileMode.Tile,
                    Viewport = tileRect,
                    ViewportUnits = BrushMappingMode.Absolute,
                    Drawing = new GeometryDrawing {
                        Pen = new Pen(gridColor, gridThickness),
                        Geometry = new GeometryGroup {
                            Children = geomCol
                        }
                    }
                };
                //ak sa jedna o prekazku
            } else {
                gridTile = new DrawingBrush {
                    Stretch = Stretch.None,
                    TileMode = TileMode.Tile,
                    Viewport = tileRect,
                    ViewportUnits = BrushMappingMode.Absolute,
                    Drawing = new GeometryDrawing {
                        Pen = new Pen(gridColor, gridThickness),
                        Geometry = new GeometryGroup {
                            Children = new GeometryCollection {
                                new LineGeometry(tileRect.TopLeft, tileRect.BottomRight),
                                new LineGeometry(tileRect.BottomLeft, tileRect.TopRight)
                            }
                        }
                    }
                };
            }
            var offsetGrid = new DrawingBrush {
                Stretch = Stretch.None,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
                Transform = new TranslateTransform(bounds.Left, bounds.Top),
                Drawing = new GeometryDrawing {
                    Geometry = new RectangleGeometry(new Rect(bounds.Size)),
                    Brush = gridTile
                }
            };
            return offsetGrid;
        }
    }
}

