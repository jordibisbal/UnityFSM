using System;

namespace JordiBisbal.FSM {
    public class IntegerValue : Value {
        private int value;

        /// <summary>
        /// Creates the value object
        /// </summary>
        /// <param name="value">Integer value</param>
        public IntegerValue(int value) {
            this.value = value;
        }

        /// <summary>
        /// return the value as integer
        /// </summary>
        /// <returns></returns>
        public int AsInt() {
            return value;
        }

        /// <summary>
        /// Returns a new value object with the value incrmente by one
        /// </summary>
        internal IntegerValue Inc() {
            return new IntegerValue(value + 1);
        }
    }
}
