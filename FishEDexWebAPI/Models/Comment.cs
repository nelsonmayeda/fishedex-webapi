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
    
    public partial class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int FishId { get; set; }
        public string CreatedUserId { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string LastEditUserId { get; set; }
        public Nullable<System.DateTime> LastEditDate { get; set; }
    
        public virtual Fish Fish { get; set; }
    }
}