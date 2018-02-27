// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

namespace CCGKit
{
    /// <summary>
    /// A card set is a named collection of cards. Their main purpose is to help organize
    /// a big collection of cards into smaller, more manageable sub-groups.
    /// </summary>
    public class CardSet
    {
        /// <summary>
        /// The name of this card set.
        /// </summary>
        public string name;

        /// <summary>
        /// The cards of this card set.
        /// </summary>
        public List<Card> cards = new List<Card>();
    }
}
