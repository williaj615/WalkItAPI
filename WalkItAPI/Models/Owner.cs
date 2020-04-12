using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WalkItAPI.Models
{
    public class Owner
    {
        public int Id { get; set; }
        [Required]
        [StringLength(40, MinimumLength = 2)]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public int NeighborhoodId { get; set; }
        public string Phone { get; set; }

        public Neighborhood Neighborhood { get; set; }

        public List<Dog> Dogs { get; set; }
    }
}
