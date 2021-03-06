// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Web.Mvc.ModelBinding
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class BindRequiredAttribute : BindingBehaviorAttribute
    {
        public BindRequiredAttribute()
            : base(BindingBehavior.Required)
        {
        }
    }
}
