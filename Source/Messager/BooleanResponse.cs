namespace JordiBisbal.Messager {
    /// <summary>
    /// A message response that is boolen, either true or flase
    /// </summary>
    public class BooleanResponse : Response {
        /// <summary>
        /// The value
        /// </summary>
        private bool myValue;

        /// <summary>
        /// Value accessor
        /// </summary>
        public bool value { get { return myValue; } set { myValue = value; } }

        /// <summary>
        /// Construcor
        /// </summary>
        /// <param name="value">The boolean value</param>
        public BooleanResponse(bool value) {
            this.myValue = value;
        }
    }
}
