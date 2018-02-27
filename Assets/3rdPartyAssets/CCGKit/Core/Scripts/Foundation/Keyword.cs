// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

namespace CCGKit
{
    /// <summary>
    /// This class represents a card keyword. Keywords are enum-like stats.
    /// </summary>
    public class Keyword : Resource
    {
        /// <summary>
        /// The current resource identifier.
        /// </summary>
        public static int currentId;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Keyword() : base(currentId++)
        {
        }

        /// <summary>
        /// The name of this keyword.
        /// </summary>
        public string name;

        /// <summary>
        /// The values of this keyword.
        /// </summary>
        public List<KeywordValue> values = new List<KeywordValue>();
    }

    /// <summary>
    /// This class represents a keyword value.
    /// </summary>
    public class KeywordValue
    {
        /// <summary>
        /// The value of this keyword.
        /// </summary>
        public string value;
    }
}
