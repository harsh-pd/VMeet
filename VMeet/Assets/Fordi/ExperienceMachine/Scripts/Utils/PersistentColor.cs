using ProtoBuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fordi.Common
{
    [ProtoContract]
    public partial class PersistentColor
    {
        [ProtoMember(256)]
        public float r;

        [ProtoMember(257)]
        public float g;

        [ProtoMember(258)]
        public float b;

        [ProtoMember(259)]
        public float a;

        public static explicit operator Color(PersistentColor surrogate)
        {
            if (surrogate == null)
                return Color.white;

            return new Color(surrogate.r, surrogate.g, surrogate.b, surrogate.a);
        }

        public static implicit operator PersistentColor(Color obj)
        {
            return new PersistentColor
            {
                r = obj.r,
                g = obj.g,
                b = obj.b,
                a = obj.a
            };
        }
    }
}
