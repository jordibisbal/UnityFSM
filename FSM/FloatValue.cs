namespace JordiBisbal.FSM {
    public class FloatValue : Value {
        private float value;

        /// <summary>
        /// Creates the value object
        /// </summary>
        /// <param name="value">Float value</param>
        public FloatValue(float value) {
            this.value = value;
        }

        /// <summary>
        /// return the value as float
        /// </summary>
        /// <returns></returns>
        public float AsFloat() {
            return value;
        }
    }
}
