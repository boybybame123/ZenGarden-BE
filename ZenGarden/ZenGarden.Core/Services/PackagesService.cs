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
        var packages = await packagesRepository.GetAllAsync();
        return mapper.Map<List<PackageDto>>(packages);
    }


    public async Task<Packages?> GetPackageByIdAsync(int packageId)
    {
        
        return await packagesRepository.GetByIdAsync(packageId)
               ?? throw new KeyNotFoundException($"Package with ID {packageId} not found.");
    }


    public async Task UpdatePackageAsync(PackageDto package)
    {
        var updatePackage = await GetPackageByIdAsync(package.PackageId);
        if (updatePackage == null)
            throw new KeyNotFoundException($"Package with ID {package.PackageId} not found.");

        mapper.Map(package, updatePackage);
        updatePackage.UpdatedAt = DateTime.Now;
        packagesRepository.Update(updatePackage);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to update Package.");
    }

    public async Task DeletePackageAsync(int packageId)
    {
        var package = await GetPackageByIdAsync(packageId);
        if (package == null)
            throw new KeyNotFoundException($"Package with ID {packageId} not found.");

        await packagesRepository.RemoveAsync(package);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to delete Package.");
    }

    public async Task<Packages?> CreatePackageAsync(PackageDto package)
    {
        var createPackage = mapper.Map<Packages>(package);
        createPackage.CreatedAt = DateTime.Now;
        createPackage.UpdatedAt = DateTime.Now;
        await packagesRepository.CreateAsync(createPackage);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to create Package.");

        return createPackage;
    }
}