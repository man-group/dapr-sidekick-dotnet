// Based on https://github.com/dotnet/extensions/blob/release/2.1/src/Logging/Logging.Abstractions/src/DaprEventId.cs
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See NET_EXTENSIONS_LICENSE in this directory for license information.

namespace Man.Dapr.Sidekick.Logging
{
    public struct DaprEventId
    {
        public static implicit operator DaprEventId(int i)
        {
            return new DaprEventId(i);
        }

        public static bool operator ==(DaprEventId left, DaprEventId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DaprEventId left, DaprEventId right)
        {
            return !left.Equals(right);
        }

        public DaprEventId(int id, string name = null)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }

        public string Name { get; }

        public override string ToString()
        {
            return Name ?? Id.ToString();
        }

        public bool Equals(DaprEventId other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return obj is DaprEventId daprEventId && Equals(daprEventId);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
