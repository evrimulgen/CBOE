//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PerkinElmer.COE.Inventory.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class INV_CONTAINER_TYPES
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public INV_CONTAINER_TYPES()
        {
            this.INV_CONTAINERS = new HashSet<INV_CONTAINERS>();
        }
    
        public short CONTAINER_TYPE_ID { get; set; }
        public string CONTAINER_TYPE_NAME { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<INV_CONTAINERS> INV_CONTAINERS { get; set; }
    }
}