﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Poprawa1.Models;

public class Staff
{
    public int StaffId { get; set; }
    public String FirstName { get; set; }
    public String LastName { get; set; }
    public DateTime HireDate { get; set; }
}

public class Staff_Assignment
{
    public int StaffId { get; set; }
    public int ProjectId { get; set; }
    public String Role { get; set; }
}

public class Artifact
{
    public int ArtifactId { get; set; }
    public int InsitutionId { get; set; }
    public String Name { get; set; }
    public DateTime OriginDate { get; set; }
}

public class Preservation_Project
{
    public int ProjectId { get; set; }
    public int ArtifactId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public String Objective { get; set; }
}

public class Institution
{
    public int InsitutionId { get; set; }
    public String Name { get; set; }
    public int FoundedYear { get; set; }
}

public class InstitutionDTO
{
    public int InsitutionId { get; set; }
    public String Name { get; set; }
    public int FoundedYear { get; set; }
}

public class ProjectDTO
{
    public int ProjectId { get; set; }
    public String Objective { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public List<ArtifactDTO> Artifacts { get; set; } = new List<ArtifactDTO>();
    public List<StaffAssignmentDTO> StaffAssignments { get; set; } = new List<StaffAssignmentDTO>();
}

public class ArtifactDTO
{
    public String Name { get; set; }
    public DateTime OriginDate { get; set; }
    public InstitutionDTO Institution { get; set; }
}

public class StaffAssignmentDTO
{
    public String Role { get; set; }
    public String FirstName { get; set; }
    public String LastName { get; set; }
    public DateTime HireDate { get; set; }
}

public class InputProjectDTO
{
    public int ProjectId { get; set; }
    public String Objective { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class InputArtifactDTO
{
    public int ArtifactId { get; set; }
    public String Name { get; set; }
    public DateTime OriginDate { get; set; }
    public int InstitutionId { get; set; }
}

public class InputDTO
{
    public InputArtifactDTO Artifact { get; set; }
    public InputProjectDTO Project { get; set; }
}