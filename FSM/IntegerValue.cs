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
    }
}
