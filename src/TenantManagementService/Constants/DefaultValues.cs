namespace DefaultProjects.Shared.Constants;

public class Tenants
{
    public class Roles
    {
        public const string TenantAdmin = nameof(TenantAdmin);
    }

    public class Routes
    {
        public const string CreateEndpoint = "/api/tenants";
        public const string GetEndpoint = "/api/tenants/{tenantId}";
        public const string UpdateEndpoint = "/api/tenants/{tenantId}";
        public const string DeleteEndpoint = "/api/tenants/{tenantId}";
    }

    public class Messages
    {
        public const string NotFound = "Tenant not found";
        public const string AlreadyExists = "Tenant already exists";
        public const string Created = "Tenant created successfully";
        public const string Updated = "Tenant updated";
        public const string Deleted = "Tenant deleted";
        public const string TenantIdMissing = "Tenant Id is required.";
        public const string TenantDataRequired = "Tenant data is required in the request body.";
        public const string FailedToCreate = "Failed to create tenant.";
        public const string FailedToUpdate = "Failed to update tenant.";
        public const string FailedToDelete = "Failed to delete tenant.";
    }
}
