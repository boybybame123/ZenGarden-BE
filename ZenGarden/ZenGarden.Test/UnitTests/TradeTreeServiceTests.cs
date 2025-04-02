//using Moq;
//using System;
//using System.Threading.Tasks;
//using Xunit;
//using ZenGarden.Core.Interfaces.IRepositories;
//using ZenGarden.Core.Interfaces.IServices;
//using ZenGarden.Core.Services;
//using ZenGarden.Domain.DTOs;
//using ZenGarden.Domain.Entities;
//using ZenGarden.Domain.Enums;

//namespace ZenGarden.Tests.Services
//{
//    public class TradeTreeServiceTests
//    {
//        private readonly Mock<ITradeHistoryService> _mockTradeHistoryService;
//        private readonly Mock<ITreeRepository> _mockTreeRepository;
//        private readonly Mock<IUserTreeRepository> _mockUserTreeRepository;
//        private readonly TradeTreeService _tradeTreeService;

//        public TradeTreeServiceTests()
//        {
//            _mockTradeHistoryService = new Mock<ITradeHistoryService>();
//            _mockTreeRepository = new Mock<ITreeRepository>();
//            _mockUserTreeRepository = new Mock<IUserTreeRepository>();
//            _tradeTreeService = new TradeTreeService(
//                _mockTradeHistoryService.Object,
//                _mockTreeRepository.Object,
//                _mockUserTreeRepository.Object
//            );
//        }

//        #region CreateTradeRequestAsync Tests

//        [Fact]
//        public async Task CreateTradeRequestAsync_Success_SameRarity()
//        {
//            // Arrange
//            var tradeDto = new TradeDto
//            {
//                requesterId = 1,
//                requesterTreeId = 10,
//                requestDesiredTreeId = 20
//            };

//            var userTree = new UserTree
//            {
//                UserTreeId = 10,
//                TreeOwnerId = 1,
//                FinalTreeId = 15,
//                FinalTree = new Tree { TreeId = 15, Rarity = "Rare", IsActive = true },
//                Name = "Test Tree",
//                TreeXpConfig = new TreeXpConfig(),
//                User = new Users(),
//                TreeOwner = new Users()
//            };

//            var desiredTree = new Tree
//            {
//                TreeId = 20,
//                Rarity = "Rare",
//                IsActive = true
//            };

//            _mockUserTreeRepository.Setup(repo => repo.GetByIdAsync(tradeDto.requesterTreeId))
//                .ReturnsAsync(userTree);

//            _mockTreeRepository.Setup(repo => repo.GetByIdAsync(tradeDto.requestDesiredTreeId))
//                .ReturnsAsync(desiredTree);

//            _mockTradeHistoryService.Setup(service => service.CreateTradeHistoryAsync(It.IsAny<TradeHistory>()))
//                .Returns((Task<TradeHistory>)Task.CompletedTask);

//            // Act
//            var result = await _tradeTreeService.CreateTradeRequestAsync(tradeDto);

//            // Assert
//            Assert.Equal("Trade request created successfully, waiting for recipient", result);

//            _mockTradeHistoryService.Verify(service => service.CreateTradeHistoryAsync(
//                It.Is<TradeHistory>(th =>
//                    th.TreeOwnerAid == tradeDto.requesterId &&
//                    th.TreeAid == tradeDto.requesterTreeId &&
//                    th.DesiredTreeAID == tradeDto.requestDesiredTreeId &&
//                    th.Status == TradeStatus.Pending &&
//                    th.TradeFee == 100)), // Rare rarity fee
//                Times.Once);
//        }

//        [Fact]
//        public async Task CreateTradeRequestAsync_TreeDoesNotExist_ReturnsErrorMessage()
//        {
//            // Arrange
//            var tradeDto = new TradeDto
//            {
//                requesterId = 1,
//                requesterTreeId = 10,
//                requestDesiredTreeId = 20
//            };

//            _mockUserTreeRepository.Setup(repo => repo.GetByIdAsync(tradeDto.requesterTreeId))
//                .ReturnsAsync((UserTree)null);

//            // Act
//            var result = await _tradeTreeService.CreateTradeRequestAsync(tradeDto);

//            // Assert
//            Assert.Equal("Tree does not exist", result);
//            _mockTradeHistoryService.Verify(service => service.CreateTradeHistoryAsync(It.IsAny<TradeHistory>()), Times.Never);
//        }

//        [Fact]
//        public async Task CreateTradeRequestAsync_TreeDoesNotBelongToUser_ReturnsErrorMessage()
//        {
//            // Arrange
//            var tradeDto = new TradeDto
//            {
//                requesterId = 1,
//                requesterTreeId = 10,
//                requestDesiredTreeId = 20
//            };

//            var userTree = new UserTree
//            {
//                UserTreeId = 10,
//                TreeOwnerId = 2, // Different owner than requester
//                FinalTreeId = 15,
//                Name = "Test Tree",
//                TreeXpConfig = new TreeXpConfig(),
//                User = new Users(),
//                TreeOwner = new Users()
//            };

//            _mockUserTreeRepository.Setup(repo => repo.GetByIdAsync(tradeDto.requesterTreeId))
//                .ReturnsAsync(userTree);

//            // Act
//            var result = await _tradeTreeService.CreateTradeRequestAsync(tradeDto);

//            // Assert
//            Assert.Equal("Tree does not belong to you", result);
//            _mockTradeHistoryService.Verify(service => service.CreateTradeHistoryAsync(It.IsAny<TradeHistory>()), Times.Never);
//        }

//        [Fact]
//        public async Task CreateTradeRequestAsync_TreeNotFinal_ReturnsErrorMessage()
//        {
//            // Arrange
//            var tradeDto = new TradeDto
//            {
//                requesterId = 1,
//                requesterTreeId = 10,
//                requestDesiredTreeId = 20
//            };

//            var userTree = new UserTree
//            {
//                UserTreeId = 10,
//                TreeOwnerId = 1,
//                FinalTreeId = null // Tree is not final
//            };

//            _mockUserTreeRepository.Setup(repo => repo.GetByIdAsync(tradeDto.requesterTreeId))
//                .ReturnsAsync(userTree);

//            // Act
//            var result = await _tradeTreeService.CreateTradeRequestAsync(tradeDto);

//            // Assert
//            Assert.Equal("Tree is not final", result);
//            _mockTradeHistoryService.Verify(service => service.CreateTradeHistoryAsync(It.IsAny<TradeHistory>()), Times.Never);
//        }

//        [Fact]
//        public async Task CreateTradeRequestAsync_DesiredTreeDoesNotExist_ReturnsErrorMessage()
//        {
//            // Arrange
//            var tradeDto = new TradeDto
//            {
//                requesterId = 1,
//                requesterTreeId = 10,
//                requestDesiredTreeId = 20
//            };

//            var userTree = new UserTree
//            {
//                UserTreeId = 10,
//                TreeOwnerId = 1,
//                FinalTreeId = 15,
//                FinalTree = new Tree { TreeId = 15, Rarity = "Rare" }
//            };

//            _mockUserTreeRepository.Setup(repo => repo.GetByIdAsync(tradeDto.requesterTreeId))
//                .ReturnsAsync(userTree);

//            _mockTreeRepository.Setup(repo => repo.GetByIdAsync(tradeDto.requestDesiredTreeId))
//                .ReturnsAsync((Tree)null);

//            // Act
//            var result = await _tradeTreeService.CreateTradeRequestAsync(tradeDto);

//            // Assert
//            Assert.Equal("The tree you want to trade does not exist", result);
//            _mockTradeHistoryService.Verify(service => service.CreateTradeHistoryAsync(It.IsAny<TradeHistory>()), Times.Never);
//        }

//        [Fact]
//        public async Task CreateTradeRequestAsync_DesiredTreeInactive_ReturnsErrorMessage()
//        {
//            // Arrange
//            var tradeDto = new TradeDto
//            {
//                requesterId = 1,
//                requesterTreeId = 10,
//                requestDesiredTreeId = 20
//            };

//            var userTree = new UserTree
//            {
//                UserTreeId = 10,
//                TreeOwnerId = 1,
//                FinalTreeId = 15,
//                FinalTree = new Tree { TreeId = 15, Rarity = "Rare" }
//            };

//            var desiredTree = new Tree
//            {
//                TreeId = 20,
//                Rarity = "Rare",
//                IsActive = false // Tree is inactive
//            };

//            _mockUserTreeRepository.Setup(repo => repo.GetByIdAsync(tradeDto.requesterTreeId))
//                .ReturnsAsync(userTree);

//            _mockTreeRepository.Setup(repo => repo.GetByIdAsync(tradeDto.requestDesiredTreeId))
//                .ReturnsAsync(desiredTree);

//            // Act
//            var result = await _tradeTreeService.CreateTradeRequestAsync(tradeDto);

//            // Assert
//            Assert.Equal("The tree you want to trade has been deactivated", result);
//            _mockTradeHistoryService.Verify(service => service.CreateTradeHistoryAsync(It.IsAny<TradeHistory>()), Times.Never);
//        }

//        [Fact]
//        public async Task CreateTradeRequestAsync_RarityMismatch_ReturnsErrorMessage()
//        {
//            // Arrange
//            var tradeDto = new TradeDto
//            {
//                requesterId = 1,
//                requesterTreeId = 10,
//                requestDesiredTreeId = 20
//            };

//            var userTree = new UserTree
//            {
//                UserTreeId = 10,
//                TreeOwnerId = 1,
//                FinalTreeId = 15,
//                FinalTree = new Tree { TreeId = 15, Rarity = "Rare" } // Rare rarity
//            };

//            var desiredTree = new Tree
//            {
//                TreeId = 20,
//                Rarity = "Super Rare", // Different rarity
//                IsActive = true
//            };

//            _mockUserTreeRepository.Setup(repo => repo.GetByIdAsync(tradeDto.requesterTreeId))
//                .ReturnsAsync(userTree);

//            _mockTreeRepository.Setup(repo => repo.GetByIdAsync(tradeDto.requestDesiredTreeId))
//                .ReturnsAsync(desiredTree);

//            // Act
//            var result = await _tradeTreeService.CreateTradeRequestAsync(tradeDto);

//            // Assert
//            Assert.Equal("Rarity does not match", result);
//            _mockTradeHistoryService.Verify(service => service.CreateTradeHistoryAsync(It.IsAny<TradeHistory>()), Times.Never);
//        }

//        [Theory]
//        [InlineData("Common", 50)]
//        [InlineData("Rare", 100)]
//        [InlineData("Super Rare", 200)]
//        [InlineData("Ultra Rare", 300)]
//        public async Task CreateTradeRequestAsync_CorrectTradeFee_BasedOnRarity(string rarity, decimal expectedFee)
//        {
//            // Arrange
//            var tradeDto = new TradeDto
//            {
//                requesterId = 1,
//                requesterTreeId = 10,
//                requestDesiredTreeId = 20
//            };

//            var userTree = new UserTree
//            {
//                UserTreeId = 10,
//                TreeOwnerId = 1,
//                FinalTreeId = 15,
//                FinalTree = new Tree { TreeId = 15, Rarity = rarity }
//            };

//            var desiredTree = new Tree
//            {
//                TreeId = 20,
//                Rarity = rarity,
//                IsActive = true
//            };

//            _mockUserTreeRepository.Setup(repo => repo.GetByIdAsync(tradeDto.requesterTreeId))
//                .ReturnsAsync(userTree);

//            _mockTreeRepository.Setup(repo => repo.GetByIdAsync(tradeDto.requestDesiredTreeId))
//                .ReturnsAsync(desiredTree);

//            _mockTradeHistoryService.Setup(service => service.CreateTradeHistoryAsync(It.IsAny<TradeHistory>()))
//                .Returns(Task.CompletedTask);

//            // Act
//            var result = await _tradeTreeService.CreateTradeRequestAsync(tradeDto);

//            // Assert
//            Assert.Equal("Trade request created successfully, waiting for recipient", result);

//            _mockTradeHistoryService.Verify(service => service.CreateTradeHistoryAsync(
//                It.Is<TradeHistory>(th => th.TradeFee == expectedFee)),
//                Times.Once);
//        }

//        #endregion

//        #region AcceptTradeAsync Tests

//        [Fact]
//        public async Task AcceptTradeAsync_Success()
//        {
//            // Arrange
//            int tradeId = 1;
//            int userBId = 2;
//            int userTreeBId = 20;

//            var trade = new TradeHistory
//            {
//                TradeId = tradeId,
//                TreeOwnerAid = 1,
//                TreeAid = 10,
//                DesiredTreeAID = 15,
//                Status = TradeStatus.Pending
//            };

//            var userTreeB = new UserTree
//            {
//                UserTreeId = userTreeBId,
//                TreeOwnerId = userBId,
//                FinalTreeId = 15,
//                FinalTree = new Tree { TreeId = 15 }
//            };

//            var requesterTree = new UserTree
//            {
//                UserTreeId = 10,
//                TreeOwnerId = 1
//            };

//            _mockTradeHistoryService.Setup(service => service.GetTradeHistoryByIdAsync(tradeId))
//                .ReturnsAsync(trade);

//            _mockUserTreeRepository.Setup(repo => repo.GetByIdAsync(userTreeBId))
//                .ReturnsAsync(userTreeB);

//            _mockUserTreeRepository.Setup(repo => repo.GetUserTreeByTreeIdAndOwnerIdAsync(trade.TreeAid.Value, trade.TreeOwnerAid.Value))
//                .ReturnsAsync(requesterTree);

//            _mockTradeHistoryService.Setup(service => service.UpdateTradeHistoryAsync(It.IsAny<TradeHistory>()))
//                .Returns(Task.CompletedTask);

//            // Act
//            var result = await _tradeTreeService.AcceptTradeAsync(tradeId, userBId, userTreeBId);

//            // Assert
//            Assert.Equal("Trade accepted successfully", result);
//            Assert.Equal(trade.TreeOwnerAid, userTreeB.TreeOwnerId);
//            Assert.Equal(userBId, requesterTree.TreeOwnerId);
//            Assert.Equal(TradeStatus.Completed, trade.Status);
//            Assert.Equal(userBId, trade.TreeOwnerBid);

//            _mockTradeHistoryService.Verify(service => service.UpdateTradeHistoryAsync(trade), Times.Once);
//        }

//        [Fact]
//        public async Task AcceptTradeAsync_TradeNotPending_ReturnsErrorMessage()
//        {
//            // Arrange
//            int tradeId = 1;
//            int userBId = 2;
//            int userTreeBId = 20;

//            var trade = new TradeHistory
//            {
//                TradeId = tradeId,
//                Status = TradeStatus.Completed // Already completed
//            };

//            _mockTradeHistoryService.Setup(service => service.GetTradeHistoryByIdAsync(tradeId))
//                .ReturnsAsync(trade);

//            // Act
//            var result = await _tradeTreeService.AcceptTradeAsync(tradeId, userBId, userTreeBId);

//            // Assert
//            Assert.Equal("Trade is not in pending status", result);
//            _mockTradeHistoryService.Verify(service => service.UpdateTradeHistoryAsync(It.IsAny<TradeHistory>()), Times.Never);
//        }

//        [Fact]
//        public async Task AcceptTradeAsync_UserTreeDoesNotExist_ReturnsErrorMessage()
//        {
//            // Arrange
//            int tradeId = 1;
//            int userBId = 2;
//            int userTreeBId = 20;

//            var trade = new TradeHistory
//            {
//                TradeId = tradeId,
//                Status = TradeStatus.Pending
//            };

//            _mockTradeHistoryService.Setup(service => service.GetTradeHistoryByIdAsync(tradeId))
//                .ReturnsAsync(trade);

//            _mockUserTreeRepository.Setup(repo => repo.GetByIdAsync(userTreeBId))
//                .ReturnsAsync((UserTree)null);

//            // Act
//            var result = await _tradeTreeService.AcceptTradeAsync(tradeId, userBId, userTreeBId);

//            // Assert
//            Assert.Equal("Your tree does not exist", result);
//            _mockTradeHistoryService.Verify(service => service.UpdateTradeHistoryAsync(It.IsAny<TradeHistory>()), Times.Never);
//        }

//        [Fact]
//        public async Task AcceptTradeAsync_TreeDoesNotBelongToUser_ReturnsErrorMessage()
//        {
//            // Arrange
//            int tradeId = 1;
//            int userBId = 2;
//            int userTreeBId = 20;

//            var trade = new TradeHistory
//            {
//                TradeId = tradeId,
//                Status = TradeStatus.Pending
//            };

//            var userTreeB = new UserTree
//            {
//                UserTreeId = userTreeBId,
//                TreeOwnerId = 3 // Different owner
//            };

//            _mockTradeHistoryService.Setup(service => service.GetTradeHistoryByIdAsync(tradeId))
//                .ReturnsAsync(trade);

//            _mockUserTreeRepository.Setup(repo => repo.GetByIdAsync(userTreeBId))
//                .ReturnsAsync(userTreeB);

//            // Act
//            var result = await _tradeTreeService.AcceptTradeAsync(tradeId, userBId, userTreeBId);

//            // Assert
//            Assert.Equal("Tree does not belong to you", result);
//            _mockTradeHistoryService.Verify(service => service.UpdateTradeHistoryAsync(It.IsAny<TradeHistory>()), Times.Never);
//        }

//        [Fact]
//        public async Task AcceptTradeAsync_TreeNotFinal_ReturnsErrorMessage()
//        {
//            // Arrange
//            int tradeId = 1;
//            int userBId = 2;
//            int userTreeBId = 20;

//            var trade = new TradeHistory
//            {
//                TradeId = tradeId,
//                Status = TradeStatus.Pending,
//                DesiredTreeAID = 15
//            };

//            var userTreeB = new UserTree
//            {
//                UserTreeId = userTreeBId,
//                TreeOwnerId = userBId,
//                FinalTreeId = null // Not final
//            };

//            _mockTradeHistoryService.Setup(service => service.GetTradeHistoryByIdAsync(tradeId))
//                .ReturnsAsync(trade);

//            _mockUserTreeRepository.Setup(repo => repo.GetByIdAsync(userTreeBId))
//                .ReturnsAsync(userTreeB);

//            // Act
//            var result = await _tradeTreeService.AcceptTradeAsync(tradeId, userBId, userTreeBId);

//            // Assert
//            Assert.Equal("Tree is not final", result);
//            _mockTradeHistoryService.Verify(service => service.UpdateTradeHistoryAsync(It.IsAny<TradeHistory>()), Times.Never);
//        }

//        [Fact]
//        public async Task AcceptTradeAsync_TreeDoesNotMatchDesiredTree_ReturnsErrorMessage()
//        {
//            // Arrange
//            int tradeId = 1;
//            int userBId = 2;
//            int userTreeBId = 20;

//            var trade = new TradeHistory
//            {
//                TradeId = tradeId,
//                Status = TradeStatus.Pending,
//                DesiredTreeAID = 15
//            };

//            var userTreeB = new UserTree
//            {
//                UserTreeId = userTreeBId,
//                TreeOwnerId = userBId,
//                FinalTreeId = 16, // Different tree ID than desired
//                FinalTree = new Tree { TreeId = 16 }
//            };

//            _mockTradeHistoryService.Setup(service => service.GetTradeHistoryByIdAsync(tradeId))
//                .ReturnsAsync(trade);

//            _mockUserTreeRepository.Setup(repo => repo.GetByIdAsync(userTreeBId))
//                .ReturnsAsync(userTreeB);

//            // Act
//            var result = await _tradeTreeService.AcceptTradeAsync(tradeId, userBId, userTreeBId);

//            // Assert
//            Assert.Equal("Tree does not match the desired tree", result);
//            _mockTradeHistoryService.Verify(service => service.UpdateTradeHistoryAsync(It.IsAny<TradeHistory>()), Times.Never);
//        }

//        public async Task AcceptTradeAsync_MissingInputData_ReturnsErrorMessage()
//        {
//            // Arrange
//            int tradeId = 1;
//            int userBId = 2;
//            int userTreeBId = 20;

//            var trade = new TradeHistory
//            {
//                TradeId = tradeId,
//                Status = TradeStatus.Pending,
//                TreeAid = null, // Missing Tree A ID
//                TreeOwnerAid = 1,
//                DesiredTreeAID = 15
//            };

//            var userTreeB = new UserTree
//            {
//                UserTreeId = userTreeBId,
//                TreeOwnerId = userBId,
//                FinalTreeId = 15,
//                FinalTree = new Tree { TreeId = 15 }
//            };

//            _mockTradeHistoryService.Setup(service => service.GetTradeHistoryByIdAsync(tradeId))
//                .ReturnsAsync(trade);

//            _mockUserTreeRepository.Setup(repo => repo.GetByIdAsync(userTreeBId))
//                .ReturnsAsync(userTreeB);

//            // Act
//            var result = await _tradeTreeService.AcceptTradeAsync(tradeId, userBId, userTreeBId);

//            // Assert
//            Assert.Equal("Missing input data", result);
//            _mockTradeHistoryService.Verify(service => service.UpdateTradeHistoryAsync(It.IsAny<TradeHistory>()), Times.Never);
//        }

//        #endregion
//    }
//}
