// SPDX-License-Identifier: MIT
pragma solidity ^0.8.24;

contract TransactionLog {
  // order must match those in backend
  enum Action {
    PermissionRequested, // 0
    PermissionApproved, // 1
    PermissionRejected, // 2
    DataViewed, // 3
    PermissionRevoked // 4
  }

  // Transaction logs
  // indexed fields can be filtered/searched efficiently
  event TransactionLogged(
    address indexed employer,
    address indexed worker,
    uint256 date,
    Action action
  );

  // emits the log event / writing to blockchain
  function logTransaction(
    address employer,
    address worker,
    uint256 date,
    Action action
  ) external {
    emit TransactionLogged(employer, worker, date, action);
  }
}
