/************************************************************************************************************
 * Author: Jan Macek                                                                                        *
 * Project: MapObjectTypes.csproj                                                                           *
 * File: Transit.cs                                                                                         *
 *      - trieda reprezentuje prechod v budove = dvere, schody, vytah                                       *
 * Date: 27.4.2013                                                                                          *
 * Mail: xmacek18@fit.vutbr.cz                                                                              *
 *                                                                                                          *
 * Copyright © Jan Macek 2013                                                                               *
 * *********************************************************************************************************/
using System;

namespace MapObjectTypes {
    /// <summary>
    /// prechody medzi miestnostami (dvere, vytahy, schody)
    /// </summary>
    [ Serializable ]
    public class Transit : ObjectOfBuilding{

        /// <summary>
        /// vyctovy typ ktory oznacuje typ prechodu
        /// </summary>
        public enum transitType { nonSet, door, elevator, stairs};

        /// <summary>
        /// reprezentuje hodnotu spomalenia pri prechadzani daneho prechodu
        /// </summary>
        private int delay;
        public int Delay {
            set { this.delay = value; }
            get { return this.delay; }
        }

        /// <summary>
        /// jednoznacne oznacenie prechodu
        /// </summary>
        private string id;
        public string Id {
            set { this.id = value; }
            get { return this.id; }
        }

        /// <summary>
        /// typ prechodu
        /// </summary>
        private transitType type;
        public transitType Type {
            set { this.type = value; }
            get { return this.type; }
        }

        /// <summary>
        /// konstruktor
        /// </summary>
        /// <param name="newDelay">hodnota spomalenia pri prechadzani</param>
        /// <param name="newId">jednoznacne oznacenie prechodu</param>
        public Transit(int newDelay, string newId, transitType newType) {
            this.delay = newDelay;
            this.id = newId;
            this.type = newType;
        }

        /// <summary>
        /// konstruktor
        /// </summary>
        public Transit() {
            this.delay = 0;
            this.id = String.Concat("transit", this.GetHashCode().ToString());
            this.type = transitType.nonSet;
        }

        /// <summary>
        /// porovnava dva prechody
        /// </summary>
        /// <param name="point">druhy porovnavany prechod</param>
        /// <returns>vracia ci sa prechody rovnaju</returns>
        public bool Equals(Transit newTransit) {
            if (this.delay == newTransit.Delay && this.Id == newTransit.Id && this.Type == newTransit.Type && this.Corners.Count == newTransit.Corners.Count) {
                for (int i = 0; i < this.Corners.Count; i++) {
                    if (this.Corners[i].X != newTransit.Corners[i].X || this.Corners[i].Y != newTransit.Corners[i].Y || this.Corners[i].Z != newTransit.Corners[i].Z) {
                        return false;
                    }
                }
                return true;
            } else {
                return false;
            }  
        }

    }
}
