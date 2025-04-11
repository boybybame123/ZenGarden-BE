// using Moq;
// using ZenGarden.Core.Interfaces.IRepositories;
// using ZenGarden.Core.Interfaces.IServices;
// using ZenGarden.Core.Services;
// using ZenGarden.Domain.Entities;
// using ZenGarden.Domain.Enums;
//
// namespace ZenGarden.Test.UnitTests;
//
// public class PurchaseServiceTests
// {
//     private readonly Mock<IBagItemRepository> _mockBagItemRepo;
//     private readonly Mock<IBagRepository> _mockBagRepo;
//     private readonly Mock<IItemDetailRepository> _mockItemDetailRepo;
//     private readonly Mock<IItemRepository> _mockItemRepo;
//     private readonly Mock<INotificationService> _mockNotificationService;
//     private readonly Mock<IPurchaseHistoryRepository> _mockPurchaseHistoryRepo;
//     private readonly Mock<IUnitOfWork> _mockUnitOfWork;
//     private readonly Mock<IWalletRepository> _mockWalletRepo;
//     private readonly PurchaseService _purchaseService;
//
//     public PurchaseServiceTests()
//     {
//         _mockWalletRepo = new Mock<IWalletRepository>();
//         _mockItemRepo = new Mock<IItemRepository>();
//         _mockBagRepo = new Mock<IBagRepository>();
//         _mockBagItemRepo = new Mock<IBagItemRepository>();
//         _mockNotificationService = new Mock<INotificationService>();
//         _mockUnitOfWork = new Mock<IUnitOfWork>();
//         _mockItemDetailRepo = new Mock<IItemDetailRepository>();
//         _mockPurchaseHistoryRepo = new Mock<IPurchaseHistoryRepository>();
//
//         _purchaseService = new PurchaseService(
//             _mockWalletRepo.Object,
//             _mockItemRepo.Object,
//             _mockBagRepo.Object,
//             _mockBagItemRepo.Object,
//             _mockNotificationService.Object,
//             _mockUnitOfWork.Object,
//             _mockItemDetailRepo.Object,
//             _mockPurchaseHistoryRepo.Object
//         );
//     }
//
//     [Fact]
//     public async Task PurchaseItem_Successful_FirstTimePurchase_ReturnsSuccessMessage()
//     {
//         // Arrange
//         var userId = 1;
//         var itemId = 1;
//         decimal itemCost = 100;
//         decimal walletBalance = 500;
//
//         var wallet = new Wallet { WalletId = 1, UserId = userId, Balance = walletBalance };
//         var item = new Item
//         {
//             ItemId = itemId,
//             Cost = itemCost,
//             Status = ItemStatus.Active,
//             ItemDetail = new ItemDetail { IsUnique = false, MonthlyPurchaseLimit = 0 }
//         };
//         var bag = new Bag { BagId = 1, UserId = userId };
//         var itemDetail = new ItemDetail { ItemDetailId = 1, ItemId = itemId, Sold = 0 };
//
//         _mockWalletRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(wallet);
//         _mockItemRepo.Setup(r => r.GetByIdAsync(itemId)).ReturnsAsync(item);
//         _mockBagRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(bag);
//         _mockBagItemRepo.Setup(r => r.GetByBagAndItemAsync(bag.BagId, itemId)).ReturnsAsync((BagItem?)null);
//         _mockItemDetailRepo.Setup(r => r.GetItemDetailsByItemId(itemId)).ReturnsAsync(itemDetail);
//         _mockUnitOfWork.Setup(u => u.CommitAsync()).ReturnsAsync(1);
//         _mockNotificationService.Setup(n => n.PushNotificationAsync(userId, "Item", "Purchase successful."))
//             .Returns(Task.CompletedTask);
//
//         // Act
//         var result = await _purchaseService.PurchaseItem(userId, itemId);
//
//         // Assert
//         Assert.Equal("Purchase successful.", result);
//         Assert.Equal(walletBalance - itemCost, wallet.Balance);
//         _mockWalletRepo.Verify(r => r.Update(wallet), Times.Once);
//         _mockPurchaseHistoryRepo.Verify(r => r.CreateAsync(It.IsAny<PurchaseHistory>()), Times.Once);
//         _mockBagItemRepo.Verify(r => r.CreateAsync(It.Is<BagItem>(bi =>
//             bi.BagId == bag.BagId &&
//             bi.ItemId == itemId &&
//             bi.Quantity == 1)), Times.Once);
//         _mockItemDetailRepo.Verify(r => r.Update(It.Is<ItemDetail>(id => id.Sold == 1)), Times.Once);
//         _mockNotificationService.Verify(n => n.PushNotificationAsync(userId, "Item", "Purchase successful."),
//             Times.Once);
//     }
//
//     /*[Fact]
//     public async Task PurchaseItem_Successful_RepeatPurchase_ReturnsSuccessMessage()
//     {
//         // Arrange
//         var userId = 1;
//         var itemId = 1;
//         decimal itemCost = 100;
//         decimal walletBalance = 500;
//         var currentQuantity = 2;
//
//         var wallet = new Wallet { WalletId = 1, UserId = userId, Balance = walletBalance };
//         var item = new Item
//         {
//             ItemId = itemId,
//             Cost = itemCost,
//             Status = ItemStatus.Active,
//             ItemDetail = new ItemDetail { IsUnique = false, MonthlyPurchaseLimit = 0 }
//         };
//         var bag = new Bag { BagId = 1, UserId = userId };
//         var bagItem = new BagItem { BagItemId = 1, BagId = bag.BagId, ItemId = itemId, Quantity = currentQuantity };
//         var itemDetail = new ItemDetail { ItemDetailId = 1, ItemId = itemId, Sold = 10 };
//
//         _mockWalletRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(wallet);
//         _mockItemRepo.Setup(r => r.GetByIdAsync(itemId)).ReturnsAsync(item);
//         _mockBagRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(bag);
//         _mockBagItemRepo.Setup(r => r.GetByBagAndItemAsync(bag.BagId, itemId)).ReturnsAsync(bagItem);
//         _mockItemDetailRepo.Setup(r => r.GetItemDetailsByItemId(itemId)).ReturnsAsync(itemDetail);
//         _mockUnitOfWork.Setup(u => u.CommitAsync()).ReturnsAsync(1);
//         _mockNotificationService.Setup(n => n.PushNotificationAsync(userId, "Item", "Purchase successful."))
//             .Returns(Task.CompletedTask);
//
//         // Act
//         var result = await _purchaseService.PurchaseItem(userId, itemId);
//
//         // Assert
//         Assert.Equal("Purchase successful.", result);
//         Assert.Equal(walletBalance - itemCost, wallet.Balance);
//         Assert.Equal(currentQuantity + 1, bagItem.Quantity);
//         _mockWalletRepo.Verify(r => r.Update(wallet), Times.Once);
//         _mockPurchaseHistoryRepo.Verify(r => r.CreateAsync(It.IsAny<PurchaseHistory>()), Times.Once);
//         _mockBagItemRepo.Verify(r => r.Update(bagItem), Times.Once);
//         _mockBagItemRepo.Verify(r => r.CreateAsync(It.IsAny<BagItem>()), Times.Never);
//         _mockItemDetailRepo.Verify(r => r.Update(It.Is<ItemDetail>(id => id.Sold == 11)), Times.Once);
//         _mockNotificationService.Verify(n => n.PushNotificationAsync(userId, "Item", "Purchase successful."),
//             Times.Once);
//     }*/
//
//     [Fact]
//     public async Task PurchaseItem_WalletNotFound_ReturnsErrorMessage()
//     {
//         // Arrange
//         var userId = 1;
//         var itemId = 1;
//
//         _mockWalletRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((Wallet?)null);
//
//         // Act
//         var result = await _purchaseService.PurchaseItem(userId, itemId);
//
//         // Assert
//         Assert.Equal("Wallet not found.", result);
//         _mockItemRepo.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
//         _mockPurchaseHistoryRepo.Verify(r => r.CreateAsync(It.IsAny<PurchaseHistory>()), Times.Never);
//     }
//
//     [Fact]
//     public async Task PurchaseItem_ItemNotFound_ReturnsErrorMessage()
//     {
//         // Arrange
//         var userId = 1;
//         var itemId = 1;
//
//         var wallet = new Wallet { WalletId = 1, UserId = userId, Balance = 500 };
//         _mockWalletRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(wallet);
//         _mockItemRepo.Setup(r => r.GetByIdAsync(itemId)).ReturnsAsync((Item?)null);
//
//         // Act
//         var result = await _purchaseService.PurchaseItem(userId, itemId);
//
//         // Assert
//         Assert.Equal("Item not found.", result);
//         _mockPurchaseHistoryRepo.Verify(r => r.CreateAsync(It.IsAny<PurchaseHistory>()), Times.Never);
//     }
//
//     [Fact]
//     public async Task PurchaseItem_ItemInactive_ReturnsErrorMessage()
//     {
//         // Arrange
//         var userId = 1;
//         var itemId = 1;
//
//         var wallet = new Wallet { WalletId = 1, UserId = userId, Balance = 500 };
//         var item = new Item { ItemId = itemId, Cost = 100, Status = ItemStatus.Inactive };
//
//         _mockWalletRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(wallet);
//         _mockItemRepo.Setup(r => r.GetByIdAsync(itemId)).ReturnsAsync(item);
//
//         // Act
//         var result = await _purchaseService.PurchaseItem(userId, itemId);
//
//         // Assert
//         Assert.Equal("Item is not available for purchase.", result);
//         _mockPurchaseHistoryRepo.Verify(r => r.CreateAsync(It.IsAny<PurchaseHistory>()), Times.Never);
//     }
//
//     [Fact]
//     public async Task PurchaseItem_InsufficientBalance_ReturnsErrorMessage()
//     {
//         // Arrange
//         var userId = 1;
//         var itemId = 1;
//         decimal itemCost = 1000;
//         decimal walletBalance = 500;
//
//         var wallet = new Wallet { WalletId = 1, UserId = userId, Balance = walletBalance };
//         var item = new Item { ItemId = itemId, Cost = itemCost, Status = ItemStatus.Active };
//
//         _mockWalletRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(wallet);
//         _mockItemRepo.Setup(r => r.GetByIdAsync(itemId)).ReturnsAsync(item);
//
//         // Act
//         var result = await _purchaseService.PurchaseItem(userId, itemId);
//
//         // Assert
//         Assert.Equal("Insufficient balance.", result);
//         Assert.Equal(walletBalance, wallet.Balance); // Balance should remain unchanged
//         _mockPurchaseHistoryRepo.Verify(r => r.CreateAsync(It.IsAny<PurchaseHistory>()), Times.Never);
//     }
//
//     [Fact]
//     public async Task PurchaseItem_MonthlyPurchaseLimitReached_ReturnsErrorMessage()
//     {
//         // Arrange
//         var userId = 1;
//         var itemId = 1;
//         var monthlyLimit = 3;
//
//         var wallet = new Wallet { WalletId = 1, UserId = userId, Balance = 500 };
//         var item = new Item
//         {
//             ItemId = itemId,
//             Cost = 100,
//             Status = ItemStatus.Active,
//             ItemDetail = new ItemDetail { MonthlyPurchaseLimit = monthlyLimit, IsUnique = false }
//         };
//
//         var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
//
//         _mockWalletRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(wallet);
//         _mockItemRepo.Setup(r => r.GetByIdAsync(itemId)).ReturnsAsync(item);
//         _mockPurchaseHistoryRepo.Setup(r => r.CountPurchaseThisMonth(userId, itemId, startOfMonth))
//             .ReturnsAsync(monthlyLimit);
//
//         // Act
//         var result = await _purchaseService.PurchaseItem(userId, itemId);
//
//         // Assert
//         Assert.Equal("Purchase limit for this month reached.", result);
//         Assert.Equal(500, wallet.Balance); // Balance should remain unchanged
//         _mockPurchaseHistoryRepo.Verify(r => r.CreateAsync(It.IsAny<PurchaseHistory>()), Times.Never);
//     }
//
//     [Fact]
//     public async Task PurchaseItem_UniqueItemAlreadyOwned_ReturnsErrorMessage()
//     {
//         // Arrange
//         var userId = 1;
//         var itemId = 1;
//         var bagId = 1;
//
//         var wallet = new Wallet { WalletId = 1, UserId = userId, Balance = 500 };
//         var item = new Item
//         {
//             ItemId = itemId,
//             Cost = 100,
//             Status = ItemStatus.Active,
//             ItemDetail = new ItemDetail { IsUnique = true }
//         };
//         var bagItem = new BagItem { BagItemId = 1, BagId = bagId, ItemId = itemId, Quantity = 1 };
//
//         _mockWalletRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(wallet);
//         _mockItemRepo.Setup(r => r.GetByIdAsync(itemId)).ReturnsAsync(item);
//         _mockBagItemRepo.Setup(r => r.GetByBagAndItemAsync(userId, itemId)).ReturnsAsync(bagItem);
//
//         // Act
//         var result = await _purchaseService.PurchaseItem(userId, itemId);
//
//         // Assert
//         Assert.Equal("Item is unique and can only be purchased once.", result);
//         Assert.Equal(500, wallet.Balance); // Balance should remain unchanged
//         _mockPurchaseHistoryRepo.Verify(r => r.CreateAsync(It.IsAny<PurchaseHistory>()), Times.Never);
//     }
//
//     /*[Fact]
//     public async Task PurchaseItem_BagNotFound_ReturnsErrorMessage()
//     {
//         // Arrange
//         var userId = 1;
//         var itemId = 1;
//
//         var wallet = new Wallet { WalletId = 1, UserId = userId, Balance = 500 };
//         var item = new Item
//         {
//             ItemId = itemId,
//             Cost = 100,
//             Status = ItemStatus.Active,
//             ItemDetail = new ItemDetail { IsUnique = false, MonthlyPurchaseLimit = 0 }
//         };
//
//         _mockWalletRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(wallet);
//         _mockItemRepo.Setup(r => r.GetByIdAsync(itemId)).ReturnsAsync(item);
//         _mockBagRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((Bag)null);
//         _mockUnitOfWork.Setup(u => u.CommitAsync()).ReturnsAsync(1);
//
//         // Act
//         var result = await _purchaseService.PurchaseItem(userId, itemId);
//
//         // Assert
//         Assert.Equal("Bag not found.", result);
//         _mockWalletRepo.Verify(r => r.Update(wallet), Times.Once); // Wallet has been updated
//         _mockPurchaseHistoryRepo.Verify(r => r.CreateAsync(It.IsAny<PurchaseHistory>()),
//             Times.Once); // Purchase history has been created
//         _mockBagItemRepo.Verify(r => r.CreateAsync(It.IsAny<BagItem>()), Times.Never); // BagItem was not created
//     }*/
//
//     /*[Fact]
//     public async Task PurchaseItem_ExceptionThrown_ReturnsErrorMessage()
//     {
//         // Arrange
//         var userId = 1;
//         var itemId = 1;
//         var exceptionMessage = "Database connection error";
//
//         _mockWalletRepo.Setup(r => r.GetByUserIdAsync(userId)).ThrowsAsync(new Exception(exceptionMessage));
//
//         // Act
//         var result = await _purchaseService.PurchaseItem(userId, itemId);
//
//         // Assert
//         Assert.Equal($"Purchase failed: {exceptionMessage}", result);
//         _mockPurchaseHistoryRepo.Verify(r => r.CreateAsync(It.IsAny<PurchaseHistory>()), Times.Never);
//     }*/
// }