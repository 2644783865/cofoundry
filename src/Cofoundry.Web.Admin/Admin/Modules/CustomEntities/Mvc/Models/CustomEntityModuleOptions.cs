﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cofoundry.Domain;

namespace Cofoundry.Web.Admin
{
    /// <summary>
    /// A set of useful settings and options output as a js object for the front
    /// end UI. Includes mostly properties from the custom entity definition but 
    /// could include other information too.
    /// </summary>
    public class CustomEntityModuleOptions
    {
        /// <summary>
        /// Unique 6 letter code representing the module (use uppercase)
        /// </summary>
        public string CustomEntityDefinitionCode { get; set; }

        /// <summary>
        /// Plural name of the entity e.g. 'Products'
        /// </summary>
        /// <remarks>
        /// The name/singular/plural are different to the domain layer
        /// because this was changed after an initial release and we didn't want to
        /// break existing implementations of the js layer silently.
        /// </remarks>
        public string Name { get; set; }

        /// <summary>
        /// Singlar name of the entity e.g. 'Product'
        /// </summary>
        public string NameSingular { get; set; }

        /// <summary>
        /// Indicates whether the UrlSlug property should be treated
        /// as a unique property and be validated as such. This will also affect
        /// the routing templates available for this entity because some routes require
        /// a unique slug.
        /// </summary>
        public bool ForceUrlSlugUniqueness { get; set; }

        /// <summary>
        /// Indicates whether the entities are partitioned by locale
        /// </summary>
        public bool HasLocale { get; set; }

        /// <summary>
        /// Indicates whether the url slug should be autogenerated. If this
        /// is selected then the user will not be shown the UrlSlug property. Useful
        /// if the entity will never be used in a page.
        /// </summary>
        public bool AutoGenerateUrlSlug { get; set; }

        /// <summary>
        /// Indicates whether this custom entity should always be published when saved, provided the
        /// user has permissions to do so. 
        /// </summary>
        public bool AutoPublish { get; set; }

        /// <summary>
        /// Indicates the type of ordering permitted on this custom entity type.
        /// </summary>
        public CustomEntityOrdering Ordering { get; set; }

        /// <summary>
        /// Terminology to use when displaying the custom entity, e.g. Title, Url Slug
        /// </summary>
        public Dictionary<string, string> Terms { get; set; }
    }
}