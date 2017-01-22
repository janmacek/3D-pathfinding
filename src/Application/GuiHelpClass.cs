/************************************************************************************************************
 * Author: Jan Macek                                                                                        *
 * Project: BakalarskaPraca.csproj                                                                          *
 * File: GuiHelpClass.cs                                                                                    *
 *      - obsahuje pomocne funkcie na pracu s GUI prostredim                                                *
 * Date: 29.4.2013                                                                                          *
 * Mail: xmacek18@fit.vutbr.cz                                                                              *
 *                                                                                                          *
 * Copyright © Jan Macek 2013                                                                               *
 * *********************************************************************************************************/
using PathfindingApp.DrawTypes;
using MapObjectTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Common;

namespace PathfindingApp {
    /// <summary>
    /// trieda ktora obsahuje pomocne metody
    /// </summary>
    public static class GuiHelpClass {

        public enum ActiveAlgorithm { hpa, astar, funnel};

        /// <summary>
        /// zobrazi tlacidla menu, ktore pracuju s nacitanou mapou
        /// </summary>
        public static void AllowMenuMapButtons(Menu topMenu, ActiveAlgorithm activeAlgorithm) {
            foreach (MenuItem menuItem in topMenu.Items) {
                if (menuItem.GetType() == typeof(MenuItem)) {
                    foreach (var item in menuItem.Items) {
                        if (item != null && item.GetType() == typeof(MenuItem)) {
                            (item as MenuItem).IsEnabled = true;
                        }
                    }
                } 
            }
            if (activeAlgorithm != ActiveAlgorithm.hpa) {
                (topMenu.FindName("btnMenuCustomGrid") as MenuItem).IsEnabled = false;
            }
        }

        /// <summary>
        /// skryje tlacidla menu, ktore pracuju s nacitanou mapou
        /// </summary>
        public static void HideMenuMapButtons(Menu topMenu) {
            foreach (MenuItem menuItem in topMenu.Items) {
                if (menuItem.GetType() == typeof(MenuItem)) {
                    foreach (var item in menuItem.Items) {
                        if (item.GetType() == typeof(MenuItem)) {
                            (item as MenuItem).IsEnabled = false;
                        }
                    }
                }
            }
            
        }

        /// <summary>
        /// nacita mapu z XML suboru
        /// </summary>
        /// <param name="fileName">nazov suboru</param>
        /// <returns>vracia -1 = nekorektny subor / -2 = neexistujuci subor</returns>
        public static Map LoadMap(string fileName, bool isLoadedMap, Map map, MainWindow mainWindow, TabControl tabs, Menu topMenu, WrapPanel WrpBtnXmlFiles, ActiveAlgorithm activeAlgorithm) {
            if (File.Exists(fileName)) {
                if (isLoadedMap) {
                    if (System.Windows.MessageBox.Show("Do you want to rewrite current buildings plan ?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                        string loadResult = map.LoadFromXML(fileName);
                        if (loadResult.Equals("Loaded")) {
                            WrpBtnXmlFiles.Visibility = Visibility.Hidden;
                            //nastavime sirku a vysku okna
                            mainWindow.MinHeight = map.GetLimitCoord(true, false) + 140;
                            mainWindow.Height = mainWindow.MinHeight;
                            mainWindow.MinWidth = map.GetLimitCoord(true, true)+50;
                            mainWindow.Width = mainWindow.MinWidth;
                            AllowMenuMapButtons(topMenu, activeAlgorithm);
                            mainWindow.btnMenuStop.IsEnabled = false;
                            //vykreslime novu mapu
                            DrawMap.Paint(map, tabs);
                            mainWindow.Title = string.Concat("3D Pathfinding - ", fileName.ToString().Split('\\')[fileName.ToString().Split('\\').Length - 1]);
                            RecentFileList recentFileList = new RecentFileList();
                            recentFileList.RemoveFile(System.IO.Path.GetFullPath(fileName));
                            recentFileList.InsertFile(System.IO.Path.GetFullPath(fileName));
                            mainWindow.lblStatusBox.Visibility = Visibility.Visible;
                        } else {
                            MessageBox.Show(string.Concat("Unfortunately, file \"", fileName.Replace(".3Dpathfinding.xml", ""), ".3Dpathfinding.xml\" ", " is not valid ! There is error: ", loadResult), "Load file error!");
                        }
                    }
                } else {
                    string loadResult = map.LoadFromXML(fileName);
                    if (loadResult.Equals("Loaded")) {
                        //nastavime sirku a vysku okna
                        WrpBtnXmlFiles.Visibility = Visibility.Hidden;
                        mainWindow.MinHeight = map.GetLimitCoord(true, false) + 140;
                        mainWindow.Height = mainWindow.MinHeight;
                        mainWindow.MinWidth = map.GetLimitCoord(true, true)+50;
                        mainWindow.Width = mainWindow.MinWidth;
                        AllowMenuMapButtons(topMenu, activeAlgorithm);
                        mainWindow.btnMenuStop.IsEnabled = false;
                        mainWindow.Title = string.Concat("3D Pathfinding - ", fileName.ToString().Split('\\')[fileName.ToString().Split('\\').Length - 1]);
                        //vykreslime novu mapu
                        DrawMap.Paint(map, tabs);
                        RecentFileList recentFileList = new RecentFileList();
                        recentFileList.RemoveFile(System.IO.Path.GetFullPath(fileName));
                        recentFileList.InsertFile(System.IO.Path.GetFullPath(fileName));
                        mainWindow.lblStatusBox.Visibility = Visibility.Visible;
                    } else {
                        MessageBox.Show(string.Concat("Unfortunately, file \"", fileName.Replace(".3Dpathfinding.xml", ""), ".3Dpathfinding.xml\" ", " is not valid ! There is error: ", loadResult), "Load file error!");
                        return map;
                    }
                }
            } else {
                MessageBox.Show(string.Concat("Unfortunately, file \"", fileName.Replace(".3Dpathfinding.xml", ""), ".3Dpathfinding.xml\" ", " not exists !"), "Load file error!");
                RecentFileList recentFileList = new RecentFileList();
                recentFileList.RemoveFile(System.IO.Path.GetFullPath(fileName));
            }
            
            return map;
        }

        /// <summary>
        /// vypocita dlzku trasy napriec hranicnymi bodmi trasy
        /// </summary>
        /// <param name="trace">trasa medzi startovacim a cielovym bodom</param>
        /// <param name="transits">mnozina prechodov</param>
        /// <returns>vracia dlzku trasy v relativnej hodnote</returns>
        public static float TraceLength(List<MapPoint> trace, List<Transit> transits) {
            float length = 0;
            for (int i = 0; i < trace.Count-1; i++) {
                length += trace[i].Distance(trace[i + 1]);
                if (trace[i].X == trace[i + 1].X && trace[i].Y == trace[i + 1].Y) {
                    foreach (Transit transit in transits) {
                        if (transit.InObject(trace[i]) && ObjectOfBuilding.Middle(transit).Equals(trace[i])) {
                          //  length += transit.Delay;
                        }
                    }
                }
            }
            return length;
        }
    }
}
