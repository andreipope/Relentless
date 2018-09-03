// ------------------------------------------------------------------------------
//  _______   _____ ___ ___   _   ___ ___ 
// |_   _\ \ / / _ \ __/ __| /_\ | __| __|
//   | |  \ V /|  _/ _|\__ \/ _ \| _|| _| 
//   |_|   |_| |_| |___|___/_/ \_\_| |___|
// 
// This file has been generated automatically by TypeSafe.
// Any changes to this file may be lost when it is regenerated.
// https://www.stompyrobot.uk/tools/typesafe
// 
// TypeSafe Version: 1.3.2-Unity5
// 
// ------------------------------------------------------------------------------

namespace LoomNetwork.CZB {
    
    
    public sealed class SRScenes {
        
        private SRScenes() {
        }
        
        private const string _tsInternal = "1.3.2-Unity5";
        
        public static global::TypeSafe.Scene APP_INIT {
            get {
                return @__all[0];
            }
        }
        
        public static global::TypeSafe.Scene GAMEPLAY {
            get {
                return @__all[1];
            }
        }
        
        private static global::System.Collections.Generic.IList<global::TypeSafe.Scene> @__all = new global::System.Collections.ObjectModel.ReadOnlyCollection<global::TypeSafe.Scene>(new global::TypeSafe.Scene[] {
                    new global::TypeSafe.Scene("APP_INIT", 0),
                    new global::TypeSafe.Scene("GAMEPLAY", 1)});
        
        public static global::System.Collections.Generic.IList<global::TypeSafe.Scene> All {
            get {
                return @__all;
            }
        }
    }
}
