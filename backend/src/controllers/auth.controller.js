const httpStatus = require('http-status');
const catchAsync = require('../utils/catchAsync');
const { authService, userService, tokenService, emailService } = require('../services');
const { verifyFirebaseToken } = require('../services/firebase.service');
const ApiError = require('../utils/ApiError');
const gsService = require('../services/gs.service');
const { v4: uuidv4 } = require('uuid');
const socketModule = require('../socket/socket.controller');
const { token } = require('morgan');

function generateUserId() {
  return `user_${uuidv4()}`;
}

const registerOrLogin = catchAsync(async (req, res) => {
  try {
    const { userId, tokenId, email } = req.body;

    await verifyFirebaseToken(userId, tokenId);

    let user;
    const doesUserExist = await userService.doesUserExist(userId);

    if (doesUserExist) {
      user = await userService.getUserByUserId(userId);

      if (!user) {
        throw new ApiError(httpStatus.INTERNAL_SERVER_ERROR, 'User exists but could not be retrieved');
      }
    } else {
      try {
        await gsService.registerUser(userId, email);

        const walletId = await gsService.getUserWalletAddress(userId);

        const name = userId.substring(0, 7);

        user = await userService.createUser({ userId, email, walletId, name });
      } catch (e) {
        if (e instanceof ApiError) {
          if (e.statusCode === 409) {
            user = await userService.getUserByUserId(userId);
            if (!user) {
              throw new ApiError(httpStatus.INTERNAL_SERVER_ERROR, 'Failed to retrieve existing user');
            }
          } else {
            throw e;
          }
        } else if (e.code === 'P2002') {
          user = await userService.getUserByUserId(userId);
          if (!user) {
            throw new ApiError(httpStatus.INTERNAL_SERVER_ERROR, 'Failed to retrieve existing user');
          }
        } else {
          throw e;
        }
      }
    }

    const tokens = await tokenService.generateAuthTokens(user);

    res.send({ user, tokens });
  } catch (e) {
    if (e instanceof ApiError) {
      res.status(e.statusCode).send({ message: e.message });
    } else {
      res.status(httpStatus.INTERNAL_SERVER_ERROR).send({ message: e.message });
    }
    console.log('Error in registerOrLogin:', e);
  }
});

const walletLogin = catchAsync(async (req, res) => {
  const { wallet, email, sessionId } = req.body;

  console.log('Wallet login latest', wallet, email, sessionId);

  try {
    let user = await userService.getUserByWallet(wallet);
    console.log('User by wallet:', user);
    let userByEmail = await userService.getUserByEmail(email);
    console.log('User by email:', userByEmail);

    if (user) {
      if (user.email !== email) {
        const abbreviatedEmail = abbreviateEmail(user.email);
        const abbreviatedInputEmail = abbreviateEmail(email);
        throw new ApiError(
          httpStatus.CONFLICT,
          `Wallet is associated with a different email: ${abbreviatedEmail}. Input email ${abbreviatedInputEmail} is ${
            userByEmail
              ? `associated with wallet: ${abbreviateWallet(userByEmail.walletId)}`
              : 'not associated with any wallet'
          }.`
        );
      }
    } else if (userByEmail) {
      const abbreviatedWallet = abbreviateWallet(userByEmail.walletId);
      const abbreviatedInputWallet = abbreviateWallet(wallet);
      throw new ApiError(
        httpStatus.CONFLICT,
        `Email is associated with a different wallet: ${abbreviatedWallet}. Input wallet ${abbreviatedInputWallet} is not associated with any email.`
      );
    } else {
      try {
        const userId = generateUserId();
        await gsService.registerUserWithWallet(userId, email, wallet);
        user = await userService.createUser({
          userId,
          walletId: wallet,
          email,
        });
      } catch (error) {
        console.error('Error creating user:', error);
        if (error.statusCode === 409) {
          // User already exists in GameShift, but not in our database
          // Retrieve the user from GameShift
          const gsUser = await gsService.getUserByWalletAddress(wallet);
          if (gsUser && gsUser.address === wallet && gsUser.email === email) {
            // Create the user in our database
            user = await userService.createUser({
              userId: gsUser.referenceId,
              walletId: wallet,
              email,
            });
          } else {
            const abbreviatedEmail = abbreviateEmail(gsUser ? gsUser.email : email);
            const abbreviatedWallet = abbreviateWallet(gsUser ? gsUser.address : wallet);
            throw new ApiError(
              httpStatus.CONFLICT,
              `User exists with different credentials. Email: ${abbreviatedEmail}, Wallet: ${abbreviatedWallet}`
            );
          }
        } else {
          // For other errors, rethrow
          throw error;
        }
      }
    }

    const tokens = await tokenService.generateAuthTokens(user);

    // only send sockets if the leaderboard request is made from a logged in session
    if (sessionId) {
      socketModule.SendQRSignupMessage(sessionId, 'qrLoginCompleted', {
        user,
        tokens,
      });
    }

    res.status(httpStatus.OK).send({ message: 'Login completed', user, tokens });
  } catch (error) {
    console.error('Error in walletLogin:', error);
    if (error instanceof ApiError) {
      res.status(error.statusCode).send({ message: error.message });
    } else {
      res.status(httpStatus.INTERNAL_SERVER_ERROR).send({ message: 'An unexpected error occurred' });
    }
  }
});

function abbreviateEmail(email) {
  const [localPart, domain] = email.split('@');
  const localLength = localPart.length;
  return `${localPart.slice(0, 3)}...${localPart.slice(localLength - 3)}@${domain}`;
}

function abbreviateWallet(wallet) {
  return `${wallet.slice(0, 6)}...${wallet.slice(-4)}`;
}

const refreshTokens = catchAsync(async (req, res) => {
  const tokens = await authService.refreshAuth(req.body.refreshToken);
  res.send({ ...tokens });
});

const forgotPassword = catchAsync(async (req, res) => {
  const resetPasswordToken = await tokenService.generateResetPasswordToken(req.body.email);
  await emailService.sendResetPasswordEmail(req.body.email, resetPasswordToken);
  res.status(httpStatus.NO_CONTENT).send();
});

const resetPassword = catchAsync(async (req, res) => {
  await authService.resetPassword(req.query.token, req.body.password);
  res.status(httpStatus.NO_CONTENT).send();
});

const sendVerificationEmail = catchAsync(async (req, res) => {
  const verifyEmailToken = await tokenService.generateVerifyEmailToken(req.user);
  await emailService.sendVerificationEmail(req.user.email, verifyEmailToken);
  res.status(httpStatus.NO_CONTENT).send();
});

const verifyEmail = catchAsync(async (req, res) => {
  await authService.verifyEmail(req.query.token);
  res.status(httpStatus.NO_CONTENT).send();
});

module.exports = {
  registerOrLogin,
  walletLogin,
  refreshTokens,
  forgotPassword,
  resetPassword,
  sendVerificationEmail,
  verifyEmail,
};
