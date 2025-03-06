using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class PackagesService(IPackagesRepository packagesRepository, IUnitOfWork unitOfWork, IMapper mapper)
    : IPackagesService
{
    public async Task<List<PackageDto>> GetAllPackagesAsync()
    {
        var Packages = await packagesRepository.GetAllAsync();
        return mapper.Map<List<PackageDto>>(Packages);
    }


    public async Task<Packages?> GetPackageByIdAsync(int PackageId)
    {
        return await packagesRepository.GetByIdAsync(PackageId)
               ?? throw new KeyNotFoundException($"Package with ID {PackageId} not found.");
    }


    public async Task UpdatePackageAsync(PackageDto Package)
    {
        var updatePackage = await GetPackageByIdAsync(Package.PackageId);
        if (updatePackage == null)
            throw new KeyNotFoundException($"Package with ID {Package.PackageId} not found.");

        mapper.Map(Package, updatePackage);

        packagesRepository.Update(updatePackage);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to update Package.");
    }

    public async Task DeletePackageAsync(int PackageId)
    {
        var Package = await GetPackageByIdAsync(PackageId);
        if (Package == null)
            throw new KeyNotFoundException($"Package with ID {PackageId} not found.");

        await packagesRepository.RemoveAsync(Package);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to delete Package.");
    }

    public async Task<Packages?> CreatePackageAsync(PackageDto Package)
    {
        var createPackage = mapper.Map<Packages>(Package);

        await packagesRepository.CreateAsync(createPackage);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to create Package.");

        return createPackage;
    }
}