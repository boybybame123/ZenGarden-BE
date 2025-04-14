//using AutoMapper;
//using Moq;
//using ZenGarden.Core.Interfaces.IRepositories;
//using ZenGarden.Core.Services;
//using ZenGarden.Domain.DTOs;
//using ZenGarden.Domain.Entities;
//using ZenGarden.Domain.Enums;

//namespace ZenGarden.Tests.Services;

//public class ItemServiceTests
//{
//    private readonly ItemService _itemService;
//    private readonly Mock<IItemRepository> _mockItemRepository;
//    private readonly Mock<IMapper> _mockMapper;
//    private readonly Mock<IUnitOfWork> _mockUnitOfWork;

//    public ItemServiceTests()
//    {
//        _mockItemRepository = new Mock<IItemRepository>();
//        _mockUnitOfWork = new Mock<IUnitOfWork>();
//        _mockMapper = new Mock<IMapper>();
//        _itemService = new ItemService(
//            _mockItemRepository.Object,
//            _mockUnitOfWork.Object,
//            _mockMapper.Object
//        );
//    }

//    #region GetAllItemsAsync Tests

//    [Fact]
//    public async Task GetAllItemsAsync_ShouldReturnAllItems()
//    {
//        // Arrange
//        var items = new List<Item>
//        {
//            new() { ItemId = 1, Name = "Item 1", Type = ItemType.Background, Rarity = "Common" },
//            new() { ItemId = 2, Name = "Item 2", Type = ItemType.Background, Rarity = "Rare" }
//        };

//        var itemDtos = new List<ItemDto>
//        {
//            new() { ItemId = 1, Name = "Item 1", Type = ItemType.Background, Rarity = "Common" },
//            new() { ItemId = 2, Name = "Item 2", Type = ItemType.Avatar, Rarity = "Rare" }
//        };

//        _mockItemRepository.Setup(repo => repo.GetAllItemAsync()).ReturnsAsync(items);
//        _mockMapper.Setup(mapper => mapper.Map<List<ItemDto>>(items)).Returns(itemDtos);

//        // Act
//        var result = await _itemService.GetAllItemsAsync();

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(2, result.Count);
//        _mockItemRepository.Verify(repo => repo.GetAllItemAsync(), Times.Once);
//        _mockMapper.Verify(mapper => mapper.Map<List<ItemDto>>(items), Times.Exactly(2)); // Called twice in the method
//    }

//    [Fact]
//    public async Task GetAllItemsAsync_WhenMappingFails_ShouldThrowException()
//    {
//        // Arrange
//        var items = new List<Item>
//        {
//            new() { ItemId = 1, Name = "Item 1" }
//        };

//        _mockItemRepository.Setup(repo => repo.GetAllItemAsync()).ReturnsAsync(items);
//        _mockMapper.Setup(mapper => mapper.Map<List<ItemDto>>(items)).Throws<AutoMapperMappingException>();

//        // Act & Assert
//        await Assert.ThrowsAsync<AutoMapperMappingException>(() => _itemService.GetAllItemsAsync());
//    }

//    #endregion

//    #region GetItemByIdAsync Tests

//    [Fact]
//    public async Task GetItemByIdAsync_WithValidId_ShouldReturnItem()
//    {
//        // Arrange
//        var itemId = 1;
//        var item = new Item { ItemId = itemId, Name = "Test Item" };
//        _mockItemRepository.Setup(repo => repo.GetItemByIdAsync(itemId)).ReturnsAsync(item);

//        // Act
//        var result = await _itemService.GetItemByIdAsync(itemId);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(itemId, result.ItemId);
//        Assert.Equal("Test Item", result.Name);
//        _mockItemRepository.Verify(repo => repo.GetItemByIdAsync(itemId), Times.Once);
//    }

//    [Fact]
//    public async Task GetItemByIdAsync_WithInvalidId_ShouldThrowKeyNotFoundException()
//    {
//        // Arrange
//        var itemId = 999;
//        _mockItemRepository.Setup(repo => repo.GetItemByIdAsync(itemId)).ReturnsAsync((Item?)null);

//        // Act & Assert
//        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _itemService.GetItemByIdAsync(itemId));
//        Assert.Equal($"Item with ID {itemId} not found.", exception.Message);
//        _mockItemRepository.Verify(repo => repo.GetItemByIdAsync(itemId), Times.Once);
//    }

//    #endregion

//    #region CreateItemAsync Tests

//    [Fact]
//    public async Task CreateItemAsync_WithValidData_ShouldCreateItem()
//    {
//        // Arrange
//        var createItemDto = new CreateItemDto
//        {
//            Name = "New Item",
//            Type = ItemType.Background,
//            Rarity = "Rare",
//            Cost = 100
//        };

//        var item = new Item
//        {
//            Name = "New Item",
//            Type = ItemType.Background,
//            Rarity = "Rare",
//            Cost = 100
//        };

//        _mockMapper.Setup(mapper => mapper.Map<Item>(createItemDto)).Returns(item);
//        _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

//        // Act
//        await _itemService.CreateItemAsync(createItemDto);

//        // Assert
//        _mockItemRepository.Verify(repo => repo.CreateAsync(item), Times.Once);
//        _mockUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Once);
//    }

//    [Fact]
//    public async Task CreateItemAsync_WhenCommitFails_ShouldThrowInvalidOperationException()
//    {
//        // Arrange
//        var createItemDto = new CreateItemDto
//        {
//            Name = "New Item",
//            Type = ItemType.Background,
//            Rarity = "Rare"
//        };

//        var item = new Item
//        {
//            Name = "New Item",
//            Type = ItemType.Background,
//            Rarity = "Rare"
//        };

//        _mockMapper.Setup(mapper => mapper.Map<Item>(createItemDto)).Returns(item);
//        _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(0);

//        // Act & Assert
//        var exception =
//            await Assert.ThrowsAsync<InvalidOperationException>(() => _itemService.CreateItemAsync(createItemDto));
//        Assert.Equal("Failed to create item.", exception.Message);
//    }

//    #endregion

//    #region UpdateItemAsync Tests

//    [Fact]
//    public async Task UpdateItemAsync_WithValidData_ShouldUpdateItem()
//    {
//        // Arrange
//        var itemId = 1;
//        var updateItemDto = new UpdateItemDto
//        {
//            ItemId = itemId,
//            Name = "Updated Item",
//            Type = ItemType.Music,
//            Rarity = "Epic",
//            Cost = 200,
//            Status = ItemStatus.Active
//        };

//        var existingItem = new Item
//        {
//            ItemId = itemId,
//            Name = "Original Item",
//            Type = ItemType.Background,
//            Rarity = "Common",
//            Cost = 100,
//            Status = ItemStatus.Inactive
//        };

//        _mockItemRepository.Setup(repo => repo.GetItemByIdAsync(itemId)).ReturnsAsync(existingItem);
//        _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

//        // Act
//        await _itemService.UpdateItemAsync(updateItemDto);

//        // Assert
//        Assert.Equal("Updated Item", existingItem.Name);
//        Assert.Equal(ItemType.Music, existingItem.Type);
//        Assert.Equal("Epic", existingItem.Rarity);
//        Assert.Equal(200, existingItem.Cost);
//        Assert.Equal(ItemStatus.Active, existingItem.Status);

//        _mockItemRepository.Verify(repo => repo.Update(existingItem), Times.Once);
//        _mockUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Once);
//    }

//    [Fact]
//    public async Task UpdateItemAsync_WithNonExistentItem_ShouldThrowKeyNotFoundException()
//    {
//        // Arrange
//        var itemId = 999;
//        var updateItemDto = new UpdateItemDto
//        {
//            ItemId = itemId,
//            Name = "Updated Item"
//        };

//        _mockItemRepository.Setup(repo => repo.GetItemByIdAsync(itemId)).ReturnsAsync((Item?)null);

//        // Act & Assert
//        var exception =
//            await Assert.ThrowsAsync<KeyNotFoundException>(() => _itemService.UpdateItemAsync(updateItemDto));
//        Assert.Equal($"Item with ID {itemId} not found.", exception.Message);
//    }

//    [Fact]
//    public async Task UpdateItemAsync_WhenCommitFails_ShouldThrowInvalidOperationException()
//    {
//        // Arrange
//        var itemId = 1;
//        var updateItemDto = new UpdateItemDto
//        {
//            ItemId = itemId,
//            Name = "Updated Item"
//        };

//        var existingItem = new Item
//        {
//            ItemId = itemId,
//            Name = "Original Item"
//        };

//        _mockItemRepository.Setup(repo => repo.GetItemByIdAsync(itemId)).ReturnsAsync(existingItem);
//        _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(0);

//        // Act & Assert
//        var exception =
//            await Assert.ThrowsAsync<InvalidOperationException>(() => _itemService.UpdateItemAsync(updateItemDto));
//        Assert.Equal("Failed to update item.", exception.Message);
//    }

//    #endregion

//    #region ActiveItem Tests

//    [Fact]
//    public async Task ActiveItem_WithValidId_ShouldActivateItem()
//    {
//        // Arrange
//        var itemId = 1;
//        var item = new Item { ItemId = itemId, Status = ItemStatus.Inactive };

//        _mockItemRepository.Setup(repo => repo.GetItemByIdAsync(itemId)).ReturnsAsync(item);
//        _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

//        // Act
//        await _itemService.ActiveItem(itemId);

//        // Assert
//        Assert.Equal(ItemStatus.Active, item.Status);
//        _mockItemRepository.Verify(repo => repo.Update(item), Times.Once);
//        _mockUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Once);
//    }

//    [Fact]
//    public async Task ActiveItem_WithNonExistentItem_ShouldThrowKeyNotFoundException()
//    {
//        // Arrange
//        var itemId = 999;
//        _mockItemRepository.Setup(repo => repo.GetItemByIdAsync(itemId)).ReturnsAsync((Item?)null);

//        // Act & Assert
//        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _itemService.ActiveItem(itemId));
//        Assert.Equal($"Item with ID {itemId} not found.", exception.Message);
//    }

//    [Fact]
//    public async Task ActiveItem_WhenCommitFails_ShouldThrowInvalidOperationException()
//    {
//        // Arrange
//        var itemId = 1;
//        var item = new Item { ItemId = itemId, Status = ItemStatus.Inactive };

//        _mockItemRepository.Setup(repo => repo.GetItemByIdAsync(itemId)).ReturnsAsync(item);
//        _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(0);

//        // Act & Assert
//        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _itemService.ActiveItem(itemId));
//        Assert.Equal("Failed to delete item.", exception.Message);
//    }

//    #endregion

//    #region DeleteItemAsync Tests

//    [Fact]
//    public async Task DeleteItemAsync_WithValidId_ShouldMarkItemAsInactive()
//    {
//        // Arrange
//        var itemId = 1;
//        var item = new Item { ItemId = itemId, Status = ItemStatus.Active };

//        _mockItemRepository.Setup(repo => repo.GetItemByIdAsync(itemId)).ReturnsAsync(item);
//        _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

//        // Act
//        await _itemService.DeleteItemAsync(itemId);

//        // Assert
//        Assert.Equal(ItemStatus.Inactive, item.Status);
//        _mockItemRepository.Verify(repo => repo.Update(item), Times.Once);
//        _mockUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Once);
//    }

//    [Fact]
//    public async Task DeleteItemAsync_WithNonExistentItem_ShouldThrowKeyNotFoundException()
//    {
//        // Arrange
//        var itemId = 999;
//        _mockItemRepository.Setup(repo => repo.GetItemByIdAsync(itemId)).ReturnsAsync((Item?)null);

//        // Act & Assert
//        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _itemService.DeleteItemAsync(itemId));
//        Assert.Equal($"Item with ID {itemId} not found.", exception.Message);
//    }

//    [Fact]
//    public async Task DeleteItemAsync_WhenCommitFails_ShouldThrowInvalidOperationException()
//    {
//        // Arrange
//        var itemId = 1;
//        var item = new Item { ItemId = itemId, Status = ItemStatus.Active };

//        _mockItemRepository.Setup(repo => repo.GetItemByIdAsync(itemId)).ReturnsAsync(item);
//        _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(0);

//        // Act & Assert
//        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _itemService.DeleteItemAsync(itemId));
//        Assert.Equal("Failed to delete item.", exception.Message);
//    }

//    #endregion
//}

