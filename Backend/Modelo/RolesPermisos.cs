using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoRuben.Backen.Modelo
{
    [Table("roles_permisos")]
    public partial class RolesPermisos
    {

        [Key]
        [Column("ROLES_ID")]
        public int RolesId { get; set; }

        [Key]
        [Column("PERMISOS_ID")]
        public int PermisosId { get; set; }

        [InverseProperty("RolesPermisos")]
        public virtual Role Roles { get; set; } = null!;

        [InverseProperty("RolesPermisos")]
        public virtual Permiso Permisos { get; set; } = null!;
    }
}