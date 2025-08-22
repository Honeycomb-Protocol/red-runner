const config = require('../config/config');
const gameShiftSdk = require('@api/gameshift');
const ApiError = require('../utils/ApiError');
const { shopService } = require('./index');
const gsUtil = require('../utils/gsUtil');
const httpStatus = require('http-status');
const axios = require('axios');

gameShiftSdk.server(config.gs.gsBaseUrl);
gameShiftSdk.core.setServer(config.gs.gsBaseUrl)

/**
 * Create a user
 * @returns {Promise<User>}
 * @param referenceId
 * @param emailId
 */
const registerUser = async (referenceId, emailId) => {
  try {
    gameShiftSdk.server(config.gs.gsBaseUrl);
  gameShiftSdk.core.setServer(config.gs.gsBaseUrl)
  console.log("spcapi",gameShiftSdk.core.spec.api)
    console.log("config.gs.gsBaseUrl", config.gs.gsBaseUrl);

    await gameShiftSdk.projectUserController_create(
      {
        referenceId: referenceId,
        email: emailId,
      },
      {
        'x-api-key': config.gs.gsApiKey,
      }
    );
  } catch (e) {
    if (e.data !== undefined) throw new ApiError(e.data.statusCode, e.data.message);
    throw e;
  }
};

const registerUserWithWallet = async (referenceId, emailId, walletId) => {
  try {
    await gameShiftSdk.projectUserController_create(
      {
        referenceId: referenceId,
        email: emailId,
        externalWalletAddress: walletId,
      },
      {
        'x-api-key': config.gs.gsApiKey,
      }
    );
  } catch (e) {
    if (e.data !== undefined) throw new ApiError(e.data.statusCode, e.data.message);
    throw e;
  }
};

const getUserWalletAddress = async (referenceId) => {
  try {
    const data = await gameShiftSdk.projectUserController_getWalletAddress({
      referenceId: referenceId,
      'x-api-key': config.gs.gsApiKey,
    });

    return data.data.address;
  } catch (e) {
    if (e.data !== undefined) throw new ApiError(e.data.statusCode, e.data.message);
    throw e;
  }
};

const getUserByWalletAddress = async (walletAddress) => {
  try {
    const response = await axios.get(`https://api.gameshift.dev/nx/users/wallet/${walletAddress}`, {
      headers: {
        accept: 'application/json',
        'x-api-key': config.gs.gsApiKey,
      },
    });

    return response.data;
  } catch (error) {
    if (error.response) {
      // The request was made and the server responded with a status code
      // that falls out of the range of 2xx
      throw new Error(`API Error: ${error.response.status} - ${error.response.data.message}`);
    } else if (error.request) {
      // The request was made but no response was received
      throw new Error('No response received from the server');
    } else {
      // Something happened in setting up the request that triggered an Error
      throw new Error('Error setting up the request');
    }
  }
};

const getUserByReferenceId = async (userId) => {
  try {
    return await gameShiftSdk.projectUserController_get({ referenceId: userId, 'x-api-key': config.gs.gsApiKey });
  } catch (error) {
    if (e.data !== undefined) throw new ApiError(e.data.statusCode, e.data.message);
  }
};

// Example usage
// const config = { gs: { gsApiKey: 'your-api-key-here' } };
// getUserWalletAddress('EpP1kc8zqJ7ZZsB5vqXuznEEBgYGMTD9JAHzmnnPTxpj', config.gs.gsApiKey)
//   .then(data => console.log(data))
//   .catch(error => console.error(error));

const getUserInventory = async (dictionary) => {
  try {
    const meta = {};

    meta['referenceId'] = dictionary.userId;
    meta['x-api-key'] = config.gs.gsApiKey;
    meta['perPage'] = 10;
    meta['page'] = dictionary['pageNumber'].toString();

    if (dictionary.hasOwnProperty('types')) {
      if (typeof dictionary['types'] !== 'string') {
        meta['types'] = dictionary['types']['value'].split(',').map(encodeURIComponent).join('%2C');
      } else {
        meta['types'] = dictionary['types'].split(',').map(encodeURIComponent).join('%2C');
      }
    }
    if (dictionary.hasOwnProperty('forSale')) {
      meta['forSale'] = dictionary['forSale'];
    }
    if (dictionary.hasOwnProperty('collectionId')) {
      meta['collectionId'] = dictionary['collectionId'];
    }

    return await gameShiftSdk.projectUserController_getUserItems(meta);
  } catch (e) {
    if (e.data !== undefined) throw new ApiError(e.data.statusCode, e.data.message);
    throw e;
  }
};

const GetMarketPlaceAssets = async (dictionary) => {
  try {
    const meta = {};

    meta['x-api-key'] = config.gs.gsApiKey;
    meta['perPage'] = 10;
    meta['page'] = dictionary['pageNumber'].toString();
    meta['forSale'] = 'true';

    if (dictionary.hasOwnProperty('types')) {
      if (typeof dictionary['types'] !== 'string') {
        meta['types'] = dictionary['types']['value'].split(',').map(encodeURIComponent).join('%2C');
      } else {
        meta['types'] = dictionary['types'].split(',').map(encodeURIComponent).join('%2C');
      }
    }
    if (dictionary.hasOwnProperty('collectionId')) {
      meta['collectionId'] = dictionary['collectionId'];
    }

    const returnDict = await gameShiftSdk.itemsController_getAll(meta);

    //Filtering the dictionary here to remove items that are owned by this particular user requesting the data//
    // return returnDict['data'].filter(asset => asset.item.owner.referenceId !== dictionary.userId);
    return returnDict;
  } catch (e) {
    if (e.data !== undefined) throw new ApiError(e.data.statusCode, e.data.message);
    throw e;
  }
};

const MintAssetToUser = async (referenceId, assetId) => {
  try {
    return await gameShiftSdk.assetController_createAsset(
      {
        details: shopService.GetItemMintableByKey(assetId),
        destinationUserReferenceId: referenceId,
      },
      {
        'x-api-key': config.gs.gsApiKey,
      }
    );
  } catch (e) {
    if (e.data !== undefined) throw new ApiError(e.data.statusCode, e.data.message);
    throw e;
  }
};

const ListAssetForSale = async (assetId, amount) => {
  try {
    return await gameShiftSdk.assetController_listAsset(
      {
        price: {
          currencyId: 'USDC',
          naturalAmount: amount.toString(),
        },
      },
      {
        itemId: assetId,
        'x-api-key': config.gs.gsApiKey,
      }
    );
  } catch (e) {
    if (e.data !== undefined) throw new ApiError(e.data.statusCode, e.data.message);
    throw e;
  }
};

const DeListAssetForSale = async (assetId) => {
  try {
    return await gameShiftSdk.assetController_cancelListingAsset({
      itemId: assetId,
      'x-api-key': config.gs.gsApiKey,
    });
  } catch (e) {
    if (e.data !== undefined) throw new ApiError(e.data.statusCode, e.data.message);
    throw e;
  }
};

const BuyAssetFromMarketplace = async (referenceId, assetId) => {
  try {
    return await gameShiftSdk.assetController_buyAsset(
      {
        buyerId: referenceId,
      },
      {
        itemId: assetId,
        'x-api-key': config.gs.gsApiKey,
      }
    );
  } catch (e) {
    if (e.data !== undefined) throw new ApiError(e.data.statusCode, e.data.message);
    throw e;
  }
};

const UpdateAsset = async (assetId, itemId, usesLeft) => {
  try {
    const nftData = shopService.GetItemMintableByKey(itemId);

    const newAttributes = nftData.attributes;

    newAttributes.forEach((attribute) => {
      if (attribute.traitType === 'UsesLeft') {
        attribute.value = usesLeft.toString();
      }
    });

    if (!nftData) new ApiError(httpStatus.NOT_FOUND, 'Item not found');

    return await gameShiftSdk.assetController_editAsset(
      {
        imageUrl: nftData.imageUrl,
        attributes: newAttributes,
        name: nftData.name,
        description: nftData.description,
      },
      {
        itemId: assetId,
        'x-api-key': config.gs.gsApiKey,
      }
    );
  } catch (e) {
    if (e.data !== undefined) throw new ApiError(e.data.statusCode, e.data.message);
    throw e;
  }
};

const CreateCreditCardPayment = async (referenceId, itemId, currencyId, currencyPrice, itemTitle, itemDesc) => {
  try {
    return await gameShiftSdk.paymentController_checkout(
      {
        price: { currencyId: currencyId, naturalAmount: currencyPrice },
        title: itemTitle,
        description: itemDesc,
        quantity: 1,
        buyerId: referenceId,
      },
      {
        'x-api-key': config.gs.gsApiKey,
      }
    );
  } catch (e) {
    if (e.data !== undefined) throw new ApiError(e.data.statusCode, e.data.message);
    throw e;
  }
};

const WithdrawFunds = async (referenceId) => {
  try {
    return await gameShiftSdk.projectUserController_withdraw({
      referenceId: referenceId,
      'x-api-key': config.gs.gsApiKey,
    });
  } catch (e) {
    if (e.data !== undefined) throw new ApiError(e.data.statusCode, e.data.message);
    throw e;
  }
};

const CreateSolPayment = async (referenceId, itemToBuy) => {
  try {
    const dataToSend = {
      details: {
        name: itemToBuy.name,
        description: itemToBuy.description,
        imageUrl: itemToBuy.imageUrl,
        collectionId: itemToBuy.collectionId,
        attributes: itemToBuy.attributes,
      },
      price: {
        currencyId: 'SOL',
        naturalAmount: itemToBuy.price['SOL'],
      },
      destinationUserReferenceId: referenceId,
    };

    const response = await gsUtil.AxiosInstance.post('payments/unique-assets', dataToSend);

    if (response.status === 201) {
      return response;
    } else {
      new ApiError(httpStatus.INTERNAL_SERVER_ERROR, 'Error creating payment');
    }
  } catch (e) {
    throw e;
  }
};

const TransferItemFromDevToUser = async (referenceId, itemId, currencyId, currencyPrice, itemTitle, itemDesc) => {
  try {
    return await gameShiftSdk.paymentController_checkout(
      {
        price: { currencyId: currencyId, naturalAmount: currencyPrice },
        title: itemTitle,
        description: itemDesc,
        quantity: 1,
        buyerId: referenceId,
      },
      {
        'x-api-key': config.gs.gsApiKey,
      }
    );
  } catch (e) {
    if (e.data !== undefined) throw new ApiError(e.data.statusCode, e.data.message);
    throw e;
  }
};

const TransferSolDevWalletToUser = async (referenceId, quantity) => {
  try {
    const data = {
      destinationUserReferenceId: referenceId,
      quantity: quantity.toString(),
    };

    const response = await gsUtil.AxiosInstance.post('developer-wallet/items/SOL/transfer', data, {
      headers: {
        'x-wallet-key': config.gs.gsWPrivateKey,
      },
    });

    if (response.status === 201) {
      return response;
    } else {
      new ApiError(httpStatus.INTERNAL_SERVER_ERROR, 'Error creating payment');
    }
  } catch (e) {
    throw e;
  }
};

const TransferRRDevWalletToUser = async (referenceId, quantity) => {
  try {
    const data = {
      destinationUserReferenceId: referenceId,
      quantity: quantity.toString(),
    };

    const response = await gsUtil.AxiosInstance.post(`developer-wallet/items/${config.gs.gsTokenRRId}/transfer`, data, {
      headers: {
        'x-wallet-key': config.gs.gsWPrivateKey,
      },
    });

    if (response.status === 201) {
      return response;
    } else {
      new ApiError(httpStatus.INTERNAL_SERVER_ERROR, 'Error creating payment');
    }
  } catch (e) {
    throw e;
  }
};

module.exports = {
  registerUser,
  registerUserWithWallet,
  getUserWalletAddress,
  getUserByWalletAddress,
  getUserInventory,
  MintAssetToUser,
  CreateCreditCardPayment,
  CreateSolPayment,
  TransferSolDevWalletToUser,
  UpdateAsset,
  WithdrawFunds,
  ListAssetForSale,
  DeListAssetForSale,
  BuyAssetFromMarketplace,
  GetMarketPlaceAssets,
  TransferRRDevWalletToUser,
  getUserByReferenceId,
};
