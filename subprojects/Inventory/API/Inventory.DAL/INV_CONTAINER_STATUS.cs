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
    
    public partial class INV_CONTAINER_STATUS
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public INV_CONTAINER_STATUS()
        {
            this.INV_CONTAINERS = new HashSet<INV_CONTAINERS>();
        }
    
        public short CONTAINER_STATUS_ID { get; set; }
        public string CONTAINER_STATUS_NAME { get; set; }
        public string CONTAINER_STATUS_DESC { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<INV_CONTAINERS> INV_CONTAINERS { get; set; }
    }
}