// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    /// <summary>
    /// Base type intended to be subclassed from in order to provide management of a specific subset
    /// of the server's functionality. This separation of concerns helps to avoid having a gigantic
    /// server class and makes the code more maintainable and reusable.
    /// </summary>
    public class ServerHandler
    {
        /// <summary>
        /// Convenient access to the server itself.
        /// </summary>
        protected Server server;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="server">Reference to the server.</param>
        public ServerHandler(Server server)
        {
            this.server = server;
        }

        /// <summary>
        /// This method is where subclasses should register to receive the network messages they are
        /// interested in.
        /// </summary>
        public virtual void RegisterNetworkHandlers()
        {
        }

        /// <summary>
        /// This method is where subclasses should unregister to stop receiving the network messages they are
        /// interested in.
        /// </summary>
        public virtual void UnregisterNetworkHandlers()
        {
        }

        /// <summary>
        /// This method provides a convenient entry point for subclasses to perform any turn-start-specific
        /// initialization logic.
        /// </summary>
        public virtual void OnStartTurn()
        {
        }

        /// <summary>
        /// This method provides a convenient entry point for subclasses to perform any turn-end-specific
        /// cleanup logic.
        /// </summary>
        public virtual void OnEndTurn()
        {
        }
    }
}
