// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

namespace CCGKit
{
    /// <summary>
    /// Custom attribute that allows to control the ordering of fields retrieved via reflection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class OrderAttribute : Attribute
    {
        /// <summary>
        /// The order of this field.
        /// </summary>
        private readonly int order;

        /// <summary>
        /// The order of this field.
        /// </summary>
        public int Order { get { return order; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="order">The order of the field.</param>
        public OrderAttribute(int order)
        {
            this.order = order;
        }
    }
}
