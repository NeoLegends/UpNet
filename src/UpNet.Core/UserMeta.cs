using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UpNet.Core
{
    [DataContract]
    public struct UserMeta : IEquatable<UserMeta>
    {
        [DataMember]
        public DateTime ReleaseDate { get; private set; }

        [DataMember]
        public String ReleaseNotes { get; private set; }

        public UserMeta(DateTime releaseDate, String releaseNotes)
            : this()
        {
            this.ReleaseDate = releaseDate;
            this.ReleaseNotes = releaseNotes;
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
            return new { this.ReleaseDate, this.ReleaseNotes }.GetHashCode();
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
