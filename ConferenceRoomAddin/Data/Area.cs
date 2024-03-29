//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ConferenceRoomAddin.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class Area
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Area()
        {
            this.Rooms = new HashSet<Room>();
        }
    
        public int id { get; set; }
        public bool disabled { get; set; }
        public string area_name { get; set; }
        public string timezone { get; set; }
        public string area_admin_email { get; set; }
        public Nullable<int> resolution { get; set; }
        public Nullable<int> default_duration { get; set; }
        public bool default_duration_all_day { get; set; }
        public Nullable<int> morningstarts { get; set; }
        public Nullable<int> morningstarts_minutes { get; set; }
        public Nullable<int> eveningends { get; set; }
        public Nullable<int> eveningends_minutes { get; set; }
        public Nullable<bool> private_enabled { get; set; }
        public Nullable<bool> private_default { get; set; }
        public Nullable<bool> private_mandatory { get; set; }
        public string private_override { get; set; }
        public Nullable<bool> min_create_ahead_enabled { get; set; }
        public Nullable<int> min_create_ahead_secs { get; set; }
        public Nullable<bool> max_create_ahead_enabled { get; set; }
        public Nullable<int> max_create_ahead_secs { get; set; }
        public Nullable<bool> min_delete_ahead_enabled { get; set; }
        public Nullable<int> min_delete_ahead_secs { get; set; }
        public Nullable<bool> max_delete_ahead_enabled { get; set; }
        public Nullable<int> max_delete_ahead_secs { get; set; }
        public bool max_per_day_enabled { get; set; }
        public int max_per_day { get; set; }
        public bool max_per_week_enabled { get; set; }
        public int max_per_week { get; set; }
        public bool max_per_month_enabled { get; set; }
        public int max_per_month { get; set; }
        public bool max_per_year_enabled { get; set; }
        public int max_per_year { get; set; }
        public bool max_per_future_enabled { get; set; }
        public int max_per_future { get; set; }
        public string custom_html { get; set; }
        public Nullable<bool> approval_enabled { get; set; }
        public Nullable<bool> reminders_enabled { get; set; }
        public Nullable<bool> enable_periods { get; set; }
        public Nullable<bool> confirmation_enabled { get; set; }
        public Nullable<bool> confirmed_default { get; set; }
        public Nullable<short> allow_level { get; set; }
        public bool max_duration_enabled { get; set; }
        public int max_duration_secs { get; set; }
        public int max_duration_periods { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Room> Rooms { get; set; }
    }
}
