using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UpNet.Core
{
    [DataContract, JsonObject]
    public struct UserMeta : ICloneable, IEquatable<UserMeta>
    {
        [DataMember, JsonProperty]
        public DateTime ReleaseDate { get; private set; }

        [DataMember, JsonProperty]
        public string ReleaseNotes { get; private set; }

        public UserMeta(DateTime releaseDate, string releaseNotes)
            : this()
        {
            this.ReleaseDate = releaseDate;
            this.ReleaseNotes = releaseNotes;
        }

        public object Clone()
        {
            return new UserMeta(this.ReleaseDate, this.ReleaseNotes);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            return (obj is UserMeta) ? this.Equals((UserMeta)obj) : false;
        }

        public bool Equals(UserMeta other)
        {
            return (this.ReleaseDate == other.ReleaseDate) && (this.ReleaseNotes == other.ReleaseNotes);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 29;
                hash = hash * 486187739 + this.ReleaseDate.GetHashCode();
                hash = hash * 486187739 + (this.ReleaseNotes ?? string.Empty).GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(UserMeta left, UserMeta right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(UserMeta left, UserMeta right)
        {
            return !(left == right);
        }
    }
}
