/************************************************************************************************************
 * Author: Jan Macek                                                                                        *
 * Project: MapObjectTypes.csproj                                                                           *
 * File: ObjectOfBuilding.cs                                                                                *
 *      - trieda reprezentuje objekt ktory popisuje umiestnenie objektu v ramci budovy                      *
 * Date: 27.4.2013                                                                                          *
 * Mail: xmacek18@fit.vutbr.cz                                                                              *
 *                                                                                                          *
 * Copyright © Jan Macek 2013                                                                               *
 * *********************************************************************************************************/
using System;
using System.Collections.Generic;

namespace MapObjectTypes {
    /// <summary>
    /// popisuje umiestnenie objektu v ramci budovy
    /// </summary>
    [ Serializable ]
    public class ObjectOfBuilding {

        /// <summary>
        /// hranicne body miestnosti
        /// </summary>
        private List<MapPoint> corners = new List<MapPoint>();
        public List<MapPoint> Corners {
            get { return this.corners; }
            set { this.corners = value; }
        }

        /// <summary>
        /// najde tazisko objektu
        /// </summary>
        /// <param name="builObject">objekt ktory spracujeme</param>
        /// <returns>vracia tazisko objektu</returns>
        public static MapPoint Middle(ObjectOfBuilding builObject) { 
            MapPoint middle;
            int x = 0;
            int y = 0;
            foreach (MapPoint point in builObject.Corners) {
                x += point.X;
                y += point.Y;
	        }
            middle = new MapPoint(x / builObject.Corners.Count, y / builObject.Corners.Count, builObject.Corners[0].Z);
            return middle;
        }

        public bool Equals(ObjectOfBuilding objectOfBuilding){
            for (int i = 0; i < objectOfBuilding.Corners.Count; i++) {
                if (!objectOfBuilding.Corners[i].Equals(this.Corners[i])) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// zisti ci sa bod nachadza v objekte
        /// </summary>
        /// <param name="point">testovany bod</param>
        /// <returns>vracia ci sa bod nachadza v objekte</returns>
        public bool InObject(MapPoint point) {
            if (point.Z == this.Corners[0].Z) {
                bool result = false;
                int k = this.Corners.Count - 1;
                for (int j = 0; j < this.Corners.Count; j++) {
                    if ((this.Corners[j].Y < point.Y && this.Corners[k].Y >= point.Y
                      || this.Corners[k].Y < point.Y && this.Corners[j].Y >= point.Y)
                      && (this.Corners[j].X <= point.X || this.Corners[k].X <= point.X)) {
                        try {
                            if (this.Corners[j].X + (point.Y - this.Corners[j].Y) / (this.Corners[k].Y - this.Corners[j].Y) * (this.Corners[k].X - this.Corners[j].X) < point.X) {
                                result = !result;
                            }
                        } catch (DivideByZeroException) {
                            Console.WriteLine("Error: Dividing by zero!");
                        }
                    }
                    k = j;
                }
                return result;
            } else {
                return false;
            }
        }
    }
}
