const { gsService, shopService } = require('../services');
const paymentService = require('../services/payment.service');
const catchAsync = require('../utils/catchAsync');
const ApiError = require('../utils/ApiError');
const httpStatus = require('http-status');
const DataParser = require('../parsers/inventoryData.parser');
const { PaymentStatus } = require('../utils/staticData');
const gsUtil = require('../utils/gsUtil');
const staticData = require('../utils/staticData');
const userService = require('../services/user.service');
const { PrismaClient } = require('@prisma/client');
const prisma = new PrismaClient();

const getUserWalletAddress = catchAsync(async (req, res) => {
  try {
    const walletAddress = await gsService.getUserWalletAddress(req.user.id);
    res.status(httpStatus.FOUND).send({ walletId: walletAddress });
  } catch (e) {
    throw new ApiError(httpStatus.NOT_FOUND, 'Not found');
  }
});

const getUserInventory = catchAsync(async (req, res) => {
  try {
    const inventory = await gsService.getUserInventory(req.body);
    const dataParser = new DataParser(inventory['data']);
    const modelToSend = dataParser.GetClientModel();
    res.status(httpStatus.OK).send(modelToSend);
  } catch (e) {
    throw new ApiError(httpStatus.NOT_FOUND, 'Not found');
  }
});

const mintAssetToUser = catchAsync(async (req, res) => {
  try {
    const asset = await gsService.MintAssetToUser(req.body.userId, req.body.itemId);
    res.status(httpStatus.OK).send(asset);
  } catch (e) {
    if (e instanceof ApiError) {
      res.status(e.statusCode).send({ message: e.message });
      return;
    }
    res.status(httpStatus.INTERNAL_SERVER_ERROR).send({ message: e.message });
  }
});

const GetMarketPlaceItems = catchAsync(async (req, res) => {
  try {
    const inventory = await gsService.GetMarketPlaceAssets(req.body);
    const dataParser = new DataParser(inventory['data']);
    const modelToSend = dataParser.GetClientModel();
    res.status(httpStatus.OK).send(modelToSend);
  } catch (e) {
    throw new ApiError(httpStatus.NOT_FOUND, 'Not found');
  }
});

const updateUserAsset = catchAsync(async (req, res) => {
  try {
    const asset = await gsService.UpdateAsset(req.body.assetId, req.body.itemId, req.body['usesLeft']);
    res.status(httpStatus.OK).send(asset);
  } catch (e) {
    if (e instanceof ApiError) {
      res.status(e.statusCode).send({ message: e.message });
      return;
    }
    res.status(httpStatus.INTERNAL_SERVER_ERROR).send({ message: e.message });
  }
});

const withdrawFundsToAccount = catchAsync(async (req, res) => {
  try {
    const asset = await gsService.WithdrawFunds(req.body.userId);
    res.status(httpStatus.OK).send(asset);
  } catch (e) {
    if (e instanceof ApiError) {
      res.status(e.statusCode).send({ message: e.message });
      return;
    }
    res.status(httpStatus.INTERNAL_SERVER_ERROR).send({ message: e.message });
  }
});

const ListAssetForSale = catchAsync(async (req, res) => {
  try {
    const asset = await gsService.ListAssetForSale(req.body.assetId, req.body.amount);
    res.status(httpStatus.OK).send(asset);
  } catch (e) {
    if (e instanceof ApiError) {
      res.status(e.statusCode).send({ message: e.message });
      return;
    }
    res.status(httpStatus.INTERNAL_SERVER_ERROR).send({ message: e.message });
  }
});

const DeListAssetForSale = catchAsync(async (req, res) => {
  try {
    const asset = await gsService.DeListAssetForSale(req.body.assetId);
    res.status(httpStatus.OK).send(asset);
  } catch (e) {
    if (e instanceof ApiError) {
      res.status(e.statusCode).send({ message: e.message });
      return;
    }
    res.status(httpStatus.INTERNAL_SERVER_ERROR).send({ message: e.message });
  }
});

const BuyAssetFromMarket = catchAsync(async (req, res) => {
  try {
    const asset = await gsService.BuyAssetFromMarketplace(req.body.userId, req.body.assetId);
    res.status(httpStatus.OK).send(asset);
  } catch (e) {
    if (e instanceof ApiError) {
      res.status(e.statusCode).send({ message: e.message });
      return;
    }
    res.status(httpStatus.INTERNAL_SERVER_ERROR).send({ message: e.message });
  }
});

const transferSolDevToUser = catchAsync(async (req, res) => {
  try {
    const response = await gsService.TransferSolDevWalletToUser(req.body.userId, req.body.quantity);
    res.status(httpStatus.OK).send(response.data);
  } catch (e) {
    if (e instanceof ApiError) {
      res.status(e.statusCode).send({ message: e.message });
      return;
    }
    res.status(httpStatus.INTERNAL_SERVER_ERROR).send({ message: e.message });
  }
});

const userClaimsCoins = catchAsync(async (req, res) => {
  const { quantity, userId } = req.body;

  console.log(`User ${userId} is attempting to claim ${quantity} coins`);

  try {
    // First, get the user by userId
    const user = await userService.getUserByUserId(userId);
    if (!user) {
      throw new ApiError(httpStatus.NOT_FOUND, 'User not found');
    }

    // Update the user's coins
    await userService.updateUserById(user.id, {
      coinsCollected: {
        increment: quantity,
      },
    });

    const rrQuantity = quantity * staticData.CoinToSolRatio;

    const response = await gsService.TransferRRDevWalletToUser(userId, rrQuantity.toString());

    // Check if response.data exists before sending it
    if (response && response.data) {
      res.status(httpStatus.OK).send(response.data);
    } else {
      res.status(httpStatus.OK).send({ message: 'Coins claimed successfully' });
    }
  } catch (error) {
    console.error('Error in userClaimsCoins:', error);
    if (error instanceof ApiError) {
      res.status(error.statusCode).send({ message: error.message });
    } else {
      res.status(httpStatus.INTERNAL_SERVER_ERROR).send({ message: 'An unexpected error occurred' });
    }
  }
});

const buyItem = catchAsync(async (req, res) => {
  const { userId, itemId, currencyId } = req.body;
  console.log(`User ${userId} is attempting to buy item ${itemId} with currency ${currencyId}`);

  try {
    // First, check if the user exists
    const user = await userService.getUserByUserId(userId);
    if (!user) {
      throw new ApiError(httpStatus.NOT_FOUND, `User with ID ${userId} not found`);
    }

    const itemToBuy = shopService.GetItemMintableByKey(itemId);
    if (!itemToBuy) {
      throw new ApiError(httpStatus.NOT_FOUND, `Item with ID ${itemId} not found`);
    }
    console.log(`Item to buy:`, itemToBuy);

    let payment;
    if (currencyId === 'SOL') {
      console.log('Creating SOL payment');
      payment = await gsService.CreateSolPayment(userId, itemToBuy);
    } else {
      console.log('Creating credit card payment');
      payment = await gsService.CreateCreditCardPayment(
        userId,
        itemId,
        currencyId,
        itemToBuy['price'][currencyId],
        itemToBuy['name'],
        itemToBuy['description']
      );
    }

    console.log('Payment created:', payment);

    if (!payment || !payment.data || !payment.data.id || !payment.data.checkoutUrl) {
      throw new ApiError(httpStatus.INTERNAL_SERVER_ERROR, 'Invalid payment response');
    }

    // Check if a payment with this paymentId already exists
    const existingPayment = await paymentService.getPaymentByPaymentId(payment.data.id);

    if (existingPayment) {
      return res.status(httpStatus.OK).send(payment.data);
    }

    const paymentData = {
      userId: user.id,
      paymentId: payment.data.id,
      itemId,
      status: 'PENDING',
      consentUrl: payment.data.checkoutUrl,
      isAutoMinted: currencyId !== 'USDC',
    };

    console.log('Creating payment record:', paymentData);
    await paymentService.createPayment(paymentData);

    console.log('Payment record created successfully');
    res.status(httpStatus.OK).send(payment.data);
  } catch (error) {
    console.error('Error in buyItem:', error);
    if (error instanceof ApiError) {
      res.status(error.statusCode).send({ message: error.message });
    } else {
      res.status(httpStatus.INTERNAL_SERVER_ERROR).send({
        message: 'An unexpected error occurred while processing the purchase',
        error: error.message,
      });
    }
  }
});

module.exports = {
  getUserWalletAddress,
  getUserInventory,
  mintAssetToUser,
  buyItem,
  transferSolDevToUser,
  userClaimsCoins,
  updateUserAsset,
  withdrawFundsToAccount,
  ListAssetForSale,
  DeListAssetForSale,
  BuyAssetFromMarket,
  GetMarketPlaceItems,
};
