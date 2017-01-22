/************************************************************************************************************
 * Author: Jan Macek                                                                                        *
 * Project: MapObjectTypes.csproj                                                                           *
 * File: MapPoint.cs                                                                                        *
 *      - trieda reprezentuje bod v trojrozmernom priestore                                                 *
 * Date: 27.4.2013                                                                                          *
 * Mail: xmacek18@fit.vutbr.cz                                                                              *
 *                                                                                                          *
 * Copyright © Jan Macek 2013                                                                               *
 * *********************************************************************************************************/
using System;

namespace MapObjectTypes {
    /// <summary>
    /// reprezentuje bod v trojrozmernom priestore
    /// </summary>
    [ Serializable ]
    public class MapPoint {
        /// <summary>
        /// x-ova suradnica bodu
        /// </summary>
        private int x;
        public int X {
            set { this.x = value; }
            get { return this.x; }
        }

        /// <summary>
        /// y-ova suradnica bodu
        /// </summary>
        private int y;
        public int Y {
            set { this.y = value; }
            get { return this.y; }
        }

        /// <summary>
        /// z-ova suradnica bodu
        /// </summary>
        private int z;
        public int Z {
            set { this.z = value; }
            get { return this.z; }
        }

        /// <summary>
        /// vytvori objekt Point s defaultnymi hodnotami [0, 0, 0]
        /// </summary>
        public MapPoint() {
            this.x = 0;
            this.y = 0;
            this.z = 0;
        }

        /// <summary>
        /// vytvori objekt Point s hodnotami v parametroch
        /// </summary>
        /// <param name="newX">x-ova suradnica</param>
        /// <param name="newY">y-ova suradnica</param>
        /// <param name="newZ">z-ova suradnica</param>
        public MapPoint(int newX, int newY, int newZ) {
            this.x = newX;
            this.y = newY;
            this.z = newZ;
        }

        /// <summary>
        /// vypocita vzdialenost od ineho bodu
        /// </summary>
        /// <param name="otherPoint">druhy bod</param>
        /// <returns>vracia vzdialenost</returns>
        public float Distance(MapPoint otherPoint) {
            float dx = x - otherPoint.X;
            float dy = y - otherPoint.Y;
            return dx * dx + dy * dy;
        }

        /// <summary>
        /// porovnava dva body
        /// </summary>
        /// <param name="point">druhy porovnavany bod</param>
        /// <returns>vracia ci sa body rovnaju</returns>
        public bool Equals(MapPoint point) {
            if (point.X == this.X && point.Y == this.Y && point.Z == this.Z) {
                return true;
            }
            return false;
        }
    }
}
