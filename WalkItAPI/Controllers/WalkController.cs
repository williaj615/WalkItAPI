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
    public class WalkController : ControllerBase
    {
        private readonly IConfiguration _config;

        public WalkController(IConfiguration config)
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

        //Get all walks from the database
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Date, Duration, WalkerId, WalkId FROM Walk";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Walk> walks = new List<Walk>();

                    while (reader.Read())
                    {
                        Walk walk = new Walk
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                            Duration = reader.GetInt32(reader.GetOrdinal("Duration")),
                            WalkerId = reader.GetInt32(reader.GetOrdinal("WalkerId")),
                            DogId = reader.GetInt32(reader.GetOrdinal("WalkId"))
                        };

                        walks.Add(walk);
                    }
                    reader.Close();

                    return Ok(walks);
                }
            }
        }


        //Get a single walk by Id
        [HttpGet("{id}", Name = "GetWalk")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, Date, Duration, WalkerId, WalkId
                        FROM Walk
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Walk walk = null;

                    if (reader.Read())
                    {
                        walk = new Walk
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                            Duration = reader.GetInt32(reader.GetOrdinal("Duration")),
                            WalkerId = reader.GetInt32(reader.GetOrdinal("WalkerId")),
                            DogId = reader.GetInt32(reader.GetOrdinal("WalkId"))
                        };
                    }
                    else
                    {
                        return NotFound();
                    }
                    reader.Close();

                    return Ok(walk);
                }
            }
        }
    }
}

    //    [HttpPost]
    //    public async Task<IActionResult> Post([FromBody] Walk walk)
    //    {
    //        using (SqlConnection conn = Connection)
    //        {
    //            conn.Open();
    //            using (SqlCommand cmd = conn.CreateCommand())
    //            {
    //                cmd.CommandText = @"INSERT INTO Walk (DName, OwnerId, Breed, Notes)
    //                                    OUTPUT INSERTED.Id
    //                                    VALUES (@DName, @OwnerId, @Breed, @Notes)";
    //                cmd.Parameters.Add(new SqlParameter("@DName", walk.Name));
    //                cmd.Parameters.Add(new SqlParameter("@OwnerId", walk.OwnerId));
    //                cmd.Parameters.Add(new SqlParameter("@Breed", walk.Breed));
    //                cmd.Parameters.Add(new SqlParameter("@Notes", walk.Notes));

    //                int newId = (int)cmd.ExecuteScalar();
    //                walk.Id = newId;
    //                return CreatedAtRoute("GetWalk", new { id = newId }, walk);
    //            }
    //        }
    //    }
    //}

