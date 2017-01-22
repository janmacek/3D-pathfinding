/************************************************************************************************************
 * Author: Jan Macek                                                                                        *
 * Project: DrawTypes.csproj                                                                                *
 * File: DrawMap.cs                                                                                         *
 *      - trieda obsahujuca metody pre vykreslenie trasy od startu do cielu mapy budovy                     *
 * Date: 29.4.2013                                                                                          *
 * Mail: xmacek18@fit.vutbr.cz                                                                              *
 *                                                                                                          *
 * Copyright © Jan Macek 2013                                                                               *
 * *********************************************************************************************************/
using MapObjectTypes;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PathfindingApp.DrawTypes {
    /// <summary>
    /// reprezentuje triedu ktora vykresli cestu
    /// </summary>
    public static class DrawTrace {
        /// <summary>
        /// vykresli trasu v budove
        /// </summary>
        /// <param name="trace">mnozina bodov reprezentujuca trasu</param>
        /// <param name="tabs">tabControl do ktoreho sa vykresluje trasa</param>
        public static bool Paint(List<MapPoint> trace, TabControl tabs) {
            if (trace != null && trace.Count != 0) {
                SolidColorBrush redBrush = new SolidColorBrush();
                redBrush.Color = Colors.Red;
                for (int i = 0; i < trace.Count - 1; i++) {
                    Line line1 = new Line();
                    line1.X1 = trace[i].X;
                    line1.Y1 = trace[i].Y;
                    line1.X2 = trace[i + 1].X;
                    line1.Y2 = trace[i + 1].Y;
                    line1.Stroke = redBrush;
                    line1.StrokeThickness = 2;
                    DoubleCollection dashes = new DoubleCollection();
                    dashes.Add(2);
                    dashes.Add(1);
                    line1.StrokeDashArray = dashes;
                    if (trace[i].Z != trace[i + 1].Z) {
                        (((tabs as TabControl).Items[trace[i + 1].Z] as TabItem).Content as Canvas).Children.Add(line1);
                    } else {
                        (((tabs as TabControl).Items[trace[i].Z] as TabItem).Content as Canvas).Children.Add(line1);
                    }
                }
                return true;
            } else {
                return false;
            }
        }
    }
}
