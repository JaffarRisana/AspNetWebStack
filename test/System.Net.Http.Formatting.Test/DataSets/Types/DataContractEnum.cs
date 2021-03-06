// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.Serialization;

namespace System.Net.Http.Formatting.DataSets.Types
{
    [DataContract]
    public enum DataContractEnum
    {
        [EnumMember]
        First,

        [EnumMember]
        Second,

        Third
    }
}
