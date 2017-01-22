/************************************************************************************************************
 * Author: Jan Macek                                                                                        *
 * File: BakalarskaPraca.cs                                                                                 *
 *      - "startUp" subor projektu ktory vytvara GUI a obsluhuje udalosti                                   *
 * Date: 12.4.2013                                                                                          *
 * Mail: xmacek18@fit.vutbr.cz                                                                              *
 *                                                                                                          *
 * Copyright © Jan Macek 2013                                                                               *
 * *********************************************************************************************************/
using PathfindingApp.DelaunayTriangulationTypes;
using PathfindingApp.DrawTypes;
using PathfindingApp.HPATypes;
using PathfindingApp.AStarTypes;
using HPATypes;
using MapObjectTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Common;
using System.Threading;
using System.Diagnostics;

namespace PathfindingApp
{
    /// <summary>
    /// Trieda pre graficke vykreslenie mapy budovy
    /// </summary>
    public partial class MainWindow : Window {
        /// <summary>
        /// mapa ktora je aktualne vykreslena
        /// </summary>
        private Map map = null;

        /// <summary>
        /// velkost clusteru pre algorimus HPA*
        /// </summary>
        private int HPAGridSize = 20;

        /// <summary>
        /// aktualne aktivny algorimus
        /// </summary>
        private GuiHelpClass.ActiveAlgorithm activeAlgorithm;

        /// <summary>
        /// premenna ktora indikuje ci sa ma alebo nema vykreslovat grid pri vykonavani hladania
        /// </summary>
        private bool showGrid;

        /// <summary>
        /// stavova premenna indikujuca nastavovanie startovacieho bodu
        /// </summary>
        private bool isSettingStart = false;

        /// <summary>
        /// stavova premenna indikujuca nastavovanie cieloveho bodu
        /// </summary>
        private bool isSettingFinal = false;

        /// <summary>
        /// obsahuje posledne stlacene tlacitko klavesnce
        /// </summary>
        private System.Windows.Input.Key lastKey;

        /// <summary>
        /// vlakno ktore sa stara o vypocet trasy napriec budovou
        /// </summary>
        private Thread pathFinding; 

        /// <summary>
        /// konstruktor programu
        /// </summary>
        public MainWindow() {
            InitializeComponent();
            this.activeAlgorithm = GuiHelpClass.ActiveAlgorithm.astar;
            this.showGrid = false;
            this.InitializeMap();
            this.InitializeGrid();
            pathFinding = new Thread(PathFinding);
            pathFinding.Name = "Pathfinding";
            this.mainWindow.Closing += mainWindowClosing;
        }

        /// <summary>
        /// ukonci vlakno procesu vypoctu cesty v budove pred ukoncenim aplikacie
        /// </summary>
        private void mainWindowClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (this.pathFinding.IsAlive) {
                this.pathFinding.Abort();
            }
        }

        /// <summary>
        /// metoda inicializuje vykreslovanu mapu na zaciatku programu
        /// </summary>
        private void InitializeMap() {
            map = new Map();
        }

        /// <summary>
        /// metoda inicializuje grid programu na zaciatku programu
        /// </summary>
        private void InitializeGrid() {
            RecentFileList.MaxNumberOfFiles = 5;
            RecentFileList.MenuClick += (s, e) => GuiHelpClass.LoadMap(e.Filepath, (this.map.Floors.Count != 0), this.map, mainWindow, tabs, topMenu, WrpBtnXmlFiles, this.activeAlgorithm);
            List<string> filePaths = RecentFileList.RecentFiles;
            if (filePaths != null && filePaths.Count > 0) {
                this.lblNoRecentFiles.Visibility = System.Windows.Visibility.Hidden;
                int max = (filePaths.Count > 3) ? 3 : filePaths.Count;
                this.mainWindow.MinHeight = 200;
                this.mainWindow.MinWidth = 160 * max;
                for (int i = 0; i < max; i++) {
                    (WrpBtnXmlFiles.Children[i] as Button).Content = filePaths[i].ToString().Split('\\')[filePaths[i].ToString().Split('\\').Length - 1].Replace(".3Dpathfinding.xml", "");
                    (WrpBtnXmlFiles.Children[i] as Button).Visibility = Visibility.Visible;
                    (WrpBtnXmlFiles.Children[i] as Button).Width = 100;
                    (WrpBtnXmlFiles.Children[i] as Button).Margin = new Thickness(20);
                }
            } else {
                this.lblNoRecentFiles.Visibility = System.Windows.Visibility.Visible;
            }
        }

        /// <summary>
        /// spracuje kliknutie tlacitka suboru v hlavnom vybere po starte programu
        /// </summary>
        private void FileButtonClick(object sender, RoutedEventArgs e) {
            foreach (string file in RecentFileList.RecentFiles) {
                if ((sender as Button).Content.ToString().Equals(file.ToString().Split('\\')[file.ToString().Split('\\').Length - 1].Replace(".3Dpathfinding.xml", ""))) {
                    GuiHelpClass.LoadMap(file.ToString(), (this.map.Floors.Count != 0), this.map, mainWindow, tabs, topMenu, WrpBtnXmlFiles, this.activeAlgorithm);
                    break;
                }
            }
        }

        /// <summary>
        /// spracuje kliknutie tlacitka "Exit" v Menu
        /// </summary>
        private void MenuExitClick(object sender, RoutedEventArgs e) {
            if (this.pathFinding.IsAlive) {
                this.pathFinding.Abort();
            }
            Application.Current.Shutdown();
        }

        /// <summary>
        /// spracuje kliknutie tlacitka "Add start" v Menu
        /// </summary>
        private void MenuStartClick(object sender, RoutedEventArgs e) {
            this.isSettingStart = true;    
        }

        /// <summary>
        /// spracuje kliknutie tlacitka "Add final" v Menu
        /// </summary>
        private void MenuFinalClick(object sender, RoutedEventArgs e) {
            this.isSettingFinal = true;
        }

        /// <summary>
        /// spracuje kliknutie tlacitka "A*" v Menu
        /// </summary>
        private void AStarClick(object sender, RoutedEventArgs e) {
            DrawMap.Paint(this.map, tabs);
            this.btnMenuCustomGrid.IsEnabled = false;
            this.gridCustomize.Visibility = System.Windows.Visibility.Hidden;
            foreach (var item in this.mItemAlgorithms.Items) {
                if (item.GetType() == typeof(MenuItem)) {
                    (item as MenuItem).IsChecked = false;
                }
            }
            this.btnMenuAStar.IsChecked = true;
            this.activeAlgorithm = GuiHelpClass.ActiveAlgorithm.astar;
        }

        /// <summary>
        /// spracuje kliknutie tlacitka "Load" v Menu
        /// </summary>
        private void MenuLoadClick(object sender, RoutedEventArgs e) {   
            Microsoft.Win32.OpenFileDialog loadFileDialog = new Microsoft.Win32.OpenFileDialog();
            //nastavi filter 
            loadFileDialog.DefaultExt = ".txt";
            loadFileDialog.Filter = "XML Files|*.3Dpathfinding.xml";
            loadFileDialog.InitialDirectory = System.IO.Path.GetFullPath(@"../../../Saved/"); 
            Nullable<bool> result = loadFileDialog.ShowDialog();
            //ak bol vybraty subor, nacita ho
            if (result == true) {
                GuiHelpClass.LoadMap(loadFileDialog.FileName, (this.map.Floors.Count != 0), this.map, mainWindow, tabs, topMenu, WrpBtnXmlFiles, this.activeAlgorithm);
                this.lblNoRecentFiles.Visibility = System.Windows.Visibility.Hidden;
            }           
        }

        /// <summary>
        /// spracuje kliknutie tlacitka "Save" v Menu
        /// </summary>
        private void MenuSaveAsClick(object sender, RoutedEventArgs e) {
            //dialog vyberu miesta ulozenia suboru
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            //nastavenie vlastnosti dialogu
            saveFileDialog.Filter = "XML Files|*.3Dpathfinding.xml";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.InitialDirectory = System.IO.Path.GetFullPath(@"../../../Saved/");
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FileName = (this.mainWindow.Title.Split('-') as string[])[this.mainWindow.Title.Split('-').Length - 1].ToString();
            if (saveFileDialog.ShowDialog() == true) {
                map.SaveToXML(saveFileDialog.FileName);
                RecentFileList.RemoveFile(System.IO.Path.GetFullPath(saveFileDialog.FileName));
                RecentFileList.InsertFile(System.IO.Path.GetFullPath(saveFileDialog.FileName));
                this.mainWindow.Title = string.Concat("3D Pathfinding - ", saveFileDialog.FileName.ToString().Split('\\')[saveFileDialog.FileName.ToString().Split('\\').Length - 1]);
            }
        }

        /// <summary>
        /// spracuje kliknutie tlacitka "Save" v Menu
        /// </summary>
        private void MenuSaveClick(object sender, RoutedEventArgs e) {
            map.SaveToXML(RecentFileList.RecentFiles[0]);
        }

        /// <summary>
        /// spracuje kliknutie tlacitka mysi v oblasti TabWrapperu
        /// </summary>
        private void TabsMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (this.isSettingFinal) {
                System.Windows.Point position = e.GetPosition(this);           
                try {
                    int z = tabs.SelectedIndex;
                    int x = (int)position.X - 4;
                    int y = (int)position.Y - 50;
                    if (map.IsPointInMap(new MapPoint(x, y, z))) {
                        this.map.Final.Z = z;
                        this.map.Final.X = x;
                        this.map.Final.Y = y;
                        DrawMap.Paint(this.map, tabs);
                        tabs.SelectedIndex = this.map.Final.Z;
                        mainWindow.lblStatusBox.Content = "Trace length: -";
                    } else {
                        MessageBox.Show("Unfortunately, new final point is not in building area!", "Start point error!");
                    }
                    this.isSettingFinal = false;
                } catch(PathTooLongException) {
                    MessageBox.Show("Unfortunately, final point is not set!", "Final point error!");
                }
            } else if (this.isSettingStart) {
                System.Windows.Point position = e.GetPosition(this);    
                try {
                    int z = tabs.SelectedIndex;
                    int x = (int)position.X - 4;
                    int y = (int)position.Y - 50;
                    if (map.IsPointInMap(new MapPoint(x, y, z))) {
                        this.map.Start.Z = z;
                        this.map.Start.X = x;
                        this.map.Start.Y = y;
                        DrawMap.Paint(this.map, tabs);
                        tabs.SelectedIndex = this.map.Start.Z;
                        mainWindow.lblStatusBox.Content = "Trace length: -";
                    } else {
                        MessageBox.Show("Unfortunately, new start point is not in building area!", "Start point error!");
                    }
                    this.isSettingStart = false;
                } catch (PathTooLongException) {
                    MessageBox.Show("Unfortunately, start point is not set!", "Start point error!");
                }
            }
        }

        /// <summary>
        /// spracuje stlacenie tlacitka "Help" v Menu
        /// </summary>
        private void MenuHelpClick(object sender, RoutedEventArgs e) {
            byte[] resourcePDF = Properties.Resources.help;
            MemoryStream memStream = new MemoryStream(resourcePDF);

            //vytvorime PDF subor z binarky help.pdf ktory je v resources
            FileStream fileStream = new FileStream("help.pdf", FileMode.OpenOrCreate);

            //zapiseme Byty do help.pdf suboru
            memStream.WriteTo(fileStream);
            fileStream.Close();
            memStream.Close();

            //otvorime pdf
            Process.Start("help.pdf");
        }

        /// <summary>
        /// spracuje kliknutie klavesovych skratiek
        /// </summary>
        private void WindowKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (this.lastKey == System.Windows.Input.Key.RightAlt || this.lastKey == System.Windows.Input.Key.LeftAlt) {
                switch (e.Key) {
                    case System.Windows.Input.Key.F4:
                        this.MenuExitClick(this.btnMenuExit, new RoutedEventArgs());
                        break;
                    case System.Windows.Input.Key.S:
                        this.MenuStartClick(this.btnMenuAddStart, new RoutedEventArgs());
                        break;
                    case System.Windows.Input.Key.F:
                        this.MenuFinalClick(this.btnMenuAddFinish, new RoutedEventArgs());
                        break;
                    default:
                        break;
                }
            } else if(this.lastKey == System.Windows.Input.Key.LeftCtrl) {
                switch (e.Key) {
                    case System.Windows.Input.Key.O:
                        this.MenuLoadClick(this.btnMenuLoadFile, new RoutedEventArgs());
                        break;
                    case System.Windows.Input.Key.A:
                        this.MenuSaveAsClick(this.btnMenuSave, new RoutedEventArgs());
                        break;
                    case System.Windows.Input.Key.S:
                        this.MenuSaveClick(this.btnMenuSave, new RoutedEventArgs());
                        break;
                    case System.Windows.Input.Key.H:
                        this.MenuHelpClick(this.btnMenuHelp, new RoutedEventArgs());
                        break;
                    default:
                        break;
                }
            }
            this.lastKey = e.Key;
        }

        /// <summary>
        /// spracuje stlacenie tlacitka "Hierarchical A*" v Menu
        /// </summary>>
        private void HPAClick(object sender, RoutedEventArgs e) {
            DrawMap.Paint(this.map, tabs);
            this.btnMenuCustomGrid.IsEnabled = true;
            foreach (var item in this.mItemAlgorithms.Items) {
                if (item.GetType() == typeof(MenuItem)) {
                    (item as MenuItem).IsChecked = false;
                }
            }
            this.btnMenuHPA.IsChecked = true;
            this.activeAlgorithm = GuiHelpClass.ActiveAlgorithm.hpa;
        }

        

        /// <summary>
        /// spracuje stlacenie tlacitka "Find Trace" v Menu
        /// </summary>
        private void FindTraceClick(object sender, RoutedEventArgs e) {
            mainWindow.Title = "Processing ... " + mainWindow.Title;
            DrawMap.Paint(this.map, tabs);
            GuiHelpClass.HideMenuMapButtons(topMenu);
            this.btnRecentFiles.IsEnabled = false;
            this.btnMenuExit.IsEnabled = true;
            this.btnMenuHelp.IsEnabled = true;
            this.btnMenuStop.IsEnabled = true;
            pathFinding = new Thread(PathFinding);
            pathFinding.Start();     
        }

        /// <summary>
        /// vyhlada cestu napriec budovou s pouzitim vybraneho algorimu
        /// </summary>
        private void PathFinding() { 
            List<MapPoint> trace = new List<MapPoint>();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            switch (this.activeAlgorithm) {
                case GuiHelpClass.ActiveAlgorithm.hpa:
                    HPA hpaGen = new HPA();
                    trace = hpaGen.Process(this.map, this.tabs, this.showGrid, this.HPAGridSize);
                    break;
                case GuiHelpClass.ActiveAlgorithm.astar:
                    Astar3D astarts = new Astar3D();
                    trace = astarts.Process(this.map, this.tabs, this.showGrid);
                    break;
                case GuiHelpClass.ActiveAlgorithm.funnel:
                    break;
                default:
                    break;
            }
            sw.Stop();
            sw.ToString();
            tabs.Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate {
                if (!DrawTypes.DrawTrace.Paint(trace, tabs)) {
                    MessageBox.Show(string.Concat("Unfortunately, there is no trace from start to finish point!"), "Trace not found");
                } else {
                    mainWindow.lblStatusBox.Content = "Trace length: " + GuiHelpClass.TraceLength(trace, this.map.GetTransits()).ToString();
                }
                tabs.SelectedIndex = map.Final.Z;
                mainWindow.Title = mainWindow.Title.ToString().Replace("Processing ... ", "");
                GuiHelpClass.AllowMenuMapButtons(topMenu, this.activeAlgorithm);
                mainWindow.btnMenuStop.IsEnabled = false;
                this.btnMenuStop.IsEnabled = false;
            });
        }

        /// <summary>
        /// obsluha Menu -> ShowGrid tlacitka v menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuShowGridClick(object sender, RoutedEventArgs e) {
            this.showGrid = !this.showGrid;
            this.btnMenuShowGrid.IsChecked = this.showGrid;
            DrawMap.Paint(this.map, tabs);
        }

        /// <summary>
        /// ukonci proces vyhladavania cesty
        /// </summary>
        private void StopFindingTraceClick(object sender, RoutedEventArgs e) {
            if (this.pathFinding.IsAlive) {
                this.pathFinding.Abort();
            }
            mainWindow.Title = mainWindow.Title.ToString().Replace("Processing ... ", "");
            GuiHelpClass.AllowMenuMapButtons(topMenu, this.activeAlgorithm);
            mainWindow.btnMenuStop.IsEnabled = false;
            this.btnMenuStop.IsEnabled = false;
        }

        private void Slider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e) {

        }

        private void CustomizeGrid(object sender, RoutedEventArgs e) {
            this.gridCustomize.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnCustomizeGridClick(object sender, RoutedEventArgs e) {
            this.HPAGridSize = (int)this.sliderGridSize.Value;
            this.gridCustomize.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
