﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UpNet.Core
{
    [DataContract]
    public struct UserMeta
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
    }
}
