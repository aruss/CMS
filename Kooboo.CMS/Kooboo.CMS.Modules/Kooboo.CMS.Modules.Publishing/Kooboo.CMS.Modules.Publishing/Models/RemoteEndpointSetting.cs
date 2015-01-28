﻿#region License
// 
// Copyright (c) 2013, Kooboo team
// 
// Licensed under the BSD License
// See the file LICENSE.txt for details.
// 
#endregion
using Kooboo.CMS.Common.Persistence.Non_Relational;
using Kooboo.CMS.Sites.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Kooboo.CMS.Modules.Publishing.Models
{
    #region Persistence
    [DataContract]
    public partial class RemoteEndpointSetting
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string CmisService { get; set; }
        [DataMember]
        public string CmisUserName { get; set; }
        [DataMember]
        public string CmisPassword { get; set; }
        [DataMember]
        public int MaxRetryTimes { get; set; }
        [DataMember]
        public bool Enabled { get; set; }
        [DataMember]
        public bool PublishPageAutomatically { get; set; }
        [DataMember]
        public string RemoteRepositoryId { get; set; }
    }
    #endregion

    public partial class RemoteEndpointSetting : IPersistable, IIdentifiable, ISiteObject
    {
        public string UUID
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }
        public RemoteEndpointSetting()
        {

        }
        public RemoteEndpointSetting(Site site, string uuid)
        {
            this.Site = site;
            this.UUID = uuid;
        }

        private bool _isDummy = true;
        public bool IsDummy
        {
            get
            {
                return this._isDummy;
            }
            private set
            {
                this._isDummy = value;
            }
        }

        public void Init(IPersistable source)
        {
            this.IsDummy = false;
            this.Site = ((ISiteObject)source).Site;
        }

        public void OnSaved()
        {
            this.IsDummy = false;
        }

        public void OnSaving()
        {

        }

        public Site Site
        {
            get;
            set;
        }
    }
}
