using AutoMapper;

using DefaultProjects.Shared.Constants;
using DefaultProjects.Shared.DTOs;
using DefaultProjects.Shared.Interfaces;
using DefaultProjects.Shared.Models;
using DefaultProjects.Shared.Options;

using FluentInjections.Validation;

using Microsoft.Extensions.Logging;

namespace DefaultProjects.Microservices.TenantManagementServices.Services
{
    internal class TenantService : ITenantService
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IUserManagementService _userManagementService;
        private readonly IMapper _mapper;
        private readonly ILogger<TenantService> _logger;

        public TenantService(ITenantRepository tenantRepository, IUserManagementService userManagementService, IMapper mapper, ILogger<TenantService> logger)
        {
            _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
            _userManagementService = userManagementService ?? throw new ArgumentNullException(nameof(userManagementService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Tenant> CreateAsync(TenantCreationDTO tenantDto, CancellationToken cancellationToken)
        {
            Guard.NotNull(tenantDto, nameof(tenantDto));
            Guard.NotNull(cancellationToken, nameof(cancellationToken));

            var tenant = _mapper.Map<Tenant>(tenantDto);

            await _tenantRepository.CreateAsync(tenant, cancellationToken);

            // Create initial tenant admin account
            var adminUser = new UserDTO(tenantDto.AdminEmail, tenantDto.AdminPassword, Tenants.Roles.TenantAdmin, tenant.TenantId);

            if (!await _userManagementService.CreateUserAsync(adminUser, cancellationToken))
            {
                throw new InvalidOperationException("Failed to create tenant admin account");
            }

            return tenant;
        }

        public async Task<Tenant?> GetAsync(string tenantId, CancellationToken cancellationToken)
        {
            Guard.NotNullOrEmpty(tenantId, nameof(tenantId));
            Guard.NotNull(cancellationToken, nameof(cancellationToken));

            return await _tenantRepository.TryGetAsync(
                tenantId,
                RepositoryOptions.Default,
                cancellationToken);
        }

        public async Task<Tenant?> UpdateAsync(string tenantId, TenantUpdateDTO tenantDto, CancellationToken cancellationToken)
        {
            Guard.NotNullOrEmpty(tenantId, nameof(tenantId));
            Guard.NotNull(tenantDto, nameof(tenantDto));
            Guard.NotNull(cancellationToken, nameof(cancellationToken));

            var tenant = await _tenantRepository.TryGetAsync(
                tenantId,
                RepositoryOptions.Default with { UseAsTracking = true },
                cancellationToken);

            if (tenant is null)
            {
                _logger.LogWarning("[Update] Tenant {TenantId} not found", tenantId);
                return null;
            }

            tenant.CompanyName = tenantDto.CompanyName;
            tenant.Plan = tenantDto.Plan;
            await _tenantRepository.UpdateAsync(tenant, cancellationToken);

            return tenant;
        }

        public async Task DeleteAsync(string tenantId, CancellationToken cancellationToken)
        {
            Guard.NotNullOrEmpty(tenantId, nameof(tenantId));
            Guard.NotNull(cancellationToken, nameof(cancellationToken));

            await _tenantRepository.DeleteAsync(tenantId, cancellationToken);
        }
    }
}