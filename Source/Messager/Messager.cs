using JordiBisbal.Messager;
using System.Collections.Generic;
using UnityEngine;

namespace JordiBisba.Messager {

    /// <summary>
    /// Message post office, it routes messages between bojects
    /// </summary>
    public class Messager {

        /// <summary>
        /// The singleton instance
        /// </summary>
        private static Messager singleton = null;

        /// <summary>
        /// Holds all the subscribers, the address book
        /// </summary>
        private Dictionary<string, MessageReceiver> subscribers = new Dictionary<string, MessageReceiver>();

        /// <summary>
        /// Obtains (or builds) the singleton instance
        /// </summary>
        /// <returns></returns>
        private static Messager getInstance() {            
            if (singleton == null) {
                singleton = new Messager();
            }

            return singleton;
        }

        /// <summary>
        /// Subscribes and object to a given subject, an addresee can be substribed just once
        /// </summary>
        /// <param name="object">The object that is subscribed, the addressee</param>
        /// <param name="subject">The message subject</param>
        /// <param name="receiver">The callback that will receive the message</param>
        /// <returns></returns>
        public static Messager subscribe(Object subscriber, string subject, MessageReceiver receiver) {
            Messager instance = getInstance();
            string key = getGameObjectKey(subscriber, subject);

            if (instance.subscribers.ContainsKey(key)) {
                Debug.LogError("A object just can subcripbe once (" + subscriber.name + " #" + subscriber.GetInstanceID() + " already subscribed)");
            }
            instance.subscribers.Add(key, receiver);

            return instance;
        }

        /// <summary>
        /// Subscribes and object to any subject, an addresee can be substribed just once (but can also be subscribed to especific messages)
        /// </summary>
        /// <param name="object">The object that is subscribed, the addressee</param>
        /// <param name="subject">The message subject</param>
        /// <param name="receiver">The callback that will receive the message</param>
        /// <returns></returns>
        public static Messager subscribe(Object subscriber, MessageReceiver receiver) {
            Messager instance = getInstance();
            string key = getGameObjectKey(subscriber);

            if (instance.subscribers.ContainsKey(key)) {
                Debug.LogError("A object just can subcripbe once (" + subscriber.name + " #" + subscriber.GetInstanceID() + " already subscribed)");
            }
            instance.subscribers.Add(key, receiver);

            return instance;
        }

        /// <summary>
        /// Buils the key to be used to store the subscribeds (address book)
        /// </summary>
        /// <param name="object"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        private static string getGameObjectKey(Object subscriber, string subject = "") {
            return subscriber.GetInstanceID() + ":" + subject;
        }

        /// <summary>
        /// Sends a message, if nobody is listering, null is returned elsewhere the responese is returned
        /// </summary>
        /// <param name="object">Addresee</param>
        /// <param name="subject">Subject</param>
        /// <param name="body">Message body (request)</param>
        /// <returns></returns>
        public static Response MessageIgnoreIfNobodyListens(Object subscriber, string subject, Request body) {
            return myMessage(subscriber, subject, body, true);
        }

        /// <summary>
        /// Send a message, there must be a receiber for the message, if not, an exception is thrownm the return
        /// </summary>
        /// <param name="object">Addresee</param>
        /// <param name="subject">Subject</param>
        /// <param name="body">Message body (request)</param>
        /// <returns></returns>
        public static Response SendMessage(Object subscriber, string subject, Request body) {
            return myMessage(subscriber, subject, body, false);
        }

        /// <summary>
        /// Sends a message, if ignoreIfNodobyListens and nobody is listering, null is returned elsewhere the responese is returned
        /// If ignoreIfNobodyListens is false and nobody is listering, and exception is thrown
        /// </summary>
        /// <param name="object"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="ignoreIfNobodyListens"></param>
        /// <returns></returns>
        private static Response myMessage(Object subscriber, string subject, Request body, bool ignoreIfNobodyListens) {
            Messager instance = getInstance();
            string key = getGameObjectKey(subscriber);

            MessageReceiver messageReceiver = null;

            if (instance.subscribers.TryGetValue(key, out messageReceiver)) {
                return messageReceiver.Invoke(subject, body);
            }

            if (instance.subscribers.TryGetValue(key, out messageReceiver)) {
                return messageReceiver.Invoke(subject, body);
            }

            if (ignoreIfNobodyListens) {
                return null;
            }

            var errorMessage = "Object " + subscriber.name + " #" + subscriber.GetInstanceID() + " not subscribed";
            Debug.LogError(errorMessage);

            throw new UnkownAddresseeException(errorMessage);            
        }
        
        /// <summary>
        /// Returns the value (if true) of a Booleanresponse Response, if Response is not a BooleanResponse, an exception is thrown
        /// </summary>
        /// <param name="response">The response to evaluate</param>
        /// <returns></returns>                        
        public static bool isYes(Response response) {
            if (!(response is BooleanResponse)) {
                throw new responseTypeMisMatch("A BooleanResponse was expected but a " + response.GetType() + " response was found");
            }

            return ((BooleanResponse)response).value;
        }

        /// <summary>
        /// Alias to isYes
        /// </summary>
        public static System.Func<Response, bool> isTrue = isYes;

        /// <summary>
        /// Returns the inverse value (if false) of a Booleanresponse Response, if Response is not a BooleanResponse, an exception is thrown
        /// </summary>
        /// <param name="response">The response to evaluate</param>
        /// <returns></returns>
        public static bool isNo(Response response) {
            return ! isYes(response);
        }

        /// <summary>
        /// Alias for isNo
        /// </summary>
        public static System.Func<Response, bool> isFalse = isNo;

        /// <summary>
        /// Unsubscribe a given gameObjects for a given subject
        /// </summary>
        /// <param name="object"></param>
        /// <param name="receiver"></param>
        /// <returns></returns>
        public static Messager unSubscribe(Object subscriber, string subject) {
            Messager instance = getInstance();
            string key = getGameObjectKey(subscriber, subject);

            if (!instance.subscribers.ContainsKey(key)) {
                Debug.LogError("Oobject " + subscriber.name + " #" + subscriber.GetInstanceID() + " not subscribed");
            }
            instance.subscribers.Remove(key);

            return instance;
        }

        /// <summary>
        /// Unsubscribe a given gameObjects for a non especify messages, ie. this will not unsubcribe subject espeficed subscriptions
        /// </summary>
        /// <param name="object">The object to unsubcribe</param>
        /// <returns></returns>
        public static Messager unSubscribe(Object subscriber) {
            return unSubscribe(subscriber, "");
        }
    }
}
