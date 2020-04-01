using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WalkItAPI.Models;

namespace WalkItAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public OwnerController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        //Get all owners from the database
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, OName, Address, NeighborhoodId, Phone FROM Owner";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Owner> owners = new List<Owner>();

                    while (reader.Read())
                    {
                        Owner owner = new Owner
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("OName")),
                            Address = reader.GetString(reader.GetOrdinal("Address")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                            Phone = reader.GetString(reader.GetOrdinal("Phone"))
                        };

                        owners.Add(owner);
                    }
                    reader.Close();

                    return Ok(owners);
                }
            }
        }


        //Get a single owner by Id
        [HttpGet("{id}", Name = "GetOwner")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, OName, Address, NeighborhoodId, Phone
                        FROM Owner
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Owner owner = null;

                    if (reader.Read())
                    {
                        owner = new Owner
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("OName")),
                            Address = reader.GetString(reader.GetOrdinal("Address")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                            Phone = reader.GetString(reader.GetOrdinal("Phone"))
                        };
                    }
                    else
                    {
                        return NotFound();
                    }
                    reader.Close();

                    return Ok(owner);
                }
            }
        }
    }
}
