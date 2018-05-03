//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FishEDexWebAPI.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Species
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Species()
        {
            this.Tags = new HashSet<Tag>();
        }
    
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }
        public string ThumbnailURL { get; set; }
        public string CreatedUserId { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string LastEditUserId { get; set; }
        public Nullable<System.DateTime> LastEditDate { get; set; }
        public int CategoryId { get; set; }
        public string TileURL { get; set; }
        public string Location { get; set; }
    
        public virtual Category Category { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tag> Tags { get; set; }
    }
}
