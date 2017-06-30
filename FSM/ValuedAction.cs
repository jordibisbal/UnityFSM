using JordiBisbal.FSM;

namespace JordiBisba.FSM {
    /// <summary>
    /// Delegate to receive state update
    /// </summary>
    /// <param name="state">The current state</param>
    /// <returns></returns>
    public delegate void ValuedAction(State state);
}
