﻿using Kooboo.CMS.Common.Persistence.Non_Relational;
using Kooboo.CMS.Modules.Publishing.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Modules.Publishing.Persistence.Default
{
    [Kooboo.CMS.Common.Runtime.Dependency.Dependency(typeof(IRemoteTextFolderMappingProvider))]
    [Kooboo.CMS.Common.Runtime.Dependency.Dependency(typeof(IProvider<RemoteTextFolderMapping>))]
    public class RemoteTextFolderMappingProvider : FileSystemProviderBase<RemoteTextFolderMapping>, IRemoteTextFolderMappingProvider
    {
        #region .ctor
        public RemoteTextFolderMappingProvider(Kooboo.CMS.Sites.Persistence.ISiteProvider siteProvider)
            : base(siteProvider)
        {}
        #endregion
        #region GetLocker
        static System.Threading.ReaderWriterLockSlim locker = new System.Threading.ReaderWriterLockSlim();
        protected override System.Threading.ReaderWriterLockSlim GetLocker()
        {
            return locker;
        }
        #endregion

        protected override string GetBasePath(Sites.Models.Site site)
        {
            return Path.Combine(site.PhysicalPath, PublishingPath.PublishingFolderName, "RemoteTextFolderMappings");
        }
    }
}
