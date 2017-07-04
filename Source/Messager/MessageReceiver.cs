using JordiBisbal.Messager;

namespace JordiBisba.Messager {
    /// <summary>
    /// Delegate to receive messages
    /// </summary>
    /// <param name="subject">Message subject</param>
    /// <param name="body">Message body (request)</param>
    /// <returns></returns>
    public delegate Response MessageReceiver(string subject, Request body);
}
