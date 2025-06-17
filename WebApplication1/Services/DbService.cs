using System.Data.Common;
using Microsoft.Data.SqlClient;
using Poprawa1.Models;
using WebApplication1.Exceptions;

namespace WebApplication1.Services;

public class DbService : IDbService
{
    private readonly IConfiguration _configuration;

    public DbService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<ProjectDTO> GetProjectAsync(int id)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();

        string sqlQuery = @"
            SELECT PR.ProjectId, PR.StartDate, PR.EndDate, PR.Objective,
            AR.Name AS ArtifactName, AR.OriginDate AS ArtifactOriginDate,
            INST.InstitutionID, INST.Name AS InstitutionName, INST.FoundedYear AS InstitutionFoundedYear,
            STA.Role as Role, STF.FirstName AS StaffFirstName, STF.LastName AS StaffLastName,
            STF.HireDate AS StaffHireDate
            FROM Preservation_Project AS PR
            JOIN Artifact AS AR ON PR.ArtifactId = AR.ArtifactId
            JOIN Institution AS INST ON AR.InstitutionId = INST.InstitutionId
            JOIN Staff_Assignment AS STA ON PR.ProjectId = STA.ProjectId
            JOIN Staff AS STF ON STA.StaffId = STF.StaffId
            WHERE PR.ProjectId = @id
        ";
        
        command.Parameters.Clear();
        command.CommandText = sqlQuery;
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync();

        ProjectDTO? projectDTO = null;

        while (await reader.ReadAsync())
        {
            if (projectDTO is null)
            {
                int endDate = reader.GetOrdinal("EndDate");
                projectDTO = new ProjectDTO()
                {
                    ProjectId = reader.GetInt32(reader.GetOrdinal("ProjectId")),
                    Objective = reader.GetString(reader.GetOrdinal("Objective")),
                    StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                    EndDate = reader.IsDBNull(endDate) ? null : reader.GetDateTime(reader.GetOrdinal("EndDate"))
                };
            }

            projectDTO.Artifacts.Add(new ArtifactDTO
            {
                Name = reader.GetString(reader.GetOrdinal("ArtifactName")),
                OriginDate = reader.GetDateTime(reader.GetOrdinal("ArtifactOriginDate")),
                Institution = new InstitutionDTO()
                {
                    InsitutionId = reader.GetInt32(reader.GetOrdinal("InstitutionId")),
                    Name = reader.GetString(reader.GetOrdinal("InstitutionName")),
                    FoundedYear = reader.GetInt32(reader.GetOrdinal("InstitutionFoundedYear")),
                }
            });
            
            projectDTO.StaffAssignments.Add(new StaffAssignmentDTO()
            {
                FirstName = reader.GetString(reader.GetOrdinal("StaffFirstName")),
                LastName = reader.GetString(reader.GetOrdinal("StaffLastName")),
                HireDate = reader.GetDateTime(reader.GetOrdinal("StaffHireDate")),
                Role = reader.GetString(reader.GetOrdinal("Role")),
            });
        }

        if (projectDTO is null)
        {
            throw new ProjectDoesntExistException();
        }

        return projectDTO;
    }

    public async Task AddArtifact(InputDTO input)
    {
        await using SqlCommand command = new SqlCommand();
        command.Connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await command.Connection.OpenAsync();
        DbTransaction transaction = await command.Connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
        try
        {
            //chceck if id already exists for artifact and project
            command.Parameters.Clear();
            command.CommandText = "SELECT COUNT(*) from Artifact where ArtifactId = @id";
            command.Parameters.AddWithValue("id", input.Artifact.ArtifactId);
            if ((int) await command.ExecuteScalarAsync() !=0)
            {
                throw new ArtifactExistsException();
            }
            command.Parameters.Clear();
            command.CommandText = "SELECT COUNT(*) from Preservation_Project where ProjectId = @id";
            command.Parameters.AddWithValue("id", input.Project.ProjectId);
            if ((int) await command.ExecuteScalarAsync() != 0)
            {
                throw new ProjectExistsException();
            }
            
            command.Parameters.Clear();
            command.CommandText = "SELECT COUNT(*) FROM Institution where InstitutionId = @id;";
            command.Parameters.AddWithValue("id", input.Artifact.InstitutionId);
            
            if ((int) await command.ExecuteScalarAsync() != 1)
            {
                throw new InstitutionDoesntExistException();
            }

            
            command.Parameters.Clear();
            command.CommandText = "INSERT INTO Artifact VALUES (@ArtifactId, @Name, @OriginDate, @InstitutionId)";

            command.Parameters.AddWithValue("@ArtifactId", input.Artifact.ArtifactId);
            command.Parameters.AddWithValue("@Name", input.Artifact.Name);
            command.Parameters.AddWithValue("@OriginDate", input.Artifact.OriginDate);
            command.Parameters.AddWithValue("@InstitutionId", input.Artifact.InstitutionId);
            
            await command.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
            
            command.Parameters.Clear();
            command.CommandText = "INSERT INTO Preservation_Project VALUES (@ProjectId, @ArtifactId, @StartDate, @EndDate, @Objective)";

            command.Parameters.AddWithValue("@ProjectId", input.Project.ProjectId);
            command.Parameters.AddWithValue("@ArtifactId", input.Artifact.ArtifactId);
            command.Parameters.AddWithValue("@StartDate", input.Project.StartDate);
            command.Parameters.AddWithValue("@EndDate", input.Project.EndDate != null ? input.Project.EndDate : DBNull.Value);
            command.Parameters.AddWithValue("@Objective", input.Project.Objective);
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw ex;
        }

    }
}
