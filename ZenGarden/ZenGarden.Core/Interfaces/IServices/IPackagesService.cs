using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices
{
    public interface IPackagesService
    {
        Task<List<PackageDto>> GetAllPackagesAsync();
        Task<Packages?> GetPackageByIdAsync(int PackageId);
        Task UpdatePackageAsync(PackageDto Package);
        Task DeletePackageAsync(int PackageId);
        Task <Packages?> CreatePackageAsync(PackageDto Package);
    }
}
